import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import type { JobPhoto, JobPhotosResponse, PhotoStoreStatus } from 'webapp-shared';

/** Which size of a photo to fetch — see job-photos.component.ts for how each is used. */
export type PhotoVariant = 'thumbnail' | 'full';

/**
 * Job photo API calls.
 *
 * Photo files sit behind the session token, so an `<img src>` pointing at the
 * API would be rejected — the browser cannot attach an Authorization header to
 * an image request. Instead the bytes are fetched as a blob through HttpClient
 * (which the auth interceptor does cover) and turned into an object URL.
 * Callers must release those URLs with `releaseObjectUrl` when done.
 */
@Injectable({ providedIn: 'root' })
export class PhotoService {
  private readonly http = inject(HttpClient);

  status(): Promise<PhotoStoreStatus> {
    return firstValueFrom(this.http.get<PhotoStoreStatus>('/api/photos/status'));
  }

  list(jobId: number): Promise<JobPhotosResponse> {
    return firstValueFrom(this.http.get<JobPhotosResponse>(`/api/photos/jobs/${jobId}`));
  }

  /**
   * Fetch one photo and wrap it in an object URL for use as an img/video src.
   * `variant` defaults to the full-size image; pass 'thumbnail' for the fast
   * 250px-wide preview used in the inline photo strip.
   */
  async objectUrl(jobId: number, name: string, variant: PhotoVariant = 'full'): Promise<string> {
    const params = variant === 'thumbnail' ? new HttpParams().set('variant', 'thumbnail') : undefined;
    const blob = await firstValueFrom(
      this.http.get(`/api/photos/jobs/${jobId}/${encodeURIComponent(name)}`, {
        responseType: 'blob',
        ...(params ? { params } : {}),
      })
    );
    return URL.createObjectURL(blob);
  }

  releaseObjectUrl(url: string | null | undefined): void {
    if (url) {
      URL.revokeObjectURL(url);
    }
  }

  /**
   * Upload a photo as a raw binary body. The server names the file after the
   * job, matching what the desktop app writes.
   *
   * `backedUp` reports whether a compressed copy was also stored in MongoDB as
   * an independent backup of the network share (see webappNode/README.md) —
   * best-effort on the server, so `false` here does not mean the upload failed.
   */
  upload(
    jobId: number,
    file: File
  ): Promise<{ photo: JobPhoto; duplicateOf: string | null; backedUp: boolean }> {
    return firstValueFrom(
      this.http.post<{ photo: JobPhoto; duplicateOf: string | null; backedUp: boolean }>(
        `/api/photos/jobs/${jobId}`,
        file,
        { headers: { 'Content-Type': file.type || 'application/octet-stream' } }
      )
    );
  }

  remove(jobId: number, name: string): Promise<unknown> {
    return firstValueFrom(
      this.http.delete(`/api/photos/jobs/${jobId}/${encodeURIComponent(name)}`)
    );
  }
}
