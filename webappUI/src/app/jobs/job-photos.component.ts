import { HttpErrorResponse } from '@angular/common/http';
import {
  Component,
  DestroyRef,
  ElementRef,
  HostListener,
  computed,
  effect,
  inject,
  input,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import type { JobPhoto } from 'webapp-shared';
import { PhotoService } from '../core/photo.service';

/**
 * A photo plus the object URLs its two variants were loaded into. Thumbnail
 * and full are tracked separately and loaded independently: the inline strip
 * only ever needs the thumbnail, the viewer only ever needs the full version,
 * and having a thumbnail already loaded must never stop the full version from
 * being fetched when the viewer opens.
 */
interface LoadedPhoto extends JobPhoto {
  thumbnailUrl: string | null;
  thumbnailFailed: boolean;
  fullUrl: string | null;
  fullFailed: boolean;
}

/** How many thumbnails the inline strip shows before "Show all" takes over. */
const INLINE_THUMBNAILS = 2;

/** Minimum horizontal travel, in px, for a swipe to count. */
const SWIPE_THRESHOLD = 45;

/**
 * JPEG quality for webcam captures. High enough that the result is
 * indistinguishable from the camera's own output, without writing a needlessly
 * large file to the share for every shot.
 */
const CAPTURE_QUALITY = 0.92;

/**
 * Photos for a job, presented as a compact cell inside the job's main details
 * card: a couple of thumbnails, "Show all", "Add photo" and "Web photo".
 * Everything else happens in the full-screen viewer, which supports buttons,
 * swipes and the keyboard.
 *
 * The desktop app showed a fixed picture box in the form's top-right corner and
 * captured from a DirectShow webcam. Here the backend serves the same shared
 * drive, and there are two ways in: "Add photo" is a file input with
 * `capture="environment"`, which opens the camera on a phone and a file picker
 * on a desktop; "Web photo" drives an attached webcam through getUserMedia,
 * which is what the desk PCs need. Both end up in `uploadFiles`, so a capture
 * is stored, named, backed up and listed exactly like a picked file.
 */
@Component({
  selector: 'app-job-photos',
  standalone: true,
  templateUrl: './job-photos.component.html',
  styleUrl: './job-photos.component.scss',
})
export class JobPhotosComponent {
  private readonly photos = inject(PhotoService);
  private readonly destroyRef = inject(DestroyRef);

  readonly jobId = input.required<number>();

  readonly loaded = signal<LoadedPhoto[]>([]);
  readonly loading = signal(false);
  readonly uploading = signal(false);
  readonly available = signal(true);
  readonly unavailableReason = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly notice = signal<string | null>(null);

  /** Index of the photo open in the viewer, or null when it is closed. */
  readonly viewerIndex = signal<number | null>(null);

  // ---------- webcam capture ----------

  /** Only queried while the camera dialog is open, so this is a signal. */
  private readonly videoEl = viewChild<ElementRef<HTMLVideoElement>>('cameraVideo');

  readonly cameraOpen = signal(false);
  readonly cameraStarting = signal(false);
  readonly cameraError = signal<string | null>(null);
  /** The captured still, held until it is uploaded or retaken. */
  readonly capturedUrl = signal<string | null>(null);

  private readonly stream = signal<MediaStream | null>(null);
  private captured: Blob | null = null;

  /** Video inputs, listed only once permission is granted (labels need it). */
  readonly cameras = signal<MediaDeviceInfo[]>([]);
  readonly cameraId = signal<string | null>(null);

  readonly cameraLive = computed(() => this.stream() !== null && this.capturedUrl() === null);

  readonly count = computed(() => this.loaded().length);

  /** The thumbnails shown inline; the rest live behind "Show all". */
  readonly inlinePhotos = computed(() => this.loaded().slice(0, INLINE_THUMBNAILS));

  readonly hiddenCount = computed(() => Math.max(0, this.count() - INLINE_THUMBNAILS));

  readonly viewerPhoto = computed(() => {
    const index = this.viewerIndex();
    if (index == null) return null;
    return this.loaded()[index] ?? null;
  });

  readonly viewerPosition = computed(() => {
    const index = this.viewerIndex();
    return index == null ? '' : `${index + 1} of ${this.count()}`;
  });

  constructor() {
    // Reload whenever the job changes. `load` both reads and writes `loaded`, so
    // it must run untracked — otherwise the effect would retrigger itself every
    // time it stored a photo and loop forever.
    effect(() => {
      const jobId = this.jobId();
      untracked(() => {
        // Never leave a camera open across jobs: the capture would be filed
        // against whichever job is now on screen.
        this.closeCamera();
        void this.load(jobId);
      });
    });

    // The <video> only exists while the dialog is open, so the stream is
    // attached here rather than at the point getUserMedia resolves: this reruns
    // as soon as the element appears, whichever happens first.
    effect(() => {
      const element = this.videoEl()?.nativeElement;
      const stream = this.stream();
      if (!element || !stream || element.srcObject === stream) return;
      element.srcObject = stream;
      // Autoplay is already on the element; this covers browsers that need the
      // explicit nudge after a late srcObject assignment.
      void element.play().catch(() => undefined);
    });

    this.destroyRef.onDestroy(() => {
      this.releaseAll();
      this.stopStream();
      this.releaseCapture();
    });
  }

  private releaseAll(): void {
    for (const photo of this.loaded()) {
      this.photos.releaseObjectUrl(photo.thumbnailUrl);
      this.photos.releaseObjectUrl(photo.fullUrl);
    }
  }

  async load(jobId: number): Promise<void> {
    this.releaseAll();
    this.loaded.set([]);
    this.loading.set(true);
    this.error.set(null);
    try {
      const response = await this.photos.list(jobId);
      this.available.set(response.available);
      this.unavailableReason.set(response.reason ?? null);

      const loaded: LoadedPhoto[] = response.photos.map((p) => ({
        ...p,
        thumbnailUrl: null,
        thumbnailFailed: false,
        fullUrl: null,
        fullFailed: false,
      }));
      this.loaded.set(loaded);

      // Thumbnails are small, so fetch every still image's preview
      // concurrently — this is the whole performance point of having them,
      // versus the old approach of fetching full-resolution bytes up front.
      await Promise.all(
        loaded
          .filter((photo) => !photo.isVideo)
          .map(async (photo) => {
            try {
              const url = await this.photos.objectUrl(jobId, photo.name, 'thumbnail');
              if (this.jobId() !== jobId) {
                // The user moved to another job while this was in flight.
                this.photos.releaseObjectUrl(url);
                return;
              }
              this.loaded.update((list) =>
                list.map((p) => (p.name === photo.name ? { ...p, thumbnailUrl: url } : p))
              );
            } catch {
              this.loaded.update((list) =>
                list.map((p) => (p.name === photo.name ? { ...p, thumbnailFailed: true } : p))
              );
            }
          })
      );
    } catch (err) {
      this.error.set(describeError(err, 'Could not load the photos for this job.'));
      this.available.set(false);
    } finally {
      this.loading.set(false);
    }
  }

  /** Handle the camera / file picker selection. */
  async onFilesPicked(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    // Clear the input so picking the same file again still fires a change event.
    input.value = '';
    if (files.length === 0) return;
    await this.uploadFiles(files);
  }

  /**
   * Upload files and report what happened, shared by the file picker and the
   * webcam. Returns false if any upload failed, so the caller can keep its own
   * UI open rather than dismissing it over an error the user cannot see.
   */
  private async uploadFiles(files: File[]): Promise<boolean> {
    this.uploading.set(true);
    this.error.set(null);
    this.notice.set(null);
    let added = 0;
    let duplicates = 0;
    let backupMisses = 0;
    // Held rather than shown straight away: the reload below starts by clearing
    // `error`, so an upload failure set here would be wiped before it was read.
    let failure: string | null = null;

    try {
      for (const file of files) {
        try {
          const result = await this.photos.upload(this.jobId(), file);
          if (result.duplicateOf) {
            duplicates++;
          } else {
            added++;
            if (!result.backedUp) backupMisses++;
          }
        } catch (err) {
          failure = describeError(err, `Could not upload ${file.name}.`);
          break;
        }
      }
      if (added > 0 || duplicates > 0) {
        const parts: string[] = [];
        if (added > 0) parts.push(`${added} photo${added === 1 ? '' : 's'} added`);
        if (duplicates > 0) parts.push(`${duplicates} already on file`);
        let notice = parts.join(', ') + '.';
        // Stay silent when the backup succeeds — this is meant to be invisible
        // day to day. Only speak up when it didn't happen, since that's the one
        // case worth knowing about.
        if (backupMisses > 0) {
          notice += ` (${backupMisses === added ? 'not' : `${backupMisses} not`} backed up to MongoDB.)`;
        }
        this.notice.set(notice);
      }
      await this.load(this.jobId());
      // Takes precedence over anything the reload reported: a photo that did not
      // upload is the more useful thing to say.
      if (failure) this.error.set(failure);
    } finally {
      this.uploading.set(false);
    }
    return failure === null;
  }

  // ---------- webcam capture ----------

  /**
   * Open the camera dialog and start the preview. Anything that goes wrong is
   * reported inside the dialog: the commonest cause by far is the app being
   * reached over plain HTTP, which browsers block outright, and the fix ("use
   * Add photo", or put it behind HTTPS) is worth spelling out.
   */
  async openCamera(): Promise<void> {
    if (this.uploading()) return;
    this.dismissMessages();
    this.cameraOpen.set(true);
    this.cameraError.set(null);
    await this.startStream(this.cameraId());
  }

  private async startStream(deviceId: string | null): Promise<void> {
    this.stopStream();
    this.releaseCapture();
    this.cameraStarting.set(true);
    this.cameraError.set(null);
    try {
      if (!window.isSecureContext) {
        throw new Error(
          'The browser only allows camera access over an HTTPS address. Use "Add photo" instead, or see the HTTPS section of the setup notes.'
        );
      }
      if (!navigator.mediaDevices?.getUserMedia) {
        throw new Error('This browser cannot use the camera. Use "Add photo" instead.');
      }

      const stream = await navigator.mediaDevices.getUserMedia({
        // A specific camera once one has been chosen, otherwise whichever the
        // browser considers the default.
        video: deviceId ? { deviceId: { exact: deviceId } } : true,
        audio: false,
      });
      if (!this.cameraOpen()) {
        // Closed while the permission prompt was up.
        stopTracks(stream);
        return;
      }
      this.stream.set(stream);
      this.cameraId.set(stream.getVideoTracks()[0]?.getSettings().deviceId ?? deviceId);
      await this.listCameras();
    } catch (err) {
      this.cameraError.set(describeCameraError(err));
    } finally {
      this.cameraStarting.set(false);
    }
  }

  /** Labels are only populated once permission has been granted, so list late. */
  private async listCameras(): Promise<void> {
    try {
      const devices = await navigator.mediaDevices.enumerateDevices();
      this.cameras.set(devices.filter((device) => device.kind === 'videoinput'));
    } catch {
      this.cameras.set([]);
    }
  }

  cameraLabel(device: MediaDeviceInfo, index: number): string {
    return device.label || `Camera ${index + 1}`;
  }

  async onCameraChange(event: Event): Promise<void> {
    const deviceId = (event.target as HTMLSelectElement).value;
    await this.startStream(deviceId || null);
  }

  /** Freeze the current frame at the camera's own resolution. */
  async takePhoto(): Promise<void> {
    const video = this.videoEl()?.nativeElement;
    if (!video || !video.videoWidth || !video.videoHeight) {
      this.cameraError.set('The camera is not ready yet. Try again in a moment.');
      return;
    }

    const canvas = document.createElement('canvas');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    const context = canvas.getContext('2d');
    if (!context) {
      this.cameraError.set('This browser could not process the captured frame.');
      return;
    }
    context.drawImage(video, 0, 0, canvas.width, canvas.height);

    const blob = await new Promise<Blob | null>((resolve) =>
      canvas.toBlob(resolve, 'image/jpeg', CAPTURE_QUALITY)
    );
    if (!blob) {
      this.cameraError.set('The photo could not be captured. Try again.');
      return;
    }

    // The stream is deliberately left running behind the still, so "Retake" is
    // instant and does not re-trigger a permission prompt.
    this.releaseCapture();
    this.captured = blob;
    this.capturedUrl.set(URL.createObjectURL(blob));
    this.cameraError.set(null);
  }

  retake(): void {
    this.releaseCapture();
    this.cameraError.set(null);
  }

  /** Upload the frozen frame down the same path as a picked file. */
  async usePhoto(): Promise<void> {
    const blob = this.captured;
    if (!blob || this.uploading()) return;

    // The server generates the real filename from the job; this one only ever
    // appears in an error message.
    const file = new File([blob], 'webcam-photo.jpg', { type: 'image/jpeg' });
    const ok = await this.uploadFiles([file]);
    if (ok) {
      this.closeCamera();
      return;
    }
    // Keep the still so it can be retried, and move the message inside the
    // dialog, which is covering the field-level one.
    this.cameraError.set(this.error() ?? 'The photo could not be uploaded.');
    this.error.set(null);
  }

  closeCamera(): void {
    this.stopStream();
    this.releaseCapture();
    this.cameraOpen.set(false);
    this.cameraStarting.set(false);
    this.cameraError.set(null);
  }

  onCameraBackdrop(event: MouseEvent): void {
    if (event.target === event.currentTarget && !this.uploading()) {
      this.closeCamera();
    }
  }

  /** Releasing the tracks is what turns the camera's indicator light off. */
  private stopStream(): void {
    const stream = this.stream();
    if (stream) {
      stopTracks(stream);
      this.stream.set(null);
    }
  }

  private releaseCapture(): void {
    this.photos.releaseObjectUrl(this.capturedUrl());
    this.capturedUrl.set(null);
    this.captured = null;
  }

  // ---------- viewer ----------

  openViewer(index = 0): void {
    if (this.count() === 0) return;
    const clamped = Math.min(Math.max(index, 0), this.count() - 1);
    this.viewerIndex.set(clamped);
    void this.ensureViewerLoaded(clamped);
  }

  closeViewer(): void {
    this.viewerIndex.set(null);
  }

  /**
   * The full-resolution version is always fetched here, on demand, the first
   * time a given photo is opened in the viewer — regardless of whether its
   * thumbnail is already loaded. This is what makes "Show all" load the full
   * non-thumbnail version.
   */
  private async ensureViewerLoaded(index: number): Promise<void> {
    const photo = this.loaded()[index];
    if (!photo || photo.fullUrl || photo.fullFailed) return;
    const jobId = this.jobId();
    try {
      const url = await this.photos.objectUrl(jobId, photo.name, 'full');
      if (this.jobId() !== jobId) {
        this.photos.releaseObjectUrl(url);
        return;
      }
      this.loaded.update((list) =>
        list.map((p) => (p.name === photo.name ? { ...p, fullUrl: url } : p))
      );
    } catch {
      this.loaded.update((list) =>
        list.map((p) => (p.name === photo.name ? { ...p, fullFailed: true } : p))
      );
    }
  }

  /** Wraps around at both ends, so browsing never dead-ends. */
  showPrevious(): void {
    const index = this.viewerIndex();
    if (index == null || this.count() === 0) return;
    const next = index === 0 ? this.count() - 1 : index - 1;
    this.viewerIndex.set(next);
    void this.ensureViewerLoaded(next);
  }

  showNext(): void {
    const index = this.viewerIndex();
    if (index == null || this.count() === 0) return;
    const next = index >= this.count() - 1 ? 0 : index + 1;
    this.viewerIndex.set(next);
    void this.ensureViewerLoaded(next);
  }

  // ---------- swipe ----------

  private touchStartX = 0;
  private touchStartY = 0;
  private touchTracking = false;

  onTouchStart(event: TouchEvent): void {
    // Ignore multi-touch, which is a pinch-zoom rather than a swipe.
    if (event.touches.length !== 1) {
      this.touchTracking = false;
      return;
    }
    const touch = event.touches[0]!;
    this.touchStartX = touch.clientX;
    this.touchStartY = touch.clientY;
    this.touchTracking = true;
  }

  onTouchEnd(event: TouchEvent): void {
    if (!this.touchTracking) return;
    this.touchTracking = false;
    const touch = event.changedTouches[0];
    if (!touch) return;

    const dx = touch.clientX - this.touchStartX;
    const dy = touch.clientY - this.touchStartY;
    // Require a mostly-horizontal movement so vertical scrolls are not swipes.
    if (Math.abs(dx) < SWIPE_THRESHOLD || Math.abs(dx) <= Math.abs(dy)) return;

    if (dx < 0) {
      this.showNext(); // swiped left — advance
    } else {
      this.showPrevious(); // swiped right — go back
    }
  }

  @HostListener('document:keydown', ['$event'])
  onKeydown(event: KeyboardEvent): void {
    // The camera dialog sits above the viewer, so it gets Escape first.
    if (this.cameraOpen()) {
      if (event.key === 'Escape' && !this.uploading()) {
        this.closeCamera();
        event.preventDefault();
      }
      return;
    }
    if (this.viewerIndex() == null) return;
    switch (event.key) {
      case 'Escape':
        this.closeViewer();
        break;
      case 'ArrowLeft':
        this.showPrevious();
        break;
      case 'ArrowRight':
        this.showNext();
        break;
      default:
        return;
    }
    event.preventDefault();
  }

  /** Delete the photo open in the viewer, confirming first as the desktop does. */
  async deleteCurrent(): Promise<void> {
    const photo = this.viewerPhoto();
    if (!photo) return;
    if (!confirm(`Are you sure you wish to delete this photo?\n\n${photo.name}\n\nThis cannot be undone.`)) {
      return;
    }
    const wasIndex = this.viewerIndex() ?? 0;
    try {
      await this.photos.remove(this.jobId(), photo.name);
      await this.load(this.jobId());
      this.notice.set('Photo deleted.');
      // Stay in the viewer on the neighbouring photo, or close if that was the last.
      if (this.count() === 0) {
        this.closeViewer();
      } else {
        this.openViewer(Math.min(wasIndex, this.count() - 1));
      }
    } catch (err) {
      this.error.set(describeError(err, 'Could not delete the photo.'));
    }
  }

  onBackdrop(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.closeViewer();
    }
  }

  dismissMessages(): void {
    this.error.set(null);
    this.notice.set(null);
  }

  sizeLabel(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} kB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}

function stopTracks(stream: MediaStream): void {
  for (const track of stream.getTracks()) {
    track.stop();
  }
}

/**
 * Turn a getUserMedia rejection into something actionable. The DOMException
 * names are the browser's own, and each has a different fix.
 */
function describeCameraError(err: unknown): string {
  if (err instanceof DOMException) {
    switch (err.name) {
      case 'NotAllowedError':
      case 'SecurityError':
        return 'Camera access was blocked. Allow it for this site in the browser (the icon in the address bar), then try again.';
      case 'NotFoundError':
      case 'OverconstrainedError':
        return 'No camera was found on this machine.';
      case 'NotReadableError':
      case 'AbortError':
        return 'The camera could not be started — another program may be using it.';
      default:
        return `The camera could not be started (${err.name}).`;
    }
  }
  if (err instanceof Error && err.message) return err.message;
  return 'The camera could not be started.';
}

function describeError(err: unknown, fallback: string): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 0) return 'Cannot reach the server.';
    if (err.status === 413) return 'That file is too large to upload.';
    const message = (err.error as { error?: string } | null)?.error;
    return typeof message === 'string' ? message : fallback;
  }
  return fallback;
}
