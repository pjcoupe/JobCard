import { randomUUID } from 'node:crypto';
import { Router, type Request, type RequestHandler, type Response } from 'express';
import {
  XERO_CURRENCY,
  XERO_INVOICE_MODES,
  buildXeroLineItems,
  calculateTotals,
  toLines,
  xeroDeleteAction,
  type JobCardDoc,
  type XeroContactMatch,
  type XeroInvoiceMode,
} from 'webapp-shared';
import { requireAuth } from '../auth.js';
import { jobCards } from '../db.js';
import {
  XeroError,
  buildAuthorizeUrl,
  createInvoice,
  ensureValidToken,
  exchangeCode,
  findContacts,
  getInvoice,
  getTenants,
  invoiceStatusOf,
  updateInvoiceStatus,
} from '../xero-client.js';
import {
  applyInvoiceFromXero,
  countUnpaidSentInvoicesForTenant,
  deleteSentInvoice,
  findSentInvoiceByJob,
  findUnpaidSentInvoicesForTenant,
  toClientView,
  upsertSentInvoice,
  type SentInvoiceRecord,
} from '../xero-invoices.js';

/**
 * How many invoices one "Sync all unpaid" run checks. Each is a separate HTTP call
 * to Xero, which allows 60 per minute per organisation, so this keeps a single run
 * inside the rate limit and inside a sane request duration. Whatever is left over
 * is reported back so it can be run again.
 */
const SYNC_UNPAID_BATCH = 50;
import {
  findXeroSettings,
  missingClientConfig,
  updateXeroSettingsFields,
  type XeroSettings,
} from '../xero-settings.js';

/**
 * Xero endpoints, replacing the desktop app's XeroManagementForm.
 *
 * requireAuth is applied per route rather than at the mount point, because
 * GET /callback is a top-level browser navigation from Xero and cannot carry an
 * Authorization header. Every other route is behind the session token.
 */
export const xeroRouter = Router();

/**
 * Wrap a handler so a rejected promise becomes a JSON error rather than a hung
 * request. Express 4 does not forward async rejections to the error middleware,
 * and unlike the other routers here every one of these makes outbound network
 * calls that will genuinely fail — timeouts, 401s, rate limits.
 */
function xeroHandler(
  handler: (req: Request, res: Response) => Promise<void>
): RequestHandler {
  return (req, res, next) => {
    handler(req, res).catch((err: unknown) => {
      if (err instanceof XeroError) {
        res.status(err.status).json({ error: err.message });
        return;
      }
      next(err);
    });
  };
}

/** Load settings or fail with a message naming what is missing. */
async function requireConfig(): Promise<XeroSettings> {
  const config = await findXeroSettings();
  const missing = missingClientConfig(config);
  if (!config || missing.length > 0) {
    throw new XeroError(
      `Xero is not configured yet. Missing in the settings.settings document: ${missing.join(', ')}.`,
      400
    );
  }
  return config;
}

/** Settings plus a valid access token, or a message telling the user to connect. */
async function requireConnection(): Promise<XeroSettings> {
  const config = await requireConfig();
  if (!config.activeXeroToken && !config.xeroRefreshToken) {
    throw new XeroError('Not connected to Xero. Connect first.', 400);
  }
  return ensureValidToken(config);
}

function requireTenant(config: XeroSettings): string {
  if (!config.xeroTenantId) {
    throw new XeroError('Choose a Xero organisation first.', 400);
  }
  return config.xeroTenantId;
}

function parseJobId(raw: unknown): number {
  const jobId = Math.trunc(Number(raw));
  if (!Number.isFinite(jobId) || jobId <= 0) {
    throw new XeroError('That job number is not valid.', 400);
  }
  return jobId;
}

async function loadJob(jobId: number): Promise<JobCardDoc> {
  const doc = (await jobCards().findOne({ jobID: jobId })) as unknown as JobCardDoc | null;
  if (!doc) throw new XeroError(`Job ${jobId} not found`, 404);
  return doc;
}

/**
 * Pending OAuth attempts, kept in memory rather than in Mongo: `state` is
 * short-lived CSRF protection for one connect attempt, and the settings document
 * is shared with the desktop app, so it is not the place for transient values.
 */
const pendingStates = new Map<string, number>();
const STATE_TTL_MS = 10 * 60 * 1000;

