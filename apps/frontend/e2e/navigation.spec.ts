import { test, expect } from '@playwright/test';
import { BasePage } from './page-objects/base.page';

test.describe('Navigation and Routing', () => {
  let page: BasePage;

  test.beforeEach(async ({ page: testPage }) => {
    page = new BasePage(testPage);
    await page.goto('/');
  });

  test('should load the home page', async ({ page: testPage }) => {
    await expect(testPage).toHaveURL('/');
    await expect(testPage).toHaveTitle(/Angular Admin Template/i);
  });

  test('should navigate to dashboard', async ({ page: testPage }) => {
    await page.clickSidebarItem('Dashboard');
    expect(await page.getCurrentPath()).toContain('/dashboard');
  });

  test('should navigate to all feature pages via sidebar', async ({ page: testPage }) => {
    const routes = [
      { name: 'Dashboard', path: '/dashboard' },
      { name: 'Forms', path: '/forms' },
      { name: 'Tables', path: '/tables' },
      { name: 'Notifications', path: '/notifications' },
      { name: 'Modals', path: '/modals' },
      { name: 'Settings', path: '/settings' },
    ];

    for (const route of routes) {
      await page.goto('/');
      await page.clickSidebarItem(route.name);
      const currentPath = await page.getCurrentPath();
      expect(currentPath).toContain(route.path);
    }
  });

  test('should display correct breadcrumbs on navigation', async ({ page: testPage }) => {
    await page.goto('/dashboard');
    await expect(page.breadcrumbs).toBeVisible();

    const breadcrumbText = await page.breadcrumbs.textContent();
    expect(breadcrumbText).toContain('Dashboard');
  });

  test('should update breadcrumbs when navigating between pages', async ({ page: testPage }) => {
    await page.goto('/forms');
    let breadcrumbText = await page.breadcrumbs.textContent();
    expect(breadcrumbText).toContain('Forms');

    await page.clickSidebarItem('Tables');
    breadcrumbText = await page.breadcrumbs.textContent();
    expect(breadcrumbText).toContain('Tables');
  });

  test('should handle browser back and forward navigation', async ({ page: testPage }) => {
    // Navigate through several pages
    await page.goto('/dashboard');
    await page.goto('/forms');
    await page.goto('/tables');

    // Go back
    await testPage.goBack();
    expect(await page.getCurrentPath()).toContain('/forms');

    // Go back again
    await testPage.goBack();
    expect(await page.getCurrentPath()).toContain('/dashboard');

    // Go forward
    await testPage.goForward();
    expect(await page.getCurrentPath()).toContain('/forms');
  });

  test('should maintain sidebar state during navigation', async ({ page: testPage }) => {
    await page.goto('/dashboard');
    await expect(page.sidebar).toBeVisible();

    await page.goto('/forms');
    await expect(page.sidebar).toBeVisible();

    await page.goto('/tables');
    await expect(page.sidebar).toBeVisible();
  });

  test('should handle deep linking', async ({ page: testPage }) => {
    await page.goto('/tables');
    expect(await page.getCurrentPath()).toBe('/tables');
    await expect(testPage.locator('table')).toBeVisible({ timeout: 5000 });
  });

  test('should redirect to home on invalid route', async ({ page: testPage }) => {
    await page.goto('/invalid-route-that-does-not-exist');
    // Should either redirect to home or show 404 page
    const path = await page.getCurrentPath();
    expect(path === '/' || path === '/404' || path.includes('not-found')).toBeTruthy();
  });
});
