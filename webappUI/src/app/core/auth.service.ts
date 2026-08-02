import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import type { LoginResponse } from 'webapp-shared';

const TOKEN_KEY = 'wheel.jobcard.token';
const USER_KEY = 'wheel.jobcard.user';

/**
 * Holds the single operator session. The token is a signed value issued by
 * webappNode; the backend is the only thing that checks credentials.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly tokenSignal = signal<string | null>(readStored(TOKEN_KEY));
  private readonly usernameSignal = signal<string | null>(readStored(USER_KEY));

  readonly username = computed(() => this.usernameSignal());
  readonly isSignedIn = computed(() => this.tokenSignal() !== null);

  get token(): string | null {
    return this.tokenSignal();
  }

  /**
   * Sign in. Resolves on success; rejects with the server's message (the
   * backend deliberately waits ~3s before denying a wrong password).
   */
  async login(username: string, password: string): Promise<void> {
    const response = await firstValueFrom(
      this.http.post<LoginResponse>('/api/auth/login', { username, password })
    );
    this.tokenSignal.set(response.token);
    this.usernameSignal.set(response.username);
    writeStored(TOKEN_KEY, response.token);
    writeStored(USER_KEY, response.username);
  }

  /** Clear the session and return to the login screen. */
  logout(): void {
    this.tokenSignal.set(null);
    this.usernameSignal.set(null);
    clearStored(TOKEN_KEY);
    clearStored(USER_KEY);
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
