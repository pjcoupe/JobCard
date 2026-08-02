import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AuthService } from './auth.service';

/** Keeps unauthenticated visitors on the login screen. */
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isSignedIn()) {
    return true;
  }
  return router.createUrlTree(['/login'], {
    queryParams: state.url && state.url !== '/' ? { returnTo: state.url } : {},
  });
};

/** Sends an already-signed-in operator straight to the job list. */
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isSignedIn() ? router.createUrlTree(['/jobs']) : true;
};
