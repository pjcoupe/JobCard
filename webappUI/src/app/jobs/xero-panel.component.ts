import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  XERO_INVOICE_MODES,
  type XeroContactMatch,
  type XeroInvoiceMode,
  type XeroSentInvoice,
  type XeroStatusResponse,
  type XeroTenant,
} from 'webapp-shared';
import { XeroService } from '../core/xero.service';

/**
 * Replaces the desktop XeroManagementForm. Same sections in the same order:
 * connection, organisation, invoice mode, customer check, send/delete, history,
 * and sync-all-unpaid.
 *
 * Two deliberate differences from the desktop form:
 *
 *  - The candidate customer list is shown inline in this sheet rather than in a
 *    second modal on top of the first (ShowContactPicker). Stacked modals are
 *    poor on a phone and nothing else in this app does it.
 *  - Where the desktop silently disables Send and shows a "Connect first!"
 *    tooltip, this shows the actual reason as text — the server works it out in
 *    one place (describeSendability) so the browser cannot disagree with it.
 */
@Component({
  selector: 'app-xero-panel',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './xero-panel.component.html',
  styleUrl: './xero-panel.component.scss',
})
export class XeroPanelComponent {
  private readonly xero = inject(XeroService);

  readonly jobId = input.required<number>();
  readonly businessName = input<string>('');

  readonly dismissed = output<void>();
  /** Emitted when Xero reports the invoice paid, so the job card can show the date. */
  readonly paidDateChanged = output<string>();

  readonly modes = XERO_INVOICE_MODES;

  readonly status = signal<XeroStatusResponse | null>(null);
  readonly sentInvoice = signal<XeroSentInvoice | null>(null);
  readonly canSend = signal(false);
  readonly blockedReason = signal<string | null>(null);
  readonly deleteAction = signal<string | null>(null);

  readonly tenants = signal<XeroTenant[]>([]);
  readonly candidates = signal<XeroContactMatch[]>([]);
  readonly selectedContact = signal<XeroContactMatch | null>(null);
  /** True once Check Customer has run and produced no usable match. */
  readonly checkedWithNoMatch = signal(false);

  readonly loading = signal(true);
  /** Label of whatever long-running action is in flight, or null. */
  readonly busy = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly notice = signal<string | null>(null);

  /** Shown when the redirect URI cannot reach this server — see connect(). */
  readonly pasteNeeded = signal(false);
  readonly pastedUrl = signal('');

  readonly connected = computed(() => this.status()?.connected === true);
  readonly configured = computed(() => this.status()?.configured === true);

  readonly tenantLabel = computed(() => {
    const s = this.status();
    if (!s?.tenantId) return null;
    return s.tenantName || s.tenantId;
  });

  /** "Sent invoice: #INV-001 | Status: PAID | ..." — the desktop's history line. */
  readonly historyLine = computed(() => {
    const sent = this.sentInvoice();
    if (!sent) return 'No invoice sent for this job yet.';
    const parts = [`Invoice ${sent.invoiceNumber ?? '(no number)'}`, `Status: ${sent.status ?? '?'}`];
    if (sent.amountTotal != null) {
      parts.push(`Amount: ${sent.amountTotal.toFixed(2)} ${sent.currency ?? ''}`.trim());
    }
    if (sent.dateSentUtc) parts.push(`Sent: ${formatDay(sent.dateSentUtc)}`);
    if (sent.datePaidUtc) parts.push(`Paid: ${formatDay(sent.datePaidUtc)}`);
    return parts.join(' | ');
  });

  readonly canDelete = computed(() => this.connected() && this.sentInvoice() != null);

  constructor() {
    void this.load();
  }

