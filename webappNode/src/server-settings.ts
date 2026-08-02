import { settings } from './db.js';

/**
 * Server configuration that can live in the shared settings.settings document
 * instead of a .env file on the host, so there is one place to configure both
 * apps. The desktop app maps these same field names (DataAccess.SettingsSettingsDoc)
 * but never reads them.
 *
 * Precedence is env -> Mongo -> built-in default. Environment wins for the same
 * reason config.ts's loadDotEnv() lets real environment variables win: it is the
 * escape hatch when Mongo is unreachable, or when a bad value in the database
 * would otherwise lock everyone out.
 *
 * Values are loaded once at startup (loadServerSettings, called from main after
 * the database connects) rather than read per request, so a request never depends
 * on a database round trip for its own auth configuration.
 */

/** The committed default. Serving with this is a security hole — see warnAboutDefaults. */
const DEFAULT_AUTH_SECRET = 'change-me-in-production';

const DEFAULT_AUTH_USERNAME = 'george';

/** sha256('jobcard-wheel:' + <the original password>). */
const DEFAULT_AUTH_PASSWORD_SHA256 =
  '043096f87386ca9dae911b7741152258bf31731fba369ed682617920eeed0732';

interface StoredServerSettings {
  PHOTO_ROOT?: unknown;
  AUTH_SECRET?: unknown;
  AUTH_PASSWORD_SHA256?: unknown;
  AUTH_USERNAME?: unknown;
}

let stored: StoredServerSettings = {};
let loaded = false;

function fromStore(key: keyof StoredServerSettings): string | null {
  const value = stored[key];
  if (typeof value !== 'string') return null;
  const trimmed = value.trim();
  return trimmed === '' ? null : trimmed;
}

function resolve(envName: string, key: keyof StoredServerSettings, fallback: string): string {
  const fromEnv = process.env[envName];
  if (fromEnv !== undefined && fromEnv.trim() !== '') return fromEnv.trim();
  return fromStore(key) ?? fallback;
}

/**
 * Read the shared settings document. Best-effort: a missing document or an
 * unreachable collection just leaves every value on its env-or-default, which is
 * exactly how the app behaved before these fields existed.
 */
export async function loadServerSettings(): Promise<void> {
  try {
    // findSettings() in DataAccess.cs prefers the document with a complete Xero
    // client config; for these fields the first document is enough, and in
    // practice there is only ever one.
    const doc = await settings().findOne({});
    stored = (doc ?? {}) as StoredServerSettings;
    loaded = true;
  } catch (err) {
    console.warn('[settings] could not read settings.settings, using env and defaults:', err);
    stored = {};
    loaded = true;
  }
}

/**
 * Root of the shared photo drive. Defaults to the desktop app's K:\ on Windows;
 * elsewhere it must be set explicitly or photos are simply turned off.
 * See webappNode/README.md for the mapped-drive trap.
 */
export function photoRoot(): string {
  return resolve('PHOTO_ROOT', 'PHOTO_ROOT', process.platform === 'win32' ? 'K:\\' : '');
}

/** HMAC key for session tokens. Rotating it invalidates every existing session. */
export function authSecret(): string {
  return resolve('AUTH_SECRET', 'AUTH_SECRET', DEFAULT_AUTH_SECRET);
}

export function authUsername(): string {
  return resolve('AUTH_USERNAME', 'AUTH_USERNAME', DEFAULT_AUTH_USERNAME).toLowerCase();
}

export function authPasswordSha256(): string {
  return resolve('AUTH_PASSWORD_SHA256', 'AUTH_PASSWORD_SHA256', DEFAULT_AUTH_PASSWORD_SHA256);
}

/**
 * Shout at startup if the app is running on the committed defaults. Both are
 * public, so a reachable server using them can be logged into, and its session
 * tokens can be forged outright.
 */
export function warnAboutDefaults(): void {
  if (!loaded) return;
  if (authSecret() === DEFAULT_AUTH_SECRET) {
    console.warn(
      '[settings] WARNING: AUTH_SECRET is the built-in default, which is public. ' +
        'Anyone can forge a session token. Set AUTH_SECRET in webappNode/.env or ' +
        'in the settings.settings document, then restart.'
    );
  }
  if (authPasswordSha256() === DEFAULT_AUTH_PASSWORD_SHA256) {
    console.warn(
      '[settings] WARNING: AUTH_PASSWORD_SHA256 is the built-in default, which is public. ' +
        "Set it from sha256('jobcard-wheel:' + <new password>)."
    );
  }
}
