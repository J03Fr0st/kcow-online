import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject, type OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ErrorLoggingService } from '@core/services/error-logging.service';
import { ErrorSeverity, ErrorType } from '@models/error.model';
import { ErrorBoundaryComponent } from '@shared/components/error-boundary/error-boundary.component';

interface ErrorStats {
  total: number;
  bySeverity: {
    info: number;
    warning: number;
    error: number;
    critical: number;
  };
  byType: Record<string, number>;
}

/**
 * Error Handling Demo Component
 *
 * Demonstrates all error handling features:
 * - Global error interceptor
 * - Error logging service
 * - Error boundary component
 * - Error pages
 * - User-friendly error notifications
 */
@Component({
  selector: 'app-error-handling-demo',
  standalone: true,
  imports: [CommonModule, ErrorBoundaryComponent],
  templateUrl: './error-handling-demo.component.html',
  styleUrls: ['./error-handling-demo.component.css'],
})
export class ErrorHandlingDemoComponent implements OnInit {
  private errorLogger = inject(ErrorLoggingService);
  private http = inject(HttpClient);
  private router = inject(Router);

  errorStats: ErrorStats = {
    total: 0,
    bySeverity: {
      info: 0,
      warning: 0,
      error: 0,
      critical: 0,
    },
    byType: {},
  };

  constructor() {
    console.log('ErrorHandlingDemoComponent initialized');
  }

  ngOnInit(): void {
    this.refreshStats();
  }

  simulateHttpError(statusCode: number): void {
    // Simulate HTTP error by making request to non-existent endpoint
    const errorUrls: Record<number, string> = {
      404: 'https://jsonplaceholder.typicode.com/nonexistent',
      403: 'https://httpstat.us/403',
      500: 'https://httpstat.us/500',
      0: 'https://nonexistent-domain-that-will-fail-12345.com/api',
    };

    const url = errorUrls[statusCode] || errorUrls[500];

    this.http.get(url).subscribe({
      next: () => console.log('Request succeeded unexpectedly'),
      error: (_error) => {
        console.log(`HTTP ${statusCode} error simulated`);
        this.refreshStats();
      },
    });
  }

  simulateRuntimeError(): void {
    try {
      // Simulate runtime error
      throw new Error('This is a simulated runtime error for testing purposes');
    } catch (error) {
      this.errorLogger.logError(error, {
        severity: ErrorSeverity.ERROR,
        context: { source: 'demo', type: ErrorType.RUNTIME },
      });
      this.refreshStats();
    }
  }

  simulateTypeError(): void {
    try {
      // Simulate type error
      const obj: any = null;
      obj.property.nested.value = 'test'; // This will throw TypeError
    } catch (error) {
      this.errorLogger.logError(error, {
        showUserNotification: true,
      });
      this.refreshStats();
    }
  }

  simulateValidationError(): void {
    this.errorLogger.logValidationError('Invalid input: Email format is incorrect', {
      field: 'email',
      value: 'invalid-email',
    });
    this.refreshStats();
  }

  logCustomError(severity: string): void {
    const messages = {
      info: 'This is an informational message',
      warning: 'This is a warning message - something might need attention',
      error: 'This is an error message - something went wrong',
      critical: 'This is a critical error - immediate action required!',
    };

    const severityMap: Record<string, ErrorSeverity> = {
      info: ErrorSeverity.INFO,
      warning: ErrorSeverity.WARNING,
      error: ErrorSeverity.ERROR,
      critical: ErrorSeverity.CRITICAL,
    };

    this.errorLogger.logError(new Error(messages[severity as keyof typeof messages]), {
      severity: severityMap[severity],
      showUserNotification: true,
      context: { source: 'demo', custom: true },
    });
    this.refreshStats();
  }

  navigateToErrorPage(errorCode: string): void {
    this.router.navigate(['/error', errorCode]);
  }

  refreshStats(): void {
    this.errorStats = this.errorLogger.getErrorStats();
  }

  clearErrorHistory(): void {
    this.errorLogger.clearHistory();
    this.refreshStats();
  }

  getErrorTypes(): string[] {
    return Object.keys(this.errorStats.byType);
  }
}
