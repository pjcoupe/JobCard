import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly username = signal('');
  readonly password = signal('');
  readonly busy = signal(false);
  readonly error = signal<string | null>(null);
  readonly showPassword = signal(false);

  async submit(): Promise<void> {
    if (this.busy()) return;
    this.error.set(null);

    const username = this.username().trim();
    const password = this.password();
    if (!username || !password) {
      this.error.set('Enter your username and password.');
      return;
    }

    this.busy.set(true);
    try {
      await this.auth.login(username, password);
      // Correct credentials go straight through to the job list.
      const returnTo = new URLSearchParams(window.location.search).get('returnTo');
      await this.router.navigateByUrl(returnTo && returnTo.startsWith('/') ? returnTo : '/jobs');
    } catch (err) {
      this.error.set(messageFor(err));
      this.password.set('');
    } finally {
      this.busy.set(false);
    }
  }

  toggleShowPassword(): void {
    this.showPassword.update((v) => !v);
  }
}

function messageFor(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 0) {
      return 'Cannot reach the server. Check that the API is running.';
    }
    const serverMessage = (err.error as { error?: string } | null)?.error;
    return serverMessage ?? 'Access denied';
  }
  return 'Access denied';
}
