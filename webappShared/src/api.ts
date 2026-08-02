/** Request/response contracts shared by webappNode and webappUI. */

import type { JobCardDoc } from './job-card.model.js';
import type { JobTypeOption } from './pricing.js';
import type {
  XeroContactMatch,
  XeroInvoiceMode,
  XeroSentInvoice,
  XeroTenant,
} from './xero.js';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
}

export interface ErrorResponse {
  error: string;
}

export interface JobListItem {
  _id: string;
  jobID: number;
  jobDate: string | null;
  jobCustomer: string | null;
  jobBusinessName: string | null;
  jobPhone: string | null;
  jobDateRequired: string | null;
  jobDateCompleted: string | null;
  jobDatePaid: string | null;
  jobQuotation: boolean | null;
  jobGoodReserved: boolean | null;
  /** First non-empty detail line, for a one-line summary in the list. */
  summary: string | null;
  /** Grand total including GST. */
  total: number | null;
}

export interface JobListResponse {
  items: JobListItem[];
  total: number;
  page: number;
  pageSize: number;
}

export interface JobTypeCatalogueResponse {
  groups: Array<{ detail: string; options: JobTypeOption[] }>;
}

export interface SaveJobResponse {
  job: JobCardDoc;
}

export interface CreateJobResponse {
  job: JobCardDoc;
}

/**
 * Xero connection and configuration state, for the panel that replaces the
 * desktop's XeroManagementForm.
 *
 * Nothing secret appears here and nothing secret ever may: the client secret and
 * the access/refresh tokens stay on the server. `missing` carries field *names*
 * only so the UI can say what still needs configuring. The redirect URI is not a
 * secret — the browser is sent to it — and the UI needs it to explain the
 * paste-the-code fallback.
 */
export interface XeroStatusResponse {
  /** clientId, clientSecret and redirectUri are all present in settings. */
  configured: boolean;
  /** A usable token is stored (or is refreshable). */
  connected: boolean;
  tokenExpiresAt: string | null;
  tenantId: string | null;
  tenantName: string | null;
  invoiceMode: XeroInvoiceMode;
  redirectUri: string | null;
  defaultAccountCode: string;
  defaultTaxType: string;
  /** Names of the settings fields that still need filling in, e.g. ['xeroClientSecret']. */
  missing: string[];
}

export interface XeroSetModeRequest {
  mode: XeroInvoiceMode;
}

export interface XeroConnectStartResponse {
  authorizeUrl: string;
  state: string;
  /** True when the redirect URI points at this server and the callback will work directly. */
  callbackHandled: boolean;
}

/**
 * The paste-the-code fallback, for connecting from a device that cannot reach
 * the redirect URI (a phone, when the URI is http://localhost). Accepts the whole
 * URL copied out of the address bar, or a bare code plus the state.
 */
export interface XeroConnectCompleteRequest {
  /** Full redirect URL copied from the browser, including ?code=&state=. */
  redirectUrl?: string;
  code?: string;
  state?: string;
}

export interface XeroTenantsResponse {
  tenants: XeroTenant[];
}

export interface XeroSelectTenantRequest {
  tenantId: string;
  tenantName: string;
}

export interface XeroContactsResponse {
  candidates: XeroContactMatch[];
  /** Set when a candidate matched the business name exactly (case-insensitively). */
  exactMatch: XeroContactMatch | null;
}

/** State of the Xero invoice for one job, driving the panel's buttons. */
export interface XeroJobInvoiceResponse {
  sentInvoice: XeroSentInvoice | null;
  /** Everything needed to send is in place (ports RefreshActionStates). */
  canSend: boolean;
  /** Why sending is unavailable, as a sentence to show the user. */
  blockedReason: string | null;
  /** What deleting would do in Xero: 'DELETED', 'VOIDED', 'NONE', or null if blocked. */
  deleteAction: string | null;
}

export interface XeroSendInvoiceRequest {
  /** The chosen Xero contact. Everything else is derived server-side from the job. */
  contactId: string;
}

export interface XeroSendInvoiceResponse {
  sentInvoice: XeroSentInvoice;
}

export interface XeroDeleteInvoiceResponse {
  /** What was actually applied in Xero, or 'NONE' when only the local record went. */
  applied: string;
  message: string;
}

/** Paid-status poll for one job (ports RefreshXeroPaidStatusAsync). */
export interface XeroRefreshJobResponse {
  sentInvoice: XeroSentInvoice | null;
  status: string | null;
  /** Set when Xero reports the invoice fully paid; written onto jobDatePaid. */
  paidDate: string | null;
}

export interface XeroSyncUnpaidResponse {
  synced: number;
  paid: number;
  /** Still unpaid but not checked this run — each check is one Xero API call, so
   *  a run is capped to stay inside Xero's rate limit. Run again to continue. */
  remaining: number;
}
