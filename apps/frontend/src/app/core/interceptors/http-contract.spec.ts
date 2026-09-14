import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { fakeAsync, TestBed, tick } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ErrorLoggingService } from '../services/error-logging.service';
import { SystemHealthService } from '../services/system-health.service';
import { authInterceptor } from './auth.interceptor';
import { errorInterceptor } from './error.interceptor';
import { httpMonitoringInterceptor } from './http-monitoring.interceptor';

describe('HTTP transport contract', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  const navigate = jest.fn();
  beforeEach(() => {
    localStorage.clear();
    navigate.mockClear();
    TestBed.configureTestingModule({ providers: [
      provideHttpClient(withInterceptors([authInterceptor, httpMonitoringInterceptor, errorInterceptor])),
      provideHttpClientTesting(),
      { provide: Router, useValue: { navigate, url: '/students' } },
      { provide: ErrorLoggingService, useValue: { logHttpError: jest.fn().mockReturnValue({ statusCode: 401 }) } },
      { provide: SystemHealthService, useValue: { trackRequestStart: jest.fn(), trackRequestSuccess: jest.fn(), trackRequestError: jest.fn() } },
    ] });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
    TestBed.inject(AuthService);
  });
  afterEach(() => { backend.verify(); localStorage.clear(); });

  it('does not replay a write with an unknown outcome', fakeAsync(() => {
    let received: unknown;
    http.post('/api/students/1/payments', { amount: 50 }).subscribe({ error: e => received = e });
    backend.expectOne('/api/students/1/payments').error(new ProgressEvent('error'));
    tick(3000);
    backend.expectNone('/api/students/1/payments');
    expect(received).toBeInstanceOf(HttpErrorResponse);
  }));

  it('retries a safe read and preserves the final ProblemDetails response', fakeAsync(() => {
    let received: HttpErrorResponse | undefined;
    http.get('/api/students').subscribe({ error: (e: HttpErrorResponse) => received = e });
    backend.expectOne('/api/students').flush({}, { status: 503, statusText: 'Unavailable' });
    tick(1000);
    const problem = { title: 'Invalid filter', status: 400, errors: { name: ['Required'] } };
    backend.expectOne('/api/students').flush(problem, { status: 400, statusText: 'Bad Request' });
    expect(received?.status).toBe(400);
    expect(received?.error).toEqual(problem);
  }));

  it('clears authenticated state and navigates once on 401', () => {
    const auth = TestBed.inject(AuthService);
    auth.login({ email: 'admin@example.test', password: 'test' }).subscribe();
    backend.expectOne(r => r.url.endsWith('/auth/login')).flush({ token: 'a.b.c', user: { id: 1, name: 'Admin', email: 'admin@example.test', role: 'Admin' } });
    expect(auth.isAuthenticated()).toBe(true);
    http.get('/api/students').subscribe({ error: () => undefined });
    backend.expectOne('/api/students').flush({ title: 'Unauthorized' }, { status: 401, statusText: 'Unauthorized' });
    expect(auth.isAuthenticated()).toBe(false);
    expect(auth.currentUser()).toBeNull();
    expect(navigate).toHaveBeenCalledTimes(1);
    expect(navigate).toHaveBeenCalledWith(['/login']);
  });
});
