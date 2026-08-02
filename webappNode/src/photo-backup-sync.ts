import { createHash } from 'node:crypto';
import { readFile } from 'node:fs/promises';
import type { ObjectId } from 'mongodb';
import { jobPictures } from './db.js';
import { prepareBackupImages } from './photo-backup.js';
import { listForJob, resolvePhotoPath } from './photo-store.js';

/**
 * Keeps the jobPictures Mongo backup (full + 250px thumbnail per photo) in
 * sync with what's actually on the photo share. This is the one place that
 * knows how to "make sure Mongo has both variants for this file," used both
 * right after a fresh upload and to repair older jobs whose photos were never
 * routed through the webapp (added directly to the share, or captured by the
 * desktop app, which never touches jobPictures at all).
 */

/**
 * Ensure both the full and thumbnail backup docs exist for one photo.
 * `readBytes` is only called if something is actually missing, so an
 * already-backed-up file costs one Mongo query and no file I/O. Safe to call
 * repeatedly — it never creates duplicates.
 */
export async function ensureBackupForFile(
  jobId: ObjectId,
  name: string,
  readBytes: () => Promise<Buffer>
): Promise<void> {
  const existing = await jobPictures()
    .find({ jobId, name })
    .project({ isThumbnail: 1 })
    .toArray();
  const hasFull = existing.some((d) => d.isThumbnail === false);
  const hasThumbnail = existing.some((d) => d.isThumbnail === true);
  if (hasFull && hasThumbnail) {
    return;
  }

  const bytes = await readBytes();
  const contentHash = createHash('sha256').update(bytes).digest('hex');
  const { full, thumbnail } = await prepareBackupImages(bytes);

  const inserts: Array<Record<string, unknown>> = [];
  if (!hasFull) {
    inserts.push({ jobId, name, contentHash, isThumbnail: false, base64Image: full.base64 });
  }
  if (!hasThumbnail) {
    inserts.push({ jobId, name, contentHash, isThumbnail: true, base64Image: thumbnail.base64 });
  }
  if (inserts.length > 0) {
    await jobPictures().insertMany(inserts as never[]);
  }
}

/**
 * Repair pass for a whole job: for every still image on the share, ensure it
 * has both backup docs, creating whichever are missing. This is what makes an
 * older job — one with photos on the share but zero jobPictures docs — fully
 * backed up and thumbnail-fast from its first view onward. Runs the checks
 * concurrently; one file's failure doesn't stop the others.
 */
export async function ensureBackupsForJob(
  jobId: ObjectId,
  jobNumericId: number,
  jobDate: Date | null
): Promise<void> {
  const listing = await listForJob(jobNumericId, jobDate);
  if (!listing.available) {
    return;
  }

  await Promise.all(
    listing.photos
      .filter((photo) => !photo.isVideo)
      .map(async (photo) => {
        try {
          await ensureBackupForFile(jobId, photo.name, async () => {
            const resolved = await resolvePhotoPath(jobNumericId, jobDate, photo.name);
            if (!resolved.ok) {
              throw new Error(resolved.reason);
            }
            return readFile(resolved.path);
          });
        } catch (err) {
          console.warn(`[photo-backup-sync] repair failed for "${photo.name}":`, err);
        }
      })
  );
}
