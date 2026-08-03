import { readFileSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { JobDatabase } from 'webapp-shared';

/** Minimal .env loader so the app has no extra dependency just for config. */
function loadDotEnv(): void {
  const here = dirname(fileURLToPath(import.meta.url));
  for (const candidate of [resolve(here, '../.env'), resolve(here, '../../.env')]) {
    try {
      const text = readFileSync(candidate, 'utf8');
      for (const rawLine of text.split(/\r?\n/)) {
        const line = rawLine.trim();
        if (!line || line.startsWith('#')) continue;
        const eq = line.indexOf('=');
        if (eq <= 0) continue;
        const key = line.slice(0, eq).trim();
        let value = line.slice(eq + 1).trim();
        if (
          (value.startsWith('"') && value.endsWith('"')) ||
          (value.startsWith("'") && value.endsWith("'"))
        ) {
          value = value.slice(1, -1);
        }
        if (process.env[key] === undefined) {
          process.env[key] = value;
        }
      }
      return;
    } catch {
      // no .env at this location — fall through to defaults
    }
  }
}

loadDotEnv();

function num(name: string, fallback: number): number {
  const raw = process.env[name];
  if (!raw) return fallback;
  const n = Number(raw);
  return Number.isFinite(n) ? n : fallback;
}

/**
 * Read `key=value` command-line arguments, e.g.
 *   node dist/server.js mongoIP=192.168.1.50 mongoPort=27017
 * Names are matched case-insensitively so mongoip and MONGOIP both work.
 */
function argValue(name: string): string | undefined {
  const wanted = name.toLowerCase() + '=';
  for (const arg of process.argv.slice(2)) {
    const cleaned = arg.startsWith('--') ? arg.slice(2) : arg;
    if (cleaned.toLowerCase().startsWith(wanted)) {
      return cleaned.slice(wanted.length).trim();
    }
  }
  return undefined;
}

/** Stop with an actionable message rather than silently connecting somewhere else. */
function configError(message: string): never {
  console.error(`[config] ${message}`);
  process.exit(1);
}

/**
 * Where MongoDB is. The desktop app prompts for a host on startup and builds
 * "mongodb://<host>:27017" from it (DataAccess.connectMongoDb via
 * MongoIPAddressInputDialog); this gives the web app the same convenience without
 * having to hand-write a whole connection string.
 *
 * In order of precedence:
 *   1. mongoIP= / mongoPort= command-line arguments
 *   2. MONGO_IP / MONGO_PORT environment variables (or .env)
 *   3. MONGO_URL, if a full connection string is needed (replica set, auth, TLS)
 *   4. mongodb://localhost:27017
 *
 * Host and port are independent: passing only mongoIP keeps port 27017, and
 * passing only mongoPort keeps localhost.
 */
function resolveMongoUrl(): string {
  const host = argValue('mongoIP') ?? process.env.MONGO_IP?.trim();
  const port = argValue('mongoPort') ?? process.env.MONGO_PORT?.trim();

  if (!host && !port) {
    return process.env.MONGO_URL ?? 'mongodb://localhost:27017';
  }

  const resolvedHost = host || 'localhost';
  // A host here is a bare host, not a URL — catching this is worth it because
  // "mongodb://..." in mongoIP would otherwise produce a nonsense address that
  // only fails later, as a confusing connection timeout.
  if (/:\/\//.test(resolvedHost) || resolvedHost.includes('/')) {
    configError(
      `mongoIP should be just a host or IP address, not a URL — got "${resolvedHost}". ` +
        'Use MONGO_URL if you need a full connection string.'
    );
  }

  const resolvedPort = port || '27017';
  const portNumber = Number(resolvedPort);
  if (!Number.isInteger(portNumber) || portNumber < 1 || portNumber > 65535) {
    configError(`mongoPort must be a whole number between 1 and 65535 — got "${resolvedPort}".`);
  }

  // Bracket a bare IPv6 address, which a connection string requires.
  const hostPart = resolvedHost.includes(':') ? `[${resolvedHost}]` : resolvedHost;
  return `mongodb://${hostPart}:${portNumber}`;
}

/** Hide any password before a connection string reaches the logs. */
export function redactMongoUrl(url: string): string {
  return url.replace(/\/\/([^/@]*):([^/@]*)@/, '//$1:***@');
}

export const config = {
  /** See resolveMongoUrl: mongoIP=/mongoPort= args, MONGO_IP/MONGO_PORT, or MONGO_URL. */
  mongoUrl: resolveMongoUrl(),
  /**
   * The job database behind each choice on the sign-in screen. Overridable only
   * for the odd deployment that renamed one; the names below are what both the
   * desktop app and this one use (DataAccess.connectMongoDb).
   */
  jobDatabases: {
    wheel: process.env.MONGO_DB_WHEEL ?? 'wheel',
    plating: process.env.MONGO_DB_PLATING ?? 'plating',
  } as Record<JobDatabase, string>,
  /** Shared by both businesses, as on the desktop: settings and sent invoices. */
  mongoSettingsDb: process.env.MONGO_SETTINGS_DB ?? 'settings',
  port: num('PORT', 3000),
  corsOrigins: (process.env.CORS_ORIGIN ?? 'http://localhost:4200')
    .split(',')
    .map((s) => s.trim())
    .filter(Boolean),
  loginFailureDelayMs: num('LOGIN_FAILURE_DELAY_MS', 3000),
  sessionTtlMs: num('SESSION_TTL_MS', 12 * 60 * 60 * 1000),

  // AUTH_SECRET, AUTH_PASSWORD_SHA256, AUTH_USERNAME and PHOTO_ROOT are not here:
  // they can also come from the shared settings.settings document, so they are
  // resolved by server-settings.ts once the database is connected.

  /** Largest photo/video upload accepted, in bytes. */
  maxPhotoBytes: num('MAX_PHOTO_BYTES', 25 * 1024 * 1024),

  /**
   * Whether new photo uploads also get a copy stored in the jobPictures Mongo
   * collection — an independent backup that survives even if the network
   * photo share is unreachable. See webappNode/README.md.
   */
  photoBackupEnabled: (process.env.PHOTO_BACKUP_ENABLED ?? 'true').trim().toLowerCase() !== 'false',

  /**
   * A photo is stored in the backup completely untouched — original bytes,
   * original format — as long as its base64-encoded form is at most this big.
   * Only a photo that would exceed this (well under Mongo's 16MB document
   * limit) gets compressed, first at quality 85, then quality 50 if still
   * needed — see photo-backup.ts.
   */
  photoBackupCompressThresholdBytes: num(
    'PHOTO_BACKUP_COMPRESS_THRESHOLD_BYTES',
    14 * 1024 * 1024
  ),
};
