import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { fakeAsync, TestBed, tick } from '@angular/core/testing';
import { Router } from '@angular/router';
import { ErrorLoggingService } from '../services/error-logging.service';
import { errorInterceptor } from './error.interceptor';

describe('errorInterceptor HTTP contract', () => {
  let http: HttpClient;
  let requests: HttpTestingController;
  let errorLogger: { logHttpError: jest.Mock };

  beforeEach(() => {
    errorLogger = { logHttpError: jest.fn().mockReturnValue({ statusCode: 401 }) };
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting(),
        { provide: Router, useValue: { navigate: jest.fn(), url: '/login' } },
        { provide: ErrorLoggingService, useValue: errorLogger },
      ],
    });
    http = TestBed.inject(HttpClient);
    requests = TestBed.inject(HttpTestingController);
  });

  afterEach(() => requests.verify());

  it('does not retry a payment POST after a transient response', fakeAsync(() => {
    let receivedError: HttpErrorResponse | undefined;
    http.post('/api/students/1/payments', { amount: 100 }).subscribe({
      error: (error: HttpErrorResponse) => { receivedError = error; },
    });

    requests.expectOne('/api/students/1/payments').flush('Unavailable', {
      status: 503,
      statusText: 'Service Unavailable',
    });
    tick(3000);

    expect(requests.match('/api/students/1/payments')).toHaveLength(0);
    expect(receivedError?.status).toBe(503);
  }));

  it('still retries a safe GET after a transient response', fakeAsync(() => {
    let result: { status: string } | undefined;
    http.get<{ status: string }>('/api/health').subscribe((value) => { result = value; });

    requests.expectOne('/api/health').flush('Unavailable', {
      status: 503,
      statusText: 'Service Unavailable',
    });
    tick(1000);
    requests.expectOne('/api/health').flush({ status: 'healthy' });

    expect(result).toEqual({ status: 'healthy' });
  }));

  it('preserves the HTTP status for callers handling authentication errors', () => {
    let receivedError: HttpErrorResponse | undefined;
    http.get('/api/auth/me').subscribe({
      error: (error: HttpErrorResponse) => { receivedError = error; },
    });

    requests.expectOne('/api/auth/me').flush('Unauthorized', {
      status: 401,
      statusText: 'Unauthorized',
    });

    expect(receivedError).toBeInstanceOf(HttpErrorResponse);
    expect(receivedError?.status).toBe(401);
  });

  it('does not pass a submitted password to the error logger', () => {
    http.post('/api/auth/login', { email: 'person@example.com', password: 'secret-value' }).subscribe({
      error: () => undefined,
    });

    requests.expectOne('/api/auth/login').flush('Unauthorized', {
      status: 401,
      statusText: 'Unauthorized',
    });

    const options = errorLogger.logHttpError.mock.calls[0][3];
    expect(options.context).not.toHaveProperty('requestBody');
    expect(JSON.stringify(options.context)).not.toContain('secret-value');
    expect(TestBed.inject(Router).navigate).not.toHaveBeenCalled();
  });
});