function rememberState(state: string): void {
  const now = Date.now();
  for (const [key, created] of pendingStates) {
    if (now - created > STATE_TTL_MS) pendingStates.delete(key);
  }
  pendingStates.set(state, now);
}

function consumeState(state: string | undefined): boolean {
  if (!state) return false;
  const created = pendingStates.get(state);
  if (created === undefined) return false;
  pendingStates.delete(state);
  return Date.now() - created <= STATE_TTL_MS;
}

/** True when the redirect URI points back at this server, so the callback works directly. */
function callbackIsHandledHere(redirectUri: string): boolean {
  return /\/api\/xero\/callback\/?$/.test(redirectUri.trim());
}

/**
 * GET /api/xero/status — connection and configuration state.
 * Ports the labels XeroManagementForm.ReloadStateAsync sets. Carries no secret:
 * the client secret and the tokens never leave the server.
 */
xeroRouter.get(
  '/status',
  requireAuth,
  xeroHandler(async (_req, res) => {
    const config = await findXeroSettings();
    const missing = missingClientConfig(config);
    res.json({
      configured: missing.length === 0,
      connected: !!(config && (config.activeXeroToken || config.xeroRefreshToken)),
      tokenExpiresAt: config?.xeroTokenExpiresAtUtc?.toISOString() ?? null,
      tenantId: config?.xeroTenantId || null,
      tenantName: config?.xeroTenantName || null,
      invoiceMode: config?.xeroInvoiceMode ?? 'Draft',
      redirectUri: config?.xeroRedirectUri || null,
      defaultAccountCode: config?.xeroDefaultSalesAccountCode ?? '200',
      defaultTaxType: config?.xeroDefaultTaxType ?? 'OUTPUT2',
      missing,
    });
  })
);

/**
 * POST /api/xero/mode — persist the invoice mode.
 * Mirrors cboMode_SelectedIndexChanged, which also saves immediately.
 *
 * The mode is validated strictly rather than run through normalizeInvoiceMode:
 * that helper maps anything unrecognised to a usable default, which is right when
 * *reading* a legacy stored value but wrong here — a malformed request would
 * silently downgrade AuthoriseAndEmail to Draft and quietly stop invoices being
 * sent to customers.
 */
xeroRouter.post(
  '/mode',
  requireAuth,
  xeroHandler(async (req, res) => {
    const body = (req.body ?? {}) as { mode?: unknown };
    const mode = typeof body.mode === 'string' ? body.mode.trim() : '';
    if (!XERO_INVOICE_MODES.includes(mode as XeroInvoiceMode)) {
      res
        .status(400)
        .json({ error: `Invoice mode must be one of: ${XERO_INVOICE_MODES.join(', ')}.` });
      return;
    }
    const config = await requireConfig();
    await updateXeroSettingsFields(config._id, { xeroInvoiceMode: mode });
    res.json({ invoiceMode: mode as XeroInvoiceMode });
  })
);

/**
 * POST /api/xero/connect/start — begin the OAuth flow.
 * Returns the consent URL for the browser to open. The desktop app spins up a
 * local HttpListener here (CaptureAuthorizationCodeFromLocalRedirectAsync); a real
 * server has GET /callback below instead.
 */
xeroRouter.post(
  '/connect/start',
  requireAuth,
  xeroHandler(async (_req, res) => {
    const config = await requireConfig();
    const state = randomUUID();
    rememberState(state);
    res.json({
      authorizeUrl: buildAuthorizeUrl(config, state),
      state,
      callbackHandled: callbackIsHandledHere(config.xeroRedirectUri),
    });
  })
);

/** Tiny page shown in the tab Xero redirected, so the user knows to close it. */
function callbackPage(title: string, detail: string): string {
  const escape = (text: string) =>
    text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
  return `<!doctype html>
<html lang="en"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>${escape(title)}</title>
<style>
  body { font: 16px/1.5 system-ui, sans-serif; margin: 0; display: grid;
         place-items: center; min-height: 100vh; background: #f4f6f9; color: #1b2537; }
  .card { background: #fff; padding: 2rem; border-radius: 14px; max-width: 26rem;
          text-align: center; box-shadow: 0 6px 30px rgba(10,20,40,.12); }
  h1 { font-size: 1.25rem; margin: 0 0 .5rem; }
  p { margin: 0; color: #55637a; }
</style></head>
<body><div class="card"><h1>${escape(title)}</h1><p>${escape(detail)}</p></div></body></html>`;
}

