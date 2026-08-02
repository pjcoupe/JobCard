/**
 * Photo storage conventions, copied from the desktop app so both programs read
 * and write the same files on the same share.
 *
 * Photos are NOT referenced from the job document at all. They live on a shared
 * drive (`K:\` by default, `D:\Kodak Pictures\` on the TCSP4 machine) in a
 * folder derived from the job's date, and a file belongs to a job when the
 * leading number in its filename equals the job ID:
 *
 *     K:\2026\2026 May\6 geeber-burger derger 021555123 Wheel repair 001.jpg
 *     ^ root  ^ year  ^ "{year} {MonthName}"  ^ jobID
 *
 * See JobCard.GetJobPictureFiles() and JobCard.SaveUniquePhoto().
 */

/** Month folder names. Index 1 is January, matching the desktop `months` array. */
export const MONTH_FOLDER_NAMES = [
  '',
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
] as const;

/** Extensions the desktop app recognises (JobCard.ImageExtensions), uppercase. */
export const PHOTO_EXTENSIONS = [
  '.JPG',
  '.JPE',
  '.BMP',
  '.GIF',
  '.PNG',
  '.MOV',
  '.MP4',
] as const;

/** The two extensions treated as movies rather than stills. */
export const VIDEO_EXTENSIONS = ['.MOV', '.MP4'] as const;

export function isPhotoFilename(name: string): boolean {
  const ext = extensionOf(name);
  return (PHOTO_EXTENSIONS as readonly string[]).includes(ext);
}

export function isVideoFilename(name: string): boolean {
  return (VIDEO_EXTENSIONS as readonly string[]).includes(extensionOf(name));
}

function extensionOf(name: string): string {
  const dot = name.lastIndexOf('.');
  return dot < 0 ? '' : name.slice(dot).toUpperCase();
}

/**
 * The desktop app matches a photo to a job by parsing the first space-delimited
 * token of the filename as an integer (GetJobPictureFiles).
 */
export function jobIdFromFilename(name: string): number | null {
  const firstToken = name.split(' ')[0] ?? '';
  const parsed = Number.parseInt(firstToken, 10);
  return Number.isFinite(parsed) ? parsed : null;
}

/** Folder segments below the photo root for a given job date: [year, monthDir]. */
export function photoFolderSegments(jobDate: Date): [string, string] {
  const year = String(jobDate.getFullYear());
  const month = MONTH_FOLDER_NAMES[jobDate.getMonth() + 1];
  return [year, `${year} ${month}`];
}

/** Characters Windows forbids in a filename, replaced with '-' by the desktop app. */
export function sanitiseFilename(name: string): string {
  return name.replace(/[<>:"/\\|?*]/g, '-');
}

/**
 * Build the next filename for a job, following SaveUniquePhoto():
 *   "{jobID} {business}-{customer} {phone} {details} {NNN}{ext}"
 * Empty parts collapse, and the sequence number is 1 + the existing photo count.
 */
export function buildPhotoFilename(options: {
  jobId: number;
  businessName?: string | null;
  customer?: string | null;
  phone?: string | null;
  details?: string | null;
  existingCount: number;
  extension: string;
}): string {
  const business = options.businessName?.trim() ? `${options.businessName.trim()}-` : '';
  const customer = options.customer?.trim() ?? '';
  const phone = options.phone?.trim() ? `${options.phone.trim()} ` : '';
  // The desktop app truncates the combined detail text at 60 characters.
  const details = (options.details ?? '').trim().slice(0, 60);
  const sequence = ` ${String(options.existingCount + 1).padStart(3, '0')}`;
  const ext = options.extension.startsWith('.') ? options.extension : `.${options.extension}`;

  const raw = `${options.jobId} ${business}${customer} ${phone}${details}${sequence}${ext}`;
  // Collapse the runs of spaces left behind by empty parts.
  return sanitiseFilename(raw.replace(/ {2,}/g, ' '));
}

/** One photo as reported by the API. */
export interface JobPhoto {
  /** Filename on the share; also the id used by the fetch and delete routes. */
  name: string;
  sizeBytes: number;
  modified: string;
  isVideo: boolean;
}

export interface JobPhotosResponse {
  /** False when the share is not mounted or not configured. */
  available: boolean;
  /** Why photos are unavailable, for display in the UI. */
  reason?: string;
  /** Absolute folder the photos were read from, for troubleshooting. */
  folder?: string;
  photos: JobPhoto[];
}

export interface PhotoStoreStatus {
  available: boolean;
  configured: boolean;
  root?: string;
  reason?: string;
}
