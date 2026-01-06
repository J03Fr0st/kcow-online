import { Page, Locator } from '@playwright/test';

/**
 * Page Object Model for Login Page
 *
 * Encapsulates login functionality for reuse across tests
 */
export class LoginPage {
  readonly page: Page;
  readonly emailInput: Locator;
  readonly passwordInput: Locator;
  readonly submitButton: Locator;
  readonly validationErrors: Locator;

  constructor(page: Page) {
    this.page = page;
    this.emailInput = page.locator('#email');
    this.passwordInput = page.locator('#password');
    this.submitButton = page.locator('button[type="submit"]');
    this.validationErrors = page.locator('text=/required|invalid|email/i');
  }

  /**
   * Navigate to login page
   */
  async goto() {
    await this.page.goto('/login');
    await this.page.waitForLoadState('networkidle');
  }

  /**
   * Login with provided credentials
   */
  async login(email: string, password: string) {
    await this.emailInput.fill(email);
    await this.passwordInput.fill(password);

    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(500);

    // Submit and wait for dashboard
    await Promise.all([
      this.page.waitForURL(/\/dashboard/, { timeout: 15000 }),
      this.submitButton.click(),
    ]);

    // Wait for auth to stabilize
    await this.page.waitForTimeout(1000);
  }

  /**
   * Get current URL (useful for checking redirects)
   */
  getCurrentUrl(): string {
    return this.page.url();
  }

  /**
   * Check if there are validation errors
   */
  async hasValidationErrors(): Promise<boolean> {
    return await this.validationErrors.count() > 0;
  }

  /**
   * Get all validation error messages
   */
  async getValidationErrors(): Promise<string[]> {
    const errors: string[] = [];
    const count = await this.validationErrors.count();

    for (let i = 0; i < count; i++) {
      const text = await this.validationErrors.nth(i).textContent();
      if (text) errors.push(text);
    }

    return errors;
  }
}
