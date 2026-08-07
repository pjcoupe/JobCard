import { CurrencyPipe } from '@angular/common';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  BANK_ACCOUNT_NAME,
  BANK_ACCOUNT_NUMBER,
  BUSINESS_NAME,
  GST_NUMBER,
  PRINT_DISCLAIMER,
  TAX_LABEL,
  calculateTotals,
  formatShortDate,
  lineHasData,
  toLines,
  type JobCardDoc,
  type JobLine,
} from 'webapp-shared';
import { JobService } from '../core/job.service';
import { PhotoService } from '../core/photo.service';

/**
 * Printable customer copy, mirroring JobCard.ShowPrintForm(): heading and tax
 * status, customer block, delivery, itemised details, totals, notes, then the
 * disclaimer and payment footer. Printing uses the browser's print dialog
 * instead of the desktop app's RichTextBox printer.
 */
@Component({
  selector: 'app-job-print',
  standalone: true,
  imports: [CurrencyPipe, RouterLink],
  templateUrl: './job-print.component.html',
  styleUrl: './job-print.component.scss',
})
export class JobPrintComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly jobs = inject(JobService);
  private readonly photos = inject(PhotoService);
  private readonly destroyRef = inject(DestroyRef);

  readonly businessName = BUSINESS_NAME;
  readonly gstNumber = GST_NUMBER;
  readonly taxLabel = TAX_LABEL;
  readonly disclaimer = PRINT_DISCLAIMER;
  readonly bankAccountName = BANK_ACCOUNT_NAME;
  readonly bankAccountNumber = BANK_ACCOUNT_NUMBER;
  readonly formatDate = formatShortDate;

  readonly job = signal<JobCardDoc | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  /** Business copy omits the customer-facing payment footer. */
  readonly copyType = signal<'customer' | 'business'>('customer');

  /** Set when public/business-logo.jpg can't be loaded; falls back to the name. */
  readonly logoFailed = signal(false);

  /**
   * First photo of the job, embedded at the top of the workshop copy — the
   * desktop app pastes `pictureBox1.Image` there when printing its own copy.
   */
  readonly photoUrl = signal<string | null>(null);

  readonly lines = computed<JobLine[]>(() => {
    const job = this.job();
    if (!job) return [];
    return toLines(job).filter((line) => line.kind !== 'freight' && lineHasData(line));
  });

  readonly freight = computed(() => this.job()?.jobFreight ?? null);

  readonly totals = computed(() => {
    const job = this.job();
    return calculateTotals(job ? toLines(job) : []);
  });

  /** A completed job prints as a tax invoice; otherwise as a quotation. */
  readonly isCompleted = computed(() => !!this.job()?.['jobDateCompleted']);
  readonly isPaid = computed(() => !!this.job()?.['jobDatePaid']);

  readonly heading = computed(() =>
    this.isCompleted() ? `Tax Invoice  ${this.taxLabel} ${this.gstNumber}` : 'Quotation / Job Card'
  );

  readonly footerNote = computed(() =>
    this.isCompleted() ? '** TAX INVOICE **' : 'Pricing above is an estimate only'
  );

  constructor() {
    this.route.paramMap.subscribe((params) => {
      const jobId = Number(params.get('jobId'));
      if (!Number.isFinite(jobId)) {
        this.error.set('That job number is not valid.');
        this.loading.set(false);
        return;
      }
      void this.load(jobId);
    });

    this.destroyRef.onDestroy(() => this.photos.releaseObjectUrl(this.photoUrl()));
  }

  async load(jobId: number): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      this.job.set(await this.jobs.get(jobId));
      void this.loadFirstPhoto(jobId);
    } catch {
      this.error.set(`Could not load job ${jobId}.`);
    } finally {
      this.loading.set(false);
    }
  }

  /** Fetch the job's first still image, if the photo share has one. */
  private async loadFirstPhoto(jobId: number): Promise<void> {
    this.photos.releaseObjectUrl(this.photoUrl());
    this.photoUrl.set(null);
    try {
      const response = await this.photos.list(jobId);
      const first = response.photos.find((p) => !p.isVideo);
      if (!first) return;
      // Explicit 'full' — print quality matters here, and it's one photo, not
      // a grid, so there's no bulk-loading cost to avoid.
      const url = await this.photos.objectUrl(jobId, first.name, 'full');
      this.photoUrl.set(url);
    } catch {
      // Photos are optional on the printout; carry on without one.
    }
  }

  value(field: string): string {
    const raw = this.job()?.[field];
    return raw == null ? '' : String(raw);
  }

  setCopyType(type: 'customer' | 'business'): void {
    this.copyType.set(type);
  }

  onLogoError(): void {
    this.logoFailed.set(true);
  }

  print(): void {
    window.print();
  }
}
