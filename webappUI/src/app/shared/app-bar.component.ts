import { Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../core/auth.service';

/**
 * Sticky top bar shared by the job list and job card screens. On a phone the
 * title truncates and the actions collapse to icons rather than wrapping into
 * a second row.
 */
@Component({
  selector: 'app-bar',
  standalone: true,
  imports: [RouterLink],
  template: `
    <header class="bar">
      @if (backTo()) {
        <a class="back" [routerLink]="backTo()" aria-label="Back to job list">
          <span aria-hidden="true">‹</span>
        </a>
      }
      <div class="titles">
        <span class="title-line">
          <span class="title">{{ title() }}</span>
          <!-- Which business this session is in. Always visible: the two look
               identical otherwise, and a job number exists in both. -->
          <span class="db-chip">{{ auth.databaseLabel() }}</span>
        </span>
        @if (subtitle()) {
          <span class="subtitle">{{ subtitle() }}</span>
        }
      </div>
      <div class="actions">
        <ng-content />
        <button type="button" class="sign-out" (click)="auth.logout()" title="Sign out">
          <span class="label">Sign out</span>
          <span class="icon" aria-hidden="true">⏻</span>
        </button>
      </div>
    </header>
  `,
  styles: `
    .bar {
      position: sticky;
      top: 0;
      z-index: 20;
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem 0.75rem;
      padding-top: max(0.5rem, env(safe-area-inset-top));
      background: var(--accent);
      color: #fff;
      box-shadow: 0 1px 6px rgba(10, 20, 40, 0.25);
    }

    .back {
      flex: none;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 38px;
      height: 38px;
      border-radius: 8px;
      color: #fff;
      text-decoration: none;
      font-size: 1.6rem;
      line-height: 1;
      background: rgba(255, 255, 255, 0.14);
    }

    .titles {
      flex: 1 1 auto;
      min-width: 0;
      display: flex;
      flex-direction: column;
      line-height: 1.2;
    }

    .title-line {
      display: flex;
      align-items: center;
      gap: 0.4rem;
      min-width: 0;
    }

    .title {
      font-weight: 700;
      font-size: 1.02rem;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .db-chip {
      flex: none;
      padding: 0.05rem 0.4rem;
      border-radius: 999px;
      background: rgba(255, 255, 255, 0.2);
      border: 1px solid rgba(255, 255, 255, 0.3);
      font-size: 0.68rem;
      font-weight: 700;
      letter-spacing: 0.04em;
      text-transform: uppercase;
      white-space: nowrap;
    }

    .subtitle {
      font-size: 0.78rem;
      opacity: 0.85;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .actions {
      flex: none;
      display: flex;
      align-items: center;
      gap: 0.4rem;
    }

    .actions ::ng-deep button,
    .sign-out {
      min-height: 38px;
      padding: 0.3rem 0.7rem;
      background: rgba(255, 255, 255, 0.16);
      border: 1px solid rgba(255, 255, 255, 0.28);
      color: #fff;
      font-size: 0.88rem;
    }

    .actions ::ng-deep button:hover:not(:disabled),
    .sign-out:hover {
      background: rgba(255, 255, 255, 0.26);
    }

    .icon {
      display: none;
    }

    @media (max-width: 620px) {
      /* Collapse the sign-out button to its icon to keep the bar one row. */
      .sign-out .label {
        display: none;
      }

      .sign-out .icon {
        display: inline;
        font-size: 1.05rem;
      }

      .sign-out {
        padding-inline: 0.55rem;
      }
    }
  `,
})
export class AppBarComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string | null>(null);
  readonly backTo = input<string | null>(null);

  protected readonly auth = inject(AuthService);
}
