import { MongoClient, type Collection, type Db } from 'mongodb';
import { config } from './config.js';

/**
 * Collections mirror DataAccess.connectMongoDb(): the wheel app uses the
 * "wheel" database for jobs/pricing/fussy customers and a separate "settings"
 * database for shared settings and sent Xero invoices.
 */
let client: MongoClient | null = null;
let jobDb: Db | null = null;
let settingsDb: Db | null = null;

export async function connect(): Promise<void> {
  if (client) return;
  client = new MongoClient(config.mongoUrl, {
    connectTimeoutMS: 15000,
    serverSelectionTimeoutMS: 15000,
  });
  await client.connect();
  jobDb = client.db(config.mongoDb);
  settingsDb = client.db(config.mongoSettingsDb);
  // Fail fast if the server is unreachable rather than on the first request.
  await jobDb.command({ ping: 1 });

  // Best-effort: these indexes make per-job and per-file photo backup lookups
  // fast, but their absence should never stop the server from starting.
  try {
    await jobDb.collection('jobPictures').createIndex({ jobId: 1 });
    // Exact-match lookups by filename (serving a thumbnail/full variant, and
    // checking whether a file is already backed up) go through this one.
    await jobDb.collection('jobPictures').createIndex({ jobId: 1, name: 1 });
  } catch (err) {
    console.warn('[db] could not ensure the jobPictures indexes:', err);
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
    await jobDb.collection('jobCard').createIndex({ jobDateCompleted: -1 });
    await jobDb.collection('jobCard').createIndex({ jobDatePaid: 1, jobDateCompleted: -1 });
  } catch (err) {
    console.warn('[db] could not ensure the jobCard list indexes:', err);
  }

  await ensureJobIdIndex(jobDb);
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
async function ensureJobIdIndex(db: Db): Promise<void> {
  try {
    await db.collection('jobCard').createIndex({ jobID: 1 });
  } catch (err) {
    // 85/86 mean an index on jobID already exists with different options — most
    // likely someone has since made it unique, which is strictly better than this.
    // Treat that as success rather than warning about it on every boot.
    const code = (err as { code?: number }).code;
    if (code === 85 || code === 86) return;
    console.warn('[db] could not ensure the jobCard jobID index:', err);
  }
}

export async function disconnect(): Promise<void> {
  await client?.close();
  client = null;
  jobDb = null;
  settingsDb = null;
}

function requireJobDb(): Db {
  if (!jobDb) throw new Error('Database not connected');
  return jobDb;
}

function requireSettingsDb(): Db {
  if (!settingsDb) throw new Error('Settings database not connected');
  return settingsDb;
}

export function jobCards(): Collection {
  return requireJobDb().collection('jobCard');
}

export function pricing(): Collection {
  return requireJobDb().collection('pricing');
}

export function fussyCustomers(): Collection {
  return requireJobDb().collection('fussyCustomer');
}

/**
 * Compressed photo backups: two documents per photo (a full copy and a
 * 250px-wide thumbnail), { jobId, name, contentHash, isThumbnail, base64Image }.
 * See photo-backup.ts and photo-backup-sync.ts. Deliberately a separate
 * collection rather than a field on jobCard — the desktop app's MongoDB
 * driver throws on unmapped fields, so this keeps it completely invisible and
 * safe to it.
 */
export function jobPictures(): Collection {
  return requireJobDb().collection('jobPictures');
}

export function settings(): Collection {
  return requireSettingsDb().collection('settings');
}

export function sentInvoices(): Collection {
  return requireSettingsDb().collection('sentInvoices');
}
