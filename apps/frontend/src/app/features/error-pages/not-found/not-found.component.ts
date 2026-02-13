import { CommonModule, type Location } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { type Router, RouterLink } from '@angular/router';

/**
 * 404 Not Found Error Page
 *
 * Displayed when:
 * - User navigates to a non-existent route
 * - API returns 404 error (via error interceptor)
 * - Resource is not found
 */
@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-base-200 p-4">
      <div class="text-center max-w-2xl">
        <!-- Error Code -->
        <div class="mb-8">
          <h1 class="text-9xl font-bold text-primary opacity-20">404</h1>
        </div>

        <!-- Error Icon and Message -->
        <div class="mb-8">
          <div class="flex justify-center mb-6">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-24 w-24 text-error"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M9.172 16.172a4 4 0 015.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
            </svg>
          </div>

          <h2 class="text-4xl font-bold mb-4">Page Not Found</h2>
          <p class="text-lg text-base-content/70 mb-2">
            Oops! The page you're looking for doesn't exist.
          </p>
          <p class="text-base text-base-content/60">
            It might have been moved or deleted, or you may have mistyped the URL.
          </p>
        </div>

        <!-- Actions -->
        <div class="flex flex-col sm:flex-row gap-4 justify-center items-center">
          <button
            class="btn btn-primary btn-lg"
            (click)="goBack()">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-5 w-5 mr-2"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M10 19l-7-7m0 0l7-7m-7 7h18" />
            </svg>
            Go Back
          </button>

          <a
            routerLink="/"
            class="btn btn-outline btn-lg">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-5 w-5 mr-2"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
            </svg>
            Go to Dashboard
          </a>
        </div>

        <!-- Helpful Links -->
        <div class="mt-12 pt-8 border-t border-base-300">
          <p class="text-sm text-base-content/60 mb-4">
            Here are some helpful links instead:
          </p>
          <div class="flex flex-wrap gap-4 justify-center">
            <a routerLink="/" class="link link-primary">Dashboard</a>
            <a routerLink="/tables" class="link link-primary">Tables</a>
            <a routerLink="/forms" class="link link-primary">Forms</a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotFoundComponent {
  constructor(
    private location: Location,
    _router: Router,
  ) {}

  goBack(): void {
    this.location.back();
  }
}
