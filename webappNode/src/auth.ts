import { createHash, createHmac, timingSafeEqual } from 'node:crypto';
import type { NextFunction, Request, Response } from 'express';
import { isJobDatabase, type JobDatabase } from 'webapp-shared';
import { config } from './config.js';
import { authPasswordSha256, authSecret, authUsername } from './server-settings.js';

/**
 * Single-operator login. The desktop app has no login at all (it trusts the
 * Windows session), so the web app adds one gate: the only account is the
 * workshop's own. The password is never stored as plaintext, only as a salted
 * SHA-256 digest, and both fields are compared in constant time.
 *
 * The username, the digest and the token signing key all come from
 * server-settings.ts, which resolves them from the environment, then the shared
 * settings.settings document, then a built-in default.
 */
const PASSWORD_SALT = 'jobcard-wheel:';

function hashPassword(password: string): string {
  return createHash('sha256').update(PASSWORD_SALT + password).digest('hex');
}

/** Constant-time string compare that tolerates differing lengths. */
function safeEqual(a: string, b: string): boolean {
  const ab = Buffer.from(a, 'utf8');
  const bb = Buffer.from(b, 'utf8');
  if (ab.length !== bb.length) {
    // Still perform a comparison so timing does not leak the length.
    timingSafeEqual(ab, ab);
    return false;
  }
  return timingSafeEqual(ab, bb);
}

export function verifyCredentials(username: unknown, password: unknown): boolean {
  if (typeof username !== 'string' || typeof password !== 'string') {
    return false;
  }
  const userOk = safeEqual(username.trim().toLowerCase(), authUsername());
  const passOk = safeEqual(hashPassword(password), authPasswordSha256());
  return userOk && passOk;
}

function base64url(input: Buffer | string): string {
  return Buffer.from(input)
    .toString('base64')
    .replace(/\+/g, '-')
    .replace(/\//g, '_')
    .replace(/=+$/, '');
}

function sign(payload: string): string {
  return base64url(createHmac('sha256', authSecret()).update(payload).digest());
}

export interface Session {
  sub: string;
  /** Which business this session works in — chosen at sign-in, fixed for its life. */
  db: JobDatabase;
}

/**
 * Issue a stateless HMAC-signed session token.
 *
 * The database is part of the signed payload rather than something the browser
 * sends per request. A client cannot then ask for wheel data on one call and
 * plating data on the next, and it cannot be talked into the wrong business by
 * a crafted request: changing business means signing in again.
 */
export function issueToken(username: string, database: JobDatabase): string {
  const payload = base64url(
    JSON.stringify({ sub: username, db: database, exp: Date.now() + config.sessionTtlMs })
  );
  return `${payload}.${sign(payload)}`;
}

export function verifyToken(token: string | undefined): Session | null {
  if (!token) return null;
  const dot = token.lastIndexOf('.');
  if (dot <= 0) return null;
  const payload = token.slice(0, dot);
  const signature = token.slice(dot + 1);
  if (!safeEqual(signature, sign(payload))) return null;
  try {
    const decoded = JSON.parse(
      Buffer.from(payload.replace(/-/g, '+').replace(/_/g, '/'), 'base64').toString('utf8')
    ) as { sub?: unknown; db?: unknown; exp?: unknown };
    if (typeof decoded.sub !== 'string' || typeof decoded.exp !== 'number') return null;
    // Tokens issued before the database choice existed have no db and are not
    // honoured: guessing which business they meant is exactly the mistake this
    // is here to prevent. They expire into a fresh sign-in instead.
    if (!isJobDatabase(decoded.db)) return null;
    if (decoded.exp < Date.now()) return null;
    return { sub: decoded.sub, db: decoded.db };
  } catch {
    return null;
  }
}

export interface AuthedRequest extends Request {
  user?: Session;
}

/** Reject requests without a valid session token. */
export function requireAuth(req: AuthedRequest, res: Response, next: NextFunction): void {
  const header = req.header('authorization') ?? '';
  const token = header.toLowerCase().startsWith('bearer ') ? header.slice(7).trim() : undefined;
  const user = verifyToken(token);
  if (!user) {
    res.status(401).json({ error: 'Not signed in' });
    return;
  }
  req.user = user;
  next();
}

/**
 * The database this request's session is bound to. Only valid behind
 * requireAuth, which is what puts it there; the throw is for the programming
 * error of mounting a route without it, never for a client to trip.
 */
export function sessionDb(req: Request): JobDatabase {
  const database = (req as AuthedRequest).user?.db;
  if (!database) {
    throw new Error('No session database — this route is missing requireAuth');
  }
  return database;
}

export function delay(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}
