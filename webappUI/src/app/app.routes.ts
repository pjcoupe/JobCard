import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [guestGuard],
    title: 'Sign in — Job Cards',
    loadComponent: () => import('./login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'jobs',
    canActivate: [authGuard],
    title: 'Jobs — Job Cards',
    loadComponent: () => import('./jobs/job-list.component').then((m) => m.JobListComponent),
  },
  {
    path: 'jobs/:jobId',
    canActivate: [authGuard],
    title: 'Job card — Job Cards',
    loadComponent: () => import('./jobs/job-card.component').then((m) => m.JobCardComponent),
  },
  {
    path: 'jobs/:jobId/print',
    canActivate: [authGuard],
    title: 'Print — Job Cards',
    loadComponent: () => import('./jobs/job-print.component').then((m) => m.JobPrintComponent),
  },
  { path: '', pathMatch: 'full', redirectTo: 'jobs' },
  { path: '**', redirectTo: 'jobs' },
];
