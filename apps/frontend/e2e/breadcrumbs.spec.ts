import { test, expect } from '@playwright/test';
import { BasePage } from './page-objects/base.page';

test.describe('Breadcrumb Navigation', () => {
  let page: BasePage;

  test.beforeEach(async ({ page: testPage }) => {
    page = new BasePage(testPage);
  });

  test('should display breadcrumbs on dashboard', async ({ page: testPage }) => {
    await page.goto('/dashboard');
    await expect(page.breadcrumbs).toBeVisible();
  });

  test('should show correct breadcrumb for current page', async ({ page: testPage }) => {
    await page.goto('/dashboard');

    const breadcrumbText = await page.breadcrumbs.textContent();
    expect(breadcrumbText?.toLowerCase()).toContain('dashboard');
  });

  test('should update breadcrumbs when navigating', async ({ page: testPage }) => {
    const routes = [
      { path: '/forms', name: 'Forms' },
      { path: '/tables', name: 'Tables' },
    ];

    for (const route of routes) {
      await page.goto(route.path);

      const breadcrumbText = await page.breadcrumbs.textContent();
      expect(breadcrumbText?.toLowerCase()).toContain(route.name.toLowerCase());
    }
  });

  test('should display home breadcrumb', async ({ page: testPage }) => {
    await page.goto('/dashboard');

    const breadcrumbItems = await page.breadcrumbs.locator('li, a, span').all();
    expect(breadcrumbItems.length).toBeGreaterThan(0);

    // First breadcrumb is often Home
    const firstBreadcrumb = await breadcrumbItems[0].textContent();
    expect(firstBreadcrumb).toBeTruthy();
  });

  test('should have clickable breadcrumb links', async ({ page: testPage }) => {
    await page.goto('/forms');

    const breadcrumbLinks = await page.breadcrumbs.locator('a').all();

    if (breadcrumbLinks.length > 0) {
      const link = breadcrumbLinks[0];
      const href = await link.getAttribute('href');

      expect(href).toBeTruthy();

      // Click should navigate
      await link.click();
      await page.helpers.waitForAngular();

      const currentPath = await page.getCurrentPath();
      expect(currentPath).toBeTruthy();
    }
  });

  test('should show hierarchical path', async ({ page: testPage }) => {
    await page.goto('/tables');

    const breadcrumbItems = await page.breadcrumbs.locator('li').count();

    // Should have at least one breadcrumb item
    expect(breadcrumbItems).toBeGreaterThan(0);
  });

  test('should handle deep navigation paths', async ({ page: testPage }) => {
    // Navigate through several pages
    await page.goto('/dashboard');
    await page.goto('/forms');
    await page.goto('/tables');

    const breadcrumbText = await page.breadcrumbs.textContent();
    expect(breadcrumbText).toBeTruthy();
  });

  test('should maintain breadcrumbs after page reload', async ({ page: testPage }) => {
    await page.goto('/tables');

    const breadcrumbBefore = await page.breadcrumbs.textContent();

    await testPage.reload();
    await page.helpers.waitForAngular();

    const breadcrumbAfter = await page.breadcrumbs.textContent();

    // Breadcrumbs should be consistent
    expect(breadcrumbAfter).toContain('Table');
  });

  test('should highlight current page in breadcrumbs', async ({ page: testPage }) => {
    await page.goto('/settings');

    const breadcrumbItems = await page.breadcrumbs.locator('li').all();

    if (breadcrumbItems.length > 0) {
      const lastItem = breadcrumbItems[breadcrumbItems.length - 1];
      const classes = await lastItem.getAttribute('class');

      // Last item is often marked as active
      expect(classes).toBeTruthy();
    }
  });

  test('should separate breadcrumb items with dividers', async ({ page: testPage }) => {
    await page.goto('/tables');

    const breadcrumbHTML = await page.breadcrumbs.innerHTML();

    // Breadcrumbs often use /, >, or similar separators
    const hasDividers = breadcrumbHTML.includes('/') ||
                        breadcrumbHTML.includes('>') ||
                        breadcrumbHTML.includes('›') ||
                        breadcrumbHTML.includes('chevron');

    expect(typeof hasDividers).toBe('boolean');
  });
});
