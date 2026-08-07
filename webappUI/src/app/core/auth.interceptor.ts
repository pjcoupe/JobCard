import { HttpErrorResponse, type HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

/**
 * Attaches the session token to API calls and signs out if the server says the
 * session has expired — except on the login call itself, whose 401 is the
 * "access denied" answer the login screen displays.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const isLoginRequest = req.url.includes('/api/auth/login');
  const token = auth.token;

  const request =
    token && !isLoginRequest
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(request).pipe(
    catchError((error: unknown) => {
      if (!isLoginRequest && error instanceof HttpErrorResponse && error.status === 401) {
        // Carry the current page across, as authGuard does, so signing in again
        // lands back on the job that was open rather than the list.
        auth.logout(router.url);
      }
      return throwError(() => error);
    })
  );
};