  /** Load everything the panel shows, then poll Xero once for the paid status. */
  async load(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      const status = await this.xero.status();
      this.status.set(status);
      await this.loadInvoiceState();
      if (status.connected) {
        // Best-effort: neither of these should stop the panel opening.
        void this.loadTenants();
        void this.refreshPaidStatus();
      }
    } catch (err) {
      this.error.set(describeError(err, 'Could not load the Xero settings.'));
    } finally {
      this.loading.set(false);
    }
  }

  private async loadInvoiceState(): Promise<void> {
    const state = await this.xero.jobInvoice(this.jobId());
    this.sentInvoice.set(state.sentInvoice);
    this.canSend.set(state.canSend);
    this.blockedReason.set(state.blockedReason);
    this.deleteAction.set(state.deleteAction);
    // A previous send recorded the contact, so Send works again without
    // re-checking (ApplyStoredContactFromSentInvoiceIfNeeded).
    const sent = state.sentInvoice;
    if (sent?.xeroContactId && !this.selectedContact()) {
      this.selectedContact.set({
        contactId: sent.xeroContactId,
        name: sent.jobBusinessName || this.businessName() || 'Xero contact',
        emailAddress: '',
      });
    }
  }

  private async loadTenants(): Promise<void> {
    try {
      this.tenants.set(await this.xero.tenants());
    } catch (err) {
      this.error.set(describeError(err, 'Could not list your Xero organisations.'));
    }
  }

  private async refreshPaidStatus(): Promise<void> {
    try {
      const result = await this.xero.refreshJob(this.jobId());
      if (result.sentInvoice) this.sentInvoice.set(result.sentInvoice);
      if (result.paidDate) this.paidDateChanged.emit(result.paidDate);
    } catch {
      // A polling failure is not worth a banner; the panel still works.
    }
  }

  /**
   * Start the OAuth flow. The consent page opens in a new tab; where Xero
   * redirects back to depends on the configured redirect URI:
   *
   *  - pointing at this server, the callback route finishes the job by itself
   *  - pointing at http://localhost from a different device (a phone), the tab
   *    lands on a dead page and the user pastes that URL back here
   */
  async connect(): Promise<void> {
    this.busy.set('connect');
    this.error.set(null);
    this.notice.set(null);
    try {
      const start = await this.xero.connectStart();
      window.open(start.authorizeUrl, '_blank', 'noopener');
      this.pasteNeeded.set(!start.callbackHandled);
      this.notice.set(
        start.callbackHandled
          ? 'Finish signing in to Xero in the new tab, then press Refresh here.'
          : 'Sign in to Xero in the new tab. It will end on a page that cannot load — copy that whole address and paste it below.'
      );
    } catch (err) {
      this.error.set(describeError(err, 'Could not start the Xero connection.'));
    } finally {
      this.busy.set(null);
    }
  }

  /** Finish the flow from a pasted redirect URL (the desktop's PromptForAuthCode). */
  async completePastedConnect(): Promise<void> {
    const pasted = this.pastedUrl().trim();
    if (!pasted) {
      this.error.set('Paste the address Xero redirected to.');
      return;
    }
    this.busy.set('connect');
    this.error.set(null);
    try {
      await this.xero.connectComplete(pasted);
      this.pastedUrl.set('');
      this.pasteNeeded.set(false);
      this.notice.set('Connected to Xero.');
      await this.load();
    } catch (err) {
      this.error.set(describeError(err, 'Could not complete the Xero connection.'));
    } finally {
      this.busy.set(null);
    }
  }

  async disconnect(): Promise<void> {
    if (!confirm('Disconnect from Xero? You will need to sign in again to send invoices.')) return;
    this.busy.set('connect');
    this.error.set(null);
    try {
      await this.xero.disconnect();
      this.selectedContact.set(null);
      this.candidates.set([]);
      this.notice.set('Disconnected from Xero.');
      await this.load();
    } catch (err) {
      this.error.set(describeError(err, 'Could not disconnect.'));
    } finally {
      this.busy.set(null);
    }
  }

  async chooseTenant(tenantId: string): Promise<void> {
    const tenant = this.tenants().find((t) => t.tenantId === tenantId);
    if (!tenant) return;
    this.busy.set('tenant');
    this.error.set(null);
    try {
      await this.xero.selectTenant(tenant.tenantId, tenant.tenantName);
      this.notice.set(`Using ${tenant.tenantName}.`);
      await this.load();
    } catch (err) {
      this.error.set(describeError(err, 'Could not save the organisation.'));
    } finally {
      this.busy.set(null);
    }
  }

  async chooseMode(mode: string): Promise<void> {
    const next = mode as XeroInvoiceMode;
    this.busy.set('mode');
    this.error.set(null);
    try {
      await this.xero.setMode(next);
      this.status.update((s) => (s ? { ...s, invoiceMode: next } : s));
      this.notice.set(
        next === 'Draft'
          ? 'Invoices will be created as drafts in Xero.'
          : 'Invoices will be approved and emailed to the customer.'
      );
    } catch (err) {
      this.error.set(describeError(err, 'Could not save the invoice mode.'));
    } finally {
      this.busy.set(null);
    }
  }

  /**
   * Match the job's business name against Xero contacts (btnCheckCustomer_Click).
   * An exact case-insensitive match is taken automatically; otherwise the
   * candidates are listed for the user to pick from.
   */
  async checkCustomer(): Promise<void> {
    this.busy.set('customer');
    this.error.set(null);
    this.notice.set(null);
    this.checkedWithNoMatch.set(false);
    try {
      const result = await this.xero.contacts(this.businessName());
      if (result.exactMatch) {
        this.selectedContact.set(result.exactMatch);
        this.candidates.set([]);
        this.notice.set(`Matched ${result.exactMatch.name} in Xero.`);
      } else if (result.candidates.length > 0) {
        this.candidates.set(result.candidates);
        this.notice.set('No exact match — choose the right customer below.');
      } else {
        this.candidates.set([]);
        this.checkedWithNoMatch.set(true);
      }
      await this.loadInvoiceState();
    } catch (err) {
      this.error.set(describeError(err, 'Could not search Xero for this customer.'));
    } finally {
      this.busy.set(null);
    }
  }

  pickCandidate(candidate: XeroContactMatch): void {
    this.selectedContact.set(candidate);
    this.candidates.set([]);
    this.notice.set(`Using ${candidate.name}.`);
  }

  async sendInvoice(): Promise<void> {
    const contact = this.selectedContact();
    if (!contact) {
      this.error.set('Check the customer first.');
      return;
    }
    const mode = this.status()?.invoiceMode ?? 'Draft';
    const question =
      mode === 'AuthoriseAndEmail'
        ? `Approve and email an invoice to ${contact.name}?`
        : `Create a draft invoice in Xero for ${contact.name}?`;
    if (!confirm(question)) return;

    this.busy.set('send');
    this.error.set(null);
    this.notice.set(null);
    try {
      const sent = await this.xero.sendInvoice(this.jobId(), contact.contactId);
      this.sentInvoice.set(sent);
      this.notice.set(`Invoice ${sent.invoiceNumber ?? ''} sent to Xero.`.replace('  ', ' '));
      await this.loadInvoiceState();
    } catch (err) {
      this.error.set(describeError(err, 'Could not send the invoice.'));
    } finally {
      this.busy.set(null);
    }
  }

  /** Delete or void, depending on the invoice's live status (btnDeleteInvoice_Click). */
  async deleteInvoice(): Promise<void> {
    if (!confirm('Delete or void this invoice in Xero? This cannot be undone.')) return;
    this.busy.set('delete');
    this.error.set(null);
    this.notice.set(null);
    try {
      const result = await this.xero.deleteInvoice(this.jobId());
      this.notice.set(result.message);
      this.selectedContact.set(null);
      await this.loadInvoiceState();
    } catch (err) {
      this.error.set(describeError(err, 'Could not delete the invoice.'));
    } finally {
      this.busy.set(null);
    }
  }

  async syncUnpaid(): Promise<void> {
    this.busy.set('sync');
    this.error.set(null);
    this.notice.set(null);
    try {
      const result = await this.xero.syncUnpaid();
      this.notice.set(
        `Checked ${result.synced} unpaid invoice(s) in Xero; ${result.paid} now paid.` +
          // A run is capped to stay inside Xero's rate limit, so say so rather than
          // letting it look as though everything was checked.
          (result.remaining > 0
            ? ` ${result.remaining} still to check — press Sync again to continue.`
            : '')
      );
      await this.loadInvoiceState();
      await this.refreshPaidStatus();
    } catch (err) {
      this.error.set(describeError(err, 'Could not sync unpaid invoices.'));
    } finally {
      this.busy.set(null);
    }
  }

  dismissMessages(): void {
    this.error.set(null);
    this.notice.set(null);
  }

  dismiss(): void {
    this.dismissed.emit();
  }

  /** Close on backdrop click, but not when the sheet itself is clicked. */
  onBackdrop(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.dismiss();
    }
  }
}

/** d/M/yy, matching the desktop's history line and paid-date formatting. */
function formatDay(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return iso;
  return `${date.getDate()}/${date.getMonth() + 1}/${String(date.getFullYear()).slice(2)}`;
}

function describeError(err: unknown, fallback: string): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 0) return 'Cannot reach the server.';
    const message = (err.error as { error?: string } | null)?.error;
    return message ?? fallback;
  }
  return fallback;
}
