/**
 * Xero integration types and the pure line-item logic, ported from the desktop
 * app's XeroService.cs / XeroManagementForm.cs / JobCard.BuildXeroLineItems.
 *
 * Shared so the browser and the server agree on invoice modes, status handling
 * and what a line item looks like. Everything here is pure — the HTTP calls
 * live in webappNode/src/xero-client.ts.
 */

import { JobCardDoc, toLines } from './job-card.model.js';

/** cboMode in XeroManagementForm — persisted as settings.xeroInvoiceMode. */
export const XERO_INVOICE_MODES = ['Draft', 'AuthoriseAndEmail'] as const;

export type XeroInvoiceMode = (typeof XERO_INVOICE_MODES)[number];

/**
 * XeroService.GetDefaultMode: blank or exactly "Draft" means Draft, anything
 * else means authorise-and-email. Deliberately as permissive as the desktop so
 * an unrecognised stored value behaves identically in both apps.
 */
export function normalizeInvoiceMode(mode: string | null | undefined): XeroInvoiceMode {
  if (!mode || !mode.trim() || mode === 'Draft') return 'Draft';
  return 'AuthoriseAndEmail';
}

/** settings.xeroDefaultSalesAccountCode default (btnSendInvoice_Click). */
export const XERO_DEFAULT_ACCOUNT_CODE = '200';

/** settings.xeroDefaultTaxType default — NZ GST on income. */
export const XERO_DEFAULT_TAX_TYPE = 'OUTPUT2';

/** Hard-coded in XeroService.CreateInvoiceAsync. */
export const XERO_CURRENCY = 'NZD';

/** DueDate = Date + 14 days (XeroService.CreateInvoiceAsync). */
export const XERO_DUE_DAYS = 14;

/** Description used for the freight line — see buildXeroLineItems. */
export const XERO_FREIGHT_DESCRIPTION = 'Freight';

/** One line of the Invoices payload sent to Xero. */
export interface XeroLineItem {
  Description: string;
  Quantity: number;
  UnitAmount: number;
  AccountCode: string;
  TaxType: string;
}

/** A candidate from GET /api.xro/2.0/Contacts. */
export interface XeroContactMatch {
  contactId: string;
  name: string;
  emailAddress: string;
}

/** One row of GET https://api.xero.com/connections — a Xero organisation. */
export interface XeroTenant {
  tenantId: string;
  tenantName: string;
}

/**
 * Browser-safe view of a settings.sentInvoices document (SentInvoiceDoc).
 * Deliberately omits rawResponseSnippet, which holds an entire raw Xero
 * response and has no business reaching the browser.
 */
export interface XeroSentInvoice {
  jobId: number;
  jobBusinessName: string | null;
  xeroTenantId: string | null;
  xeroContactId: string | null;
  xeroInvoiceId: string | null;
  invoiceNumber: string | null;
  invoiceMode: string | null;
  amountTotal: number | null;
  currency: string | null;
  dateSentUtc: string | null;
  datePaidUtc: string | null;
  status: string | null;
  lineItemsSnapshot: XeroLineItem[];
}

/**
 * What deleting a sent invoice should do, given its live status in Xero.
 * Ports XeroManagementForm.GetInvoiceDeleteAction exactly:
 *
 *   DRAFT / SUBMITTED -> set status DELETED in Xero
 *   AUTHORISED        -> set status VOIDED in Xero
 *   VOIDED / DELETED  -> 'NONE': already gone, just drop the local record
 *   anything else     -> null: blocked (notably PAID — a paid invoice must not
 *                        be voided out from under the payment)
 */
export function xeroDeleteAction(
  status: string | null | undefined
): 'DELETED' | 'VOIDED' | 'NONE' | null {
  const s = (status ?? '').trim().toUpperCase();
  if (!s) return null;
  if (s === 'DRAFT' || s === 'SUBMITTED') return 'DELETED';
  if (s === 'AUTHORISED') return 'VOIDED';
  if (s === 'VOIDED' || s === 'DELETED') return 'NONE';
  return null;
}

