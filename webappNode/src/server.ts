import { existsSync } from 'node:fs';
import { dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import cors from 'cors';
import express, { type NextFunction, type Request, type Response } from 'express';
import { requireAuth } from './auth.js';
import { config, redactMongoUrl } from './config.js';
import { connect, disconnect } from './db.js';
import { mappedDriveWarning, status as photoStatus } from './photo-store.js';
import { authRouter } from './routes/auth.js';
import { customersRouter } from './routes/customers.js';
import { jobsRouter } from './routes/jobs.js';
import { jobTypesRouter } from './routes/job-types.js';
import { photosRouter } from './routes/photos.js';
import { xeroRouter } from './routes/xero.js';
import { loadServerSettings, warnAboutDefaults } from './server-settings.js';

const app = express();

app.use(
  cors({
    origin: config.corsOrigins.length > 0 ? config.corsOrigins : true,
  })
);
app.use(express.json({ limit: '1mb' }));

// Photo uploads arrive as raw binary, so they need their own parser and limit.
app.use(
  '/api/photos',
  express.raw({
    type: ['image/*', 'video/*'],
    limit: config.maxPhotoBytes,
  })
);

app.get('/api/health', (_req, res) => {
  res.json({
    status: 'ok',
    // Both businesses are served by one deployment; which one a request reads is
    // decided by the session token, not by the server. See src/databases.ts.
    databases: config.jobDatabases,
    settingsDatabase: config.mongoSettingsDb,
  });
});

app.use('/api/auth', authRouter);

// Everything below the login endpoint requires a valid session token.
app.use('/api/jobs', requireAuth, jobsRouter);
app.use('/api/job-types', requireAuth, jobTypesRouter);
app.use('/api/customers', requireAuth, customersRouter);
app.use('/api/photos', requireAuth, photosRouter);

// Xero applies requireAuth per route rather than here: the OAuth callback is a
// top-level browser navigation from Xero and cannot carry an Authorization header.
app.use('/api/xero', xeroRouter);

/**
 * Where `npm run build` in webappUI leaves the compiled app. Serving it from here
 * makes the whole deployment one process on one port: nothing else to install,
 * and the browser is same-origin so CORS never comes into play. Set UI_ROOT to
 * override, or leave the app unbuilt to run the API on its own.
 */
const uiRoot = process.env.UI_ROOT
  ? resolve(process.env.UI_ROOT)
  : resolve(dirname(fileURLToPath(import.meta.url)), '../../webappUI/dist/webapp-ui/browser');
const uiAvailable = existsSync(resolve(uiRoot, 'index.html'));

if (uiAvailable) {
  // index.html is served by the fallback below rather than here, so there is one
  // place deciding what a non-file path returns.
  app.use(express.static(uiRoot, { index: false }));
}

app.use((req: Request, res: Response) => {
  // A path that is not a file on disk is an Angular route like /jobs/123, so it
  // gets index.html and the router resolves it in the browser. /api is deliberately
  // excluded: an unknown endpoint there is a real 404 and must stay JSON, never a
  // page handed to something expecting data.
  if (uiAvailable && req.method === 'GET' && !req.path.startsWith('/api/')) {
    res.sendFile(resolve(uiRoot, 'index.html'));
    return;
  }
  res.status(404).json({ error: 'Not found' });
});

// Errors thrown in async handlers surface here rather than crashing the process.
app.use((err: unknown, _req: Request, res: Response, _next: NextFunction) => {
  const message = err instanceof Error ? err.message : 'Unexpected server error';
  console.error('[api] request failed:', err);
  res.status(500).json({ error: message });
});

async function main(): Promise<void> {
  // Only the connection attempt belongs in this try: a failure here is fatal and
  // is genuinely a database problem. Anything else must not be reported as one.
  try {
    await connect();
    const names = Object.values(config.jobDatabases).join('", "');
    console.log(
      `[api] connected to ${redactMongoUrl(config.mongoUrl)} — job databases "${names}", ` +
        `settings "${config.mongoSettingsDb}"`
    );
  } catch (err) {
    console.error(
      `[api] cannot reach MongoDB at ${redactMongoUrl(config.mongoUrl)}. ` +
        'Start mongod, or point the app somewhere else with mongoIP=<host> ' +
        '(see webappNode/README.md), then restart.',
      err
    );
    process.exit(1);
  }

  // Auth and photo-share config may live in the shared settings document, so it
  // has to be read before anything uses it. Best-effort by design: on failure
  // every value falls back to its environment variable or built-in default.
  await loadServerSettings();
  warnAboutDefaults();

  // Report the photo share up front — a missing mount is the likeliest reason for
  // photos to be silently absent. These diagnostics must never stop the server.
  try {
    const photos = await photoStatus();
    if (photos.available) {
      console.log(`[api] photo share ready at ${photos.root}`);
    } else {
      console.warn(`[api] photos unavailable: ${photos.reason}`);
    }
    const driveWarning = mappedDriveWarning();
    if (driveWarning) {
      console.warn(`[api] warning: ${driveWarning}`);
    }
  } catch (err) {
    console.warn('[api] could not check the photo share:', err);
  }

  if (uiAvailable) {
    console.log(`[api] serving the web app from ${uiRoot}`);
  } else {
    console.warn(
      `[api] no web app at ${uiRoot} — API only. ` +
        'Run "npm run build" in webappUI to have this server serve it too.'
    );
  }

  const server = app.listen(config.port, () => {
    console.log(`[api] listening on http://localhost:${config.port}`);
  });

  const shutdown = async (signal: string) => {
    console.log(`[api] ${signal} received, shutting down`);
    server.close();
    await disconnect();
    process.exit(0);
  };
  process.on('SIGINT', () => void shutdown('SIGINT'));
  process.on('SIGTERM', () => void shutdown('SIGTERM'));
}

void main();
