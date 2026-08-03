import { Router, type Request, type Response } from 'express';
import { JOB_DATABASES, isJobDatabase } from 'webapp-shared';
import { config } from '../config.js';
import { delay, issueToken, requireAuth, verifyCredentials, type AuthedRequest } from '../auth.js';

export const authRouter = Router();

/**
 * POST /api/auth/login
 *
 * A correct username/password returns a session token immediately. A wrong one
 * is answered with "Access denied" only after a deliberate delay
 * (LOGIN_FAILURE_DELAY_MS, 3s by default), which slows credential guessing.
 *
 * `database` says which business to work in — the desktop app takes this from
 * its executable name, the web app asks on the sign-in screen. It is required
 * rather than defaulted: silently landing someone in the wrong company's jobs
 * is worse than making them choose.
 */
authRouter.post('/login', async (req: Request, res: Response) => {
  const { username, password, database } = (req.body ?? {}) as {
    username?: unknown;
    password?: unknown;
    database?: unknown;
  };

  if (!isJobDatabase(database)) {
    // Not a credential failure, so it is answered immediately and says what is
    // wrong — no point rate-limiting a client-side mistake.
    res.status(400).json({
      error: `Choose which database to sign in to (${JOB_DATABASES.join(' or ')}).`,
    });
    return;
  }

  if (verifyCredentials(username, password)) {
    const name = String(username).trim();
    res.json({ token: issueToken(name, database), username: name, database });
    return;
  }

  await delay(config.loginFailureDelayMs);
  res.status(401).json({ error: 'Access denied' });
});

/** GET /api/auth/me — lets the UI confirm a stored token is still valid. */
authRouter.get('/me', requireAuth, (req: AuthedRequest, res: Response) => {
  res.json({ username: req.user?.sub ?? null, database: req.user?.db });
});