/**
 * GET /api/xero/callback — where Xero sends the browser after consent.
 *
 * Public by necessity: this is a top-level navigation and carries no session
 * token. That is safe because it does nothing without a `state` this server
 * generated moments ago, and the code alone is useless without the client secret.
 * The query string holds a credential, so it is never logged.
 */
xeroRouter.get(
  '/callback',
  xeroHandler(async (req, res) => {
    const query = req.query as Record<string, string | undefined>;
    res.setHeader('Content-Type', 'text/html; charset=utf-8');

    if (query.error) {
      res
        .status(400)
        .send(callbackPage('Xero declined the connection', query.error_description ?? query.error));
      return;
    }
    if (!consumeState(query.state)) {
      res
        .status(400)
        .send(
          callbackPage(
            'That connection attempt has expired',
            'Go back to the job card and press Connect to Xero again.'
          )
        );
      return;
    }
    const code = (query.code ?? '').trim();
    if (!code) {
      res.status(400).send(callbackPage('No authorization code arrived', 'Please try connecting again.'));
      return;
    }

    const config = await requireConfig();
    await exchangeCode(config, code);
    console.log('[xero] connected via the callback route');
    res.send(
      callbackPage('Connected to Xero', 'You can close this tab and go back to the job card.')
    );
  })
);

/**
 * POST /api/xero/connect/complete — the paste-the-code fallback.
 *
 * Needed whenever the browser cannot reach the redirect URI: with the URI set to
 * http://localhost:3000/..., connecting from a phone lands on a dead page, but the
 * code is still in the address bar. Accepts the whole URL so the user does not
 * have to pick the code out of it by hand. The desktop's equivalent
 * (PromptForAuthCode) verifies nothing; this checks `state` like the real callback.
 */
xeroRouter.post(
  '/connect/complete',
  requireAuth,
  xeroHandler(async (req, res) => {
    const body = (req.body ?? {}) as { redirectUrl?: unknown; code?: unknown; state?: unknown };

    let code = typeof body.code === 'string' ? body.code.trim() : '';
    let state = typeof body.state === 'string' ? body.state.trim() : '';

    if (typeof body.redirectUrl === 'string' && body.redirectUrl.trim()) {
      const raw = body.redirectUrl.trim();
      try {
        // Accept a full URL, or just the query string on its own.
        const url = new URL(raw, 'http://localhost');
        code = (url.searchParams.get('code') ?? code).trim();
        state = (url.searchParams.get('state') ?? state).trim();
        const error = url.searchParams.get('error');
        if (error) {
          throw new XeroError(`Xero declined the connection: ${error}`, 400);
        }
      } catch (err) {
        if (err instanceof XeroError) throw err;
        throw new XeroError(
          'That does not look like the address Xero redirected to. Paste the whole URL from the address bar.',
          400
        );
      }
    }

    if (!code) {
      throw new XeroError(
        'No authorization code found. Paste the whole URL from the address bar after logging into Xero.',
        400
      );
    }
    if (!consumeState(state)) {
      throw new XeroError(
        'That connection attempt has expired or does not match. Press Connect to Xero and try again.',
        400
      );
    }

    const config = await requireConfig();
    await exchangeCode(config, code);
    console.log('[xero] connected via the pasted redirect URL');
    res.json({ connected: true });
  })
);

/**
 * POST /api/xero/disconnect — forget the stored tokens.
 * The desktop app has no real disconnect: it only relabels itself "Disconnected"
 * and leaves the tokens in Mongo.
 */
xeroRouter.post(
  '/disconnect',
  requireAuth,
  xeroHandler(async (_req, res) => {
    const config = await findXeroSettings();
    if (config) {
      await updateXeroSettingsFields(config._id, {
        activeXeroToken: '',
        xeroAccessToken: '',
        xeroRefreshToken: '',
        xeroTokenExpiresAtUtc: null,
        xeroTokenLockUntilUtc: null,
      });
    }
    res.json({ connected: false });
  })
);

/** GET /api/xero/tenants — the organisations this connection can reach. */
xeroRouter.get(
  '/tenants',
  requireAuth,
  xeroHandler(async (_req, res) => {
    const config = await requireConnection();
    res.json({ tenants: await getTenants(config) });
  })
);

/**
 * POST /api/xero/tenant — remember the chosen organisation.
 * Writes all three fields together, as PersistTenantSelectionFromComboAsync does.
 */
