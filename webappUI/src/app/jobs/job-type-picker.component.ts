import { CurrencyPipe } from '@angular/common';
import { Component, computed, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import type { JobTypeOption } from 'webapp-shared';
import { JobService } from '../core/job.service';

/**
 * Replaces the desktop JobTypePopup for wheel mode: a sheet of priced job types
 * grouped exactly as the popup's group boxes were. Picking one fills the line's
 * detail (group name), type (button caption) and unit price, which is the same
 * assignment doCheckChange() performs.
 *
 * Prices and captions come from the wheel.pricing collection and can be edited
 * here, mirroring the desktop's Ctrl-click override.
 */
@Component({
  selector: 'app-job-type-picker',
  standalone: true,
  imports: [CurrencyPipe, FormsModule],
  templateUrl: './job-type-picker.component.html',
  styleUrl: './job-type-picker.component.scss',
})
export class JobTypePickerComponent {
  private readonly jobs = inject(JobService);

  /** Human label for the line being filled, shown in the sheet header. */
  readonly lineLabel = input<string>('');

  readonly picked = output<JobTypeOption>();
  readonly dismissed = output<void>();

  readonly groups = signal<Array<{ detail: string; options: JobTypeOption[] }>>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly filter = signal('');

  /** The option whose price is being edited inline, if any. */
  readonly editing = signal<JobTypeOption | null>(null);
  readonly editPrice = signal<string>('');
  readonly editLabel = signal<string>('');
  readonly saving = signal(false);

  readonly filteredGroups = computed(() => {
    const needle = this.filter().trim().toLowerCase();
    if (!needle) return this.groups();
    return this.groups()
      .map((g) => ({
        detail: g.detail,
        options: g.options.filter(
          (o) =>
            o.label.toLowerCase().includes(needle) || g.detail.toLowerCase().includes(needle)
        ),
      }))
      .filter((g) => g.options.length > 0);
  });

  constructor() {
    void this.load();
  }

  async load(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      const response = await this.jobs.jobTypes();
      this.groups.set(response.groups);
    } catch {
      this.error.set('Could not load the job type prices.');
    } finally {
      this.loading.set(false);
    }
  }

  choose(option: JobTypeOption): void {
    if (this.editing()) return;
    this.picked.emit(option);
  }

  startEdit(option: JobTypeOption, event: Event): void {
    event.stopPropagation();
    this.editing.set(option);
    this.editPrice.set(String(option.price));
    this.editLabel.set(option.label);
  }

  cancelEdit(): void {
    this.editing.set(null);
    this.saving.set(false);
  }

  async saveEdit(): Promise<void> {
    const option = this.editing();
    if (!option || this.saving()) return;

    const price = Number(this.editPrice());
    if (!Number.isFinite(price) || price < 0) {
      this.error.set('Enter a price of 0 or more.');
      return;
    }

    this.saving.set(true);
    this.error.set(null);
    try {
      await this.jobs.updateJobTypePrice(option.controlName, {
        price,
        label: this.editLabel().trim() || option.label,
      });
      await this.load();
      this.editing.set(null);
    } catch {
      this.error.set('Could not save the new price.');
    } finally {
      this.saving.set(false);
    }
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
