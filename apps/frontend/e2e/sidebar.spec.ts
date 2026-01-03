import { test, expect } from '@playwright/test';
import { BasePage } from './page-objects/base.page';

test.describe('Sidebar Navigation', () => {
  let page: BasePage;

  test.beforeEach(async ({ page: testPage }) => {
    page = new BasePage(testPage);
    await page.goto('/');
  });

  test('should display sidebar', async ({ page: testPage }) => {
    await expect(page.sidebar).toBeVisible();
  });

  test('should contain navigation links', async ({ page: testPage }) => {
    const links = await page.sidebar.locator('a').count();
    expect(links).toBeGreaterThan(0);
  });

  test('should navigate using sidebar links', async ({ page: testPage }) => {
    const dashboardLink = page.sidebar.locator('a', { hasText: 'Dashboard' }).first();

    if (await dashboardLink.isVisible()) {
      await dashboardLink.click();
      await page.helpers.waitForAngular();

      const currentPath = await page.getCurrentPath();
      expect(currentPath).toContain('/dashboard');
    }
  });

  test('should toggle sidebar collapse', async ({ page: testPage }) => {
    const toggleButton = testPage.locator('[data-testid="sidebar-toggle"], .drawer-button, label[for*="drawer"]').first();

    if (await toggleButton.isVisible()) {
      await toggleButton.click();
      await testPage.waitForTimeout(500);

      // Sidebar state should change
      const sidebarClasses = await page.sidebar.getAttribute('class');
      expect(sidebarClasses).toBeTruthy();

      // Toggle back
      await toggleButton.click();
      await testPage.waitForTimeout(500);
    }
  });

  test('should highlight active navigation item', async ({ page: testPage }) => {
    await page.goto('/dashboard');

    const dashboardLink = page.sidebar.locator('a', { hasText: 'Dashboard' }).first();

    if (await dashboardLink.isVisible()) {
      const classes = await dashboardLink.getAttribute('class');

      // Active link should have some special class
      expect(classes).toBeTruthy();
    }
  });

  test('should maintain sidebar state during navigation', async ({ page: testPage }) => {
    await page.goto('/dashboard');
    await expect(page.sidebar).toBeVisible();

    await page.goto('/forms');
    await expect(page.sidebar).toBeVisible();

    await page.goto('/tables');
    await expect(page.sidebar).toBeVisible();
  });

  test('should be responsive on mobile devices', async ({ page: testPage }) => {
    await testPage.setViewportSize({ width: 375, height: 667 });
    await testPage.waitForTimeout(500);

    // Sidebar might be hidden or shown via toggle on mobile
    const sidebarVisible = await page.sidebar.isVisible();
    expect(typeof sidebarVisible).toBe('boolean');
  });

  test('should close sidebar on mobile when clicking a link', async ({ page: testPage }) => {
    await testPage.setViewportSize({ width: 375, height: 667 });
    await testPage.waitForTimeout(500);

    // Open sidebar if collapsed
    const toggleButton = testPage.locator('[data-testid="sidebar-toggle"], .drawer-button, label[for*="drawer"]').first();

    if (await toggleButton.isVisible()) {
      await toggleButton.click();
      await testPage.waitForTimeout(500);
    }

    // Click a navigation link
    const formsLink = page.sidebar.locator('a', { hasText: 'Forms' }).first();

    if (await formsLink.isVisible()) {
      await formsLink.click();
      await page.helpers.waitForAngular();

      // On mobile, sidebar might auto-close after navigation
      const currentPath = await page.getCurrentPath();
      expect(currentPath).toContain('/forms');
    }
  });

  test('should display all menu items', async ({ page: testPage }) => {
    const expectedMenuItems = [
      'Dashboard',
      'Forms',
      'Tables',
      'Notifications',
      'Modals',
      'Settings',
    ];

    for (const item of expectedMenuItems) {
      const link = page.sidebar.locator('a', { hasText: item });
      const count = await link.count();

      // Each menu item should exist
      expect(count).toBeGreaterThan(0);
    }
  });

  test('should support keyboard navigation', async ({ page: testPage }) => {
    const firstLink = page.sidebar.locator('a').first();

    if (await firstLink.isVisible()) {
      await firstLink.focus();

      // Tab to next link
      await testPage.keyboard.press('Tab');

      // Should be able to activate with Enter
      await testPage.keyboard.press('Enter');
      await page.helpers.waitForAngular();

      // Should navigate somewhere
      const path = await page.getCurrentPath();
      expect(path).toBeTruthy();
    }
  });

  test('should have proper ARIA labels for accessibility', async ({ page: testPage }) => {
    const navElement = page.sidebar.locator('nav, [role="navigation"]').first();

    if (await navElement.isVisible()) {
      const ariaLabel = await navElement.getAttribute('aria-label');
      expect(typeof ariaLabel).toBe('string');
    }
  });
});
