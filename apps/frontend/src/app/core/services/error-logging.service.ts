import { Injectable, inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, type Observable } from 'rxjs';
import {
  type AppError,
  type ErrorHandlerOptions,
  type ErrorLogEntry,
  ErrorSeverity,
  ErrorType,
  HttpErrorStatus,
} from '../../models/error.model';
import { NotificationService } from './notification.service';

/**
 * Centralized error logging and handling service
 *
 * Features:
 * - Error logging with detailed context
 * - Error categorization and severity tracking
 * - User-friendly error messages
 * - Error history and statistics
 * - Integration with notification service
 * - Remote error logging capability
 *
 * @example
 * ```typescript
 * constructor(private errorLogger: ErrorLoggingService) {}
 *
 * try {
 *   // risky operation
 * } catch (error) {
 *   this.errorLogger.logError(error, { context: { userId: '123' } });
 * }
 * ```
 */
@Injectable({
  providedIn: 'root',
})
export class ErrorLoggingService {
  private router = inject(Router);
  private notificationService = inject(NotificationService);

  private errorHistory: ErrorLogEntry[] = [];
  private maxHistorySize = 100;

  private errorCount$ = new BehaviorSubject<number>(0);
  private lastError$ = new BehaviorSubject<ErrorLogEntry | null>(null);

  /**
   * Observable of total error count
   */
  get errorCount(): Observable<number> {
    return this.errorCount$.asObservable();
  }

  /**
   * Observable of the last error
   */
  get lastError(): Observable<ErrorLogEntry | null> {
    return this.lastError$.asObservable();
  }

  /**
   * Get error history
   */
  getErrorHistory(): ErrorLogEntry[] {
    return [...this.errorHistory];
  }

  /**
   * Clear error history
   */
  clearHistory(): void {
    this.errorHistory = [];
    this.errorCount$.next(0);
    this.lastError$.next(null);
  }

  /**
   * Log an error with optional configuration
   */
  logError(error: any, options: ErrorHandlerOptions = {}): AppError {
    const appError = this.createAppError(error, options);
    const logEntry = this.createLogEntry(appError);

    // Add to history
    this.addToHistory(logEntry);

    // Log to console if enabled (default: true)
    if (options.logToConsole !== false) {
      this.logToConsole(logEntry);
    }

    // Log to server if enabled
    if (options.logToServer) {
      this.logToServer(logEntry);
    }

    // Show user notification if enabled (default: true for errors)
    if (options.showUserNotification !== false && appError.severity !== ErrorSeverity.INFO) {
      this.showUserNotification(appError);
    }

    return appError;
  }

  /**
   * Log HTTP error
   */
  logHttpError(
    error: any,
    url: string,
    method: string,
    options: ErrorHandlerOptions = {},
  ): AppError {
    const statusCode = error.status || 0;
    const errorType = this.getHttpErrorType(statusCode);
    const severity = this.getHttpErrorSeverity(statusCode);
    const _userMessage = this.getHttpErrorUserMessage(statusCode);

    return this.logError(error, {
      ...options,
      severity,
      context: {
        ...options.context,
        url,
        method,
        statusCode,
        errorType,
      },
    });
  }

  /**
   * Log validation error
   */
  logValidationError(message: string, context?: Record<string, any>): AppError {
    return this.logError(new Error(message), {
      severity: ErrorSeverity.WARNING,
      context: { ...context, type: ErrorType.VALIDATION },
      showUserNotification: true,
    });
  }

  /**
   * Get error statistics
   */
  getErrorStats(): {
    total: number;
    bySeverity: Record<ErrorSeverity, number>;
    byType: Record<ErrorType, number>;
    recentErrors: ErrorLogEntry[];
  } {
    const stats = {
      total: this.errorHistory.length,
      bySeverity: {
        [ErrorSeverity.INFO]: 0,
        [ErrorSeverity.WARNING]: 0,
        [ErrorSeverity.ERROR]: 0,
        [ErrorSeverity.CRITICAL]: 0,
      },
      byType: {
        [ErrorType.HTTP]: 0,
        [ErrorType.VALIDATION]: 0,
        [ErrorType.RUNTIME]: 0,
        [ErrorType.NETWORK]: 0,
        [ErrorType.AUTHENTICATION]: 0,
        [ErrorType.AUTHORIZATION]: 0,
        [ErrorType.NOT_FOUND]: 0,
        [ErrorType.SERVER]: 0,
        [ErrorType.UNKNOWN]: 0,
      },
      recentErrors: this.errorHistory.slice(-10).reverse(),
    };

    this.errorHistory.forEach((error) => {
      stats.bySeverity[error.severity]++;
      stats.byType[error.type]++;
    });

    return stats;
  }

