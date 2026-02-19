import { CommonModule, Location } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * 500 Server Error Page
 *
 * Displayed when:
 * - Server returns 500 error (via error interceptor)
 * - Internal server error occurs
 * - API is unavailable (502, 503, 504)
 */
@Component({
  selector: 'app-server-error',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-base-200 p-4">
      <div class="text-center max-w-2xl">
        <!-- Error Code -->
        <div class="mb-8">
          <h1 class="text-9xl font-bold text-error opacity-20">500</h1>
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
                d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
            </svg>
          </div>

          <h2 class="text-4xl font-bold mb-4">Server Error</h2>
          <p class="text-lg text-base-content/70 mb-2">
            Something went wrong on our end.
          </p>
          <p class="text-base text-base-content/60">
            We're working to fix the problem. Please try again in a few minutes.
          </p>
        </div>

        <!-- Actions -->
        <div class="flex flex-col sm:flex-row gap-4 justify-center items-center">
          <button
            class="btn btn-primary btn-lg"
            (click)="reload()">
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
                d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            Try Again
          </button>

          <button
            class="btn btn-outline btn-lg"
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
            class="btn btn-ghost btn-lg">
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
            Dashboard
          </a>
        </div>

        <!-- Additional Info -->
        <div class="mt-12 pt-8 border-t border-base-300">
          <div class="bg-error/10 rounded-lg p-6">
            <h3 class="font-semibold mb-3 flex items-center justify-center gap-2">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                class="h-5 w-5 text-error"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              What happened?
            </h3>
            <ul class="text-sm text-base-content/70 space-y-2 text-left max-w-md mx-auto">
              <li class="flex items-start gap-2">
                <span class="text-error">•</span>
                <span>An unexpected error occurred on our servers</span>
              </li>
              <li class="flex items-start gap-2">
                <span class="text-error">•</span>
                <span>Our team has been automatically notified</span>
              </li>
              <li class="flex items-start gap-2">
                <span class="text-error">•</span>
                <span>Your data is safe and no information was lost</span>
              </li>
            </ul>

            <div class="mt-6 p-4 bg-base-200 rounded-lg">
              <p class="text-xs text-base-content/60">
                If the problem persists, please contact support with error code:
                <code class="font-mono bg-base-300 px-2 py-1 rounded ml-1">
                  ERR_{{ errorId }}
                </code>
              </p>
            </div>
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
export class ServerErrorComponent {
  private readonly location = inject(Location);

  readonly errorId: string;

  constructor() {
    // Generate a random error ID for reference
    this.errorId = this.generateErrorId();
  }

  goBack(): void {
    this.location.back();
  }

  reload(): void {
    window.location.reload();
  }

  private generateErrorId(): string {
    // Generate a short random ID (8 characters)
    return Math.random().toString(36).substring(2, 10).toUpperCase();
  }
}
