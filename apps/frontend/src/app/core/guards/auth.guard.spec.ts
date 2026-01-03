import { TestBed } from '@angular/core/testing';
import { Injector, runInInjectionContext } from '@angular/core';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  let injector: Injector;
  let router: Router;
  let authServiceSpy: jest.Mocked<AuthService>;
  let mockActivatedRouteSnapshot: ActivatedRouteSnapshot;
  let mockRouterStateSnapshot: RouterStateSnapshot;

  beforeEach(() => {
    authServiceSpy = {
      getToken: jest.fn(),
      setReturnUrl: jest.fn(),
    } as unknown as jest.Mocked<AuthService>;

    TestBed.configureTestingModule({
      providers: [
        {
          provide: Router,
          useValue: {
            navigate: jest.fn(),
            url: '/test',
          },
        },
        { provide: AuthService, useValue: authServiceSpy },
      ],
    });

    injector = TestBed.inject(Injector);
    router = TestBed.inject(Router);

    mockActivatedRouteSnapshot = {} as ActivatedRouteSnapshot;
    mockRouterStateSnapshot = {
      url: '/dashboard',
    } as RouterStateSnapshot;
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  describe('when user is authenticated', () => {
    it('should allow access to protected route', () => {
      authServiceSpy.getToken.mockReturnValue('valid-token');

      const result = runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, mockRouterStateSnapshot)
      );

      expect(result).toBe(true);
      expect(router.navigate).not.toHaveBeenCalled();
    });
  });

  describe('when user is not authenticated', () => {
    it('should deny access', () => {
      authServiceSpy.getToken.mockReturnValue(null);

      const result = runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, mockRouterStateSnapshot)
      );

      expect(result).toBe(false);
    });

    it('should set return URL via AuthService', () => {
      authServiceSpy.getToken.mockReturnValue(null);

      runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, mockRouterStateSnapshot)
      );

      expect(authServiceSpy.setReturnUrl).toHaveBeenCalledWith('/dashboard');
    });

    it('should redirect to login', () => {
      authServiceSpy.getToken.mockReturnValue(null);

      runInInjectionContext(injector, () =>
        authGuard(mockActivatedRouteSnapshot, mockRouterStateSnapshot)
      );

      expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });
  });
});