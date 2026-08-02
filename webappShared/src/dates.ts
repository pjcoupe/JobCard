/**
 * Date helpers matching the desktop app's "d/M/yy" display format
 * (JobQueryForm.ParsedDateOK accepts d/M/yy and d/M/yyyy).
 */

/** Format a date as d/M/yy for display, as the desktop form does. */
export function formatShortDate(value: string | Date | null | undefined): string {
  const d = toDate(value);
  if (!d) return '';
  const yy = String(d.getFullYear()).slice(-2);
  return `${d.getDate()}/${d.getMonth() + 1}/${yy}`;
}

/** Format as yyyy-MM-dd for <input type="date">. */
export function toDateInputValue(value: string | Date | null | undefined): string {
  const d = toDate(value);
  if (!d) return '';
  const mm = String(d.getMonth() + 1).padStart(2, '0');
  const dd = String(d.getDate()).padStart(2, '0');
  return `${d.getFullYear()}-${mm}-${dd}`;
}

/**
 * Parse an <input type="date"> value (yyyy-MM-dd) into a Date at local noon.
 * Noon avoids the date shifting a day when serialized to UTC, which is how the
 * existing documents were written (e.g. "2025-07-31T12:00:00.000Z").
 */
export function fromDateInputValue(value: string | null | undefined): Date | null {
  if (!value) return null;
  const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value.trim());
  if (!m) return null;
  const [, y, mo, d] = m;
  return new Date(Date.UTC(Number(y), Number(mo) - 1, Number(d), 12, 0, 0));
}

export function toDate(value: string | Date | null | undefined): Date | null {
  if (!value) return null;
  const d = value instanceof Date ? value : new Date(value);
  return Number.isNaN(d.getTime()) ? null : d;
}

/** Add days to a date, used by the "+1 week" button. */
export function addDays(value: Date, days: number): Date {
  const d = new Date(value.getTime());
  d.setDate(d.getDate() + days);
  return d;
}

/** Today at local noon, matching how the desktop writes date-only values. */
export function todayAtNoon(): Date {
  const now = new Date();
  return new Date(Date.UTC(now.getFullYear(), now.getMonth(), now.getDate(), 12, 0, 0));
}
