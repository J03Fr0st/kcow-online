import { Location } from '@angular/common';
import { Injector } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { AuthService } from '@core/services/auth.service';
import { routes } from './app.routes';

describe('AppRoutes', () => {
  let _router: Router;
  let _location: Location;
  let mockAuthService: jest.Mocked<AuthService>;
  let _injector: Injector;

  beforeEach(async () => {
    // Clear sessionStorage before each test
    sessionStorage.clear();

    // Mock AuthService
    mockAuthService = {
      isAuthenticated: jest.fn().mockReturnValue(false),
      login: jest.fn(),
      logout: jest.fn(),
      getCurrentUser: jest.fn(),
      getReturnUrl: jest.fn().mockReturnValue('/dashboard'),
      checkSession: jest.fn(),
    } as any;

    await TestBed.configureTestingModule({
      providers: [provideRouter(routes), { provide: AuthService, useValue: mockAuthService }],
    }).compileComponents();

    _router = TestBed.inject(Router);
    _location = TestBed.inject(Location);
    _injector = TestBed.inject(Injector);
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  describe('route configuration', () => {
    it('should have login route as public route', () => {
      const loginRoute = routes.find((r) => r.path === 'login');
      expect(loginRoute).toBeDefined();
      expect(loginRoute?.canActivate).toBeUndefined();
    });

    it('should have root route with authGuard', () => {
      const rootRoute = routes.find((r) => r.path === '');
      expect(rootRoute).toBeDefined();
      expect(rootRoute?.canActivate).toBeDefined();
      expect(rootRoute?.canActivate?.length).toBeGreaterThan(0);
    });

    it('should have error pages as public routes', () => {
      const errorRoute = routes.find((r) => r.path === 'error');
      expect(errorRoute).toBeDefined();
      expect(errorRoute?.canActivate).toBeUndefined();
    });
  });

  describe('public route: login', () => {
    it('should configure login route without canActivate guard', () => {
      const loginRoute = routes.find((r) => r.path === 'login');
      expect(loginRoute?.canActivate).toBeUndefined();
    });

    it('should have correct loadComponent for login route', () => {
      const loginRoute = routes.find((r) => r.path === 'login');
      expect(loginRoute?.loadComponent).toBeDefined();
    });
  });

  describe('protected routes configuration', () => {
    it('should apply canActivate to root route (protects all children)', () => {
      const rootRoute = routes.find((r) => r.path === '');
      expect(rootRoute?.canActivate).toBeDefined();
      expect(rootRoute?.canActivate?.length).toBeGreaterThan(0);
    });

    it('should have dashboard as protected route under root', () => {
      const rootRoute = routes.find((r) => r.path === '');
      const dashboardRoute = rootRoute?.children?.find((c) => c.path === 'dashboard');
      expect(dashboardRoute).toBeDefined();
    });

    it('should have tables as protected route under root', () => {
      const rootRoute = routes.find((r) => r.path === '');
      const tablesRoute = rootRoute?.children?.find((c) => c.path === 'tables');
      expect(tablesRoute).toBeDefined();
    });

    it('should have forms as protected route under root', () => {
      const rootRoute = routes.find((r) => r.path === '');
      const formsRoute = rootRoute?.children?.find((c) => c.path === 'forms');
      expect(formsRoute).toBeDefined();
    });

    it('should have workspace-settings as protected route under root', () => {
      const rootRoute = routes.find((r) => r.path === '');
      const workspaceRoute = rootRoute?.children?.find((c) => c.path === 'workspace-settings');
      expect(workspaceRoute).toBeDefined();
    });

    it('should have system-health as protected route under root', () => {
      const rootRoute = routes.find((r) => r.path === '');
      const healthRoute = rootRoute?.children?.find((c) => c.path === 'system-health');
      expect(healthRoute).toBeDefined();
    });

    it('should have notifications as protected route under root', () => {
      const rootRoute = routes.find((r) => r.path === '');
      const notificationsRoute = rootRoute?.children?.find((c) => c.path === 'notifications');
      expect(notificationsRoute).toBeDefined();
    });

    it('should have modals as protected route under root', () => {
      const rootRoute = routes.find((r) => r.path === '');
      const modalsRoute = rootRoute?.children?.find((c) => c.path === 'modals');
      expect(modalsRoute).toBeDefined();
    });

    it('should have error-handling as protected route under root', () => {
      const rootRoute = routes.find((r) => r.path === '');
      const errorHandlingRoute = rootRoute?.children?.find((c) => c.path === 'error-handling');
      expect(errorHandlingRoute).toBeDefined();
    });

    it('should have schools as protected route under root', () => {
      const rootRoute = routes.find((r) => r.path === '');
      const schoolsRoute = rootRoute?.children?.find((c) => c.path === 'schools');
      expect(schoolsRoute).toBeDefined();
    });
  });

  describe('error pages configuration', () => {
    it('should configure error pages as public routes', () => {
      const errorRoute = routes.find((r) => r.path === 'error');
      expect(errorRoute?.canActivate).toBeUndefined();
    });

    it('should have 404 error page', () => {
      const errorRoute = routes.find((r) => r.path === 'error');
      const notFoundRoute = errorRoute?.children?.find((c) => c.path === '404');
      expect(notFoundRoute).toBeDefined();
    });

    it('should have 403 forbidden page', () => {
      const errorRoute = routes.find((r) => r.path === 'error');
      const forbiddenRoute = errorRoute?.children?.find((c) => c.path === '403');
      expect(forbiddenRoute).toBeDefined();
    });

    it('should have 500 server error page', () => {
      const errorRoute = routes.find((r) => r.path === 'error');
      const serverErrorRoute = errorRoute?.children?.find((c) => c.path === '500');
      expect(serverErrorRoute).toBeDefined();
    });
  });

  describe('root route behavior', () => {
    it('should redirect to dashboard when accessing root', () => {
      const rootRoute = routes.find((r) => r.path === '');
      const emptyPathRoute = rootRoute?.children?.find((c) => c.path === '');
      expect(emptyPathRoute?.redirectTo).toBe('dashboard');
      expect(emptyPathRoute?.pathMatch).toBe('full');
    });
  });

  describe('catch-all route', () => {
    it('should have catch-all route for unknown paths', () => {
      const catchAllRoute = routes.find((r) => r.path === '**');
      expect(catchAllRoute).toBeDefined();
    });

    it('should load NotFoundComponent for catch-all route', () => {
      const catchAllRoute = routes.find((r) => r.path === '**');
      expect(catchAllRoute?.loadComponent).toBeDefined();
    });
  });

  describe('route metadata', () => {
    it('should have breadcrumb data for root route', () => {
      const rootRoute = routes.find((r) => r.path === '');
      expect(rootRoute?.data?.['breadcrumb']).toBe('Home');
      expect(rootRoute?.data?.['breadcrumbIcon']).toBe('🏠');
    });

    it('should have metadata for dashboard route', () => {
      const rootRoute = routes.find((r) => r.path === '');
      const dashboardRoute = rootRoute?.children?.find((c) => c.path === 'dashboard');
      expect(dashboardRoute?.data?.['title']).toBe('Dashboard');
      expect(dashboardRoute?.data?.['description']).toBeDefined();
    });

    it('should have SEO metadata for login route', () => {
      const loginRoute = routes.find((r) => r.path === 'login');
      expect(loginRoute?.data?.['title']).toBe('Admin Login');
      expect(loginRoute?.data?.['description']).toBeDefined();
      expect(loginRoute?.data?.['keywords']).toBeDefined();
    });
  });

  describe('lazy loading configuration', () => {
    it('should use loadComponent for login route', () => {
      const loginRoute = routes.find((r) => r.path === 'login');
      expect(loginRoute?.loadComponent).toBeDefined();
      expect(typeof loginRoute?.loadComponent).toBe('function');
    });

    it('should use loadComponent for admin layout', () => {
      const rootRoute = routes.find((r) => r.path === '');
      expect(rootRoute?.loadComponent).toBeDefined();
      expect(typeof rootRoute?.loadComponent).toBe('function');
    });

    it('should use loadComponent for dashboard route', () => {
      const rootRoute = routes.find((r) => r.path === '');
      const dashboardRoute = rootRoute?.children?.find((c) => c.path === 'dashboard');
      expect(dashboardRoute?.loadComponent).toBeDefined();
      expect(typeof dashboardRoute?.loadComponent).toBe('function');
    });
  });

  describe('route order and precedence', () => {
    it('should have login route before protected routes', () => {
      const loginIndex = routes.findIndex((r) => r.path === 'login');
      const rootIndex = routes.findIndex((r) => r.path === '');
      expect(loginIndex).toBeLessThan(rootIndex);
    });

    it('should have error routes before catch-all', () => {
      const errorIndex = routes.findIndex((r) => r.path === 'error');
      const catchAllIndex = routes.findIndex((r) => r.path === '**');
      expect(errorIndex).toBeLessThan(catchAllIndex);
    });
  });
});
