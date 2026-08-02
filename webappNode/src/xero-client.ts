import {
  XERO_CURRENCY,
  XERO_DUE_DAYS,
  type XeroContactMatch,
  type XeroInvoiceMode,
  type XeroLineItem,
  type XeroTenant,
} from 'webapp-shared';
import { settings } from './db.js';
import { findXeroSettings, updateXeroSettingsFields, type XeroSettings } from './xero-settings.js';

/**
 * Xero API client — a port of the desktop app's XeroService.cs.
 *
 * Uses the built-in fetch rather than adding an HTTP dependency, in keeping with
 * the rest of this server (hand-rolled .env loader, hand-rolled HMAC tokens).
 *
 * Differences from the C# original, all deliberate:
 *
 *  - `Accept: application/json` is sent on every call. GetInvoiceAsync and
 *    UpdateInvoiceStatusAsync omit it and rely on Xero defaulting to JSON.
 *  - `offline_access` is in the scope list. It is commented out in the C#, which
 *    is why the refresh token stored in Mongo is an unusable 8 characters and the
 *    desktop app has to be reconnected by hand whenever the token expires.
 *  - The scopes are `accounting.transactions` and `accounting.contacts`, per the
 *    approved design doc. The C# asks for `accounting.invoices` and
 *    `accounting.payments`, which are not Xero scope names.
 *  - Failures carry the response body. The C# token calls discard it entirely, so
 *    a failed connect reports nothing at all.
 */

const AUTHORIZE_URL = 'https://login.xero.com/identity/connect/authorize';
const TOKEN_URL = 'https://identity.xero.com/connect/token';
const CONNECTIONS_URL = 'https://api.xero.com/connections';
const API_BASE = 'https://api.xero.com/api.xro/2.0';

const SCOPES = [
  'openid',
  'profile',
  'email',
  'accounting.transactions',
  'accounting.contacts',
  'offline_access',
].join(' ');

/** Treat a token as expired this far ahead, matching the C#'s 1-minute buffer. */
const EXPIRY_BUFFER_MS = 60 * 1000;

/** How long the refresh lease is held, and how long a loser waits for the winner. */
const LEASE_MS = 30 * 1000;
const LEASE_POLL_MS = 500;
const LEASE_POLL_ATTEMPTS = 20;

/** Thrown for anything the caller should report to the user verbatim. */
export class XeroError extends Error {
  readonly status: number;
  constructor(message: string, status = 502) {
    super(message);
    this.name = 'XeroError';
    this.status = status;
  }
}

function basicAuth(config: XeroSettings): string {
  return Buffer.from(`${config.xeroClientId}:${config.xeroClientSecret}`, 'utf8').toString('base64');
}

