import { CommonModule, type Location } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { type Router, RouterLink } from '@angular/router';

/**
 * 403 Forbidden Error Page
 *
 * Displayed when:
 * - User doesn't have permission to access a resource
 * - API returns 403 error (via error interceptor)
 * - Authorization check fails
 */
@Component({
  selector: 'app-forbidden',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-base-200 p-4">
      <div class="text-center max-w-2xl">
        <!-- Error Code -->
        <div class="mb-8">
          <h1 class="text-9xl font-bold text-warning opacity-20">403</h1>
        </div>

        <!-- Error Icon and Message -->
        <div class="mb-8">
          <div class="flex justify-center mb-6">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              class="h-24 w-24 text-warning"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
            </svg>
          </div>

          <h2 class="text-4xl font-bold mb-4">Access Forbidden</h2>
          <p class="text-lg text-base-content/70 mb-2">
            You don't have permission to access this resource.
          </p>
          <p class="text-base text-base-content/60">
            If you believe this is an error, please contact your administrator.
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

        <!-- Additional Info -->
        <div class="mt-12 pt-8 border-t border-base-300">
          <div class="bg-info/10 rounded-lg p-6">
            <h3 class="font-semibold mb-3 flex items-center justify-center gap-2">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                class="h-5 w-5 text-info"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
              </svg>
              Why am I seeing this?
            </h3>
            <ul class="text-sm text-base-content/70 space-y-2 text-left max-w-md mx-auto">
              <li class="flex items-start gap-2">
                <span class="text-info">•</span>
                <span>You may not have the required permissions or role</span>
              </li>
              <li class="flex items-start gap-2">
                <span class="text-info">•</span>
                <span>Your session may have expired - try logging in again</span>
              </li>
              <li class="flex items-start gap-2">
                <span class="text-info">•</span>
                <span>This resource may be restricted to certain users</span>
              </li>
            </ul>
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
export class ForbiddenComponent {
  constructor(
    private location: Location,
    _router: Router,
  ) {}

  goBack(): void {
    this.location.back();
  }
}
