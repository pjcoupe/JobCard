import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import {
  DEFAULT_JOB_DATABASE,
  isJobDatabase,
  jobDatabaseLabel,
  type JobDatabase,
  type LoginResponse,
} from 'webapp-shared';

const TOKEN_KEY = 'wheel.jobcard.token';
const USER_KEY = 'wheel.jobcard.user';
const DATABASE_KEY = 'wheel.jobcard.database';

/**
 * Treat a token as finished slightly before its stated expiry, so a request is
 * never sent in the moment between "still valid here" and the server refusing it.
 */
const EXPIRY_SKEW_MS = 30_000;

/** setTimeout fires immediately for delays past this, rather than far later. */
const MAX_TIMEOUT_MS = 2_147_483_647;

/**
 * Holds the single operator session. The token is a signed value issued by
 * webappNode; the backend is the only thing that checks credentials.
 *
 * A session also belongs to one business — wheel or plating — chosen on the
 * sign-in screen. The server binds that choice into the token, so what is kept
 * here is only for display and for defaulting the next sign-in; nothing the
 * browser stores can move a request to the other database.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly tokenSignal = signal<string | null>(null);
  private readonly usernameSignal = signal<string | null>(null);
  private readonly databaseSignal = signal<JobDatabase>(readDatabase());

  private expiryTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    // An expired token is not a session. Without this the guard would let the
    // job list render on yesterday's token and only the first failed API call
    // would send the operator to sign in.
    const token = readLiveToken();
    if (token !== null) {
      this.tokenSignal.set(token);
      this.usernameSignal.set(readStored(USER_KEY));
    }
    this.scheduleExpiryLogout();
  }

  readonly username = computed(() => this.usernameSignal());
  readonly isSignedIn = computed(() => this.tokenSignal() !== null);

  /** Which business this session is working in. */
  readonly database = computed(() => this.databaseSignal());
  readonly databaseLabel = computed(() => jobDatabaseLabel(this.databaseSignal()));

  get token(): string | null {
    return this.tokenSignal();
  }

  /**
   * Sign in. Resolves on success; rejects with the server's message (the
   * backend deliberately waits ~3s before denying a wrong password).
   */
  async login(username: string, password: string, database: JobDatabase): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<LoginResponse>('/api/auth/login', { username, password, database })
    );
    this.tokenSignal.set(response.token);
    this.usernameSignal.set(response.username);
    // The server's answer wins over what was asked for: the token is bound to
    // that database, so it is the truth about what this session can see.
    this.databaseSignal.set(response.database);
    writeStored(TOKEN_KEY, response.token);
    writeStored(USER_KEY, response.username);
    writeStored(DATABASE_KEY, response.database);
    this.scheduleExpiryLogout();
  }

  /**
   * Clear the session and return to the login screen, optionally remembering
   * where the operator was so signing in again carries on from there.
   */
  logout(returnTo?: string): void {
    if (this.expiryTimer !== null) {
      clearTimeout(this.expiryTimer);
      this.expiryTimer = null;
    }
    this.tokenSignal.set(null);
    this.usernameSignal.set(null);
    clearStored(TOKEN_KEY);
    clearStored(USER_KEY);
    // The database choice deliberately survives: a workshop signs in to the same
    // business every day, so it stays as the pre-selected option next time.
    const keepPlace = returnTo && returnTo.startsWith('/') && returnTo !== '/';
    void this.router.navigate(['/login'], keepPlace ? { queryParams: { returnTo } } : {});
  }

  /**
   * Sign out at the moment the token dies, rather than waiting for a request to
   * be refused. A tablet left open on the job list overnight is then showing the
   * sign-in screen in the morning, not a stale list that errors when touched.
   */
  private scheduleExpiryLogout(): void {
    if (this.expiryTimer !== null) {
      clearTimeout(this.expiryTimer);
      this.expiryTimer = null;
    }
    const token = this.tokenSignal();
    const expiry = token === null ? null : tokenExpiry(token);
    if (expiry === null) return;

    const delay = expiry - EXPIRY_SKEW_MS - Date.now();
    // A delay past the timer ceiling would fire straight away and sign the
    // operator out on arrival; the 401 interceptor covers that case instead.
    if (delay > MAX_TIMEOUT_MS) return;
    this.expiryTimer = setTimeout(() => this.logout(this.router.url), Math.max(0, delay));
  }
}

/**
 * The `exp` the server signed into the token, or null if it is absent or the
 * token is malformed. Read for timing only — webappNode verifies the signature,
 * so nothing here is trusted as proof of a session.
 */
function tokenExpiry(token: string): number | null {
  const dot = token.lastIndexOf('.');
  if (dot <= 0) return null;
  try {
    const base64 = token.slice(0, dot).replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
    const payload = JSON.parse(atob(padded)) as { exp?: unknown };
    return typeof payload.exp === 'number' ? payload.exp : null;
  } catch {
    return null;
  }
}

/**
 * The stored token if it is still good, otherwise null — clearing it out, so a
 * dead token cannot sit in storage looking like a session.
 */
function readLiveToken(): string | null {
  const token = readStored(TOKEN_KEY);
  if (token === null) return null;
  const expiry = tokenExpiry(token);
  if (expiry === null || expiry - EXPIRY_SKEW_MS <= Date.now()) {
    clearStored(TOKEN_KEY);
    clearStored(USER_KEY);
    return null;
  }
  return token;
}

function readStored(key: string): string | null {
  try {
    return localStorage.getItem(key);
  } catch {
    // Private browsing can block storage; the session then lasts one page load.
    return null;
  }
}

/** The remembered business, falling back to the default if never set or invalid. */
function readDatabase(): JobDatabase {
  const stored = readStored(DATABASE_KEY);
  return isJobDatabase(stored) ? stored : DEFAULT_JOB_DATABASE;
}

function writeStored(key: string, value: string): void {
  try {
    localStorage.setItem(key, value);
  } catch {
    /* not fatal — session simply will not survive a reload */
  }
}

function clearStored(key: string): void {
  try {
    localStorage.removeItem(key);
  } catch {
    /* ignore */
  }
}
