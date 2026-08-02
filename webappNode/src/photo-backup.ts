import sharp from 'sharp';
import { config } from './config.js';

/**
 * Prepares a photo for the MongoDB backup copy (see jobPictures in db.ts).
 *
 * Three tiers, all checked against the same threshold (base64-encoded size vs
 * `photoBackupCompressThresholdBytes`, ~14MB — comfortably under MongoDB's
 * 16MB per-document limit):
 *
 *   1. Store the original bytes completely untouched — no compression, no
 *      format conversion, no quality loss. This covers the vast majority of
 *      real photos: even an unusually large phone photo comfortably fits.
 *   2. Too big as-is: re-encode as JPEG at quality 85, full original
 *      resolution (no resizing — a wheel-damage close-up needs its detail,
 *      and quality alone is normally enough: even a 9000x6248 test image
 *      compressed to ~2.5MB at this quality).
 *   3. Still too big (a genuinely pathological image — verified against pure
 *      random noise at 24 megapixels, which real camera sensor noise never
 *      approaches): quality 50, same full resolution. Whatever this produces
 *      is used regardless — if it is somehow still oversized, the insert
 *      simply fails and is logged as a backup miss (see routes/photos.ts);
 *      the share copy, which is the critical path, is entirely unaffected.
 *
 * Re-encoding also auto-orients from EXIF (phone photos are often stored
 * upright-pixels-but-rotated-by-tag) and drops EXIF entirely on the way
 * through, which quietly strips GPS/location tags from any photo that needed
 * compression.
 *
 * Every photo also gets a 250px-wide thumbnail sibling (`generateThumbnail`),
 * used for the UI's fast inline preview so it never has to fetch a full-size
 * image just to render a small tile. `prepareBackupImages` produces both from
 * one decode.
 */

const QUALITY_STEPS = [85, 50];

/** Thumbnails are always scaled to exactly this many pixels wide. */
const THUMBNAIL_WIDTH = 250;
const THUMBNAIL_QUALITY = 80;

export interface BackupImage {
  base64: string;
  /** True if the original was too large and had to be re-encoded. */
  wasCompressed: boolean;
}

async function encodeAtQuality(input: Buffer, quality: number): Promise<Buffer> {
  return sharp(input).rotate().jpeg({ quality }).toBuffer();
}

function fitsThreshold(base64Length: number): boolean {
  return base64Length <= config.photoBackupCompressThresholdBytes;
}

export async function prepareBackupImage(input: Buffer): Promise<BackupImage> {
  const asIs = input.toString('base64');
  if (fitsThreshold(asIs.length)) {
    return { base64: asIs, wasCompressed: false };
  }

  let best = await encodeAtQuality(input, QUALITY_STEPS[0]!);
  let bestBase64 = best.toString('base64');
  for (const quality of QUALITY_STEPS.slice(1)) {
    if (fitsThreshold(bestBase64.length)) break;
    best = await encodeAtQuality(input, quality);
    bestBase64 = best.toString('base64');
  }

  // Accept whatever the final tier produced, even if still oversized — the
  // caller treats the backup as best-effort and logs a miss rather than
  // failing the upload.
  return { base64: bestBase64, wasCompressed: true };
}

/**
 * A small preview copy for the inline photo strip: scaled to exactly 250px
 * wide (height follows automatically to keep the aspect ratio), auto-oriented
 * from EXIF the same way the full backup is. `withoutEnlargement` means a
 * source already narrower than 250px is left at its native width rather than
 * blown up and blurry — in practice this never applies to a real photo.
 */
export async function generateThumbnail(input: Buffer): Promise<BackupImage> {
  const buf = await sharp(input)
    .rotate()
    .resize({ width: THUMBNAIL_WIDTH, withoutEnlargement: true })
    .jpeg({ quality: THUMBNAIL_QUALITY })
    .toBuffer();
  return { base64: buf.toString('base64'), wasCompressed: true };
}

/** Full backup and thumbnail together, since both derive from the same input. */
export async function prepareBackupImages(
  input: Buffer
): Promise<{ full: BackupImage; thumbnail: BackupImage }> {
  const [full, thumbnail] = await Promise.all([
    prepareBackupImage(input),
    generateThumbnail(input),
  ]);
  return { full, thumbnail };
}
