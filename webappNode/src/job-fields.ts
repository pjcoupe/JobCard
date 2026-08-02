import {
  FIXED_ROWS,
  NUMBERED_LINE_COUNT,
  fixedRowFields,
  numberedLineFields,
} from 'webapp-shared';

export type FieldKind = 'string' | 'int' | 'float' | 'bool' | 'date';

/**
 * Whitelist of writable job fields and their storage types. Requests may only
 * set fields listed here, and values are coerced to the same BSON types the
 * desktop app writes (DataAccess.JobCardDoc) so both apps stay compatible.
 */
export const JOB_FIELD_TYPES: Record<string, FieldKind> = (() => {
  const map: Record<string, FieldKind> = {
    jobCustomer: 'string',
    jobAddress: 'string',
    jobPhone: 'string',
    jobEmail: 'string',
    jobOrderNumber: 'string',
    jobFussyNotes: 'string',
    jobDelivery: 'string',
    jobReceivedFrom: 'string',
    jobPaymentBy: 'string',
    jobNotes: 'string',
    jobBusinessName: 'string',
    jobDate: 'date',
    jobDateRequired: 'date',
    jobDateCompleted: 'date',
    jobDatePaid: 'date',
    jobFreight: 'float',
    jobSubTotal: 'float',
    jobGST: 'float',
    jobTOTAL: 'float',
    jobCompleted: 'bool',
    jobCollected: 'bool',
    jobGoodReserved: 'bool',
    jobQuotation: 'bool',
  };
  for (let i = 0; i < NUMBERED_LINE_COUNT; i++) {
    const f = numberedLineFields(i);
    map[f.detail] = 'string';
    map[f.type] = 'string';
    map[f.qty] = 'int';
    map[f.unitPrice] = 'float';
    map[f.price] = 'float';
  }
  for (const name of FIXED_ROWS) {
    const f = fixedRowFields(name);
    map[f.checked] = 'bool';
    map[f.detail] = 'string';
    map[f.type] = 'string';
    map[f.qty] = 'int';
    map[f.unitPrice] = 'float';
    map[f.price] = 'float';
  }
  return map;
})();

/** Fields the desktop computes; the server always recomputes them on save. */
export const COMPUTED_FIELDS = ['jobSubTotal', 'jobGST', 'jobTOTAL'] as const;

function isBlank(value: unknown): boolean {
  return value === null || value === undefined || (typeof value === 'string' && value.trim() === '');
}

/**
 * Coerce an incoming value to its stored type. Blank values become null, which
 * is how the desktop app clears a field.
 */
export function coerceField(kind: FieldKind, value: unknown): unknown {
  if (isBlank(value)) return null;
  switch (kind) {
    case 'string':
      return String(value);
    case 'int': {
      const n = Number(value);
      return Number.isFinite(n) ? Math.trunc(n) : null;
    }
    case 'float': {
      const n = Number(value);
      return Number.isFinite(n) ? n : null;
    }
    case 'bool': {
      if (typeof value === 'boolean') return value;
      const s = String(value).toLowerCase();
      if (s === 'true') return true;
      if (s === 'false') return false;
      return null;
    }
    case 'date': {
      const d = value instanceof Date ? value : new Date(String(value));
      return Number.isNaN(d.getTime()) ? null : d;
    }
  }
}

/**
 * Build a $set document from an arbitrary request body, keeping only known
 * fields and coercing each to its stored type.
 */
export function buildUpdate(body: unknown): Record<string, unknown> {
  const update: Record<string, unknown> = {};
  if (!body || typeof body !== 'object') return update;
  for (const [key, value] of Object.entries(body as Record<string, unknown>)) {
    const kind = JOB_FIELD_TYPES[key];
    if (!kind) continue;
    update[key] = coerceField(kind, value);
  }
  return update;
}
