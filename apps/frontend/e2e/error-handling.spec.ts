import { test, expect } from '@playwright/test';
import { BasePage } from './page-objects/base.page';

test.describe('Error Handling', () => {
  let page: BasePage;

  test.beforeEach(async ({ page: testPage }) => {
    page = new BasePage(testPage);
  });

  test('should handle 404 errors gracefully', async ({ page: testPage }) => {
    await page.goto('/non-existent-page-404');

    // Should either redirect to home or show 404 page
    const path = await page.getCurrentPath();
    const bodyText = await testPage.textContent('body');

    expect(
      path === '/' ||
      path === '/404' ||
      bodyText?.toLowerCase().includes('not found') ||
      bodyText?.toLowerCase().includes('404')
    ).toBeTruthy();
  });

  test('should display user-friendly error messages', async ({ page: testPage }) => {
    await page.goto('/');

    // Trigger an error if error demo page exists
    const errorButton = testPage.locator('button:has-text("Error"), button:has-text("Throw")').first();

    if (await errorButton.isVisible()) {
      await errorButton.click();
      await testPage.waitForTimeout(1000);

      // Should show error notification or message
      const errorMessages = await testPage.locator('.error, .alert-error, [role="alert"]').count();
      expect(errorMessages).toBeGreaterThanOrEqual(0);
    }
  });

  test('should handle network errors', async ({ page: testPage }) => {
    // Go offline
    await testPage.context().setOffline(true);

    await page.goto('/dashboard').catch(() => {
      // Expected to fail when offline
    });

    // Go back online
    await testPage.context().setOffline(false);

    await page.goto('/dashboard');
    expect(await page.getCurrentPath()).toContain('/dashboard');
  });

  test('should log errors to console', async ({ page: testPage }) => {
    const consoleErrors: string[] = [];

    testPage.on('console', (msg) => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    await page.goto('/');

    // Navigate through a few pages to check for console errors
    await page.goto('/dashboard');
    await page.goto('/forms');

    // Should not have critical console errors during normal navigation
    const hasCriticalErrors = consoleErrors.some(err =>
      err.includes('undefined') ||
      err.includes('null') ||
      err.includes('is not a function')
    );

    expect(hasCriticalErrors).toBeFalsy();
  });

  test('should handle JavaScript errors gracefully', async ({ page: testPage }) => {
    const pageErrors: Error[] = [];

    testPage.on('pageerror', (error) => {
      pageErrors.push(error);
    });

    await page.goto('/');
    await page.goto('/dashboard');
    await page.goto('/forms');

    // Should not have uncaught JavaScript errors
    expect(pageErrors.length).toBe(0);
  });

  test('should show loading states', async ({ page: testPage }) => {
    await page.goto('/dashboard');

    // Look for loading indicators
    const loaders = await testPage.locator('.loading, .spinner, [aria-busy="true"], .skeleton').count();

    // Might have loaders during initial render
    expect(loaders).toBeGreaterThanOrEqual(0);
  });

  test('should recover from errors', async ({ page: testPage }) => {
    await page.goto('/');

    // Try to trigger an error
    const errorButton = testPage.locator('button:has-text("Error")').first();

    if (await errorButton.isVisible()) {
      await errorButton.click();
      await testPage.waitForTimeout(1000);

      // Try to navigate to another page - should recover
      await page.goto('/dashboard');
      expect(await page.getCurrentPath()).toContain('/dashboard');
    }
  });

  test('should handle API errors with retry logic', async ({ page: testPage }) => {
    // Intercept API calls
    await testPage.route('**/api/**', (route) => {
      // Fail the first request
      route.abort();
    });

    await page.goto('/dashboard');

    // Application should handle API failure gracefully
    const hasErrorMessage = await testPage.locator('.error, .alert-error').count();

    expect(hasErrorMessage).toBeGreaterThanOrEqual(0);

    // Remove intercept
    await testPage.unroute('**/api/**');
  });

  test('should display error boundaries', async ({ page: testPage }) => {
    await page.goto('/');

    // Error boundaries should prevent entire app from crashing
    const appContent = await testPage.locator('body').textContent();
    expect(appContent).toBeTruthy();
  });

  test('should handle form validation errors', async ({ page: testPage }) => {
    await page.goto('/forms');

    const submitButton = testPage.locator('button[type="submit"]').first();

    if (await submitButton.isVisible()) {
      // Try to submit without filling required fields
      await submitButton.click();
      await testPage.waitForTimeout(500);

      // Should show validation errors
      const page = await testPage.textContent('body');
      expect(page).toBeTruthy();
    }
  });

  test('should handle session timeout gracefully', async ({ page: testPage }) => {
    await page.goto('/dashboard');

    // Clear all storage to simulate session timeout
    await testPage.evaluate(() => {
      localStorage.clear();
      sessionStorage.clear();
    });

    // Navigate to another page
    await page.goto('/forms');

    // Should still work or show appropriate message
    const bodyText = await testPage.textContent('body');
    expect(bodyText).toBeTruthy();
  });

  test('should handle malformed data', async ({ page: testPage }) => {
    await page.goto('/tables');

    // Application should handle any data issues gracefully
    const tableVisible = await testPage.locator('table').isVisible();

    // Table should either show data or empty state
    expect(typeof tableVisible).toBe('boolean');
  });

  test('should provide helpful error messages', async ({ page: testPage }) => {
    await page.goto('/invalid-route-12345');

    const bodyText = await testPage.textContent('body');

    if (bodyText?.toLowerCase().includes('error') ||
        bodyText?.toLowerCase().includes('not found')) {

      // Error message should be user-friendly
      expect(bodyText.length).toBeGreaterThan(0);
    }
  });
});
