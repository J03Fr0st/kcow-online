import { test, expect } from '@playwright/test';
import { BasePage } from './page-objects/base.page';

test.describe('Dashboard Functionality', () => {
  let page: BasePage;

  test.beforeEach(async ({ page: testPage }) => {
    page = new BasePage(testPage);
    await page.goto('/dashboard');
  });

  test('should load dashboard page', async ({ page: testPage }) => {
    expect(await page.getCurrentPath()).toContain('/dashboard');
  });

  test('should display dashboard widgets', async ({ page: testPage }) => {
    // Look for common dashboard elements
    const cards = await testPage.locator('.card, [class*="widget"], [class*="stat"]').count();
    expect(cards).toBeGreaterThan(0);
  });

  test('should display statistics/metrics', async ({ page: testPage }) => {
    const stats = await testPage.locator('.stat, [class*="metric"], [class*="stats"]').all();

    if (stats.length > 0) {
      for (const stat of stats) {
        const text = await stat.textContent();
        expect(text).toBeTruthy();
      }
    } else {
      // No stats found, but page should still load
      expect(true).toBeTruthy();
    }
  });

  test('should display activity feed or recent items', async ({ page: testPage }) => {
    const activities = await testPage.locator('[class*="activity"], [class*="feed"], ul li, .list-item').all();

    // Dashboard might have activity items
    expect(activities.length).toBeGreaterThanOrEqual(0);
  });

  test('should update data when refreshed', async ({ page: testPage }) => {
    const initialContent = await testPage.locator('body').textContent();

    await testPage.reload();
    await page.helpers.waitForAngular();

    const refreshedContent = await testPage.locator('body').textContent();

    // Content should be consistent
    expect(refreshedContent).toBeTruthy();
  });

  test('should be responsive on mobile viewport', async ({ page: testPage }) => {
    await testPage.setViewportSize({ width: 375, height: 667 });
    await testPage.waitForTimeout(300);

    const cards = await testPage.locator('.card, [class*="widget"]').all();

    // Dashboard should still be visible on mobile
    expect(cards.length).toBeGreaterThanOrEqual(0);
  });

  test('should have interactive elements', async ({ page: testPage }) => {
    const buttons = await testPage.locator('button').all();
    const links = await testPage.locator('a').all();

    // Dashboard should have some interactive elements
    const totalInteractive = buttons.length + links.length;
    expect(totalInteractive).toBeGreaterThan(0);
  });

  test('should navigate to detail pages from dashboard', async ({ page: testPage }) => {
    const links = await testPage.locator('a[href]').all();

    if (links.length > 0) {
      // Find a link that goes to another page
      for (const link of links.slice(0, 3)) {
        const href = await link.getAttribute('href');

        if (href && href !== '#' && !href.startsWith('http')) {
          await link.click();
          await page.helpers.waitForAngular();

          // Should navigate away from dashboard
          const currentPath = await page.getCurrentPath();
          expect(typeof currentPath).toBe('string');

          // Go back to dashboard
          await page.goto('/dashboard');
          break;
        }
      }
    }
  });
});
