import { createHash } from 'node:crypto';
import { constants as fsConstants } from 'node:fs';
import { access, mkdir, readdir, readFile, stat, unlink, writeFile } from 'node:fs/promises';
import { basename, join, resolve, sep } from 'node:path';
import {
  buildPhotoFilename,
  isPhotoFilename,
  isVideoFilename,
  jobIdFromFilename,
  photoFolderSegments,
  type JobPhoto,
  type PhotoStoreStatus,
} from 'webapp-shared';
import { photoRoot } from './server-settings.js';

/**
 * Reads and writes job photos on the shared drive the desktop app uses, keeping
 * its folder layout and filename convention so both programs see the same files.
 *
 * The store is optional: when PHOTO_ROOT is unset or the share is not mounted,
 * every call reports unavailable rather than throwing, so the rest of the app
 * keeps working without photos.
 */

export interface PhotoFolder {
  available: boolean;
  reason?: string;
  folder?: string;
}

/** Is a photo root configured at all? */
export function isConfigured(): boolean {
  return !!photoRoot();
}

/**
 * Warn about a Windows pitfall: a mapped network drive such as `K:\` exists only
 * inside the logon session that created it. A process running as a Windows
 * Service (session 0, LocalSystem) sees no drive mappings, so the path silently
 * fails to resolve. A UNC path has no such dependency and works either way.
 *
 * Returns null when the configured root is not a drive-letter path.
 */
export function mappedDriveWarning(): string | null {
  if (process.platform !== 'win32' || !photoRoot()) {
    return null;
  }
  if (!/^[A-Za-z]:/.test(photoRoot())) {
    return null;
  }
  return (
    `PHOTO_ROOT is the drive letter "${photoRoot()}". If that is a mapped ` +
    'network drive it will only resolve while this process runs as the signed-in ' +
    'user — a Windows Service would not see it. Prefer the UNC path, e.g. ' +
    'PHOTO_ROOT=\\\\SERVERNAME\\ShareName (run `net use` to see what the letter maps to).'
  );
}

/** Check the root exists and is readable. */
export async function status(): Promise<PhotoStoreStatus> {
  if (!photoRoot()) {
    return {
      available: false,
      configured: false,
      reason: 'PHOTO_ROOT is not set, so photos are turned off.',
    };
  }
  try {
    await access(photoRoot(), fsConstants.R_OK);
    return { available: true, configured: true, root: photoRoot() };
  } catch {
    return {
      available: false,
      configured: true,
      root: photoRoot(),
      reason: `Photo share ${photoRoot()} is not reachable from the server.`,
    };
  }
}

/**
 * Resolve the folder holding a job's photos: {root}/{year}/{year} {MonthName},
 * derived from the job's own date the way UpdatePhotos() does.
 *
 * Unlike the desktop app, a read never creates directories — only an upload
 * does (`create: true`), so browsing jobs cannot litter the share with empty
 * month folders.
 */
export async function folderForJob(
  jobDate: Date | null,
  options: { create?: boolean } = {}
): Promise<PhotoFolder> {
  const rootStatus = await status();
  if (!rootStatus.available) {
    return { available: false, reason: rootStatus.reason };
  }
  // A job with no date falls back to today, which is where a photo taken now
  // would naturally be filed.
  const effectiveDate = jobDate ?? new Date();
  const [yearDir, monthDir] = photoFolderSegments(effectiveDate);
  const folder = join(photoRoot(), yearDir, monthDir);

  try {
    await access(folder, fsConstants.R_OK);
    return { available: true, folder };
  } catch {
    if (!options.create) {
      // Nothing filed for that month yet — not an error, just no photos.
      return { available: true, folder };
    }
  }

  try {
    await mkdir(folder, { recursive: true });
    return { available: true, folder };
  } catch (err) {
    return {
      available: false,
      reason: `Could not create ${folder}: ${(err as Error).message}`,
    };
  }
}

/** List a job's photos, newest last (the desktop app shows directory order). */
export async function listForJob(
  jobId: number,
  jobDate: Date | null
): Promise<{ available: boolean; reason?: string; folder?: string; photos: JobPhoto[] }> {
  const target = await folderForJob(jobDate);
  if (!target.available) {
    return { available: false, reason: target.reason, photos: [] };
  }

  let names: string[];
  try {
    names = await readdir(target.folder!);
  } catch {
    // Month folder does not exist yet.
    return { available: true, folder: target.folder, photos: [] };
  }

  const photos: JobPhoto[] = [];
  for (const name of names) {
    if (!isPhotoFilename(name)) continue;
    if (jobIdFromFilename(name) !== jobId) continue;
    try {
      const info = await stat(join(target.folder!, name));
      if (!info.isFile()) continue;
      photos.push({
        name,
        sizeBytes: info.size,
        modified: info.mtime.toISOString(),
        isVideo: isVideoFilename(name),
      });
    } catch {
      // File vanished between listing and stat — skip it.
    }
  }
  photos.sort((a, b) => a.name.localeCompare(b.name, undefined, { numeric: true }));
  return { available: true, folder: target.folder, photos };
}

