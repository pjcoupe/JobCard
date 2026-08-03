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

  private readonly tokenSignal = signal<string | null>(readStored(TOKEN_KEY));
  private readonly usernameSignal = signal<string | null>(readStored(USER_KEY));
  private readonly databaseSignal = signal<JobDatabase>(readDatabase());

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
  }

  /** Clear the session and return to the login screen. */
  logout(): void {
    this.tokenSignal.set(null);
    this.usernameSignal.set(null);
    clearStored(TOKEN_KEY);
    clearStored(USER_KEY);
    // The database choice deliberately survives: a workshop signs in to the same
    // business every day, so it stays as the pre-selected option next time.
    void this.router.navigate(['/login']);
  }
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
