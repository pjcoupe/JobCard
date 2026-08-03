import { createReadStream } from 'node:fs';
import { readFile } from 'node:fs/promises';
import { Router, type Request, type Response } from 'express';
import type { ObjectId } from 'mongodb';
import { isVideoFilename, toLines, type JobCardDoc, type JobDatabase } from 'webapp-shared';
import { sessionDb } from '../auth.js';
import { config } from '../config.js';
import { jobCards, jobPictures } from '../db.js';
import { ensureBackupForFile, ensureBackupsForJob } from '../photo-backup-sync.js';
import { deleteForJob, listForJob, resolvePhotoPath, saveForJob, status } from '../photo-store.js';

export const photosRouter = Router();

/** Content types for the extensions the desktop app recognises. */
const CONTENT_TYPES: Record<string, string> = {
  '.JPG': 'image/jpeg',
  '.JPE': 'image/jpeg',
  '.BMP': 'image/bmp',
  '.GIF': 'image/gif',
  '.PNG': 'image/png',
  '.MOV': 'video/quicktime',
  '.MP4': 'video/mp4',
};

/** Extension to use for an upload, based on its declared content type. */
const EXTENSION_FOR_TYPE: Record<string, string> = {
  'image/jpeg': '.jpg',
  'image/jpg': '.jpg',
  'image/png': '.png',
  'image/gif': '.gif',
  'image/bmp': '.bmp',
  'video/quicktime': '.mov',
  'video/mp4': '.mp4',
};

function contentTypeFor(name: string): string {
  const dot = name.lastIndexOf('.');
  const ext = dot < 0 ? '' : name.slice(dot).toUpperCase();
  return CONTENT_TYPES[ext] ?? 'application/octet-stream';
}

/**
 * Identify an image's real format from its bytes rather than trusting a
 * filename extension or assuming JPEG. Necessary because a Mongo-stored "full"
 * doc is not always JPEG: it's only re-encoded as JPEG when compression was
 * actually needed (see photo-backup.ts) — otherwise it's a byte-for-byte copy
 * of whatever format the original upload was, regardless of what its filename
 * extension claims. Thumbnails are always genuinely JPEG, so this also just
 * confirms that case correctly.
 */
function sniffImageContentType(buffer: Buffer): string {
  if (buffer.length >= 3 && buffer[0] === 0xff && buffer[1] === 0xd8 && buffer[2] === 0xff) {
    return 'image/jpeg';
  }
  if (
    buffer.length >= 8 &&
    buffer[0] === 0x89 &&
    buffer[1] === 0x50 &&
    buffer[2] === 0x4e &&
    buffer[3] === 0x47
  ) {
    return 'image/png';
  }
  if (buffer.length >= 3 && buffer.toString('ascii', 0, 3) === 'GIF') {
    return 'image/gif';
  }
  if (buffer.length >= 2 && buffer[0] === 0x42 && buffer[1] === 0x4d) {
    return 'image/bmp';
  }
  return 'application/octet-stream';
}

/** Load the job so photos can be filed under its date and named after it. */
async function loadJob(database: JobDatabase, jobIdRaw: string): Promise<JobCardDoc | null> {
  const jobId = Number(jobIdRaw);
  if (!Number.isFinite(jobId)) return null;
  return (await jobCards(database).findOne({
    jobID: Math.trunc(jobId),
  })) as unknown as JobCardDoc | null;
}

function jobDateOf(job: JobCardDoc): Date | null {
  const raw = job.jobDate;
  if (!raw) return null;
  const d = raw instanceof Date ? raw : new Date(String(raw));
  return Number.isNaN(d.getTime()) ? null : d;
}

/**
 * The desktop app puts the job's combined detail text into the filename
 * (CombinedDetailText, truncated to 60 chars by SaveUniquePhoto).
 */
function combinedDetailText(job: JobCardDoc): string {
  return toLines(job)
    .map((line) => (line.detail ?? '').trim())
    .filter((text) => text !== '')
    .join(' ')
    .trim();
}

/** GET /api/photos/status — is the share reachable? Used for diagnostics. */
photosRouter.get('/status', async (_req: Request, res: Response) => {
  res.json(await status());
});

/**
 * GET /api/photos/jobs/:jobID — list a job's photos.
 *
 * Also self-heals: any still photo on the share with no Mongo backup yet (an
 * older job whose photos were added directly to the share, or captured by the
 * desktop app, which never touches jobPictures) gets its full + thumbnail docs
 * created here, before the response goes out. This makes the first view of
 * such a job slower — each photo is read from the share once — and every view
 * after that fast, since the check is a no-op once backed up.
 */
photosRouter.get('/jobs/:jobID', async (req: Request, res: Response) => {
  const database = sessionDb(req);
  const job = await loadJob(database, req.params.jobID);
  if (!job) {
    res.status(404).json({ error: 'Job not found' });
    return;
  }
  const jobDate = jobDateOf(job);
  const result = await listForJob(job.jobID, jobDate);
  if (config.photoBackupEnabled && result.available) {
    try {
      await ensureBackupsForJob(database, job._id as unknown as ObjectId, job.jobID, jobDate);
    } catch (err) {
      console.warn(`[photos] repair pass failed for job ${job.jobID}:`, err);
    }
  }
  res.json(result);
});

/**
 * GET /api/photos/jobs/:jobID/:name — fetch one photo.
 *
 * `?variant=thumbnail` requests the 250px-wide preview; anything else (or
 * omitted, for existing callers) returns the full version. Mongo is tried
 * first — a hit means no share I/O at all, which is the entire point of
 * having thumbnails, given how slow/fragile the network share can be. A miss
 * (a video, which has no Mongo backup, or a not-yet-repaired edge case) falls
 * back to the share file, self-healing a still image at the same time.
 */
