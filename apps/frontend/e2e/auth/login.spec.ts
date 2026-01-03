import { test, expect } from '@playwright/test';

test.describe('Login Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
  });

  test('should display login page with form', async ({ page }) => {
    // Check page title and form elements
    await expect(page.locator('h1')).toContainText('Admin Login');
    await expect(page.locator('input[type="email"]')).toBeVisible();
    await expect(page.locator('input[type="password"]')).toBeVisible();
    await expect(page.locator('button[type="submit"]')).toBeVisible();
  });

  test('should show validation errors for empty form', async ({ page }) => {
    const emailInput = page.locator('input[type="email"]');
    const passwordInput = page.locator('input[type="password"]');
    const submitButton = page.locator('button[type="submit"]');

    // Focus and blur to trigger validation
    await emailInput.focus();
    await emailInput.blur();
    await passwordInput.focus();
    await passwordInput.blur();

    // Check that submit button is disabled
    await expect(submitButton).toBeDisabled();
  });

  test('should show error for invalid email format', async ({ page }) => {
    const emailInput = page.locator('input[type="email"]');

    await emailInput.fill('invalid-email');
    await emailInput.blur();

    // Check for email validation error
    await expect(page.locator('text=valid email')).toBeVisible();
  });

  test('should show error for short password', async ({ page }) => {
    const passwordInput = page.locator('input[type="password"]');

    await passwordInput.fill('12345');
    await passwordInput.blur();

    // Check for password length error
    await expect(page.locator('text=at least 6 characters')).toBeVisible();
  });

  test('should enable submit button with valid credentials', async ({ page }) => {
    const emailInput = page.locator('input[type="email"]');
    const passwordInput = page.locator('input[type="password"]');
    const submitButton = page.locator('button[type="submit"]');

    await emailInput.fill('admin@kcow.test');
    await passwordInput.fill('password123');

    // Submit button should be enabled
    await expect(submitButton).toBeEnabled();
  });

  test('should show loading state during submission', async ({ page }) => {
    const emailInput = page.locator('input[type="email"]');
    const passwordInput = page.locator('input[type="password"]');
    const submitButton = page.locator('button[type="submit"]');

    await emailInput.fill('admin@kcow.test');
    await passwordInput.fill('password123');
    await submitButton.click();

    // Check for loading spinner
    await expect(page.locator('.loading-spinner')).toBeVisible();
  });

  test('should redirect to dashboard on successful login', async ({ page, context }) => {
    // Mock the API response
    await page.route('**/auth/login', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          token: 'fake-jwt-token',
          user: {
            id: 1,
            email: 'admin@kcow.test',
            name: 'Admin User',
            role: 'Admin',
          },
        }),
      });
    });

    const emailInput = page.locator('input[type="email"]');
    const passwordInput = page.locator('input[type="password"]');
    const submitButton = page.locator('button[type="submit"]');

    await emailInput.fill('admin@kcow.test');
    await passwordInput.fill('password123');
    await submitButton.click();

    // Wait for navigation to dashboard
    await page.waitForURL('**/dashboard');
    expect(page.url()).toContain('/dashboard');

    // Verify token is stored in localStorage
    const token = await page.evaluate(() => localStorage.getItem('auth_token'));
    expect(token).toBe('fake-jwt-token');
  });

  test('should show error message on login failure', async ({ page }) => {
    // Mock the API error response
    await page.route('**/auth/login', (route) => {
      route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({
          detail: 'Invalid email or password',
        }),
      });
    });

    const emailInput = page.locator('input[type="email"]');
    const passwordInput = page.locator('input[type="password"]');
    const submitButton = page.locator('button[type="submit"]');

    await emailInput.fill('admin@kcow.test');
    await passwordInput.fill('wrongpassword');
    await submitButton.click();

    // Check for error alert
    await expect(page.locator('.alert-error')).toBeVisible();
    await expect(page.locator('.alert-error')).toContainText('Invalid email or password');

    // Verify user stays on login page
    expect(page.url()).toContain('/login');
  });

  test('should clear password field on login failure', async ({ page }) => {
    // Mock the API error response
    await page.route('**/auth/login', (route) => {
      route.fulfill({
        status: 401,
        contentType: 'application/json',
        body: JSON.stringify({
          detail: 'Invalid credentials',
        }),
      });
    });

    const emailInput = page.locator('input[type="email"]');
    const passwordInput = page.locator('input[type="password"]');
    const submitButton = page.locator('button[type="submit"]');

    await emailInput.fill('admin@kcow.test');
    await passwordInput.fill('wrongpassword');
    await submitButton.click();

    // Wait for error to appear
    await expect(page.locator('.alert-error')).toBeVisible();

    // Verify password field is cleared
    await expect(passwordInput).toHaveValue('');
  });

  test('should redirect to return URL after successful login', async ({ page }) => {
    // Navigate to login with returnUrl query param
    await page.goto('/login?returnUrl=/students');

    // Mock the API response
    await page.route('**/auth/login', (route) => {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          token: 'fake-jwt-token',
          user: {
            id: 1,
            email: 'admin@kcow.test',
            name: 'Admin User',
            role: 'Admin',
          },
        }),
      });
    });

    const emailInput = page.locator('input[type="email"]');
    const passwordInput = page.locator('input[type="password"]');
    const submitButton = page.locator('button[type="submit"]');

    await emailInput.fill('admin@kcow.test');
    await passwordInput.fill('password123');
    await submitButton.click();

    // Wait for navigation to return URL
    await page.waitForURL('**/students');
    expect(page.url()).toContain('/students');
  });
});
