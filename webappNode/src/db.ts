import { MongoClient, type Collection, type Db } from 'mongodb';
import { JOB_DATABASES, type JobDatabase } from 'webapp-shared';
import { config } from './config.js';

/**
 * Collections mirror DataAccess.connectMongoDb().
 *
 * There are two job databases — "wheel" and "plating" — and each session works
 * against exactly one of them, chosen at sign-in and carried by the session
 * token (see auth.ts). Every accessor below therefore takes that choice as an
 * argument: making it a parameter rather than ambient state means the compiler
 * catches a caller that has not thought about which business it is reading, and
 * there is no request-scoped global to leak from one request into the next.
 *
 * The "settings" database is shared by both, exactly as it is on the desktop:
 * shared settings and sent Xero invoices live there regardless of mode.
 */
let client: MongoClient | null = null;
const jobDbs = new Map<JobDatabase, Db>();
let settingsDb: Db | null = null;

export async function connect(): Promise<void> {
  if (client) return;
  client = new MongoClient(config.mongoUrl, {
    connectTimeoutMS: 15000,
    serverSelectionTimeoutMS: 15000,
  });
  await client.connect();
  for (const name of JOB_DATABASES) {
    jobDbs.set(name, client.db(config.jobDatabases[name]));
  }
  settingsDb = client.db(config.mongoSettingsDb);
  // Fail fast if the server is unreachable rather than on the first request.
  await requireJobDb('wheel').command({ ping: 1 });

  // Only touch databases that already exist. createIndex would otherwise bring
  // an absent one into being — a deployment that runs plating only should not
  // end up with an empty "wheel" database next to it, and vice versa. A missing
  // database still works if someone signs in to it; it just starts unindexed.
  let existing: string[] = [];
  try {
    const listed = await client.db().admin().listDatabases({ nameOnly: true });
    existing = listed.databases.map((d) => d.name);
  } catch (err) {
    console.warn('[db] could not list databases, skipping index checks:', err);
    return;
  }

  for (const name of JOB_DATABASES) {
    const dbName = config.jobDatabases[name];
    if (!existing.includes(dbName)) {
      console.warn(
        `[db] the "${dbName}" database does not exist on this server. Signing in as ` +
          `"${name}" will create it on the first save.`
      );
      continue;
    }
    await ensureJobIndexes(requireJobDb(name), dbName);
  }
}

/**
 * Indexes the web app needs, created per job database.
 *
 * Best-effort throughout: their absence makes queries slower, never wrong, and
 * must not stop the server from starting.
 */
async function ensureJobIndexes(db: Db, label: string): Promise<void> {
  // These make per-job and per-file photo backup lookups fast.
  try {
    await db.collection('jobPictures').createIndex({ jobId: 1 });
    // Exact-match lookups by filename (serving a thumbnail/full variant, and
    // checking whether a file is already backed up) go through this one.
    await db.collection('jobPictures').createIndex({ jobId: 1, name: 1 });
  } catch (err) {
    console.warn(`[db] could not ensure the ${label} jobPictures indexes:`, err);
  }

  // The job list sorts on these. Without them the "Completed jobs" and "Unpaid
  // customers" views fall back to a full collection scan on every request:
  // measured against 10,000 seeded jobs, each request examined all 10,000
  // documents instead of 25. (It does not run out of memory — a sort with a limit
  // uses a bounded top-K sort — it just reads the whole collection every time.)
  //
  // The other two views are already covered by indexes the desktop app created:
  // "Incomplete" and "All"/latest sort on jobDate and jobID, which existing
  // compound indexes can walk. Verified with explain() — see README.
  try {
    await db.collection('jobCard').createIndex({ jobDateCompleted: -1 });
    await db.collection('jobCard').createIndex({ jobDatePaid: 1, jobDateCompleted: -1 });
  } catch (err) {
    console.warn(`[db] could not ensure the ${label} jobCard list indexes:`, err);
  }

  await ensureJobIdIndex(db, label);
}

/**
 * Ensure a plain index on jobID exists.
 *
 * Every jobID lookup already runs off the desktop app's compound
 * `{ jobID: 1, jobDate: -1 }` index, and measured against 5,000 jobs this index
 * changes nothing — one key and one document examined either way. It is here so
 * the web app does not silently depend on those compound indexes surviving: drop
 * them and `GET /api/jobs/:id`, `/latest` and `/neighbours` would all start
 * scanning the whole collection.
 *
 * Deliberately NOT unique. A unique index is what would actually close the
 * duplicate-job-number race in insertWithNextJobId — its retry expects a
 * duplicate-key error that a non-unique index never raises — but building one
 * fails outright if the collection already contains duplicate jobID values, which
 * years of production data may well do. That has to be checked before it can be
 * changed; see README.
 */
async function ensureJobIdIndex(db: Db, label: string): Promise<void> {
  try {
    await db.collection('jobCard').createIndex({ jobID: 1 });
  } catch (err) {
    // 85/86 mean an index on jobID already exists with different options — most
    // likely someone has since made it unique, which is strictly better than this.
    // Treat that as success rather than warning about it on every boot.
    const code = (err as { code?: number }).code;
    if (code === 85 || code === 86) return;
    console.warn(`[db] could not ensure the ${label} jobCard jobID index:`, err);
  }
}

export async function disconnect(): Promise<void> {
  await client?.close();
  client = null;
  jobDbs.clear();
  settingsDb = null;
}

function requireJobDb(database: JobDatabase): Db {
  const db = jobDbs.get(database);
  if (!db) throw new Error('Database not connected');
  return db;
}

function requireSettingsDb(): Db {
  if (!settingsDb) throw new Error('Settings database not connected');
  return settingsDb;
}

export function jobCards(database: JobDatabase): Collection {
  return requireJobDb(database).collection('jobCard');
}

export function pricing(database: JobDatabase): Collection {
  return requireJobDb(database).collection('pricing');
}

export function fussyCustomers(database: JobDatabase): Collection {
  return requireJobDb(database).collection('fussyCustomer');
}

/**
 * Compressed photo backups: two documents per photo (a full copy and a
 * 250px-wide thumbnail), { jobId, name, contentHash, isThumbnail, base64Image }.
 * See photo-backup.ts and photo-backup-sync.ts. Deliberately a separate
 * collection rather than fields on jobCard: base64 image data has no business
 * bloating every job document, and a collection the desktop app never queries
 * cannot affect it at all.
 */
export function jobPictures(database: JobDatabase): Collection {
  return requireJobDb(database).collection('jobPictures');
}

/** Shared by both businesses — not per-database. */
export function settings(): Collection {
  return requireSettingsDb().collection('settings');
}

/** Shared by both businesses — not per-database. See xero-invoices.ts. */
export function sentInvoices(): Collection {
  return requireSettingsDb().collection('sentInvoices');
}
