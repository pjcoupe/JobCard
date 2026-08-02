import { readFileSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';

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

export const config = {
  mongoUrl: process.env.MONGO_URL ?? 'mongodb://localhost:27017',
  /** The wheel app always targets the "wheel" database. */
  mongoDb: process.env.MONGO_DB ?? 'wheel',
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
