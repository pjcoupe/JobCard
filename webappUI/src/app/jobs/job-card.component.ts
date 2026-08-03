import { HttpErrorResponse } from '@angular/common/http';
import { CurrencyPipe } from '@angular/common';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import {
  COLLECT_TEXT,
  NUMBERED_LINE_COUNT,
  PAYMENT_BY_OPTIONS,
  RD_SURCHARGE,
  RECEIVED_FROM_OPTIONS,
  TAX_LABEL,
  addDays,
  addPlatingType,
  applyLine,
  calculateTotals,
  fromDateInputValue,
  lineHasData,
  lineTotalFor,
  round2,
  toDate,
  toDateInputValue,
  toLines,
  todayAtNoon,
  type JobCardDoc,
  type JobLine,
  type JobTypeOption,
  type XeroSentInvoice,
} from 'webapp-shared';
import { AuthService } from '../core/auth.service';
import { JobService } from '../core/job.service';
import { XeroService } from '../core/xero.service';
import { AppBarComponent } from '../shared/app-bar.component';
import { JobPhotosComponent } from './job-photos.component';
import { JobTypePickerComponent } from './job-type-picker.component';
import { XeroPanelComponent } from './xero-panel.component';

/**
 * How often to re-check Xero for the paid status of the open job, matching the
 * desktop's xeroSyncTimer interval (JobCard.cs, 300000ms).
 */
const XERO_POLL_MS = 300000;

/** A line paired with the UI state needed to render and edit it. */
interface EditableLine extends JobLine {
  /** Human label for fixed/computed rows (e.g. "Wheel Crack"). */
  label: string;
}

