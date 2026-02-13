import { type ErrorHandler, Injectable, inject } from '@angular/core';
import { ErrorSeverity, ErrorType } from '../../models/error.model';
import { ErrorLoggingService } from './error-logging.service';

/**
 * Global error handler for uncaught errors
 *
 * Catches and handles all unhandled errors in the application, including:
 * - Runtime errors (TypeError, ReferenceError, etc.)
 * - Template errors
 * - Component lifecycle errors
 * - Promise rejections
 *
 * @example
 * Automatically registered in app config:
 * ```typescript
 * providers: [
 *   { provide: ErrorHandler, useClass: GlobalErrorHandler }
 * ]
 * ```
 */
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private errorLogger = inject(ErrorLoggingService);

  handleError(error: unknown): void {
    // Prevent infinite error loops
    if (this.isErrorHandlingError(error)) {
      console.error('Error in error handler:', error);
      return;
    }

    // Extract the actual error from Angular's error wrapper
    const actualError = this.extractError(error);

    // Determine error type and severity
    const errorType = this.determineErrorType(actualError);
    const severity = this.determineSeverity(actualError, errorType);

    // Extract Angular-specific zone/task info if available
    const errorRecord = error != null && typeof error === 'object' ? error as Record<string, unknown> : null;

    // Log the error
    this.errorLogger.logError(actualError, {
      logToConsole: true,
      logToServer: severity === ErrorSeverity.CRITICAL,
      showUserNotification: true,
      severity,
      context: {
        type: errorType,
        zone: errorRecord?.['zone'],
        task: errorRecord?.['task'],
      },
    });

    // Also log to console for development
    console.error('Global error caught:', actualError);
  }

  /**
   * Extract the actual error from Angular's error wrapper
   */
  private extractError(error: unknown): Error {
    // Angular wraps errors in a custom object with rejection or error properties
    if (error != null && typeof error === 'object') {
      const errorObj = error as Record<string, unknown>;

      if (errorObj['rejection'] instanceof Error) {
        return errorObj['rejection'];
      }

      if (errorObj['error'] instanceof Error) {
        return errorObj['error'];
      }
    }

    if (error instanceof Error) {
      return error;
    }

    // Create a generic error if we can't extract one
    return new Error(typeof error === 'string' ? error : JSON.stringify(error));
  }

  /**
   * Determine the type of error
   */
  private determineErrorType(error: Error): ErrorType {
    if (error instanceof TypeError) {
      return ErrorType.RUNTIME;
    }

    if (error instanceof ReferenceError) {
      return ErrorType.RUNTIME;
    }

    if (error.name === 'ValidationError') {
      return ErrorType.VALIDATION;
    }

    if (error.message?.includes('network') || error.message?.includes('fetch')) {
      return ErrorType.NETWORK;
    }

    return ErrorType.UNKNOWN;
  }

  /**
   * Determine error severity
   */
  private determineSeverity(error: Error, type: ErrorType): ErrorSeverity {
    // Critical errors
    if (error instanceof ReferenceError || error.message?.toLowerCase().includes('critical')) {
      return ErrorSeverity.CRITICAL;
    }

    // Runtime errors are usually errors
    if (type === ErrorType.RUNTIME) {
      return ErrorSeverity.ERROR;
    }

    // Validation errors are warnings
    if (type === ErrorType.VALIDATION) {
      return ErrorSeverity.WARNING;
    }

    // Default to error
    return ErrorSeverity.ERROR;
  }

  /**
   * Check if this is an error that occurred during error handling
   */
  private isErrorHandlingError(error: unknown): boolean {
    // Check if error originated from error handling code
    const stack = error instanceof Error ? (error.stack ?? '') : '';
    return stack.includes('error-logging.service') || stack.includes('global-error-handler');
  }
}
