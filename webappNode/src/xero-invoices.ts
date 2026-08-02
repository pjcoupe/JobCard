import { parseXeroDate, type XeroLineItem, type XeroSentInvoice } from 'webapp-shared';
import { jobCards, sentInvoices } from './db.js';

/**
 * The settings.sentInvoices collection — one record per invoice sent for a job in
 * a tenant. Ports the sentInvoices half of DataAccess.cs
 * (FindSentInvoiceByJobAsync, FindUnpaidSentInvoicesForTenantAsync,
 * UpsertSentInvoiceAsync, DeleteSentInvoiceAsync, UpdateJobPaidStatusAsync).
 *
 * Field names and BSON types match SentInvoiceDoc exactly, because the desktop
 * app reads these same documents. Note jobId here is the human job number
 * (jobID), not a Mongo ObjectId — a different convention from jobPictures.
 */

/** Full server-side shape, including the field the browser must never see. */
export interface SentInvoiceRecord extends XeroSentInvoice {
  /** The entire create-invoice response body, kept for audit. Never sent to the browser. */
  rawResponseSnippet: string | null;
}

function str(value: unknown): string | null {
  return typeof value === 'string' ? value : null;
}

function num(value: unknown): number | null {
  return typeof value === 'number' && Number.isFinite(value) ? value : null;
}

function isoOrNull(value: unknown): string | null {
  if (value instanceof Date) return Number.isNaN(value.getTime()) ? null : value.toISOString();
  if (typeof value === 'string' && value.trim()) {
    const parsed = new Date(value);
    return Number.isNaN(parsed.getTime()) ? null : parsed.toISOString();
  }
  return null;
}

function shape(doc: Record<string, unknown>): SentInvoiceRecord {
  return {
    jobId: num(doc.jobId) ?? 0,
    jobBusinessName: str(doc.jobBusinessName),
    xeroTenantId: str(doc.xeroTenantId),
    xeroContactId: str(doc.xeroContactId),
    xeroInvoiceId: str(doc.xeroInvoiceId),
    invoiceNumber: str(doc.invoiceNumber),
    invoiceMode: str(doc.invoiceMode),
    amountTotal: num(doc.amountTotal),
    currency: str(doc.currency),
    dateSentUtc: isoOrNull(doc.dateSentUtc),
    datePaidUtc: isoOrNull(doc.datePaidUtc),
    status: str(doc.status),
    lineItemsSnapshot: Array.isArray(doc.lineItemsSnapshot)
      ? (doc.lineItemsSnapshot as XeroLineItem[])
      : [],
    rawResponseSnippet: str(doc.rawResponseSnippet),
  };
}

/** Strip the audit blob before anything goes to the browser. */
export function toClientView(record: SentInvoiceRecord): XeroSentInvoice {
  const { rawResponseSnippet: _rawResponseSnippet, ...view } = record;
  return view;
}

/**
 * The most recent invoice sent for this job, mirroring
 * DataAccess.FindSentInvoiceByJobAsync: the tenant is only part of the filter
 * when one is given.
 */
export async function findSentInvoiceByJob(
  jobId: number,
  tenantId: string | null
): Promise<SentInvoiceRecord | null> {
  const filter: Record<string, unknown> = { jobId };
  if (tenantId && tenantId.trim()) filter.xeroTenantId = tenantId;
  const docs = await sentInvoices().find(filter).sort({ dateSentUtc: -1 }).limit(1).toArray();
  return docs.length > 0 ? shape(docs[0]!) : null;
}

/**
 * Unpaid invoices for a tenant (DataAccess.FindUnpaidSentInvoicesForTenantAsync).
 * "Unpaid" means datePaidUtc is null — which in MongoDB also matches documents
 * where the field is absent, the same as the C# driver's behaviour. There is
 * deliberately no status filter, so a VOIDED invoice with no paid date is still
 * re-checked.
 *
 * Bounded, unlike the desktop's version. Each record costs one sequential HTTP
 * call to Xero, and Xero allows 60 calls per minute per tenant, so a workshop with
 * hundreds of unpaid invoices would blow the rate limit and time out the request.
 * The caller reports how many were left over so the truncation is never silent.
 * Oldest first, so repeated runs work through the backlog instead of re-checking
 * the same newest rows.
 */
export async function findUnpaidSentInvoicesForTenant(
  tenantId: string,
  limit: number
): Promise<SentInvoiceRecord[]> {
  if (!tenantId || !tenantId.trim()) return [];
  const docs = await sentInvoices()
    .find({ xeroTenantId: tenantId, datePaidUtc: null })
    .sort({ dateSentUtc: 1 })
    .limit(limit)
    .toArray();
  return docs.map(shape);
}

