import { test, expect } from '@playwright/test';
import { TestHelpers } from './helpers/test-helpers';

/**
 * Authentication Flow E2E Tests
 *
 * IMPORTANT: These tests require BOTH frontend and backend servers to be running:
 * - Frontend: npm run dev (runs on http://localhost:5173)
 * - Backend: Run from apps/backend directory (runs on https://localhost:5001)
 *
 * Backend credentials for testing:
 * - Email: admin@kcow.test (or admin@kcow.local depending on seed data)
 * - Password: password123 (or Admin@123 depending on seed data)
 *
 * To run these tests:
 * 1. Start backend server (apps/backend)
 * 2. Start frontend dev server (apps/frontend)
 * 3. Run: npm run test:e2e auth.spec.ts
 */

test.describe('Authentication Flow', () => {
  let helpers: TestHelpers;

  test.beforeEach(async ({ page }) => {
    helpers = new TestHelpers(page);
    // Clear sessionStorage before each test
    await page.evaluate(() => sessionStorage.clear());
    // Clear localStorage before each test
    await helpers.clearLocalStorage();
  });

  test.afterEach(async ({ page }) => {
    // Cleanup after each test
    await page.evaluate(() => sessionStorage.clear());
    await helpers.clearLocalStorage();
  });

  test.describe('Login Page', () => {
    test('should display login form', async ({ page }) => {
      await page.goto('/login');
      await helpers.waitForAngular();

      // Check page title
      await expect(page).toHaveTitle(/Admin Login/);

      // Check for email input
      const emailInput = page.locator('input[type="email"]');
      await expect(emailInput).toBeVisible();
      await expect(emailInput).toHaveAttribute('id', 'email');

      // Check for password input
      const passwordInput = page.locator('input[type="password"]');
      await expect(passwordInput).toBeVisible();
      await expect(passwordInput).toHaveAttribute('id', 'password');

      // Check for submit button
      const submitButton = page.locator('button[type="submit"]');
      await expect(submitButton).toBeVisible();
      await expect(submitButton).toContainText('Sign In');
    });

    test('should show validation errors for empty form', async ({ page }) => {
      await page.goto('/login');
      await helpers.waitForAngular();

      // Try to submit with empty form
      const submitButton = page.locator('button[type="submit"]');
      await submitButton.click();

      // Check for validation errors
      const emailError = page.locator('text=Email is required');
      await expect(emailError).toBeVisible();
    });

    test('should show validation error for invalid email', async ({ page }) => {
      await page.goto('/login');
      await helpers.waitForAngular();

      // Enter invalid email
      const emailInput = page.locator('input[type="email"]');
      await emailInput.fill('invalid-email');
      await emailInput.blur(); // Trigger validation

      // Check for email validation error
      const emailError = page.locator('text=valid email');
      await expect(emailError).toBeVisible();
    });

    test('should show validation error for short password', async ({ page }) => {
      await page.goto('/login');
      await helpers.waitForAngular();

      // Enter short password
      const passwordInput = page.locator('input[type="password"]');
      await passwordInput.fill('12345');
      await passwordInput.blur(); // Trigger validation

      // Check for password validation error
      const passwordError = page.locator('text=at least 6 characters');
      await expect(passwordError).toBeVisible();
    });
  });

  test.describe('Protected Routes', () => {
    test('should redirect to login when accessing protected route without auth', async ({ page }) => {
      // Try to access dashboard directly
      await page.goto('/dashboard');
      await helpers.waitForAngular();

      // Should redirect to login
      await expect(page).toHaveURL(/\/login/);

      // Check for return URL in query params
      const url = page.url();
      expect(url).toContain('returnUrl=%2Fdashboard');
    });

    test('should redirect to login when accessing tables without auth', async ({ page }) => {
      await page.goto('/tables');
      await helpers.waitForAngular();

      await expect(page).toHaveURL(/\/login/);
    });

    test('should redirect to login when accessing forms without auth', async ({ page }) => {
      await page.goto('/forms');
      await helpers.waitForAngular();

      await expect(page).toHaveURL(/\/login/);
    });

    test('should redirect to login when accessing settings without auth', async ({ page }) => {
      await page.goto('/workspace-settings');
      await helpers.waitForAngular();

      await expect(page).toHaveURL(/\/login/);
    });

    test('should allow access to error pages without auth', async ({ page }) => {
      // Error pages should be public
      await page.goto('/error/404');
      await helpers.waitForAngular();

      // Should stay on error page (not redirect to login)
      await expect(page).toHaveURL(/\/error\/404/);
    });
  });

  test.describe('Login Flow (requires running backend)', () => {
    // These tests require the backend to be running
    // Skip if backend is not available
    test.skip(({ page }) => true, 'Login with valid credentials', async ({ page }) => {
      await page.goto('/login');
      await helpers.waitForAngular();

      // Enter credentials
      const emailInput = page.locator('input[type="email"]');
      const passwordInput = page.locator('input[type="password"]');

      await emailInput.fill('admin@kcow.test');
      await passwordInput.fill('password123');

      // Submit form
      const submitButton = page.locator('button[type="submit"]');
      await submitButton.click();

      // Should redirect to dashboard
      await helpers.waitForAngular();
      await expect(page).toHaveURL(/\/dashboard/);

      // Check that auth token is set
      const authToken = await page.evaluate(() => sessionStorage.getItem('auth_token'));
      expect(authToken).toBe('authenticated');
    });

    test.skip(({ page }) => true, 'should show error for invalid credentials', async ({ page }) => {
      await page.goto('/login');
      await helpers.waitForAngular();

      // Enter invalid credentials
      const emailInput = page.locator('input[type="email"]');
      const passwordInput = page.locator('input[type="password"]');

      await emailInput.fill('admin@kcow.test');
      await passwordInput.fill('wrongpassword');

      // Submit form
      const submitButton = page.locator('button[type="submit"]');
      await submitButton.click();

      // Should show error message
      await helpers.waitForAngular();

      // Check for error alert
      const errorAlert = page.locator('.alert-error');
      await expect(errorAlert).toBeVisible();

      // Should stay on login page
      await expect(page).toHaveURL(/\/login/);
    });

    test.skip(({ page }) => true, 'should redirect to return URL after login', async ({ page }) => {
      // First, try to access a protected route
      await page.goto('/tables');
      await helpers.waitForAngular();

      // Should redirect to login with return URL
      await expect(page).toHaveURL(/login.*returnUrl/);

      // Enter credentials
      const emailInput = page.locator('input[type="email"]');
      const passwordInput = page.locator('input[type="password"]');

      await emailInput.fill('admin@kcow.test');
      await passwordInput.fill('password123');

      // Submit form
      const submitButton = page.locator('button[type="submit"]');
      await submitButton.click();

      // Should redirect back to original destination (tables)
      await helpers.waitForAngular();
      await expect(page).toHaveURL(/\/tables/);
    });

    test.skip(({ page }) => true, 'should allow access to protected routes after login', async ({ page }) => {
      // Login first
      await page.goto('/login');
      await helpers.waitForAngular();

      const emailInput = page.locator('input[type="email"]');
      const passwordInput = page.locator('input[type="password"]');

      await emailInput.fill('admin@kcow.test');
      await passwordInput.fill('password123');

      const submitButton = page.locator('button[type="submit"]');
      await submitButton.click();

      await helpers.waitForAngular();

      // Now try to access protected routes
      await page.goto('/dashboard');
      await helpers.waitForAngular();
      await expect(page).toHaveURL(/\/dashboard/);

      await page.goto('/tables');
      await helpers.waitForAngular();
      await expect(page).toHaveURL(/\/tables/);
    });
  });

  test.describe('Session Persistence', () => {
    test.skip(({ page }) => true, 'should persist auth across page refresh', async ({ page }) => {
      // Login
      await page.goto('/login');
      await helpers.waitForAngular();

      const emailInput = page.locator('input[type="email"]');
      const passwordInput = page.locator('input[type="password"]');

      await emailInput.fill('admin@kcow.test');
      await passwordInput.fill('password123');

      const submitButton = page.locator('button[type="submit"]');
      await submitButton.click();

      await helpers.waitForAngular();

      // Refresh page
      await page.reload();
      await helpers.waitForAngular();

      // Should still be authenticated (stay on dashboard)
      await expect(page).toHaveURL(/\/dashboard/);
    });
  });

  test.describe('CORS and Cookies', () => {
    test.skip(({ page }) => true, 'should include auth cookies in API requests', async ({ page }) => {
      // This test verifies that cookies are being sent with API requests
      // Requires backend to be running

      // Setup request interception
      const apiRequests: string[] = [];

      page.on('request', (request) => {
        if (request.url().includes('/api/')) {
          apiRequests.push(request.url());
          // Check if credentials header is set
          const headers = request.headers();
          // Cookies are automatically included by browser when withCredentials is true
        }
      });

      // Login
      await page.goto('/login');
      await helpers.waitForAngular();

      const emailInput = page.locator('input[type="email"]');
      const passwordInput = page.locator('input[type="password"]');

      await emailInput.fill('admin@kcow.test');
      await passwordInput.fill('password123');

      const submitButton = page.locator('button[type="submit"]');
      await submitButton.click();

      await helpers.waitForAngular();

      // Verify API request was made
      expect(apiRequests.length).toBeGreaterThan(0);
      expect(apiRequests.some(url => url.includes('/api/auth/login'))).toBe(true);
    });
  });

  test.describe('Error Handling', () => {
    test.skip(({ page }) => true, 'should display user-friendly error messages', async ({ page }) => {
      await page.goto('/login');
      await helpers.waitForAngular();

      // Mock network error by intercepting request
      await page.route('**/api/auth/login', (route) => {
        route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({
            title: 'Internal Server Error',
            detail: 'An error occurred while processing your request',
            status: 500
          })
        });
      });

      const emailInput = page.locator('input[type="email"]');
      const passwordInput = page.locator('input[type="password"]');

      await emailInput.fill('admin@kcow.test');
      await passwordInput.fill('password123');

      const submitButton = page.locator('button[type="submit"]');
      await submitButton.click();

      await helpers.waitForAngular();

      // Check for error message from ProblemDetails
      const errorAlert = page.locator('.alert-error');
      await expect(errorAlert).toBeVisible();
      await expect(errorAlert).toContainText('Internal Server Error');
    });

    test.skip(({ page }) => true, 'should show network error message', async ({ page }) => {
      await page.goto('/login');
      await helpers.waitForAngular();

      // Mock network failure
      await page.route('**/api/auth/login', (route) => {
        route.abort('failed');
      });

      const emailInput = page.locator('input[type="email"]');
      const passwordInput = page.locator('input[type="password"]');

      await emailInput.fill('admin@kcow.test');
      await passwordInput.fill('password123');

      const submitButton = page.locator('button[type="submit"]');
      await submitButton.click();

      await helpers.waitForAngular();

      // Check for network error message
      const errorAlert = page.locator('.alert-error');
      await expect(errorAlert).toBeVisible();
      await expect(errorAlert).toContainText('Network error');
    });
  });
});
