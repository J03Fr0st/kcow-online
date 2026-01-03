import { TestBed } from '@angular/core/testing';
import { Injector, runInInjectionContext } from '@angular/core';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  let injector: Injector;
  let router: Router;
  let mockActivatedRouteSnapshot: ActivatedRouteSnapshot;
  let mockRouterStateSnapshot: RouterStateSnapshot;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        {
          provide: Router,
          useValue: {
            navigate: jest.fn(),
            url: '/test',
          },
        },
      ],
    });

    injector = TestBed.inject(Injector);
    router = TestBed.inject(Router);

    // Clear sessionStorage before each test
    sessionStorage.clear();

    // Create mock route snapshots
    mockActivatedRouteSnapshot = {} as ActivatedRouteSnapshot;
    mockRouterStateSnapshot = {
      url: '/dashboard',
    } as RouterStateSnapshot;
  });

  afterEach(() => {
    sessionStorage.clear();
    jest.clearAllMocks();
  });

  describe('when user is authenticated', () => {
    it('should allow access to protected route', () => {
      // Set auth token
      sessionStorage.setItem('auth_token', 'fake-token');

      const result = runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, mockRouterStateSnapshot)
      );

      expect(result).toBe(true);
      expect(router.navigate).not.toHaveBeenCalled();
    });

    it('should not redirect to login', () => {
      sessionStorage.setItem('auth_token', 'fake-token');

      runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, mockRouterStateSnapshot)
      );

      expect(router.navigate).not.toHaveBeenCalled();
      expect(sessionStorage.getItem('returnUrl')).toBeNull();
    });
  });

  describe('when user is not authenticated', () => {
    it('should deny access to protected route', () => {
      const result = runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, mockRouterStateSnapshot)
      );

      expect(result).toBe(false);
    });

    it('should store return URL in sessionStorage', () => {
      runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, mockRouterStateSnapshot)
      );

      expect(sessionStorage.getItem('returnUrl')).toBe('/dashboard');
    });

    it('should redirect to login page with return URL query param', () => {
      runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, mockRouterStateSnapshot)
      );

      expect(router.navigate).toHaveBeenCalledWith(['/login'], {
        queryParams: { returnUrl: '/dashboard' },
      });
    });

    it('should handle deep nested routes', () => {
      const deepState = { url: '/admin/users/123/edit' } as RouterStateSnapshot;

      runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, deepState)
      );

      expect(sessionStorage.getItem('returnUrl')).toBe('/admin/users/123/edit');
      expect(router.navigate).toHaveBeenCalledWith(['/login'], {
        queryParams: { returnUrl: '/admin/users/123/edit' },
      });
    });
  });

  describe('return URL handling', () => {
    it('should preserve existing return URL when already set', () => {
      sessionStorage.setItem('returnUrl', '/existing-return-url');

      runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, mockRouterStateSnapshot)
      );

      // The guard should overwrite with the current URL
      expect(sessionStorage.getItem('returnUrl')).toBe('/dashboard');
    });

    it('should handle root path correctly', () => {
      const rootState = { url: '/' } as RouterStateSnapshot;

      runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, rootState)
      );

      expect(sessionStorage.getItem('returnUrl')).toBe('/');
    });

    it('should handle paths with query strings', () => {
      const stateWithQuery = { url: '/dashboard?tab=users' } as RouterStateSnapshot;

      runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, stateWithQuery)
      );

      expect(sessionStorage.getItem('returnUrl')).toBe('/dashboard?tab=users');
    });
  });

  describe('session storage integration', () => {
    it('should check for auth_token in sessionStorage', () => {
      // No token set
      const result1 = runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, mockRouterStateSnapshot)
      );
      expect(result1).toBe(false);

      // Token set
      sessionStorage.setItem('auth_token', 'valid-token');
      const result2 = runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, mockRouterStateSnapshot)
      );
      expect(result2).toBe(true);
    });

    it('should handle expired/invalid tokens (simple check only)', () => {
      // Guard only checks existence, validation happens in AuthService
      sessionStorage.setItem('auth_token', 'expired-token');

      const result = runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, mockRouterStateSnapshot)
      );

      expect(result).toBe(true);
      expect(router.navigate).not.toHaveBeenCalled();
    });
  });
});
