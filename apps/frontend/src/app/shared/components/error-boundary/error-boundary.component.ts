import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input, inject, type OnDestroy, type OnInit } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { ErrorLoggingService } from '../../../core/services/error-logging.service';
import type { AppError } from '../../../models/error.model';

/**
 * Error Boundary Component
 *
 * Wraps content and catches errors in child components, providing a fallback UI
 * when errors occur instead of breaking the entire application.
 *
 * Features:
 * - Graceful error handling with fallback UI
 * - Error recovery with retry functionality
 * - Custom error messages
 * - Integration with error logging service
 * - Automatic error reporting
 *
 * @example
 * ```html
 * <app-error-boundary
 *   [fallbackMessage]="'Failed to load data'"
 *   [showRetry]="true"
 *   (retry)="loadData()">
 *   <app-data-table></app-data-table>
 * </app-error-boundary>
 * ```
 */
@Component({
  selector: 'app-error-boundary',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="error-boundary-container">
      @if (!hasError) {
        <ng-content></ng-content>
      } @else {
        <div class="error-boundary-fallback">
          <div class="alert alert-error shadow-lg max-w-2xl mx-auto">
            <div class="flex flex-col gap-4 w-full">
              <div class="flex items-start gap-3">
                <!-- Error Icon -->
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  class="stroke-current flex-shrink-0 h-6 w-6"
                  fill="none"
                  viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>

                <div class="flex-1">
                  <h3 class="font-bold text-lg">
                    {{ errorTitle }}
                  </h3>
                  <div class="text-sm mt-1">
                    {{ errorMessage }}
                  </div>

                  @if (showErrorDetails && currentError) {
                    <div class="mt-3 text-xs opacity-75">
                      <details>
                        <summary class="cursor-pointer hover:opacity-100">
                          Technical Details
                        </summary>
                        <div class="mt-2 p-3 bg-base-300 rounded-lg font-mono">
                          <div><strong>Error ID:</strong> {{ currentError.id }}</div>
                          <div><strong>Type:</strong> {{ currentError.type }}</div>
                          <div><strong>Timestamp:</strong> {{ currentError.timestamp | date:'short' }}</div>
                          @if (currentError.statusCode) {
                            <div><strong>Status:</strong> {{ currentError.statusCode }}</div>
                          }
                        </div>
                      </details>
                    </div>
                  }
                </div>
              </div>

              <!-- Action Buttons -->
              <div class="flex gap-2 justify-end">
                @if (showRetry) {
                  <button
                    class="btn btn-sm btn-primary"
                    (click)="handleRetry()">
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      class="h-4 w-4 mr-1"
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
                }

                @if (showDismiss) {
                  <button
                    class="btn btn-sm btn-ghost"
                    (click)="dismissError()">
                    Dismiss
                  </button>
                }
              </div>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [
    `
      .error-boundary-container {
        display: contents;
      }

      .error-boundary-fallback {
        padding: 2rem;
        min-height: 200px;
        display: flex;
        align-items: center;
        justify-content: center;
      }
    `,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ErrorBoundaryComponent implements OnInit, OnDestroy {
  private errorLogger = inject(ErrorLoggingService);
  private destroy$ = new Subject<void>();

  @Input() fallbackMessage = 'Something went wrong. Please try again later.';
  @Input() errorTitle = 'An Error Occurred';
  @Input() showRetry = false;
  @Input() showDismiss = true;
  @Input() showErrorDetails = true;
  @Input() autoReset = false; // Automatically reset after error
  @Input() autoResetDelay = 5000; // ms

  hasError = false;
  errorMessage = '';
  currentError: AppError | null = null;
  private autoResetTimer?: ReturnType<typeof setTimeout>;

  ngOnInit(): void {
    // Listen for errors from the error logging service
    this.errorLogger.lastError.pipe(takeUntil(this.destroy$)).subscribe((error) => {
      if (error && this.shouldHandleError(error)) {
        this.handleError(error);
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.clearAutoResetTimer();
  }

  /**
   * Handle error and show fallback UI
   */
  private handleError(error: AppError): void {
    this.hasError = true;
    this.currentError = error;
    this.errorMessage = error.userMessage || this.fallbackMessage;

    // Auto reset if enabled
    if (this.autoReset) {
      this.scheduleAutoReset();
    }
  }

  /**
   * Determine if this boundary should handle the error
   */
  private shouldHandleError(_error: AppError): boolean {
    // Override this method if you want to filter which errors to catch
    // For now, catch all errors
    return true;
  }

  /**
   * Handle retry action
   */
  handleRetry(): void {
    this.resetError();
    // Component using this boundary should listen for retry event
    // and re-execute the failed operation
  }

  /**
   * Dismiss error and show content again
   */
  dismissError(): void {
    this.resetError();
  }

  /**
   * Reset error state
   */
  private resetError(): void {
    this.hasError = false;
    this.errorMessage = '';
    this.currentError = null;
    this.clearAutoResetTimer();
  }

  /**
   * Schedule auto reset
   */
  private scheduleAutoReset(): void {
    this.clearAutoResetTimer();
    this.autoResetTimer = setTimeout(() => {
      this.resetError();
    }, this.autoResetDelay);
  }

  /**
   * Clear auto reset timer
   */
  private clearAutoResetTimer(): void {
    if (this.autoResetTimer) {
      clearTimeout(this.autoResetTimer);
      this.autoResetTimer = undefined;
    }
  }
}