/**
 * Parse a date out of a Xero JSON response.
 *
 * The api.xro/2.0 endpoints return Microsoft-style dates such as
 * "/Date(1748649600000+0000)/". The desktop app got these converted to DateTime
 * for free by JavaScriptSerializer and then round-tripped them through a
 * culture-formatted string — two call sites, two different parses
 * (XeroManagementForm used InvariantCulture+AssumeLocal, JobCard used the
 * current culture). This is the single unified version: read the epoch millis
 * directly, and fall back to ISO for the newer endpoints.
 */
export function parseXeroDate(value: unknown): Date | null {
  if (value == null) return null;
  if (value instanceof Date) return Number.isNaN(value.getTime()) ? null : value;
  const raw = String(value).trim();
  if (!raw) return null;
  const msDate = /^\/Date\((-?\d+)([+-]\d{4})?\)\/$/.exec(raw);
  if (msDate) {
    const millis = Number(msDate[1]);
    if (!Number.isFinite(millis)) return null;
    // The trailing offset only describes how Xero rendered the instant; the
    // millis are already UTC-based, so it is not applied.
    return new Date(millis);
  }
  const parsed = new Date(raw);
  return Number.isNaN(parsed.getTime()) ? null : parsed;
}

/**
 * Build the Xero line items for a job, porting JobCard.BuildXeroLineItems.
 *
 * The desktop loops its flat control arrays from index 0 to freightIndex
 * inclusive; toLines() already produces exactly that ordering and length
 * (18 numbered lines, then the 11 fixed rows, then freight last), so this walks
 * toLines() rather than re-deriving the field names.
 *
 * Rules carried over verbatim:
 *  - a row with a blank description is skipped, whatever else it holds
 *  - quantity defaults to 1 when missing or <= 0
 *  - unit amount falls back to the line price when no unit price is set
 *
 * Two deliberate differences, both documented in the plan:
 *
 *  1. Freight. The desktop reads its description from txtFreightText, a
 *     UI-only control with no BSON field — the text is never persisted, so it
 *     cannot be ported. A fixed "Freight" description is used instead, and the
 *     line is included only when there is a non-zero freight amount (the
 *     desktop skips it when that textbox happens to be blank).
 *
 *  2. The unit-amount fallback divides by quantity. The desktop assigns the
 *     *extended* line total to UnitAmount without dividing, so a qty-2 line
 *     carrying only a $100 total invoices the customer $200. That is an
 *     over-billing bug rather than intended behaviour, and it only triggers on
 *     legacy rows with no unit price, since the UI computes price = qty x
 *     unitPrice (see lineTotalFor in totals.ts).
 */
export function buildXeroLineItems(
  doc: JobCardDoc,
  accountCode: string,
  taxType: string
): XeroLineItem[] {
  const items: XeroLineItem[] = [];
  for (const line of toLines(doc)) {
    const isFreight = line.kind === 'freight';
    const description = isFreight ? XERO_FREIGHT_DESCRIPTION : (line.detail ?? '').trim();
    if (!description) continue;

    const price = typeof line.price === 'number' && Number.isFinite(line.price) ? line.price : 0;
    // Freight has no description of its own to blank out, so a zero amount is
    // what marks it absent.
    if (isFreight && price === 0) continue;

    const rawQty = line.qty;
    const quantity = rawQty == null || !Number.isFinite(rawQty) || rawQty <= 0 ? 1 : rawQty;

    const rawUnit = line.unitPrice;
    const hasUnitPrice = rawUnit != null && Number.isFinite(rawUnit) && rawUnit > 0;
    const unitAmount = hasUnitPrice ? rawUnit : price / quantity;

    items.push({
      Description: description,
      Quantity: quantity,
      UnitAmount: unitAmount,
      AccountCode: accountCode,
      TaxType: taxType,
    });
  }
  return items;
}