  /**
   * Create structured AppError from any error
   */
  private createAppError(error: any, options: ErrorHandlerOptions): AppError {
    const timestamp = new Date();
    const id = this.generateErrorId(timestamp);

    // Extract error details
    const message = this.extractErrorMessage(error);
    const stack = error?.stack;
    const statusCode = error?.status;
    const url = error?.url;

    // Determine error type
    const type = this.determineErrorType(error, options.context);

    // Determine severity
    const severity = options.severity || this.determineSeverity(error);

    // Create user-friendly message
    const userMessage = this.createUserMessage(error, type, statusCode);

    // Check if retryable
    const retryable = this.isRetryable(error);

    return {
      id,
      timestamp,
      type,
      severity,
      message,
      userMessage,
      statusCode,
      url,
      method: options.context?.['method'],
      stack,
      context: options.context,
      handled: true,
      retryable,
    };
  }

  /**
   * Create detailed log entry
   */
  private createLogEntry(appError: AppError): ErrorLogEntry {
    return {
      ...appError,
      userAgent: navigator.userAgent,
      route: this.router.url,
      // Add sessionId and userId if available from auth service
      sessionId: this.getSessionId(),
      userId: this.getUserId(),
    };
  }

  /**
   * Add error to history
   */
  private addToHistory(logEntry: ErrorLogEntry): void {
    this.errorHistory.push(logEntry);

    // Maintain max history size
    if (this.errorHistory.length > this.maxHistorySize) {
      this.errorHistory = this.errorHistory.slice(-this.maxHistorySize);
    }

    this.errorCount$.next(this.errorHistory.length);
    this.lastError$.next(logEntry);
  }

  /**
   * Log to console with formatting
   */
  private logToConsole(logEntry: ErrorLogEntry): void {
    const style = this.getConsoleStyle(logEntry.severity);

    console.group(`%c[${logEntry.severity.toUpperCase()}] ${logEntry.type} Error`, style);
    console.error('Message:', logEntry.message);
    console.error('User Message:', logEntry.userMessage);
    console.error('Timestamp:', logEntry.timestamp.toISOString());

    if (logEntry.statusCode) {
      console.error('Status Code:', logEntry.statusCode);
    }

    if (logEntry.url) {
      console.error('URL:', logEntry.url);
      console.error('Method:', logEntry.method);
    }

    if (logEntry.route) {
      console.error('Route:', logEntry.route);
    }

    if (logEntry.context) {
      console.error('Context:', logEntry.context);
    }

    if (logEntry.stack) {
      console.error('Stack:', logEntry.stack);
    }

    console.groupEnd();
  }

  /**
   * Log to remote server (implement based on your backend)
   */
  private logToServer(_logEntry: ErrorLogEntry): void {
    // TODO: Implement remote logging
    // Example:
    // this.http.post('/api/errors', {
    //   ...logEntry,
    //   stack: undefined, // Don't send stack trace to server for security
    // }).subscribe();

    console.info('Remote logging not implemented. Error logged locally only.');
  }

  /**
   * Show user notification
   */
  private showUserNotification(appError: AppError): void {
    const message = appError.userMessage || appError.message;

    switch (appError.severity) {
      case ErrorSeverity.CRITICAL:
      case ErrorSeverity.ERROR:
        this.notificationService.error(message);
        break;
      case ErrorSeverity.WARNING:
        this.notificationService.warning(message);
        break;
      case ErrorSeverity.INFO:
        this.notificationService.info(message);
        break;
    }
  }

  /**
   * Extract error message from various error types
   */
  private extractErrorMessage(error: any): string {
    if (typeof error === 'string') {
      return error;
    }

    if (error?.error?.message) {
      return error.error.message;
    }

    if (error?.message) {
      return error.message;
    }

    if (error?.statusText) {
      return error.statusText;
    }

    return 'An unknown error occurred';
  }

  /**
   * Determine error type
   */
  private determineErrorType(error: any, context?: Record<string, any>): ErrorType {
    // Check context first
    if (context?.['type']) {
      return context['type'];
    }

    // Check HTTP status
    if (error?.status) {
      const status = error.status;

      if (status === 401) return ErrorType.AUTHENTICATION;
      if (status === 403) return ErrorType.AUTHORIZATION;
      if (status === 404) return ErrorType.NOT_FOUND;
      if (status >= 500) return ErrorType.SERVER;
      if (status === 0) return ErrorType.NETWORK;
      if (status >= 400) return ErrorType.HTTP;
    }

    // Check error name/type
    if (error?.name === 'ValidationError') {
      return ErrorType.VALIDATION;
    }

    if (error instanceof TypeError || error instanceof ReferenceError) {
      return ErrorType.RUNTIME;
    }

    return ErrorType.UNKNOWN;
  }

  /**
   * Determine error severity
   */
  private determineSeverity(error: any): ErrorSeverity {
    if (error?.status >= 500) {
      return ErrorSeverity.CRITICAL;
    }

    if (error?.status === 401 || error?.status === 403) {
      return ErrorSeverity.ERROR;
    }

    if (error?.status >= 400) {
      return ErrorSeverity.WARNING;
    }

    if (error instanceof TypeError || error instanceof ReferenceError) {
      return ErrorSeverity.ERROR;
    }

    return ErrorSeverity.ERROR;
  }

