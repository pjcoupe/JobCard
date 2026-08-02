import { createHash, createHmac, timingSafeEqual } from 'node:crypto';
import type { NextFunction, Request, Response } from 'express';
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

/** Issue a stateless HMAC-signed session token. */
export function issueToken(username: string): string {
  const payload = base64url(
    JSON.stringify({ sub: username, exp: Date.now() + config.sessionTtlMs })
  );
  return `${payload}.${sign(payload)}`;
}

export function verifyToken(token: string | undefined): { sub: string } | null {
  if (!token) return null;
  const dot = token.lastIndexOf('.');
  if (dot <= 0) return null;
  const payload = token.slice(0, dot);
  const signature = token.slice(dot + 1);
  if (!safeEqual(signature, sign(payload))) return null;
  try {
    const decoded = JSON.parse(
      Buffer.from(payload.replace(/-/g, '+').replace(/_/g, '/'), 'base64').toString('utf8')
    ) as { sub?: unknown; exp?: unknown };
    if (typeof decoded.sub !== 'string' || typeof decoded.exp !== 'number') return null;
    if (decoded.exp < Date.now()) return null;
    return { sub: decoded.sub };
  } catch {
    return null;
  }
}

export interface AuthedRequest extends Request {
  user?: { sub: string };
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

export function delay(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}
