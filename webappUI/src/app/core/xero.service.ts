import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import type {
  XeroConnectStartResponse,
  XeroContactsResponse,
  XeroDeleteInvoiceResponse,
  XeroInvoiceMode,
  XeroJobInvoiceResponse,
  XeroRefreshJobResponse,
  XeroSentInvoice,
  XeroStatusResponse,
  XeroSyncUnpaidResponse,
  XeroTenant,
} from 'webapp-shared';

/** All Xero API calls, replacing the desktop XeroManagementForm's direct Xero use. */
@Injectable({ providedIn: 'root' })
export class XeroService {
  private readonly http = inject(HttpClient);

  status(): Promise<XeroStatusResponse> {
    return firstValueFrom(this.http.get<XeroStatusResponse>('/api/xero/status'));
  }

  setMode(mode: XeroInvoiceMode): Promise<unknown> {
    return firstValueFrom(this.http.post('/api/xero/mode', { mode }));
  }

  connectStart(): Promise<XeroConnectStartResponse> {
    return firstValueFrom(
      this.http.post<XeroConnectStartResponse>('/api/xero/connect/start', {})
    );
  }

  /** Finish a connection by pasting the URL Xero redirected to. */
  connectComplete(redirectUrl: string): Promise<unknown> {
    return firstValueFrom(this.http.post('/api/xero/connect/complete', { redirectUrl }));
  }

  disconnect(): Promise<unknown> {
    return firstValueFrom(this.http.post('/api/xero/disconnect', {}));
  }

  tenants(): Promise<XeroTenant[]> {
    return firstValueFrom(
      this.http.get<{ tenants: XeroTenant[] }>('/api/xero/tenants')
    ).then((r) => r.tenants);
  }

  selectTenant(tenantId: string, tenantName: string): Promise<unknown> {
    return firstValueFrom(this.http.post('/api/xero/tenant', { tenantId, tenantName }));
  }

  contacts(businessName: string): Promise<XeroContactsResponse> {
    const params = new HttpParams().set('businessName', businessName);
    return firstValueFrom(
      this.http.get<XeroContactsResponse>('/api/xero/contacts', { params })
    );
  }

  jobInvoice(jobId: number): Promise<XeroJobInvoiceResponse> {
    return firstValueFrom(
      this.http.get<XeroJobInvoiceResponse>(`/api/xero/jobs/${jobId}/invoice`)
    );
  }

  sendInvoice(jobId: number, contactId: string): Promise<XeroSentInvoice> {
    return firstValueFrom(
      this.http.post<{ sentInvoice: XeroSentInvoice }>(`/api/xero/jobs/${jobId}/invoice`, {
        contactId,
      })
    ).then((r) => r.sentInvoice);
  }

  deleteInvoice(jobId: number): Promise<XeroDeleteInvoiceResponse> {
    return firstValueFrom(
      this.http.delete<XeroDeleteInvoiceResponse>(`/api/xero/jobs/${jobId}/invoice`)
    );
  }

  /** Poll Xero for this job's paid status (the desktop's 5-minute timer). */
  refreshJob(jobId: number): Promise<XeroRefreshJobResponse> {
    return firstValueFrom(
      this.http.post<XeroRefreshJobResponse>(`/api/xero/jobs/${jobId}/refresh`, {})
    );
  }

  syncUnpaid(): Promise<XeroSyncUnpaidResponse> {
    return firstValueFrom(
      this.http.post<XeroSyncUnpaidResponse>('/api/xero/sync-unpaid', {})
    );
  }
}
