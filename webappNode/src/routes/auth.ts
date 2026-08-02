import { Router, type Request, type Response } from 'express';
import { config } from '../config.js';
import { delay, issueToken, requireAuth, verifyCredentials, type AuthedRequest } from '../auth.js';

export const authRouter = Router();

/**
 * POST /api/auth/login
 *
 * A correct username/password returns a session token immediately. A wrong one
 * is answered with "Access denied" only after a deliberate delay
 * (LOGIN_FAILURE_DELAY_MS, 3s by default), which slows credential guessing.
 */
authRouter.post('/login', async (req: Request, res: Response) => {
  const { username, password } = (req.body ?? {}) as {
    username?: unknown;
    password?: unknown;
  };

  if (verifyCredentials(username, password)) {
    const name = String(username).trim();
    res.json({ token: issueToken(name), username: name });
    return;
  }

  await delay(config.loginFailureDelayMs);
  res.status(401).json({ error: 'Access denied' });
});

/** GET /api/auth/me — lets the UI confirm a stored token is still valid. */
authRouter.get('/me', requireAuth, (req: AuthedRequest, res: Response) => {
  res.json({ username: req.user?.sub ?? null });
});