function sleep(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

/** True when the stored token exists and is not about to expire. */
function tokenIsFresh(config: XeroSettings | null): boolean {
  return !!(
    config &&
    config.activeXeroToken &&
    config.xeroTokenExpiresAtUtc &&
    config.xeroTokenExpiresAtUtc.getTime() > Date.now() + EXPIRY_BUFFER_MS
  );
}

/**
 * Build the Xero consent URL (XeroService.BuildAuthorizeUrl). The caller keeps
 * `state` and must check it matches when the callback arrives.
 */
export function buildAuthorizeUrl(config: XeroSettings, state: string): string {
  const params = new URLSearchParams({
    response_type: 'code',
    client_id: config.xeroClientId,
    redirect_uri: config.xeroRedirectUri,
    scope: SCOPES,
    state,
  });
  return `${AUTHORIZE_URL}?${params.toString()}`;
}

interface TokenResponse {
  access_token?: unknown;
  refresh_token?: unknown;
  expires_in?: unknown;
}

/**
 * Persist a token response. activeXeroToken is the field the desktop app also
 * reads; xeroAccessToken is written alongside it so an older build still works.
 */
async function storeTokens(
  config: XeroSettings,
  body: TokenResponse,
  previousRefreshToken: string
): Promise<void> {
  const accessToken = typeof body.access_token === 'string' ? body.access_token : '';
  const refreshToken =
    typeof body.refresh_token === 'string' ? body.refresh_token : previousRefreshToken;
  const fields: Record<string, unknown> = {
    activeXeroToken: accessToken,
    xeroAccessToken: accessToken,
    xeroRefreshToken: refreshToken,
  };
  if (typeof body.expires_in === 'number' && Number.isFinite(body.expires_in)) {
    // Store the expiry already shortened by the buffer, as the C# does.
    fields.xeroTokenExpiresAtUtc = new Date(Date.now() + (body.expires_in - 60) * 1000);
  }
  await updateXeroSettingsFields(config._id, fields);
}

/**
 * Exchange an authorization code for tokens (XeroService.ExchangeCodeAsync).
 * Throws with the response body on failure so the user sees why.
 */
export async function exchangeCode(config: XeroSettings, authCode: string): Promise<void> {
  const response = await fetch(TOKEN_URL, {
    method: 'POST',
    headers: {
      Authorization: `Basic ${basicAuth(config)}`,
      'Content-Type': 'application/x-www-form-urlencoded',
      Accept: 'application/json',
    },
    body: new URLSearchParams({
      grant_type: 'authorization_code',
      code: authCode,
      redirect_uri: config.xeroRedirectUri,
    }),
  });
  const text = await response.text();
  if (!response.ok) {
    // Never log or echo the code itself — it is a credential.
    throw new XeroError(
      `Xero rejected the authorization code (HTTP ${response.status}). ${text || '(no details)'}`,
      400
    );
  }
  await storeTokens(config, JSON.parse(text) as TokenResponse, '');
}

/**
 * Try to take the token-refresh lease, atomically. Xero rotates the refresh token
 * on every use and retires the old one, so if this server and the desktop app
 * refresh at the same moment one of them is locked out and has to reconnect.
 * Mirrors DataAccess.TryAcquireXeroTokenLockAsync.
 */
async function tryAcquireLease(config: XeroSettings): Promise<boolean> {
  const now = new Date();
  const result = await settings().updateOne(
    {
      _id: config._id as never,
      // An equality test against null also matches documents where the field is
      // absent, which is the state before the lease has ever been taken.
      $or: [{ xeroTokenLockUntilUtc: null }, { xeroTokenLockUntilUtc: { $lt: now } }],
    },
    { $set: { xeroTokenLockUntilUtc: new Date(now.getTime() + LEASE_MS) } }
  );
  return result.modifiedCount > 0;
}

async function releaseLease(config: XeroSettings): Promise<void> {
  await updateXeroSettingsFields(config._id, { xeroTokenLockUntilUtc: null });
}

/**
 * Within this process, stop two concurrent requests both queueing for the lease.
 * The Mongo lease handles cross-process contention; this handles in-process.
 */
let refreshInFlight: Promise<XeroSettings> | null = null;

async function refreshTokens(config: XeroSettings): Promise<XeroSettings> {
  // Another party may have refreshed since our caller loaded settings.
  let latest = await findXeroSettings();
  if (tokenIsFresh(latest)) return latest!;

  let holdsLease = await tryAcquireLease(config);
  if (!holdsLease) {
    for (let attempt = 0; attempt < LEASE_POLL_ATTEMPTS; attempt++) {
      await sleep(LEASE_POLL_MS);
      latest = await findXeroSettings();
      if (tokenIsFresh(latest)) return latest!;
    }
    // Whoever held it never finished. The lease has expired by now, so rather
    // than leave the user stuck, take it and refresh ourselves.
    holdsLease = await tryAcquireLease(config);
    if (!holdsLease) {
      throw new XeroError('Another app is refreshing the Xero token. Try again in a moment.', 503);
    }
  }

  try {
    // Always refresh with the newest token on record, not the one our caller read.
    const refreshToken = latest?.xeroRefreshToken || config.xeroRefreshToken;
    if (!refreshToken) {
      throw new XeroError('Not connected to Xero. Connect first.', 400);
    }
    const response = await fetch(TOKEN_URL, {
      method: 'POST',
      headers: {
        Authorization: `Basic ${basicAuth(config)}`,
        'Content-Type': 'application/x-www-form-urlencoded',
        Accept: 'application/json',
      },
      body: new URLSearchParams({ grant_type: 'refresh_token', refresh_token: refreshToken }),
    });
    const text = await response.text();
    if (!response.ok) {
      // invalid_grant means the refresh token is spent — usually because the other
      // app rotated it. Re-read once in case it has: if the stored token changed
      // under us, retry with the new one before giving up.
      if (text.includes('invalid_grant')) {
        const reread = await findXeroSettings();
        if (reread && reread.xeroRefreshToken && reread.xeroRefreshToken !== refreshToken) {
          console.warn('[xero] refresh token was rotated by another app, retrying with the new one');
          const retry = await fetch(TOKEN_URL, {
            method: 'POST',
            headers: {
              Authorization: `Basic ${basicAuth(config)}`,
              'Content-Type': 'application/x-www-form-urlencoded',
              Accept: 'application/json',
            },
            body: new URLSearchParams({
              grant_type: 'refresh_token',
              refresh_token: reread.xeroRefreshToken,
            }),
          });
          const retryText = await retry.text();
          if (retry.ok) {
            await storeTokens(config, JSON.parse(retryText) as TokenResponse, reread.xeroRefreshToken);
            const updated = await findXeroSettings();
            if (updated) return updated;
          }
        }
        // Genuinely spent. Clear the tokens so the UI reports disconnected rather
        // than retrying a dead token on every request.
        await updateXeroSettingsFields(config._id, {
          activeXeroToken: '',
          xeroAccessToken: '',
          xeroRefreshToken: '',
          xeroTokenExpiresAtUtc: null,
        });
        throw new XeroError('The Xero connection has expired. Reconnect to Xero.', 401);
      }
      throw new XeroError(
        `Could not refresh the Xero token (HTTP ${response.status}). ${text || '(no details)'}`,
        502
      );
    }
    await storeTokens(config, JSON.parse(text) as TokenResponse, refreshToken);
    const updated = await findXeroSettings();
    if (!updated) throw new XeroError('Xero settings disappeared while refreshing the token.');
    return updated;
  } finally {
    await releaseLease(config);
  }
}

/**
 * Return settings holding a usable access token, refreshing if needed.
 * Ports XeroService.EnsureValidTokenAsync, plus the cross-app lease.
 */
export async function ensureValidToken(config: XeroSettings): Promise<XeroSettings> {
  if (tokenIsFresh(config)) return config;
  if (!config.xeroRefreshToken) {
    throw new XeroError('Not connected to Xero. Connect first.', 400);
  }
  if (!refreshInFlight) {
    refreshInFlight = refreshTokens(config).finally(() => {
      refreshInFlight = null;
    });
  }
  return refreshInFlight;
}

/** Standard headers for an api.xro call. */
function apiHeaders(config: XeroSettings, tenantId: string): Record<string, string> {
  return {
    Authorization: `Bearer ${config.activeXeroToken}`,
    'xero-tenant-id': tenantId,
    Accept: 'application/json',
  };
}

/**
 * The Xero organisations this connection can reach
 * (XeroService.GetTenantsAsync). The C# silently returns an empty list on
 * failure, which is indistinguishable from "no organisations"; this reports it.
 */
export async function getTenants(config: XeroSettings): Promise<XeroTenant[]> {
  const response = await fetch(CONNECTIONS_URL, {
    headers: {
      Authorization: `Bearer ${config.activeXeroToken}`,
      Accept: 'application/json',
    },
  });
  const text = await response.text();
  if (!response.ok) {
    throw new XeroError(
      `Could not list Xero organisations (HTTP ${response.status}). ${text || '(no details)'}`
    );
  }
  const rows = JSON.parse(text) as Array<Record<string, unknown>>;
  return rows.map((row) => ({
    tenantId: typeof row.tenantId === 'string' ? row.tenantId : '',
    tenantName: typeof row.tenantName === 'string' ? row.tenantName : '',
  }));
}

/**
 * Escape a value for a Xero `where=` clause. The C# escapes only the double
 * quote, so a name containing a backslash breaks the query.
 */
function escapeXeroWhereString(value: string): string {
  return value.replace(/\\/g, '\\\\').replace(/"/g, '\\"');
}

/** Contacts whose name contains the business name (XeroService.FindContactsAsync). */
export async function findContacts(
  config: XeroSettings,
  tenantId: string,
  businessName: string
): Promise<XeroContactMatch[]> {
  const where = `Name!=null&&Name.Contains("${escapeXeroWhereString(businessName.trim())}")`;
  const url = `${API_BASE}/Contacts?where=${encodeURIComponent(where)}`;
  const response = await fetch(url, { headers: apiHeaders(config, tenantId) });
  const text = await response.text();
  if (!response.ok) {
    throw new XeroError(
      `Could not search Xero contacts (HTTP ${response.status}). ${text || '(no details)'}`
    );
  }
  const body = JSON.parse(text) as { Contacts?: unknown };
  if (!Array.isArray(body.Contacts)) return [];
  return (body.Contacts as Array<Record<string, unknown>>).map((row) => ({
    contactId: typeof row.ContactID === 'string' ? row.ContactID : '',
    name: typeof row.Name === 'string' ? row.Name : '',
    emailAddress: typeof row.EmailAddress === 'string' ? row.EmailAddress : '',
  }));
}

/** yyyy-MM-dd, as the C# formats Date and DueDate. */
function isoDay(date: Date): string {
  return date.toISOString().slice(0, 10);
}

export interface CreatedInvoice {
  invoiceId: string;
  invoiceNumber: string;
  status: string;
  rawResponse: string;
}

/**
 * Create the invoice, and email it when the mode says so
 * (XeroService.CreateInvoiceAsync). Payload fields match the C# exactly.
 */
export async function createInvoice(
  config: XeroSettings,
  tenantId: string,
  contactId: string,
  mode: XeroInvoiceMode,
  reference: string,
  lineItems: XeroLineItem[]
): Promise<CreatedInvoice> {
  const now = new Date();
  const due = new Date(now.getTime());
  due.setDate(due.getDate() + XERO_DUE_DAYS);

  const payload = {
    Invoices: [
      {
        Type: 'ACCREC',
        Contact: { ContactID: contactId },
        Date: isoDay(now),
        DueDate: isoDay(due),
        Reference: reference,
        LineItems: lineItems,
        LineAmountTypes: 'Exclusive',
        CurrencyCode: XERO_CURRENCY,
        Status: mode === 'AuthoriseAndEmail' ? 'AUTHORISED' : 'DRAFT',
      },
    ],
  };

  const response = await fetch(`${API_BASE}/Invoices`, {
    method: 'POST',
    headers: { ...apiHeaders(config, tenantId), 'Content-Type': 'application/json' },
    body: JSON.stringify(payload),
  });
  const text = await response.text();
  if (!response.ok) {
    throw new XeroError(
      `Xero rejected the invoice (HTTP ${response.status}). ${text || '(no details)'}`
    );
  }

  const body = JSON.parse(text) as { Invoices?: unknown };
  const first = Array.isArray(body.Invoices)
    ? (body.Invoices[0] as Record<string, unknown> | undefined)
    : undefined;
  if (!first) {
    // The C# leaves the error message null here, so the desktop shows a bare
    // "Send failed: " with nothing after it.
    throw new XeroError(`Xero accepted the request but returned no invoice. ${text}`);
  }

  const created: CreatedInvoice = {
    invoiceId: typeof first.InvoiceID === 'string' ? first.InvoiceID : '',
    invoiceNumber: typeof first.InvoiceNumber === 'string' ? first.InvoiceNumber : '',
    status: typeof first.Status === 'string' ? first.Status : '',
    rawResponse: text,
  };

  if (mode === 'AuthoriseAndEmail' && created.invoiceId) {
    // The C# fires this and ignores the outcome, so a failed email is invisible.
    // The invoice itself is already created, so this must not fail the send —
    // but it is worth a log line.
    try {
      const email = await fetch(`${API_BASE}/Invoices/${created.invoiceId}/Email`, {
        method: 'POST',
        headers: { ...apiHeaders(config, tenantId), 'Content-Type': 'application/json' },
        body: '',
      });
      if (!email.ok) {
        const emailText = await email.text();
        console.warn(
          `[xero] invoice ${created.invoiceNumber} was created but emailing it failed ` +
            `(HTTP ${email.status}): ${emailText || '(no details)'}`
        );
      }
    } catch (err) {
      console.warn(`[xero] invoice ${created.invoiceNumber} was created but emailing it failed:`, err);
    }
  }

  return created;
}

/** Fetch one invoice (XeroService.GetInvoiceAsync). Null when Xero does not have it. */
export async function getInvoice(
  config: XeroSettings,
  tenantId: string,
  invoiceId: string
): Promise<Record<string, unknown> | null> {
  const response = await fetch(`${API_BASE}/Invoices/${encodeURIComponent(invoiceId)}`, {
    headers: apiHeaders(config, tenantId),
  });
  const text = await response.text();
  if (response.status === 404) return null;
  if (!response.ok) {
    throw new XeroError(
      `Could not read the invoice from Xero (HTTP ${response.status}). ${text || '(no details)'}`
    );
  }
  const body = JSON.parse(text) as { Invoices?: unknown };
  if (!Array.isArray(body.Invoices) || body.Invoices.length === 0) return null;
  const first = body.Invoices[0];
  return first && typeof first === 'object' ? (first as Record<string, unknown>) : null;
}

/**
 * Change an invoice's status (XeroService.UpdateInvoiceStatusAsync). Xero treats
 * a POST to the collection with just an ID and a status as an update.
 */
export async function updateInvoiceStatus(
  config: XeroSettings,
  tenantId: string,
  invoiceId: string,
  status: string
): Promise<void> {
  const response = await fetch(`${API_BASE}/Invoices`, {
    method: 'POST',
    headers: { ...apiHeaders(config, tenantId), 'Content-Type': 'application/json' },
    body: JSON.stringify({ Invoices: [{ InvoiceID: invoiceId, Status: status }] }),
  });
  const text = await response.text();
  if (!response.ok) {
    throw new XeroError(
      `Xero rejected the status change to ${status} (HTTP ${response.status}). ${text || '(no details)'}`
    );
  }
}

/** The invoice status Xero currently reports, upper-cased. */
export function invoiceStatusOf(invoice: Record<string, unknown> | null): string | null {
  if (!invoice) return null;
  const status = invoice.Status;
  if (typeof status !== 'string' || !status.trim()) return null;
  return status.trim().toUpperCase();
}
