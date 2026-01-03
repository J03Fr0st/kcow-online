import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, type Routes } from '@angular/router';
import { BreadcrumbService } from './breadcrumb.service';

@Component({ template: '' })
class DummyComponent {}

describe('BreadcrumbService', () => {
  let service: BreadcrumbService;
  let router: Router;

  const testRoutes: Routes = [
    {
      path: '',
      component: DummyComponent,
      data: { breadcrumb: 'Home', breadcrumbIcon: '🏠' },
      children: [
        {
          path: 'dashboard',
          component: DummyComponent,
          data: {
            breadcrumb: 'Dashboard',
            breadcrumbIcon: '📊',
          },
        },
        {
          path: 'settings',
          component: DummyComponent,
          data: {
            breadcrumb: 'Settings',
            breadcrumbIcon: '⚙️',
          },
          children: [
            {
              path: 'profile',
              component: DummyComponent,
              data: {
                breadcrumb: 'Profile',
                breadcrumbIcon: '👤',
              },
            },
          ],
        },
      ],
    },
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [BreadcrumbService, provideRouter(testRoutes)],
    });
    service = TestBed.inject(BreadcrumbService);
    router = TestBed.inject(Router);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should build breadcrumbs from route data', async () => {
    await router.navigate(['/dashboard']);

    const breadcrumbs = service.getBreadcrumbs();
    expect(breadcrumbs.length).toBe(2);
    expect(breadcrumbs[0].label).toBe('Home');
    expect(breadcrumbs[0].icon).toBe('🏠');
    expect(breadcrumbs[1].label).toBe('Dashboard');
    expect(breadcrumbs[1].icon).toBe('📊');
  });

  it('should build nested breadcrumbs', async () => {
    await router.navigate(['/settings/profile']);

    const breadcrumbs = service.getBreadcrumbs();
    expect(breadcrumbs.length).toBe(3);
    expect(breadcrumbs[0].label).toBe('Home');
    expect(breadcrumbs[1].label).toBe('Settings');
    expect(breadcrumbs[2].label).toBe('Profile');
  });

  it('should update breadcrumbs on navigation', async () => {
    await router.navigate(['/dashboard']);
    let breadcrumbs = service.getBreadcrumbs();
    expect(breadcrumbs.length).toBe(2);

    await router.navigate(['/settings']);
    breadcrumbs = service.getBreadcrumbs();
    expect(breadcrumbs.length).toBe(2);
    expect(breadcrumbs[1].label).toBe('Settings');
  });

  it('should get current page breadcrumb', async () => {
    await router.navigate(['/settings/profile']);

    const currentPage = service.currentPage();
    expect(currentPage).toBeTruthy();
    expect(currentPage?.label).toBe('Profile');
    expect(currentPage?.icon).toBe('👤');
  });

  it('should allow setting custom breadcrumbs', () => {
    const customCrumbs = [
      { label: 'Custom', url: '/custom', icon: '🎯' },
      { label: 'Page', url: '/custom/page' },
    ];

    service.setBreadcrumbs(customCrumbs);
    const breadcrumbs = service.getBreadcrumbs();

    expect(breadcrumbs.length).toBe(2);
    expect(breadcrumbs[0].label).toBe('Custom');
    expect(breadcrumbs[1].label).toBe('Page');
  });

  it('should clear custom breadcrumbs', async () => {
    const customCrumbs = [{ label: 'Custom', url: '/custom' }];
    service.setBreadcrumbs(customCrumbs);

    expect(service.getBreadcrumbs().length).toBe(1);

    service.clearCustomBreadcrumbs();
    await router.navigate(['/dashboard']);

    const breadcrumbs2 = service.getBreadcrumbs();
    expect(breadcrumbs2[0].label).toBe('Home');
  });

  it('should navigate to breadcrumb URL', async () => {
    const navigateSpy = jest.spyOn(router, 'navigate');
    const breadcrumb = { label: 'Test', url: '/test' };

    service.navigateTo(breadcrumb);

    expect(navigateSpy).toHaveBeenCalledWith(['/test']);
  });

  it('should build breadcrumb URLs correctly', async () => {
    await router.navigate(['/settings/profile']);

    const breadcrumbs = service.getBreadcrumbs();
    expect(breadcrumbs[0].url).toBe('/');
    expect(breadcrumbs[1].url).toBe('/settings');
    expect(breadcrumbs[2].url).toBe('/settings/profile');
  });

  it('should handle routes without breadcrumb data', async () => {
    await TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        BreadcrumbService,
        provideRouter([
          {
            path: 'no-breadcrumb',
            component: DummyComponent,
          },
        ]),
      ],
    });

    service = TestBed.inject(BreadcrumbService);
    router = TestBed.inject(Router);

    await router.navigate(['/no-breadcrumb']);
    const breadcrumbs = service.getBreadcrumbs();

    expect(breadcrumbs.length).toBe(0);
  });
});
