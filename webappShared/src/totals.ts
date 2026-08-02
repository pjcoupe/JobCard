import { JobCardDoc, JobLine, toLines } from './job-card.model.js';

/** NZ GST rate used by the desktop app (JobCard.UpdateAllTotals). */
export const GST_RATE = 0.15;

/** Round half-away-from-zero to 2dp, matching MidpointRounding.AwayFromZero. */
export function round2(value: number): number {
  const sign = value < 0 ? -1 : 1;
  return (sign * Math.round(Math.abs(value) * 100 + Number.EPSILON)) / 100;
}

export interface JobTotals {
  /** Sum of all line prices — GST exclusive (stored in jobTOTAL). */
  totalExcludingGst: number;
  /** 15% of the ex-GST total (stored in jobGST). */
  gst: number;
  /** Grand total including GST (stored in jobSubTotal). */
  totalIncludingGst: number;
  /** What the customer pays — same as totalIncludingGst. */
  amountToPay: number;
}

/**
 * Mirrors JobCard.UpdateAllTotals(): line prices are GST exclusive, GST is
 * added on top, and the grand total is the amount to pay.
 */
export function calculateTotals(lines: JobLine[]): JobTotals {
  let sum = 0;
  for (const line of lines) {
    if (typeof line.price === 'number' && Number.isFinite(line.price)) {
      sum += line.price;
    }
  }
  const totalExcludingGst = round2(sum);
  const gst = round2(totalExcludingGst * GST_RATE);
  const totalIncludingGst = round2(totalExcludingGst + gst);
  return {
    totalExcludingGst,
    gst,
    totalIncludingGst,
    amountToPay: totalIncludingGst,
  };
}

/** Recompute a line's price from qty x unitPrice (as the desktop popup does). */
export function lineTotalFor(qty: number | null, unitPrice: number | null): number | null {
  if (unitPrice == null) return null;
  const q = qty == null || qty === 0 ? 1 : qty;
  return round2(q * unitPrice);
}

/** Recompute totals from a document and write them back onto it. */
export function applyTotals(doc: JobCardDoc): JobTotals {
  const totals = calculateTotals(toLines(doc));
  doc.jobTOTAL = totals.totalExcludingGst;
  doc.jobGST = totals.gst;
  doc.jobSubTotal = totals.totalIncludingGst;
  return totals;
}