xeroRouter.post(
  '/tenant',
  requireAuth,
  xeroHandler(async (req, res) => {
    const body = (req.body ?? {}) as { tenantId?: unknown; tenantName?: unknown };
    const tenantId = typeof body.tenantId === 'string' ? body.tenantId.trim() : '';
    const tenantName = typeof body.tenantName === 'string' ? body.tenantName.trim() : '';
    if (!tenantId) {
      res.status(400).json({ error: 'Choose a Xero organisation.' });
      return;
    }
    const config = await requireConfig();
    await updateXeroSettingsFields(config._id, {
      xeroTenantId: tenantId,
      xeroTenantName: tenantName,
      xeroLastTenant: tenantId,
    });
    res.json({ tenantId, tenantName });
  })
);

/**
 * GET /api/xero/contacts?businessName= — candidate customers.
 * Ports btnCheckCustomer_Click: an exact case-insensitive name match wins
 * outright, otherwise the caller picks from the candidates.
 */
xeroRouter.get(
  '/contacts',
  requireAuth,
  xeroHandler(async (req, res) => {
    const businessName = String(req.query.businessName ?? '').trim();
    if (!businessName) {
      res.status(400).json({ error: 'Enter a business name on the job card first.' });
      return;
    }
    const config = await requireConnection();
    const tenantId = requireTenant(config);
    const candidates = await findContacts(config, tenantId, businessName);
    const exactMatch: XeroContactMatch | null =
      candidates.find((c) => c.name.toLowerCase() === businessName.toLowerCase()) ?? null;
    res.json({ candidates, exactMatch });
  })
);

/**
 * Work out whether this job can be invoiced, porting the enable conditions in
 * XeroManagementForm.RefreshActionStates. Computed here rather than in the browser
 * so the reason can be shown as text instead of a silently disabled button.
 */
async function describeSendability(
  jobId: number
): Promise<{
  sentInvoice: SentInvoiceRecord | null;
  canSend: boolean;
  blockedReason: string | null;
  deleteAction: string | null;
}> {
  const config = await findXeroSettings();
  const missing = missingClientConfig(config);
  const sentInvoice = await findSentInvoiceByJob(jobId, config?.xeroTenantId ?? null);
  const deleteAction = sentInvoice ? xeroDeleteAction(sentInvoice.status) : null;

  const blocked = (reason: string) => ({ sentInvoice, canSend: false, blockedReason: reason, deleteAction });

  if (!config || missing.length > 0) {
    return blocked(`Xero is not configured yet (missing ${missing.join(', ')}).`);
  }
  if (!config.activeXeroToken && !config.xeroRefreshToken) {
    return blocked('Not connected to Xero.');
  }
  if (!config.xeroTenantId) {
    return blocked('No Xero organisation chosen.');
  }

  const job = (await jobCards().findOne({ jobID: jobId })) as unknown as JobCardDoc | null;
  if (!job) return blocked(`Job ${jobId} not found.`);

  if (!String(job.jobBusinessName ?? '').trim()) {
    return blocked('This job has no business name, so there is no Xero customer to invoice.');
  }
  if (!String(job.jobOrderNumber ?? '').trim()) {
    return blocked('Order Number cannot be blank!');
  }
  if (calculateTotals(toLines(job)).totalIncludingGst <= 0) {
    return blocked('The job total is zero, so there is nothing to invoice.');
  }
  // v1 rule from the design doc: one invoice per job per organisation. A record
  // whose Xero status came back DELETED no longer counts.
  if (sentInvoice && (sentInvoice.status ?? '').trim().toUpperCase() !== 'DELETED') {
    return blocked(
      `Invoice ${sentInvoice.invoiceNumber ?? ''} has already been sent for this job.`.replace(
        '  ',
        ' '
      )
    );
  }
  return { sentInvoice, canSend: true, blockedReason: null, deleteAction };
}

/**
 * GET /api/xero/jobs/:jobID/invoice — the local invoice record and what can be
 * done with it. Deliberately makes no Xero call, so the panel opens instantly;
 * POST /refresh below is the one that goes out to Xero.
 */
xeroRouter.get(
  '/jobs/:jobID/invoice',
  requireAuth,
  xeroHandler(async (req, res) => {
    const jobId = parseJobId(req.params.jobID);
    const state = await describeSendability(jobId);
    res.json({
      sentInvoice: state.sentInvoice ? toClientView(state.sentInvoice) : null,
      canSend: state.canSend,
      blockedReason: state.blockedReason,
      deleteAction: state.deleteAction,
    });
  })
);

