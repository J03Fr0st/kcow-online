import { HttpErrorResponse, HttpHeaders, type HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, retry, throwError, timer } from 'rxjs';
import { ErrorLoggingService } from '../services/error-logging.service';
import { ProblemDetails } from '../../models/problem-details.model';

/**
 * Global HTTP error interceptor
 *
 * Features:
 * - Automatic error logging
 * - Retry logic for retryable errors
 * - User-friendly error notifications
 * - ProblemDetails format parsing (ASP.NET Core)
 * - Automatic redirect for authentication errors
 * - Integration with error logging service
 *
 * @example
 * Automatically applied to all HTTP requests via app config
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const errorLogger = inject(ErrorLoggingService);
  const router = inject(Router);

  return next(req).pipe(
    // Retry logic for specific errors
    retry({
      count: 2,
      delay: (error, retryCount) => {
        // Only retry for specific status codes
        if (error instanceof HttpErrorResponse) {
          const retryableStatuses = [0, 408, 503, 504]; // Network, timeout, service unavailable, gateway timeout

          if (retryableStatuses.includes(error.status)) {
            // Exponential backoff: 1s, 2s
            const delayMs = 2 ** (retryCount - 1) * 1000;
            console.log(
              `Retrying request to ${req.url} (attempt ${retryCount}/2) after ${delayMs}ms`,
            );
            return timer(delayMs);
          }
        }

        // Don't retry for other errors
        throw error;
      },
    }),

    // Error handling
    catchError((error: HttpErrorResponse) => {
      // Try to parse as ProblemDetails
      const problemDetails = parseProblemDetails(error);

      // Log the error with ProblemDetails context
      const appError = errorLogger.logHttpError(error, req.url, req.method, {
        logToConsole: true,
        logToServer: shouldLogToServer(error),
        showUserNotification: shouldShowNotification(error),
        context: {
          requestBody: req.body,
          headers: extractHeaders(req.headers),
          problemDetails: problemDetails,  // Include ProblemDetails for debugging
        },
      });

      // Handle specific error types using ProblemDetails when available
      handleSpecificErrors(error, router, problemDetails);

      // Re-throw the error for component handling
      return throwError(() => appError);
    }),
  );
};

/**
 * Parse HTTP error as ProblemDetails format
 * Returns parsed ProblemDetails or null if not in ProblemDetails format
 */
function parseProblemDetails(error: HttpErrorResponse): ProblemDetails | null {
  // Try to parse error body as JSON
  if (error.error && typeof error.error === 'object') {
    // Check if it's already a ProblemDetails object
    if ('type' in error.error || 'title' in error.error || 'detail' in error.error) {
      return error.error as ProblemDetails;
    }
  }

  // Try to parse error.text as JSON
  if (error.error instanceof ArrayBuffer) {
    try {
      const text = new TextDecoder().decode(error.error);
      const json = JSON.parse(text);

      if ('type' in json || 'title' in json || 'detail' in json) {
        return json as ProblemDetails;
      }
    } catch {
      // Not JSON or not ProblemDetails format
    }
  }

  // Not a ProblemDetails error
  return null;
}

/**
 * Handle specific error types with custom logic
 * Updated to use ProblemDetails information when available
 */
function handleSpecificErrors(
  error: HttpErrorResponse,
  router: Router,
  problemDetails: ProblemDetails | null
): void {
  const status = error.status;
  const title = problemDetails?.title || getErrorMessage(status);
  const detail = problemDetails?.detail;

  switch (status) {
    case 401:
      // Unauthorized - redirect to login
      console.log('Unauthorized access - redirecting to login');
      // Clear expired token from storage
      localStorage.removeItem('auth_token');
      router.navigate(['/login'], {
        queryParams: { returnUrl: router.url },
      });
      break;

    case 403:
      // Forbidden - redirect to error page
      console.log('Forbidden access - redirecting to 403 page');
      router.navigate(['/error/403']);
      break;

    case 404:
      // Not found
      console.log('Resource not found:', detail || title);
      // Optional: router.navigate(['/error/404']);
      break;

    case 500:
    case 502:
    case 503:
    case 504:
      // Server errors - redirect to error page
      console.log('Server error:', title, detail);
      router.navigate(['/error/500']);
      break;

    case 0:
      // Network error
      console.log('Network error - no internet connection');
      break;

    default:
      // Other errors
      console.log('HTTP Error:', status, title, detail);
      break;
  }
}

/**
 * Get user-friendly error message from status code
 * Fallback when ProblemDetails not available
 */
function getErrorMessage(status: number): string {
  const messages: Record<number, string> = {
    400: 'Bad Request',
    401: 'Unauthorized',
    403: 'Forbidden',
    404: 'Not Found',
    409: 'Conflict',
    500: 'Internal Server Error',
    502: 'Bad Gateway',
    503: 'Service Unavailable',
    504: 'Gateway Timeout',
  };

  return messages[status] || `HTTP Error ${status}`;
}

/**
 * Determine if error should be logged to server
 */
function shouldLogToServer(error: HttpErrorResponse): boolean {
  // Log critical errors to server
  if (error.status >= 500) return true;

  // Log authentication/authorization errors
  if (error.status === 401 || error.status === 403) return true;

  // Log network errors
  if (error.status === 0) return true;

  return false;
}

/**
 * Determine if user notification should be shown
 */
function shouldShowNotification(error: HttpErrorResponse): boolean {
  // Don't show notification for 401 (will redirect to login)
  if (error.status === 401) return false;

  // Show notification for all other errors
  return true;
}

/**
 * Extract relevant headers for logging
 */
function extractHeaders(headers: HttpHeaders): Record<string, string> {
  const relevantHeaders: Record<string, string> = {};

  // Only extract non-sensitive headers
  const headersToExtract = [
    'Content-Type',
    'Accept',
    'Authorization', // Will be sanitized
  ];

  headersToExtract.forEach((headerName) => {
    const value = headers.get(headerName);
    if (value) {
      // Sanitize Authorization header
      if (headerName === 'Authorization') {
        relevantHeaders[headerName] = value.includes('Bearer') ? 'Bearer [REDACTED]' : '[REDACTED]';
      } else {
        relevantHeaders[headerName] = value;
      }
    }
  });

  return relevantHeaders;
}