@Component({
  selector: 'app-job-card',
  standalone: true,
  imports: [
    CurrencyPipe,
    FormsModule,
    RouterLink,
    AppBarComponent,
    JobPhotosComponent,
    JobTypePickerComponent,
    XeroPanelComponent,
  ],
  templateUrl: './job-card.component.html',
  styleUrl: './job-card.component.scss',
})
export class JobCardComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly jobs = inject(JobService);
  private readonly xero = inject(XeroService);
  private readonly auth = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  /** Plating lines take several processes each; wheel lines take one. */
  readonly isPlating = computed(() => this.auth.database() === 'plating');

  readonly receivedFromOptions = RECEIVED_FROM_OPTIONS;
  readonly paymentByOptions = PAYMENT_BY_OPTIONS;
  readonly taxLabel = TAX_LABEL;

  readonly job = signal<JobCardDoc | null>(null);
  readonly lines = signal<EditableLine[]>([]);
  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly error = signal<string | null>(null);
  readonly notice = signal<string | null>(null);
  readonly isFussy = signal(false);
  readonly neighbours = signal<{ previous: number | null; next: number | null }>({
    previous: null,
    next: null,
  });

  /** Which line the job-type picker is filling, or null when it is closed. */
  readonly pickerForKey = signal<string | null>(null);

  /** True while the Xero sheet is open. */
  readonly xeroOpen = signal(false);

  /** Last invoice sent for this job, for the one-line summary on the card. */
  readonly xeroInvoice = signal<XeroSentInvoice | null>(null);

  /** Collapse empty numbered lines, as the desktop's Collapse/Expand does. */
  readonly collapsed = signal(true);

  /** Snapshot of the loaded document, used to detect unsaved changes. */
  private original: Record<string, unknown> = {};
  private readonly formVersion = signal(0);

  readonly totals = computed(() => {
    this.formVersion();
    return calculateTotals(this.lines());
  });

  readonly hasUnsavedChanges = computed(() => {
    this.formVersion();
    return Object.keys(this.pendingChanges()).length > 0;
  });

  /**
   * Lines shown in the items table.
   *
   * Normally: every line that holds data, plus exactly one blank row to fill in
   * next. Filling that row reveals the following one, so the list grows as work
   * is added instead of showing all 18 slots up front.
   *
   * "Show all lines" reveals the remaining blank numbered rows, for typing
   * straight into a particular row.
   *
   * The eleven fixed legacy rows (Repair, Strip, Plating, …) belong to the
   * plating app and are only ever shown when an old job actually has data in
   * them — never as empty rows to fill in.
   */
  readonly visibleLines = computed(() => {
    this.formVersion();
    const showAllNumbered = !this.collapsed();
    const kept: EditableLine[] = [];
    let blankNumberedShown = 0;

    for (const line of this.lines()) {
      if (line.kind === 'freight') continue;
      if (lineHasData(line)) {
        kept.push(line);
        continue;
      }
      if (line.kind !== 'numbered') continue;
      if (showAllNumbered || blankNumberedShown < 1) {
        kept.push(line);
        blankNumberedShown++;
      }
    }
    return kept;
  });

  readonly freightLine = computed(() => {
    this.formVersion();
    return this.lines().find((l) => l.kind === 'freight') ?? null;
  });

  readonly pickerLineLabel = computed(() => {
    const key = this.pickerForKey();
    if (!key) return '';
    const line = this.lines().find((l) => l.key === key);
    return line ? line.label : '';
  });

  readonly jobId = computed(() => this.job()?.jobID ?? null);

  /**
   * Xero only applies to business jobs, so the button follows the business name
   * exactly as the desktop's jobBusinessName_TextChanged gates btnXero.
   */
  readonly hasBusinessName = computed(() => {
    this.formVersion();
    return this.value('jobBusinessName').trim().length > 0;
  });

  /** One-line invoice summary for the Xero card, or null when nothing was sent. */
  readonly xeroSummary = computed(() => {
    const sent = this.xeroInvoice();
    if (!sent) return null;
    const parts = [`Invoice ${sent.invoiceNumber ?? '(no number)'}`];
    if (sent.status) parts.push(sent.status);
    if (sent.datePaidUtc) parts.push(`paid ${toDateInputValue(sent.datePaidUtc)}`);
    return parts.join(' · ');
  });

  readonly statusChips = computed(() => {
    this.formVersion();
    const job = this.job();
    if (!job) return [] as Array<{ label: string; kind: string }>;
    const chips: Array<{ label: string; kind: string }> = [];
    if (job['jobDatePaid']) chips.push({ label: 'Paid', kind: 'ok' });
    else if (job['jobDateCompleted']) chips.push({ label: 'Awaiting payment', kind: 'warn' });
    else chips.push({ label: 'In progress', kind: 'muted' });
    if (job['jobQuotation']) chips.push({ label: 'Quotation', kind: 'info' });
    if (job['jobGoodReserved']) chips.push({ label: 'Reserved', kind: 'info' });
    return chips;
  });

  constructor() {
    this.route.paramMap.subscribe((params) => {
      const raw = params.get('jobId');
      const jobId = Number(raw);
      if (!Number.isFinite(jobId)) {
        this.error.set('That job number is not valid.');
        this.loading.set(false);
        return;
      }
      void this.load(jobId);
    });

    // Poll Xero for the paid status of the open job, as the desktop's 5-minute
    // xeroSyncTimer does. The endpoint is a cheap no-op when this job has no
    // invoice or Xero is not connected, so it can run unconditionally.
    const timer = setInterval(() => {
      // A forgotten background tab should not keep calling Xero.
      if (document.visibilityState !== 'visible') return;
      void this.refreshXeroPaidStatus();
    }, XERO_POLL_MS);
    this.destroyRef.onDestroy(() => clearInterval(timer));
  }

  // ---------- loading ----------

  async load(jobId: number): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    this.notice.set(null);
    try {
      const job = await this.jobs.get(jobId);
      this.setJob(job);
      void this.refreshNeighbours(jobId);
      void this.refreshFussy();
      void this.refreshXeroSummary(jobId);
    } catch (err) {
      this.error.set(describeError(err, `Could not load job ${jobId}.`));
      this.job.set(null);
    } finally {
      this.loading.set(false);
    }
  }

  private setJob(job: JobCardDoc): void {
    this.job.set({ ...job });
    this.lines.set(toLines(job).map((line) => ({ ...line, label: labelFor(line) })));
    this.original = this.snapshot();
    this.bump();
  }

  private async refreshNeighbours(jobId: number): Promise<void> {
    try {
      this.neighbours.set(await this.jobs.neighbours(jobId));
    } catch {
      this.neighbours.set({ previous: null, next: null });
    }
  }

  private async refreshFussy(): Promise<void> {
    const job = this.job();
    if (!job) return;
    try {
      this.isFussy.set(
        await this.jobs.isFussyCustomer(
          String(job['jobPhone'] ?? ''),
          String(job['jobEmail'] ?? '')
        )
      );
    } catch {
      this.isFussy.set(false);
    }
  }

  // ---------- Xero ----------

  /**
   * Read the local invoice record for the summary line. Best-effort and silent:
   * Xero being unconfigured is the normal case for a non-business job and must
   * not put an error banner on the job card.
   */
  private async refreshXeroSummary(jobId: number): Promise<void> {
    try {
      const state = await this.xero.jobInvoice(jobId);
      // Guard against a slow response arriving after the user moved on.
      if (this.jobId() !== jobId) return;
      this.xeroInvoice.set(state.sentInvoice);
    } catch {
      this.xeroInvoice.set(null);
    }
  }

  /**
   * Ask Xero whether this job's invoice has been paid, and reflect it if so.
   * Ports JobCard.RefreshXeroPaidStatusAsync.
   */
  private async refreshXeroPaidStatus(): Promise<void> {
    const jobId = this.jobId();
    if (jobId == null) return;
    try {
      const result = await this.xero.refreshJob(jobId);
      if (this.jobId() !== jobId) return;
      if (result.sentInvoice) this.xeroInvoice.set(result.sentInvoice);
      if (result.paidDate) this.applyXeroPaidDate(result.paidDate);
    } catch {
      // A background poll failing is not worth interrupting the user over.
    }
  }

  openXero(): void {
    this.xeroOpen.set(true);
  }

  closeXero(): void {
    this.xeroOpen.set(false);
    const jobId = this.jobId();
    if (jobId != null) void this.refreshXeroSummary(jobId);
  }

  /**
   * Show a paid date that Xero reported. The server has already written it to the
   * job document, so `original` is updated alongside `job` — otherwise the page
   * would claim there are unsaved changes for something already saved.
   */
  applyXeroPaidDate(iso: string): void {
    const job = this.job();
    if (!job) return;
    const paid = toDate(iso);
    if (!paid) return;
    this.job.set({ ...job, jobDatePaid: paid, jobPaymentBy: 'Xero' });
    this.original = { ...this.original, jobDatePaid: paid, jobPaymentBy: 'Xero' };
    this.bump();
    this.notice.set('Xero reports this invoice as paid.');
  }

  // ---------- field editing ----------

  /** Read a scalar field for binding. */
  value(field: string): string {
    const raw = this.job()?.[field];
    return raw == null ? '' : String(raw);
  }

  /** Read a date field formatted for <input type="date">. */
  dateValue(field: string): string {
    return toDateInputValue(this.job()?.[field] as string | Date | null | undefined);
  }

  boolValue(field: string): boolean {
    return this.job()?.[field] === true;
  }

  setField(field: string, value: string | boolean | null): void {
    const job = this.job();
    if (!job) return;
    const next = { ...job };
    if (typeof value === 'string' && value.trim() === '') {
      next[field] = null;
    } else {
      next[field] = value;
    }
    this.job.set(next);
    this.bump();
  }

  setDateField(field: string, value: string): void {
    const job = this.job();
    if (!job) return;
    const next = { ...job };
    next[field] = fromDateInputValue(value);
    this.job.set(next);
    this.bump();
  }

  /** True when a field differs from its loaded value (drives the yellow tint). */
  isChanged(field: string): boolean {
    this.formVersion();
    const current = normalize(this.job()?.[field]);
    const before = normalize(this.original[field]);
    return current !== before;
  }

  // ---------- line editing ----------

  setLineField(
    key: string,
    field: 'detail' | 'type' | 'qty' | 'unitPrice' | 'price',
    value: string
  ): void {
    this.lines.update((lines) =>
      lines.map((line) => {
        if (line.key !== key) return line;
        const updated: EditableLine = { ...line };
        if (field === 'detail' || field === 'type') {
          updated[field] = value.trim() === '' ? null : value;
        } else {
          const parsed = value.trim() === '' ? null : Number(value);
          const numeric = parsed == null || !Number.isFinite(parsed) ? null : parsed;
          if (field === 'qty') {
            updated.qty = numeric == null ? null : Math.trunc(numeric);
          } else {
            updated[field] = numeric;
          }
          // Qty and unit price drive the line total, as they do on the desktop.
          if (field === 'qty' || field === 'unitPrice') {
            updated.price = lineTotalFor(updated.qty, updated.unitPrice);
          }
        }
        return updated;
      })
    );
    this.bump();
  }

  setLineChecked(key: string, checked: boolean): void {
    this.lines.update((lines) =>
      lines.map((line) => (line.key === key ? { ...line, checked } : line))
    );
    this.bump();
  }

  clearLine(key: string): void {
    this.lines.update((lines) =>
      lines.map((line) =>
        line.key === key
          ? { ...line, detail: null, type: null, qty: null, unitPrice: null, price: null, checked: null }
          : line
      )
    );
    this.bump();
  }

  // ---------- job type picker ----------

  openPicker(key: string): void {
    this.pickerForKey.set(key);
  }

  closePicker(): void {
    this.pickerForKey.set(null);
  }

  /**
   * Apply a picked job type, which the two businesses do differently — see
   * doCheckChange in JobTypePopup.cs.
   *
   * Wheel: one type per line. The group name becomes the detail, the caption
   * becomes the type, the quantity steps up by one and the line total is
   * quantity x unit price.
   *
   * Plating: one line holds the whole sequence of processes, so the pick is
   * added to the type field ("Strip, Polish, (2x)Nickle") and nothing else on
   * the line is touched — detail, quantity and price all describe the items
   * being plated, not the processes, and are filled in by hand.
   */
  applyPickedType(option: JobTypeOption): void {
    const key = this.pickerForKey();
    if (!key) return;
    const plating = this.isPlating();

    this.lines.update((lines) =>
      lines.map((line) => {
        if (line.key !== key) return line;
        if (plating) {
          return { ...line, type: addPlatingType(line.type, option.label) };
        }
        const sameType = line.type === option.label;
        const qty = sameType ? (line.qty ?? 0) + 1 : 1;
        return {
          ...line,
          detail: option.detail ?? line.detail,
          type: option.label,
          qty,
          unitPrice: option.price,
          price: lineTotalFor(qty, option.price),
        };
      })
    );
    // Wheel picks one thing and is done. Plating almost always adds several
    // processes at once, so the sheet stays open until it is dismissed.
    if (!plating) this.pickerForKey.set(null);
    this.bump();
  }

  /** Type field of the line the picker is filling, for its running summary. */
  readonly pickerLineTypes = computed(() => {
    this.formVersion();
    const key = this.pickerForKey();
    if (!key) return '';
    return this.lines().find((l) => l.key === key)?.type ?? '';
  });

  /** The picker's CLEAR button — empties the whole line, as ClearClicked does. */
  clearPickerLine(): void {
    const key = this.pickerForKey();
    if (key) this.clearLine(key);
  }

  /**
   * Open the picker on the first empty numbered line — the blank row already
   * shown at the end of the list. This deliberately leaves the collapse state
   * alone; expanding here would replace the single spare row with all 18 slots.
   */
  addWorkLine(): void {
    const lines = this.lines();
    for (let i = 0; i < NUMBERED_LINE_COUNT; i++) {
      const key = String(i).padStart(2, '0');
      const line = lines.find((l) => l.key === key);
      if (line && !lineHasData(line)) {
        this.openPicker(key);
        return;
      }
    }
    this.notice.set(`All ${NUMBERED_LINE_COUNT} work lines are in use.`);
  }

  // ---------- quick actions from the desktop form ----------

  setDeliveryCollect(): void {
    this.setField('jobDelivery', COLLECT_TEXT);
  }

  setDeliveryCourier(): void {
    const current = this.value('jobDelivery');
    this.setField('jobDelivery', current.startsWith('Courier to:') ? current : `Courier to:${current}`);
  }

  markCompletedToday(): void {
    this.setDateField('jobDateCompleted', toDateInputValue(todayAtNoon()));
  }

  markPaidToday(): void {
    this.setDateField('jobDatePaid', toDateInputValue(todayAtNoon()));
  }

  /** "+1 week" on the required date; hold Shift to step back a week. */
  addWeekToRequired(event: MouseEvent): void {
    const current = toDate(this.job()?.['jobDateRequired'] as string | null) ?? new Date();
    let next = addDays(current, event.shiftKey ? -7 : 7);
    if (next.getTime() < Date.now()) {
      next = new Date();
    }
    this.setDateField('jobDateRequired', toDateInputValue(next));
  }

  /** Add the rural-delivery freight surcharge (the desktop's "RD" button). */
  addRuralDeliverySurcharge(): void {
    const freight = this.freightLine();
    const current = freight?.price ?? 0;
    this.setLineField('Freight', 'price', String(round2(current + RD_SURCHARGE)));
  }

  async flagFussy(): Promise<void> {
    const job = this.job();
    if (!job) return;
    try {
      await this.jobs.flagFussyCustomer(
        String(job['jobPhone'] ?? ''),
        String(job['jobEmail'] ?? '')
      );
      this.isFussy.set(true);
      this.notice.set('Customer flagged for extra care.');
    } catch (err) {
      this.error.set(describeError(err, 'Could not flag this customer.'));
    }
  }

  // ---------- saving ----------

  /** Fields whose value differs from the loaded document. */
  private pendingChanges(): Record<string, unknown> {
    const job = this.job();
    if (!job) return {};

    const candidate: Record<string, unknown> = { ...this.snapshot() };
    const changes: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(candidate)) {
      if (normalize(value) !== normalize(this.original[key])) {
        changes[key] = value;
      }
    }
    return changes;
  }

  /** Flatten the current form state into document fields. */
  private snapshot(): Record<string, unknown> {
    const job = this.job();
    if (!job) return {};
    const doc: JobCardDoc = { ...job };
    for (const line of this.lines()) {
      applyLine(doc, line);
    }
    const flat: Record<string, unknown> = {};
    for (const [key, value] of Object.entries(doc)) {
      if (key === '_id') continue;
      flat[key] = value;
    }
    return flat;
  }

  async save(): Promise<void> {
    const job = this.job();
    if (!job || this.saving()) return;

    // The desktop treats customer name and job date as compulsory.
    if (!this.value('jobCustomer').trim()) {
      this.error.set('Customer name is required before saving.');
      return;
    }
    if (!this.dateValue('jobDate')) {
      this.error.set('Job date is required before saving.');
      return;
    }

    const changes = this.pendingChanges();
    if (Object.keys(changes).length === 0) {
      this.notice.set('No changes to save.');
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    this.notice.set(null);
    try {
      const saved = await this.jobs.save(job.jobID, changes);
      this.setJob(saved);
      this.notice.set('Job saved.');
      void this.refreshFussy();
    } catch (err) {
      this.error.set(describeError(err, 'Save failed.'));
    } finally {
      this.saving.set(false);
    }
  }

  /** Discard edits by reloading the stored document. */
  async revert(): Promise<void> {
    const jobId = this.jobId();
    if (jobId != null) {
      await this.load(jobId);
      this.notice.set('Changes discarded.');
    }
  }

  async duplicate(): Promise<void> {
    const jobId = this.jobId();
    if (jobId == null) return;
    if (this.hasUnsavedChanges() && !confirm('Discard unsaved changes and duplicate this job?')) {
      return;
    }
    try {
      const created = await this.jobs.duplicate(jobId);
      await this.router.navigate(['/jobs', created.jobID]);
    } catch (err) {
      this.error.set(describeError(err, 'Could not duplicate this job.'));
    }
  }

  async remove(): Promise<void> {
    const jobId = this.jobId();
    if (jobId == null) return;
    if (!confirm(`Delete job #${jobId}? This cannot be undone.`)) return;
    if (!confirm(`Really delete job #${jobId}? All of its details will be lost.`)) return;
    try {
      await this.jobs.remove(jobId);
      await this.router.navigate(['/jobs']);
    } catch (err) {
      this.error.set(describeError(err, 'Could not delete this job.'));
    }
  }

  async goTo(jobId: number | null): Promise<void> {
    if (jobId == null) return;
    if (this.hasUnsavedChanges() && !confirm('Discard unsaved changes and leave this job?')) {
      return;
    }
    await this.router.navigate(['/jobs', jobId]);
  }

  toggleCollapsed(): void {
    this.collapsed.update((v) => !v);
  }

  dismissNotice(): void {
    this.notice.set(null);
    this.error.set(null);
  }

  /** Force the computed signals to re-evaluate after a mutation. */
  private bump(): void {
    this.formVersion.update((v) => v + 1);
  }
}

/** Turn a line key into something a person can read. */
function labelFor(line: JobLine): string {
  if (line.kind === 'freight') return 'Freight';
  if (line.kind === 'numbered') return `Line ${Number(line.key) + 1}`;
  // Split camel case fixed row names, e.g. WheelCrack -> Wheel Crack.
  return line.key.replace(/([a-z])([A-Z])/g, '$1 $2');
}

/** Compare values loosely so 35 and "35" or null and "" are not false changes. */
function normalize(value: unknown): string {
  if (value == null || value === '') return '';
  if (value instanceof Date) return value.toISOString();
  if (typeof value === 'string') {
    const asDate = /^\d{4}-\d{2}-\d{2}T/.test(value) ? new Date(value) : null;
    if (asDate && !Number.isNaN(asDate.getTime())) return asDate.toISOString();
    return value;
  }
  return String(value);
}

function describeError(err: unknown, fallback: string): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 0) return 'Cannot reach the server.';
    const message = (err.error as { error?: string } | null)?.error;
    return message ?? fallback;
  }
  return fallback;
}