/** How many unpaid invoices are outstanding, for reporting what a sync left behind. */
export async function countUnpaidSentInvoicesForTenant(tenantId: string): Promise<number> {
  if (!tenantId || !tenantId.trim()) return 0;
  return sentInvoices().countDocuments({ xeroTenantId: tenantId, datePaidUtc: null });
}

/** The fields written when an invoice is first sent. */
export interface NewSentInvoice {
  jobId: number;
  jobBusinessName: string;
  xeroTenantId: string;
  xeroContactId: string;
  xeroInvoiceId: string;
  invoiceNumber: string;
  invoiceMode: string;
  amountTotal: number;
  currency: string;
  status: string;
  lineItemsSnapshot: XeroLineItem[];
  rawResponseSnippet: string;
}

/**
 * Insert or update the record for a job+tenant
 * (DataAccess.UpsertSentInvoiceAsync). A per-field $set rather than a replace, so
 * nothing else on the document is disturbed.
 */
export async function upsertSentInvoice(doc: NewSentInvoice): Promise<SentInvoiceRecord> {
  const filter = { jobId: doc.jobId, xeroTenantId: doc.xeroTenantId };
  await sentInvoices().updateOne(
    filter,
    {
      $set: {
        jobBusinessName: doc.jobBusinessName,
        xeroContactId: doc.xeroContactId,
        xeroInvoiceId: doc.xeroInvoiceId,
        invoiceNumber: doc.invoiceNumber,
        invoiceMode: doc.invoiceMode,
        amountTotal: doc.amountTotal,
        currency: doc.currency,
        dateSentUtc: new Date(),
        status: doc.status,
        lineItemsSnapshot: doc.lineItemsSnapshot,
        rawResponseSnippet: doc.rawResponseSnippet,
      },
      // A fresh send is unpaid. $setOnInsert so re-sending never wipes a paid date.
      $setOnInsert: { jobId: doc.jobId, xeroTenantId: doc.xeroTenantId, datePaidUtc: null },
    },
    { upsert: true }
  );
  const saved = await findSentInvoiceByJob(doc.jobId, doc.xeroTenantId);
  if (!saved) throw new Error('The sent-invoice record could not be read back after saving.');
  return saved;
}

/** Remove the local record (DataAccess.DeleteSentInvoiceAsync). */
export async function deleteSentInvoice(jobId: number, tenantId: string): Promise<boolean> {
  const result = await sentInvoices().deleteOne({ jobId, xeroTenantId: tenantId });
  return result.deletedCount > 0;
}

/**
 * Mark a job paid on the job card itself (DataAccess.UpdateJobPaidStatusAsync).
 * Both fields already exist and are already writable through the normal job save
 * path: jobDatePaid is a date and 'Xero' is one of PAYMENT_BY_OPTIONS.
 */
export async function markJobPaid(jobId: number, paidDate: Date): Promise<void> {
  await jobCards().updateOne(
    { jobID: jobId },
    { $set: { jobDatePaid: paidDate, jobPaymentBy: 'Xero' } }
  );
}

export interface AppliedInvoice {
  record: SentInvoiceRecord;
  status: string | null;
  /** Set when Xero reports the invoice fully paid. */
  paidDate: Date | null;
}

/**
 * Copy the live state of a Xero invoice onto the local record, and onto the job
 * card when it is paid. Ports XeroManagementForm.ApplyInvoiceFromXeroToSentAsync.
 *
 * FullyPaidOnDate is the only paid signal the desktop uses, and as there it is
 * never cleared once set: an invoice does not transition back to unpaid, so
 * payment history is not silently erased. Re-running this is idempotent.
 */
export async function applyInvoiceFromXero(
  record: SentInvoiceRecord,
  invoice: Record<string, unknown>
): Promise<AppliedInvoice> {
  const status = typeof invoice.Status === 'string' ? invoice.Status : record.status;
  const paidDate = parseXeroDate(invoice.FullyPaidOnDate);

  const fields: Record<string, unknown> = { status };
  if (paidDate) fields.datePaidUtc = paidDate;

  await sentInvoices().updateOne(
    { jobId: record.jobId, xeroTenantId: record.xeroTenantId },
    { $set: fields }
  );

  if (paidDate) {
    await markJobPaid(record.jobId, paidDate);
  }

  return {
    record: {
      ...record,
      status: status ?? null,
      datePaidUtc: paidDate ? paidDate.toISOString() : record.datePaidUtc,
    },
    status: status ?? null,
    paidDate,
  };
}