/**
 * POST /api/xero/jobs/:jobID/invoice — create the invoice in Xero.
 *
 * Server-authoritative, matching how job totals work everywhere else here: the
 * browser sends only the chosen contact, and the order number, line items and
 * amount are all derived from the stored job document.
 */
xeroRouter.post(
  '/jobs/:jobID/invoice',
  requireAuth,
  xeroHandler(async (req, res) => {
    const jobId = parseJobId(req.params.jobID);
    const body = (req.body ?? {}) as { contactId?: unknown };
    const contactId = typeof body.contactId === 'string' ? body.contactId.trim() : '';
    if (!contactId) {
      res.status(400).json({ error: 'Check the customer and choose a Xero contact first.' });
      return;
    }

    const state = await describeSendability(jobId);
    if (!state.canSend) {
      res.status(400).json({ error: state.blockedReason ?? 'This job cannot be invoiced.' });
      return;
    }

    const config = await requireConnection();
    const tenantId = requireTenant(config);
    const job = await loadJob(jobId);

    const lineItems = buildXeroLineItems(
      job,
      config.xeroDefaultSalesAccountCode,
      config.xeroDefaultTaxType
    );
    if (lineItems.length === 0) {
      res.status(400).json({ error: 'This job has no priced lines to invoice.' });
      return;
    }

    const orderNumber = String(job.jobOrderNumber ?? '').trim();
    const totals = calculateTotals(toLines(job));

    // A priced line with a blank description counts towards the job total but is
    // not invoiceable, so the invoice can legitimately come to less than the job
    // does. The desktop app has the same behaviour and says nothing about it;
    // under-billing a customer silently is worth a log line at least.
    const invoiceExGst = lineItems.reduce((sum, item) => sum + item.Quantity * item.UnitAmount, 0);
    const shortfall = Math.round((totals.totalExcludingGst - invoiceExGst) * 100) / 100;
    if (shortfall !== 0) {
      console.warn(
        `[xero] job ${jobId}: invoice lines total ${invoiceExGst.toFixed(2)} ex-GST but the job ` +
          `totals ${totals.totalExcludingGst.toFixed(2)} (difference ${shortfall.toFixed(2)}). ` +
          'Usually a priced line with no description, which cannot be invoiced.'
      );
    }
    const created = await createInvoice(
      config,
      tenantId,
      contactId,
      config.xeroInvoiceMode,
      // The desktop sends the order number as the Xero Reference.
      orderNumber,
      lineItems
    );

    const saved = await upsertSentInvoice({
      jobId,
      jobBusinessName: String(job.jobBusinessName ?? '').trim(),
      xeroTenantId: tenantId,
      xeroContactId: contactId,
      xeroInvoiceId: created.invoiceId,
      invoiceNumber: created.invoiceNumber,
      invoiceMode: config.xeroInvoiceMode,
      amountTotal: totals.totalIncludingGst,
      currency: XERO_CURRENCY,
      status: created.status,
      lineItemsSnapshot: lineItems,
      rawResponseSnippet: created.rawResponse,
    });

    console.log(
      `[xero] job ${jobId}: sent invoice ${created.invoiceNumber} (${created.status}) to ${tenantId}`
    );
    res.status(201).json({ sentInvoice: toClientView(saved) });
  })
);

/**
 * DELETE /api/xero/jobs/:jobID/invoice — delete or void the invoice.
 *
 * Ports btnDeleteInvoice_Click: check the live status first, then apply whatever
 * Xero permits for that status (xeroDeleteAction). The local record is only
 * removed once Xero has agreed, so a failure never loses the link to a live
 * invoice.
 */
