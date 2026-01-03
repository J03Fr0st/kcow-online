import { HttpErrorResponse, type HttpInterceptorFn, HttpResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { SystemHealthService } from '@core/services/system-health.service';
import { tap } from 'rxjs/operators';

/**
 * HTTP Interceptor for monitoring and tracking all HTTP requests
 * Demonstrates best practices for:
 * - Request/response timing
 * - Error tracking
 * - Performance monitoring
 * - Health metrics collection
 */
export const httpMonitoringInterceptor: HttpInterceptorFn = (req, next) => {
  const healthService = inject(SystemHealthService);
  const startTime = performance.now();

  // Track request start
  healthService.trackRequestStart(req.url, req.method);

  return next(req).pipe(
    tap({
      next: (event) => {
        if (event instanceof HttpResponse) {
          const duration = performance.now() - startTime;

          // Track successful response
          healthService.trackRequestSuccess(req.url, req.method, event.status, duration);
        }
      },
      error: (error: unknown) => {
        if (error instanceof HttpErrorResponse) {
          const duration = performance.now() - startTime;

          // Track failed response
          healthService.trackRequestError(
            req.url,
            req.method,
            error.status,
            error.message,
            duration,
          );
        }
      },
    }),
  );
};

/**
 * Extended HTTP Interceptor with additional monitoring capabilities
 * Provides comprehensive HTTP request tracking for system health monitoring
 */
export const advancedHttpMonitoringInterceptor: HttpInterceptorFn = (req, next) => {
  const healthService = inject(SystemHealthService);
  const startTime = performance.now();

  // Enrich request metadata
  const requestMetadata = {
    url: req.url,
    method: req.method,
    headers: req.headers.keys(),
    timestamp: new Date(),
    id: generateRequestId(),
  };

  // Track request start with metadata
  healthService.trackRequestStart(req.url, req.method);

  // Log request for debugging (in development)
  if (typeof window !== 'undefined' && window.location.hostname === 'localhost') {
    console.debug(`🌐 [${requestMetadata.id}] ${req.method} ${req.url}`, {
      headers: Object.fromEntries(req.headers.keys().map((key) => [key, req.headers.get(key)])),
      body: req.body,
    });
  }

  return next(req).pipe(
    tap({
      next: (event) => {
        if (event instanceof HttpResponse) {
          const duration = performance.now() - startTime;

          // Enhanced success tracking
          healthService.trackRequestSuccess(req.url, req.method, event.status, duration);

          // Log response for debugging (in development)
          if (typeof window !== 'undefined' && window.location.hostname === 'localhost') {
            console.debug(
              `✅ [${requestMetadata.id}] ${event.status} ${event.statusText} (${duration.toFixed(2)}ms)`,
              {
                headers: Object.fromEntries(
                  event.headers.keys().map((key) => [key, event.headers.get(key)]),
                ),
                body: event.body,
              },
            );
          }

          // Performance warnings
          if (duration > 2000) {
            healthService.createAlert(
              'warning',
              'Slow API Response',
              `${req.method} ${req.url} took ${duration.toFixed(2)}ms`,
              'http',
            );
          }
        }
      },
      error: (error: unknown) => {
        if (error instanceof HttpErrorResponse) {
          const duration = performance.now() - startTime;

          // Enhanced error tracking
          healthService.trackRequestError(
            req.url,
            req.method,
            error.status,
            error.message,
            duration,
          );

          // Log error for debugging (in development)
          if (typeof window !== 'undefined' && window.location.hostname === 'localhost') {
            console.error(
              `❌ [${requestMetadata.id}] ${error.status} ${error.statusText} (${duration.toFixed(2)}ms)`,
              {
                error: error.error,
                message: error.message,
              },
            );
          }

          // Create alerts for different error types
          if (error.status >= 500) {
            healthService.createAlert(
              'error',
              'Server Error',
              `${req.method} ${req.url} failed with ${error.status}: ${error.statusText}`,
              'http',
            );
          } else if (error.status >= 400) {
            healthService.createAlert(
              'warning',
              'Client Error',
              `${req.method} ${req.url} failed with ${error.status}: ${error.statusText}`,
              'http',
            );
          } else if (error.status === 0) {
            healthService.createAlert(
              'error',
              'Network Error',
              `Failed to connect to ${req.url} - network or CORS issue`,
              'http',
            );
          }
        }
      },
    }),
  );
};

/**
 * Generate unique request ID for tracking
 */
function generateRequestId(): string {
  return Date.now().toString(36) + Math.random().toString(36).substr(2, 5);
}