/**
 * Resolve a single photo path, rejecting anything that escapes the job's folder
 * or does not belong to the job. Guards against `..` and absolute paths in the
 * filename taken from the URL.
 */
export async function resolvePhotoPath(
  jobId: number,
  jobDate: Date | null,
  filename: string
): Promise<{ ok: true; path: string; isVideo: boolean } | { ok: false; reason: string }> {
  // Strip any directory portion before doing anything else.
  const name = basename(filename);
  if (!name || name !== filename) {
    return { ok: false, reason: 'Invalid photo name' };
  }
  if (!isPhotoFilename(name)) {
    return { ok: false, reason: 'Not a photo file' };
  }
  if (jobIdFromFilename(name) !== jobId) {
    return { ok: false, reason: 'That photo belongs to a different job' };
  }

  const target = await folderForJob(jobDate);
  if (!target.available || !target.folder) {
    return { ok: false, reason: target.reason ?? 'Photo share unavailable' };
  }

  const folder = resolve(target.folder);
  const path = resolve(folder, name);
  // Belt and braces: the resolved path must still sit inside the job folder.
  if (path !== join(folder, name) || !path.startsWith(folder + sep)) {
    return { ok: false, reason: 'Invalid photo name' };
  }

  try {
    const info = await stat(path);
    if (!info.isFile()) {
      return { ok: false, reason: 'Photo not found' };
    }
  } catch {
    return { ok: false, reason: 'Photo not found' };
  }

  return { ok: true, path, isVideo: isVideoFilename(name) };
}

/**
 * Save a new photo for a job, naming it the way SaveUniquePhoto() does and
 * skipping the write when an identical file is already filed against the job
 * (the desktop app compares image bytes to avoid duplicate saves).
 */
export async function saveForJob(options: {
  jobId: number;
  jobDate: Date | null;
  businessName?: string | null;
  customer?: string | null;
  phone?: string | null;
  details?: string | null;
  extension: string;
  data: Buffer;
}): Promise<
  | { ok: true; photo: JobPhoto; contentHash: string; duplicateOf?: string }
  | { ok: false; reason: string }
> {
  const target = await folderForJob(options.jobDate, { create: true });
  if (!target.available || !target.folder) {
    return { ok: false, reason: target.reason ?? 'Photo share unavailable' };
  }

  const existing = await listForJob(options.jobId, options.jobDate);
  const incomingHash = createHash('sha256').update(options.data).digest('hex');

  for (const photo of existing.photos) {
    try {
      const bytes = await readFile(join(target.folder, photo.name));
      if (createHash('sha256').update(bytes).digest('hex') === incomingHash) {
        return { ok: true, photo, contentHash: incomingHash, duplicateOf: photo.name };
      }
    } catch {
      // Unreadable existing file — ignore it for dedup purposes.
    }
  }

  const filename = buildPhotoFilename({
    jobId: options.jobId,
    businessName: options.businessName,
    customer: options.customer,
    phone: options.phone,
    details: options.details,
    existingCount: existing.photos.length,
    extension: options.extension,
  });

  const path = join(target.folder, filename);
  try {
    // 'wx' fails rather than clobbering a file that already has this name.
    await writeFile(path, options.data, { flag: 'wx' });
  } catch (err) {
    if ((err as NodeJS.ErrnoException).code === 'EEXIST') {
      return { ok: false, reason: `A photo named "${filename}" already exists` };
    }
    return { ok: false, reason: `Could not save the photo: ${(err as Error).message}` };
  }

  const info = await stat(path);
  return {
    ok: true,
    photo: {
      name: filename,
      sizeBytes: info.size,
      modified: info.mtime.toISOString(),
      isVideo: isVideoFilename(filename),
    },
    contentHash: incomingHash,
  };
}

/**
 * Delete one of a job's photos. Returns the deleted file's content hash (the
 * same hash saveForJob computed at upload time) so the caller can find and
 * remove its matching jobPictures backup, if the hash could be determined.
 */
export async function deleteForJob(
  jobId: number,
  jobDate: Date | null,
  filename: string
): Promise<{ ok: true; contentHash: string | null } | { ok: false; reason: string }> {
  const resolved = await resolvePhotoPath(jobId, jobDate, filename);
  if (!resolved.ok) {
    return { ok: false, reason: resolved.reason };
  }

  // Hash before unlinking — this only matters for finding the matching backup,
  // so a failure here must not block the delete itself.
  let contentHash: string | null = null;
  try {
    const bytes = await readFile(resolved.path);
    contentHash = createHash('sha256').update(bytes).digest('hex');
  } catch {
    contentHash = null;
  }

  try {
    await unlink(resolved.path);
    return { ok: true, contentHash };
  } catch (err) {
    return { ok: false, reason: `Could not delete the photo: ${(err as Error).message}` };
  }
}