photosRouter.get('/jobs/:jobID/:name', async (req: Request, res: Response) => {
  const database = sessionDb(req);
  const job = await loadJob(database, req.params.jobID);
  if (!job) {
    res.status(404).json({ error: 'Job not found' });
    return;
  }
  const wantsThumbnail = req.query.variant === 'thumbnail';
  const jobId = job._id as unknown as ObjectId;
  const name = req.params.name;

  if (config.photoBackupEnabled) {
    const doc = await jobPictures(database).findOne({ jobId, name, isThumbnail: wantsThumbnail });
    if (doc) {
      const bytes = Buffer.from(doc.base64Image as string, 'base64');
      res.setHeader('Content-Type', sniffImageContentType(bytes));
      res.setHeader('Cache-Control', 'private, max-age=300');
      res.send(bytes);
      return;
    }
  }

  const resolved = await resolvePhotoPath(job.jobID, jobDateOf(job), name);
  if (!resolved.ok) {
    res.status(404).json({ error: resolved.reason });
    return;
  }

  // Self-heal: this still image had no Mongo backup at all for the requested
  // variant. Best-effort, and must never delay or break serving the file.
  if (config.photoBackupEnabled && !resolved.isVideo) {
    ensureBackupForFile(database, jobId, name, () => readFile(resolved.path)).catch((err) => {
      console.warn(`[photos] on-demand backup failed for "${name}":`, err);
    });
  }

  res.setHeader('Content-Type', contentTypeFor(name));
  // These files are only reachable with a valid session, so keep them private.
  res.setHeader('Cache-Control', 'private, max-age=300');
  const stream = createReadStream(resolved.path);
  stream.on('error', () => {
    if (!res.headersSent) {
      res.status(500).json({ error: 'Could not read the photo' });
    } else {
      res.end();
    }
  });
  stream.pipe(res);
});

/**
 * POST /api/photos/jobs/:jobID — upload a photo as a raw binary body.
 *
 * The filename is generated server-side from the job, exactly as the desktop
 * app names its captures, so files stay consistent between the two apps.
 */
photosRouter.post('/jobs/:jobID', async (req: Request, res: Response) => {
  const database = sessionDb(req);
  const job = await loadJob(database, req.params.jobID);
  if (!job) {
    res.status(404).json({ error: 'Job not found' });
    return;
  }

  // Check the declared type first: an unsupported type is not parsed into a
  // Buffer at all, and "unsupported type" is the useful message for it.
  const declaredType = (req.header('content-type') ?? '').split(';')[0]!.trim().toLowerCase();
  const extension = EXTENSION_FOR_TYPE[declaredType];
  if (!extension) {
    res.status(415).json({
      error: `Unsupported photo type "${declaredType || 'unknown'}". Use JPEG, PNG, GIF, BMP, MP4 or MOV.`,
    });
    return;
  }

  const body = req.body;
  if (!Buffer.isBuffer(body) || body.length === 0) {
    res.status(400).json({ error: 'Send the photo as a raw binary body' });
    return;
  }

  const result = await saveForJob({
    jobId: job.jobID,
    jobDate: jobDateOf(job),
    businessName: job.jobBusinessName ?? null,
    customer: job.jobCustomer ?? null,
    phone: job.jobPhone ?? null,
    details: combinedDetailText(job),
    extension,
    data: body,
  });

  if (!result.ok) {
    res.status(503).json({ error: result.reason });
    return;
  }

  // Independent Mongo backup of the photo (full + thumbnail) — see
  // webappNode/README.md. Best effort only: the share write above is the
  // critical, desktop-app compatible path, and must never be blocked by a
  // problem here. Skipped for videos (no cheap way to compress those) and for
  // share-side duplicates (already backed up when they were first uploaded).
  let backedUp = false;
  if (config.photoBackupEnabled && !result.duplicateOf && !isVideoFilename(result.photo.name)) {
    try {
      await ensureBackupForFile(
        database,
        job._id as unknown as ObjectId,
        result.photo.name,
        async () => body
      );
      backedUp = true;
    } catch (err) {
      console.warn(`[photos] backup failed for job ${job.jobID}:`, err);
    }
  }

  res.status(result.duplicateOf ? 200 : 201).json({
    photo: result.photo,
    duplicateOf: result.duplicateOf ?? null,
    backedUp,
  });
});

/** DELETE /api/photos/jobs/:jobID/:name */
photosRouter.delete('/jobs/:jobID/:name', async (req: Request, res: Response) => {
  const database = sessionDb(req);
  const job = await loadJob(database, req.params.jobID);
  if (!job) {
    res.status(404).json({ error: 'Job not found' });
    return;
  }
  const result = await deleteForJob(job.jobID, jobDateOf(job), req.params.name);
  if (!result.ok) {
    res.status(400).json({ error: result.reason });
    return;
  }

  // Remove the matching Mongo backups too (both the full and thumbnail docs),
  // so "delete" actually means delete. Best effort: the file is already gone,
  // which is what the user is watching for, so a failure here is logged
  // rather than surfaced as an error.
  try {
    await jobPictures(database).deleteMany({
      jobId: job._id as unknown as ObjectId,
      name: req.params.name,
    });
  } catch (err) {
    console.warn(`[photos] backup cleanup failed for job ${job.jobID}:`, err);
  }

  res.json({ deleted: req.params.name });
});
