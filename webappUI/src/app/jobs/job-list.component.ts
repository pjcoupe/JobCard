import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  JOB_LIST_VIEWS,
  SEARCH_FIELDS,
  formatShortDate,
  type JobListItem,
} from 'webapp-shared';
import { JobService } from '../core/job.service';
import { AppBarComponent } from '../shared/app-bar.component';

const PAGE_SIZE = 25;

@Component({
  selector: 'app-job-list',
  standalone: true,
  imports: [CurrencyPipe, FormsModule, RouterLink, AppBarComponent],
  templateUrl: './job-list.component.html',
  styleUrl: './job-list.component.scss',
})
export class JobListComponent implements OnInit {
  private readonly jobs = inject(JobService);
  private readonly router = inject(Router);

  readonly views = JOB_LIST_VIEWS;
  readonly searchFields = SEARCH_FIELDS;

  readonly view = signal<string>('incomplete');
  readonly searchField = signal<string>('');
  readonly query = signal<string>('');

  readonly items = signal<JobListItem[]>([]);
  readonly total = signal(0);
  readonly page = signal(0);
  readonly loading = signal(false);
  readonly creating = signal(false);
  readonly error = signal<string | null>(null);

  readonly formatDate = formatShortDate;

  ngOnInit(): void {
    void this.load();
  }

  async load(page = 0): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      const response = await this.jobs.list({
        view: this.view(),
        field: this.searchField(),
        q: this.query().trim(),
        page,
        pageSize: PAGE_SIZE,
      });
      this.items.set(response.items);
      this.total.set(response.total);
      this.page.set(response.page);
    } catch (err) {
      this.error.set(describeError(err, 'Could not load jobs.'));
      this.items.set([]);
      this.total.set(0);
    } finally {
      this.loading.set(false);
    }
  }

  selectView(view: string): void {
    this.view.set(view);
    void this.load(0);
  }

  search(): void {
    void this.load(0);
  }

  clearSearch(): void {
    this.query.set('');
    this.searchField.set('');
    void this.load(0);
  }

  nextPage(): void {
    if ((this.page() + 1) * PAGE_SIZE < this.total()) {
      void this.load(this.page() + 1);
    }
  }

  previousPage(): void {
    if (this.page() > 0) {
      void this.load(this.page() - 1);
    }
  }

  /** Create a blank job and open it, as the desktop New Job button does. */
  async newJob(): Promise<void> {
    if (this.creating()) return;
    this.creating.set(true);
    this.error.set(null);
    try {
      const job = await this.jobs.create();
      await this.router.navigate(['/jobs', job.jobID]);
    } catch (err) {
      this.error.set(describeError(err, 'Could not create a new job.'));
    } finally {
      this.creating.set(false);
    }
  }

  /** Status shown per row, derived the same way the desktop colours a card. */
  statusOf(item: JobListItem): { label: string; kind: string } {
    if (item.jobDatePaid) return { label: 'Paid', kind: 'ok' };
    if (item.jobDateCompleted) return { label: 'Awaiting payment', kind: 'warn' };
    if (item.jobQuotation) return { label: 'Quotation', kind: 'info' };
    if (item.jobGoodReserved) return { label: 'Reserved', kind: 'info' };
    return { label: 'In progress', kind: 'muted' };
  }

  get rangeLabel(): string {
    if (this.total() === 0) return 'No jobs';
    const from = this.page() * PAGE_SIZE + 1;
    const to = Math.min(from + this.items().length - 1, this.total());
    return `${from}–${to} of ${this.total()}`;
  }

  get canGoNext(): boolean {
    return (this.page() + 1) * PAGE_SIZE < this.total();
  }

  get canGoPrevious(): boolean {
    return this.page() > 0;
  }
}

function describeError(err: unknown, fallback: string): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 0) return 'Cannot reach the server.';
    const message = (err.error as { error?: string } | null)?.error;
    return message ?? fallback;
  }
  return fallback;
}
