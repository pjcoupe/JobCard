/**
 * Data model for a job card document in the `wheel.jobCard` MongoDB collection.
 * Field names mirror the BsonElement names used by the desktop app (DataAccess.cs)
 * so the web app reads/writes the exact same documents.
 */

/** Number of free-form (numbered) detail lines: jobDetail00 .. jobDetail17 */
export const NUMBERED_LINE_COUNT = 18;

/**
 * Fixed legacy service rows (indexes 18..28 on the desktop form). Each has
 * job{Name}Text / Type / Qty / UnitPrice / Price fields plus a job{Name} checkbox.
 * On the wheel app these mostly exist for legacy data; they are shown only
 * when they contain data.
 */
export const FIXED_ROWS = [
  'Repair',
  'Strip',
  'Polish',
  'Plating',
  'Laquer',
  'SilvGalv',
  'GoldGalv',
  'WheelCrack',
  'WheelDent',
  'WheelMachine',
  'Tyre',
] as const;

export type FixedRowName = (typeof FIXED_ROWS)[number];

/** One editable line of the job (normalized view over the flat document). */
export interface JobLine {
  /** stable key: '00'..'17' for numbered lines, or a FIXED_ROWS name, or 'Freight' */
  key: string;
  kind: 'numbered' | 'fixed' | 'freight';
  detail: string | null;
  type: string | null;
  qty: number | null;
  unitPrice: number | null;
  price: number | null;
  /** fixed rows only: the legacy checkbox value (job{Name}) */
  checked?: boolean | null;
}

export interface JobCardDoc {
  _id?: string;
  jobID: number;
  jobDate?: string | Date | null;
  jobCustomer?: string | null;
  jobAddress?: string | null;
  jobPhone?: string | null;
  jobEmail?: string | null;
  jobOrderNumber?: string | null;
  jobFussyNotes?: string | null;
  jobDelivery?: string | null;
  jobReceivedFrom?: string | null;
  jobDateRequired?: string | Date | null;
  jobDateCompleted?: string | Date | null;
  jobPaymentBy?: string | null;
  jobNotes?: string | null;
  jobDatePaid?: string | Date | null;
  jobFreight?: number | null;
  /** Grand total including GST (desktop stores it in jobSubTotal) */
  jobSubTotal?: number | null;
  jobGST?: number | null;
  /** Total excluding GST (desktop stores it in jobTOTAL) */
  jobTOTAL?: number | null;
  jobCompleted?: boolean | null;
  jobCollected?: boolean | null;
  jobBusinessName?: string | null;
  jobGoodReserved?: boolean | null;
  jobQuotation?: boolean | null;
  /** flat numbered/fixed line fields such as jobDetail00, jobWheelCrackText ... */
  [field: string]: unknown;
}

/** Field names composing one numbered line NN (zero padded 2 digits). */
export function numberedLineFields(i: number) {
  const nn = String(i).padStart(2, '0');
  return {
    detail: `jobDetail${nn}`,
    type: `jobType${nn}`,
    qty: `jobQty${nn}`,
    unitPrice: `jobUnitPrice${nn}`,
    price: `jobPrice${nn}`,
  };
}

/** Field names composing one fixed row. */
export function fixedRowFields(name: FixedRowName) {
  return {
    checked: `job${name}`,
    detail: `job${name}Text`,
    type: `job${name}Type`,
    qty: `job${name}Qty`,
    unitPrice: `job${name}UnitPrice`,
    price: `job${name}Price`,
  };
}

function asNum(v: unknown): number | null {
  if (v === null || v === undefined || v === '') return null;
  const n = Number(v);
  return Number.isFinite(n) ? n : null;
}

function asStr(v: unknown): string | null {
  if (v === null || v === undefined) return null;
  return String(v);
}

/** Normalize a flat document into an ordered list of editable lines. */
export function toLines(doc: JobCardDoc): JobLine[] {
  const lines: JobLine[] = [];
  for (let i = 0; i < NUMBERED_LINE_COUNT; i++) {
    const f = numberedLineFields(i);
    lines.push({
      key: String(i).padStart(2, '0'),
      kind: 'numbered',
      detail: asStr(doc[f.detail]),
      type: asStr(doc[f.type]),
      qty: asNum(doc[f.qty]),
      unitPrice: asNum(doc[f.unitPrice]),
      price: asNum(doc[f.price]),
    });
  }
  for (const name of FIXED_ROWS) {
    const f = fixedRowFields(name);
    lines.push({
      key: name,
      kind: 'fixed',
      detail: asStr(doc[f.detail]),
      type: asStr(doc[f.type]),
      qty: asNum(doc[f.qty]),
      unitPrice: asNum(doc[f.unitPrice]),
      price: asNum(doc[f.price]),
      checked: (doc[f.checked] as boolean | null | undefined) ?? null,
    });
  }
  lines.push({
    key: 'Freight',
    kind: 'freight',
    detail: null,
    type: null,
    qty: null,
    unitPrice: null,
    price: asNum(doc.jobFreight),
  });
  return lines;
}

/** Write a normalized line back onto the flat document fields. */
export function applyLine(doc: JobCardDoc, line: JobLine): void {
  if (line.kind === 'freight') {
    doc.jobFreight = line.price;
    return;
  }
  if (line.kind === 'numbered') {
    const f = numberedLineFields(Number(line.key));
    doc[f.detail] = line.detail;
    doc[f.type] = line.type;
    doc[f.qty] = line.qty;
    doc[f.unitPrice] = line.unitPrice;
    doc[f.price] = line.price;
    return;
  }
  const f = fixedRowFields(line.key as FixedRowName);
  doc[f.detail] = line.detail;
  doc[f.type] = line.type;
  doc[f.qty] = line.qty;
  doc[f.unitPrice] = line.unitPrice;
  doc[f.price] = line.price;
  doc[f.checked] = line.checked ?? null;
}

/** True when a line carries any user data (used for collapse + printing). */
export function lineHasData(line: JobLine): boolean {
  return !!(
    (line.detail && line.detail.trim()) ||
    (line.type && line.type.trim()) ||
    line.qty != null ||
    line.unitPrice != null ||
    line.price != null ||
    line.checked
  );
}
