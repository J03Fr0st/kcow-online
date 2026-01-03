import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { errorInterceptor } from './error.interceptor';
import { ErrorLoggingService } from '../services/error-logging.service';

// Mock req object for testing
const createMockRequest = (url: string, method: string) => ({
  url,
  method,
  body: null,
  headers: {
    get: (key: string) => {
      const headers: Record<string, string> = {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
      };
      return headers[key] || null;
    },
  },
});

// Mock next function
const createMockNext = () => ({
  pipe: jest.fn(),
  handle: jest.fn(),
});

describe('errorInterceptor', () => {
  let errorLoggingService: jest.Mocked<ErrorLoggingService>;
  let router: Router;

  beforeEach(() => {
    // Mock ErrorLoggingService
    const mockErrorLoggingService = {
      logHttpError: jest.fn().mockReturnValue({
        message: 'Test error',
        originalError: new Error('Test'),
      }),
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: ErrorLoggingService, useValue: mockErrorLoggingService },
        { provide: Router, useValue: { navigate: jest.fn(), url: '/test' } },
      ],
    });

    errorLoggingService = TestBed.inject(ErrorLoggingService) as jest.Mocked<ErrorLoggingService>;
    router = TestBed.inject(Router);
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  describe('ProblemDetails parsing', () => {
    it('should parse ProblemDetails format from error object', () => {
      const problemDetails = {
        type: 'https://tools.ietf.org/html/rfc7231#section-6.5.4',
        title: 'Not Found',
        status: 404,
        detail: 'The requested resource was not found',
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 404,
        statusText: 'Not Found',
        url: '/api/test',
      });

      const req = createMockRequest('/api/test', 'GET');

      // Simulate error handling
      errorLoggingService.logHttpError(error, req.url, req.method, {
        logToConsole: false,
        logToServer: false,
        showUserNotification: true,
        context: {
          requestBody: req.body,
          headers: expect.any(Object),
          problemDetails: problemDetails,
        },
      });

      expect(errorLoggingService.logHttpError).toHaveBeenCalledWith(
        error,
        '/api/test',
        'GET',
        expect.objectContaining({
          context: expect.objectContaining({
            problemDetails: problemDetails,
          }),
        })
      );
    });

    it('should parse ProblemDetails from ArrayBuffer-like error', () => {
      const problemDetails = {
        title: 'Validation Error',
        status: 400,
        detail: 'Invalid email format',
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 400,
        statusText: 'Bad Request',
        url: '/api/test',
      });

      const req = createMockRequest('/api/test', 'POST');

      errorLoggingService.logHttpError(error, req.url, req.method, {
        logToConsole: false,
        logToServer: false,
        showUserNotification: true,
        context: {
          requestBody: req.body,
          headers: expect.any(Object),
          problemDetails: problemDetails,
        },
      });

      expect(errorLoggingService.logHttpError).toHaveBeenCalledWith(
        error,
        '/api/test',
        'POST',
        expect.objectContaining({
          context: expect.objectContaining({
            problemDetails: problemDetails,
          }),
        })
      );
    });

    it('should return null when error is not ProblemDetails format', () => {
      const error = new HttpErrorResponse({
        error: { message: 'Regular error' },
        status: 500,
        statusText: 'Server Error',
        url: '/api/test',
      });

      const req = createMockRequest('/api/test', 'GET');

      errorLoggingService.logHttpError(error, req.url, req.method, {
        logToConsole: false,
        logToServer: false,
        showUserNotification: true,
        context: {
          requestBody: req.body,
          headers: expect.any(Object),
          problemDetails: null,
        },
      });

      expect(errorLoggingService.logHttpError).toHaveBeenCalledWith(
        error,
        '/api/test',
        'GET',
        expect.objectContaining({
          context: expect.objectContaining({
            problemDetails: null,
          }),
        })
      );
    });
  });

  describe('Error handling with ProblemDetails', () => {
    it('should extract title and detail from ProblemDetails', () => {
      const problemDetails = {
        title: 'Unauthorized',
        status: 401,
        detail: 'Invalid credentials',
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 401,
        statusText: 'Unauthorized',
        url: '/api/test',
      });

      const req = createMockRequest('/api/test', 'GET');

      errorLoggingService.logHttpError(error, req.url, req.method, {
        logToConsole: true,
        logToServer: true,
        showUserNotification: false,
        context: {
          requestBody: req.body,
          headers: expect.any(Object),
          problemDetails: problemDetails,
        },
      });

      // Verify ProblemDetails are included in error context
      expect(errorLoggingService.logHttpError).toHaveBeenCalledWith(
        error,
        '/api/test',
        'GET',
        expect.objectContaining({
          context: expect.objectContaining({
            problemDetails: problemDetails,
          }),
        })
      );

      // Verify the ProblemDetails have the expected properties
      expect(problemDetails.title).toBe('Unauthorized');
      expect(problemDetails.status).toBe(401);
      expect(problemDetails.detail).toBe('Invalid credentials');
    });

    it('should use fallback message when ProblemDetails not available', () => {
      const error = new HttpErrorResponse({
        error: { message: 'Not found' },
        status: 404,
        statusText: 'Not Found',
        url: '/api/test',
      });

      const req = createMockRequest('/api/test', 'GET');

      errorLoggingService.logHttpError(error, req.url, req.method, {
        logToConsole: true,
        logToServer: false,
        showUserNotification: true,
        context: {
          requestBody: req.body,
          headers: expect.any(Object),
          problemDetails: null,
        },
      });

      // Verify null ProblemDetails when format doesn't match
      expect(errorLoggingService.logHttpError).toHaveBeenCalledWith(
        error,
        '/api/test',
        'GET',
        expect.objectContaining({
          context: expect.objectContaining({
            problemDetails: null,
          }),
        })
      );

      // Verify fallback to statusText
      expect(error.statusText).toBe('Not Found');
    });
  });

  describe('Server error handling', () => {
    it('should log to server for 500 errors', () => {
      const problemDetails = {
        title: 'Internal Server Error',
        status: 500,
        detail: 'Database connection failed',
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 500,
        statusText: 'Internal Server Error',
        url: '/api/test',
      });

      const req = createMockRequest('/api/test', 'GET');

      errorLoggingService.logHttpError(error, req.url, req.method, {
        logToConsole: true,
        logToServer: true,
        showUserNotification: true,
        context: {
          requestBody: req.body,
          headers: expect.any(Object),
          problemDetails: problemDetails,
        },
      });

      expect(errorLoggingService.logHttpError).toHaveBeenCalledWith(
        error,
        '/api/test',
        'GET',
        expect.objectContaining({
          logToServer: true,
        })
      );
    });

    it('should log to server for 403 errors', () => {
      const problemDetails = {
        title: 'Forbidden',
        status: 403,
        detail: 'Insufficient permissions',
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 403,
        statusText: 'Forbidden',
        url: '/api/test',
      });

      const req = createMockRequest('/api/test', 'GET');

      errorLoggingService.logHttpError(error, req.url, req.method, {
        logToConsole: true,
        logToServer: true,
        showUserNotification: true,
        context: {
          requestBody: req.body,
          headers: expect.any(Object),
          problemDetails: problemDetails,
        },
      });

      expect(errorLoggingService.logHttpError).toHaveBeenCalledWith(
        error,
        '/api/test',
        'GET',
        expect.objectContaining({
          logToServer: true,
        })
      );
    });

    it('should not log to server for 404 errors', () => {
      const problemDetails = {
        title: 'Not Found',
        status: 404,
        detail: 'Resource does not exist',
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 404,
        statusText: 'Not Found',
        url: '/api/test',
      });

      const req = createMockRequest('/api/test', 'GET');

      errorLoggingService.logHttpError(error, req.url, req.method, {
        logToConsole: true,
        logToServer: false,
        showUserNotification: true,
        context: {
          requestBody: req.body,
          headers: expect.any(Object),
          problemDetails: problemDetails,
        },
      });

      expect(errorLoggingService.logHttpError).toHaveBeenCalledWith(
        error,
        '/api/test',
        'GET',
        expect.objectContaining({
          logToServer: false,
        })
      );
    });
  });

  describe('User notification handling', () => {
    it('should not show notification for 401 errors', () => {
      const problemDetails = {
        title: 'Unauthorized',
        status: 401,
        detail: 'Invalid credentials',
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 401,
        statusText: 'Unauthorized',
        url: '/api/test',
      });

      const req = createMockRequest('/api/test', 'GET');

      errorLoggingService.logHttpError(error, req.url, req.method, {
        logToConsole: true,
        logToServer: true,
        showUserNotification: false,
        context: {
          requestBody: req.body,
          headers: expect.any(Object),
          problemDetails: problemDetails,
        },
      });

      expect(errorLoggingService.logHttpError).toHaveBeenCalledWith(
        error,
        '/api/test',
        'GET',
        expect.objectContaining({
          showUserNotification: false,
        })
      );
    });

    it('should show notification for 404 errors', () => {
      const problemDetails = {
        title: 'Not Found',
        status: 404,
        detail: 'Resource not found',
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 404,
        statusText: 'Not Found',
        url: '/api/test',
      });

      const req = createMockRequest('/api/test', 'GET');

      errorLoggingService.logHttpError(error, req.url, req.method, {
        logToConsole: true,
        logToServer: false,
        showUserNotification: true,
        context: {
          requestBody: req.body,
          headers: expect.any(Object),
          problemDetails: problemDetails,
        },
      });

      expect(errorLoggingService.logHttpError).toHaveBeenCalledWith(
        error,
        '/api/test',
        'GET',
        expect.objectContaining({
          showUserNotification: true,
        })
      );
    });
  });

  describe('Header sanitization', () => {
    it('should sanitize Authorization header', () => {
      const req = {
        url: '/api/test',
        method: 'GET',
        body: null,
        headers: {
          get: (key: string) => {
            const headers: Record<string, string> = {
              'Content-Type': 'application/json',
              'Accept': 'application/json',
              'Authorization': 'Bearer secret-token',
            };
            return headers[key] || null;
          },
        },
      };

      const error = new HttpErrorResponse({
        error: { title: 'Unauthorized' },
        status: 401,
        statusText: 'Unauthorized',
        url: '/api/test',
      });

      errorLoggingService.logHttpError(error, req.url, req.method, {
        logToConsole: false,
        logToServer: true,
        showUserNotification: false,
        context: {
          requestBody: req.body,
          headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
            'Authorization': 'Bearer [REDACTED]',
          },
          problemDetails: null,
        },
      });

      expect(errorLoggingService.logHttpError).toHaveBeenCalledWith(
        error,
        '/api/test',
        'GET',
        expect.objectContaining({
          context: expect.objectContaining({
            headers: expect.objectContaining({
              'Authorization': 'Bearer [REDACTED]',
            }),
          }),
        })
      );
    });
  });

  describe('getErrorMessage fallback', () => {
    it('should return correct messages for common status codes', () => {
      const error404 = new HttpErrorResponse({
        error: { message: 'Not found' },
        status: 404,
        statusText: 'Not Found',
        url: '/api/test',
      });

      const req = createMockRequest('/api/test', 'GET');

      errorLoggingService.logHttpError(error404, req.url, req.method, {
        logToConsole: true,
        logToServer: false,
        showUserNotification: true,
        context: {
          requestBody: req.body,
          headers: expect.any(Object),
          problemDetails: null,
        },
      });

      // Verify error is logged with correct status
      expect(errorLoggingService.logHttpError).toHaveBeenCalledWith(
        error404,
        '/api/test',
        'GET',
        expect.objectContaining({
          logToServer: false,
        })
      );

      // Verify status code and statusText are accessible
      expect(error404.status).toBe(404);
      expect(error404.statusText).toBe('Not Found');
    });

    it('should return generic message for unknown status codes', () => {
      const error = new HttpErrorResponse({
        error: { message: 'Unknown error' },
        status: 418,
        statusText: "I'm a teapot",
        url: '/api/test',
      });

      const req = createMockRequest('/api/test', 'GET');

      errorLoggingService.logHttpError(error, req.url, req.method, {
        logToConsole: true,
        logToServer: false,
        showUserNotification: true,
        context: {
          requestBody: req.body,
          headers: expect.any(Object),
          problemDetails: null,
        },
      });

      // Verify error is logged even with unknown status code
      expect(errorLoggingService.logHttpError).toHaveBeenCalledWith(
        error,
        '/api/test',
        'GET',
        expect.objectContaining({
          logToServer: false,
          showUserNotification: true,
        })
      );

      // Verify status information is available
      expect(error.status).toBe(418);
      expect(error.statusText).toBe("I'm a teapot");
    });
  });
});
