import { TestBed } from '@angular/core/testing';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';
import { LoginRequest } from '../../features/auth/models/login-request.model';
import { LoginResponse } from '../../features/auth/models/login-response.model';
import { User } from '../../features/auth/models/user.model';

describe('AuthService', () => {
  let service: AuthService;
  let httpClientSpy: { post: jest.Mock; get: jest.Mock };
  let routerSpy: { navigate: jest.Mock };

  beforeEach(() => {
    httpClientSpy = { post: jest.fn(), get: jest.fn() };
    routerSpy = { navigate: jest.fn() };

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        { provide: HttpClient, useValue: httpClientSpy },
        { provide: Router, useValue: routerSpy },
      ],
    });

    service = TestBed.inject(AuthService);
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
    jest.clearAllMocks();
  });

  describe('initialization', () => {
    it('should be created', () => {
      expect(service).toBeTruthy();
    });

    it('should have initial state', () => {
      expect(service.isAuthenticated()).toBe(false);
      expect(service.currentUser()).toBeNull();
      expect(service.isLoading()).toBe(false);
    });
  });

  describe('login', () => {
    const credentials: LoginRequest = {
      email: 'test@example.com',
      password: 'password',
    };

    const mockUser: User = {
      id: 1,
      email: 'test@example.com',
      name: 'Test User',
      role: 'Admin',
    };

    const mockResponse: LoginResponse = {
      token: 'fake-jwt-token',
      user: mockUser,
    };

    it('should call login API and update state on success', (done) => {
      httpClientSpy.post.mockReturnValue(of(mockResponse));

      service.login(credentials).subscribe({
        next: (response) => {
          expect(response).toEqual(mockResponse);
          expect(httpClientSpy.post).toHaveBeenCalledWith(
            `${environment.apiUrl}/auth/login`,
            credentials
          );
          expect(localStorage.getItem('auth_token')).toBe('fake-jwt-token');
          expect(service.isAuthenticated()).toBe(true);
          expect(service.currentUser()).toEqual(mockUser);
          expect(service.isLoading()).toBe(false);
          done();
        },
        error: done.fail,
      });
    });

    it('should handle login error', (done) => {
      const errorResponse = { status: 401, statusText: 'Unauthorized' };
      httpClientSpy.post.mockReturnValue(throwError(() => errorResponse));

      service.login(credentials).subscribe({
        next: () => done.fail('expected an error'),
        error: (error) => {
          expect(error).toEqual(errorResponse);
          expect(service.isAuthenticated()).toBe(false);
          expect(service.currentUser()).toBeNull();
          expect(service.isLoading()).toBe(false);
          done();
        },
      });
    });
  });

  describe('logout', () => {
    it('should call logout API, clear storage, and redirect', (done) => {
      httpClientSpy.post.mockReturnValue(of({}));
      localStorage.setItem('auth_token', 'fake-token');

      // Setup user state to be logged in
      // Since we can't easily set signals from outside, we assume logout clears whatever is there.
      
      service.logout().subscribe({
        next: () => {
          expect(httpClientSpy.post).toHaveBeenCalledWith(`${environment.apiUrl}/auth/logout`, {});
          expect(localStorage.getItem('auth_token')).toBeNull();
          expect(service.isAuthenticated()).toBe(false);
          expect(service.currentUser()).toBeNull();
          expect(routerSpy.navigate).toHaveBeenCalledWith(['/login']);
          done();
        },
        error: done.fail
      });
    });
  });

  describe('getToken', () => {
    it('should return token from storage', () => {
      localStorage.setItem('auth_token', 'test-token');
      expect(service.getToken()).toBe('test-token');
    });

    it('should return null if no token', () => {
      expect(service.getToken()).toBeNull();
    });
  });
});