  /**
   * Create user-friendly error message
   */
  private createUserMessage(_error: any, type: ErrorType, statusCode?: number): string {
    if (statusCode) {
      return this.getHttpErrorUserMessage(statusCode);
    }

    switch (type) {
      case ErrorType.NETWORK:
        return 'Network connection error. Please check your internet connection.';
      case ErrorType.AUTHENTICATION:
        return 'Authentication failed. Please log in again.';
      case ErrorType.AUTHORIZATION:
        return "You don't have permission to access this resource.";
      case ErrorType.NOT_FOUND:
        return 'The requested resource was not found.';
      case ErrorType.VALIDATION:
        return 'Please check your input and try again.';
      case ErrorType.SERVER:
        return 'A server error occurred. Please try again later.';
      default:
        return 'An unexpected error occurred. Please try again.';
    }
  }

  /**
   * Get HTTP error type based on status code
   */
  private getHttpErrorType(statusCode: number): ErrorType {
    if (statusCode === 401) return ErrorType.AUTHENTICATION;
    if (statusCode === 403) return ErrorType.AUTHORIZATION;
    if (statusCode === 404) return ErrorType.NOT_FOUND;
    if (statusCode >= 500) return ErrorType.SERVER;
    if (statusCode === 0) return ErrorType.NETWORK;
    return ErrorType.HTTP;
  }

  /**
   * Get HTTP error severity based on status code
   */
  private getHttpErrorSeverity(statusCode: number): ErrorSeverity {
    if (statusCode >= 500) return ErrorSeverity.CRITICAL;
    if (statusCode === 401 || statusCode === 403) return ErrorSeverity.ERROR;
    if (statusCode >= 400) return ErrorSeverity.WARNING;
    return ErrorSeverity.ERROR;
  }

  /**
   * Get user-friendly HTTP error message
   */
  private getHttpErrorUserMessage(statusCode: number): string {
    switch (statusCode) {
      case HttpErrorStatus.BAD_REQUEST:
        return 'Invalid request. Please check your input.';
      case HttpErrorStatus.UNAUTHORIZED:
        return 'You need to log in to access this resource.';
      case HttpErrorStatus.FORBIDDEN:
        return "You don't have permission to access this resource.";
      case HttpErrorStatus.NOT_FOUND:
        return 'The requested resource was not found.';
      case HttpErrorStatus.CONFLICT:
        return 'A conflict occurred. The resource may have been modified.';
      case HttpErrorStatus.UNPROCESSABLE_ENTITY:
        return 'Unable to process your request. Please check your input.';
      case HttpErrorStatus.INTERNAL_SERVER_ERROR:
        return 'A server error occurred. Please try again later.';
      case HttpErrorStatus.BAD_GATEWAY:
        return 'Bad gateway. Please try again later.';
      case HttpErrorStatus.SERVICE_UNAVAILABLE:
        return 'Service temporarily unavailable. Please try again later.';
      case HttpErrorStatus.GATEWAY_TIMEOUT:
        return 'Request timeout. Please try again.';
      case 0:
        return 'Network error. Please check your internet connection.';
      default:
        if (statusCode >= 500) {
          return 'A server error occurred. Please try again later.';
        } else if (statusCode >= 400) {
          return 'An error occurred while processing your request.';
        }
        return 'An unexpected error occurred.';
    }
  }

  /**
   * Check if error is retryable
   */
  private isRetryable(error: any): boolean {
    const status = error?.status;

    // Network errors are retryable
    if (status === 0) return true;

    // Server errors are retryable
    if (status === 503 || status === 504) return true;

    // Timeout errors are retryable
    if (error?.name === 'TimeoutError') return true;

    return false;
  }

  /**
   * Generate unique error ID
   */
  private generateErrorId(timestamp: Date): string {
    return `err_${timestamp.getTime()}_${Math.random().toString(36).substr(2, 9)}`;
  }

  /**
   * Get console style based on severity
   */
  private getConsoleStyle(severity: ErrorSeverity): string {
    switch (severity) {
      case ErrorSeverity.CRITICAL:
        return 'color: white; background-color: #dc2626; font-weight: bold; padding: 2px 4px;';
      case ErrorSeverity.ERROR:
        return 'color: white; background-color: #ea580c; font-weight: bold; padding: 2px 4px;';
      case ErrorSeverity.WARNING:
        return 'color: black; background-color: #fbbf24; font-weight: bold; padding: 2px 4px;';
      case ErrorSeverity.INFO:
        return 'color: white; background-color: #3b82f6; font-weight: bold; padding: 2px 4px;';
      default:
        return 'color: white; background-color: #6b7280; font-weight: bold; padding: 2px 4px;';
    }
  }

  /**
   * Get session ID (implement based on your auth service)
   */
  private getSessionId(): string | undefined {
    // TODO: Implement session ID retrieval
    return undefined;
  }

  /**
   * Get user ID (implement based on your auth service)
   */
  private getUserId(): string | undefined {
    // TODO: Implement user ID retrieval
    return undefined;
  }
}
