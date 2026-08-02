import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import type {
  JobCardDoc,
  JobListResponse,
  JobTypeCatalogueResponse,
} from 'webapp-shared';

/** All job-card API calls. */
@Injectable({ providedIn: 'root' })
export class JobService {
  private readonly http = inject(HttpClient);

  list(options: {
    view?: string;
    field?: string;
    q?: string;
    page?: number;
    pageSize?: number;
  }): Promise<JobListResponse> {
    let params = new HttpParams();
    if (options.view) params = params.set('view', options.view);
    if (options.field) params = params.set('field', options.field);
    if (options.q) params = params.set('q', options.q);
    if (options.page != null) params = params.set('page', options.page);
    if (options.pageSize != null) params = params.set('pageSize', options.pageSize);
    return firstValueFrom(this.http.get<JobListResponse>('/api/jobs', { params }));
  }

  get(jobId: number): Promise<JobCardDoc> {
    return firstValueFrom(
      this.http.get<{ job: JobCardDoc }>(`/api/jobs/${jobId}`)
    ).then((r) => r.job);
  }

  latest(): Promise<JobCardDoc> {
    return firstValueFrom(this.http.get<{ job: JobCardDoc }>('/api/jobs/latest')).then(
      (r) => r.job
    );
  }

  neighbours(jobId: number): Promise<{ previous: number | null; next: number | null }> {
    return firstValueFrom(
      this.http.get<{ previous: number | null; next: number | null }>(
        `/api/jobs/${jobId}/neighbours`
      )
    );
  }

  create(): Promise<JobCardDoc> {
    return firstValueFrom(this.http.post<{ job: JobCardDoc }>('/api/jobs', {})).then(
      (r) => r.job
    );
  }

  duplicate(jobId: number): Promise<JobCardDoc> {
    return firstValueFrom(
      this.http.post<{ job: JobCardDoc }>(`/api/jobs/${jobId}/duplicate`, {})
    ).then((r) => r.job);
  }

  save(jobId: number, changes: Record<string, unknown>): Promise<JobCardDoc> {
    return firstValueFrom(
      this.http.put<{ job: JobCardDoc }>(`/api/jobs/${jobId}`, changes)
    ).then((r) => r.job);
  }

  remove(jobId: number): Promise<void> {
    return firstValueFrom(this.http.delete(`/api/jobs/${jobId}`)).then(() => undefined);
  }

  jobTypes(): Promise<JobTypeCatalogueResponse> {
    return firstValueFrom(this.http.get<JobTypeCatalogueResponse>('/api/job-types'));
  }

  updateJobTypePrice(
    controlName: string,
    changes: { price?: number | string; label?: string }
  ): Promise<unknown> {
    return firstValueFrom(this.http.put(`/api/job-types/${controlName}`, changes));
  }

  isFussyCustomer(phone: string, email: string): Promise<boolean> {
    const params = new HttpParams().set('phone', phone ?? '').set('email', email ?? '');
    return firstValueFrom(
      this.http.get<{ isFussy: boolean }>('/api/customers/fussy', { params })
    ).then((r) => r.isFussy);
  }

  flagFussyCustomer(phone: string, email: string): Promise<unknown> {
    return firstValueFrom(this.http.post('/api/customers/fussy', { phone, email }));
  }
}