xeroRouter.delete(
  '/jobs/:jobID/invoice',
  requireAuth,
  xeroHandler(async (req, res) => {
    const jobId = parseJobId(req.params.jobID);
    const config = await requireConnection();
    const sent = await findSentInvoiceByJob(jobId, config.xeroTenantId);
    if (!sent) {
      res.status(404).json({ error: 'There is no sent invoice for this job.' });
      return;
    }
    // Use the tenant the invoice was sent to, not whatever is selected now.
    const tenantId = sent.xeroTenantId ?? config.xeroTenantId;
    if (!sent.xeroInvoiceId || !tenantId) {
      await deleteSentInvoice(jobId, tenantId ?? '');
      res.json({ applied: 'NONE', message: 'Removed the incomplete local invoice record.' });
      return;
    }

    const invoice = await getInvoice(config, tenantId, sent.xeroInvoiceId);
    const liveStatus = invoiceStatusOf(invoice);
    const action = xeroDeleteAction(liveStatus);

    if (action === null) {
      res.status(400).json({
        error: `Cannot delete or void this invoice while Xero reports it as ${liveStatus ?? '(unknown)'}.`,
      });
      return;
    }

    if (action === 'NONE') {
      await deleteSentInvoice(jobId, tenantId);
      res.json({
        applied: 'NONE',
        message: `The invoice is already ${liveStatus} in Xero. Removed the local record.`,
      });
      return;
    }

    await updateInvoiceStatus(config, tenantId, sent.xeroInvoiceId, action);
    await deleteSentInvoice(jobId, tenantId);
    console.log(`[xero] job ${jobId}: invoice ${sent.invoiceNumber} marked ${action}`);
    res.json({
      applied: action,
      message: `Invoice marked ${action} in Xero and removed locally.`,
    });
  })
);

/**
 * POST /api/xero/jobs/:jobID/refresh — poll Xero for this job's paid status.
 * Ports JobCard.RefreshXeroPaidStatusAsync, which the desktop's 5-minute timer
 * calls; here the browser calls it on the same interval while a job is open.
 */
xeroRouter.post(
  '/jobs/:jobID/refresh',
  requireAuth,
  xeroHandler(async (req, res) => {
    const jobId = parseJobId(req.params.jobID);
    const config = await findXeroSettings();
    if (!config || (!config.activeXeroToken && !config.xeroRefreshToken) || !config.xeroTenantId) {
      // Nothing to poll: report rather than erroring, since the browser polls this
      // on a timer and a disconnected Xero is not a request failure.
      res.json({ sentInvoice: null, status: null, paidDate: null });
      return;
    }
    const sent = await findSentInvoiceByJob(jobId, config.xeroTenantId);
    if (!sent || !sent.xeroInvoiceId || !sent.xeroTenantId) {
      res.json({ sentInvoice: null, status: null, paidDate: null });
      return;
    }

    const connected = await ensureValidToken(config);
    const invoice = await getInvoice(connected, sent.xeroTenantId, sent.xeroInvoiceId);
    if (!invoice) {
      res.json({ sentInvoice: toClientView(sent), status: sent.status, paidDate: null });
      return;
    }

    const applied = await applyInvoiceFromXero(sent, invoice);
    res.json({
      sentInvoice: toClientView(applied.record),
      status: applied.status,
      paidDate: applied.paidDate ? applied.paidDate.toISOString() : null,
    });
  })
);

/**
 * POST /api/xero/sync-unpaid — re-check every unpaid invoice for the current
 * organisation. Ports btnSyncAllUnpaid_Click, including its one-request-per-
 * invoice sequential loop.
 */
xeroRouter.post(
  '/sync-unpaid',
  requireAuth,
  xeroHandler(async (_req, res) => {
    const config = await requireConnection();
    const tenantId = requireTenant(config);
    const outstanding = await countUnpaidSentInvoicesForTenant(tenantId);
    const unpaid = await findUnpaidSentInvoicesForTenant(tenantId, SYNC_UNPAID_BATCH);

    let synced = 0;
    let paid = 0;
    for (const sent of unpaid) {
      if (!sent.xeroInvoiceId || !sent.xeroTenantId) continue;
      // One failure must not abandon the rest of the batch.
      try {
        const invoice = await getInvoice(config, sent.xeroTenantId, sent.xeroInvoiceId);
        if (!invoice) continue;
        const applied = await applyInvoiceFromXero(sent, invoice);
        synced += 1;
        if (applied.paidDate) paid += 1;
      } catch (err) {
        console.warn(`[xero] could not sync invoice ${sent.invoiceNumber} for job ${sent.jobId}:`, err);
      }
    }
    // Anything still unpaid beyond this batch — never truncate silently.
    const remaining = Math.max(0, outstanding - unpaid.length);
    console.log(
      `[xero] synced ${synced} unpaid invoice(s), ${paid} now paid, ${remaining} not checked this run`
    );
    res.json({ synced, paid, remaining });
  })
);
