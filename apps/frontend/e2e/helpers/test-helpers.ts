import { type Page, type Locator, expect } from '@playwright/test';

/**
 * Test helper utilities for Playwright tests
 */
export class TestHelpers {
  constructor(private page: Page) {}

  /**
   * Wait for Angular to be stable
   */
  async waitForAngular(): Promise<void> {
    await this.page.waitForLoadState('networkidle');
    await this.page.waitForTimeout(100); // Small buffer for Angular to settle
  }

  /**
   * Get notification by text content
   */
  getNotification(text: string): Locator {
    return this.page.locator('.alert').filter({ hasText: text });
  }

  /**
   * Wait for notification to appear
   */
  async waitForNotification(text: string, timeout = 5000): Promise<void> {
    await this.getNotification(text).waitFor({ state: 'visible', timeout });
  }

  /**
   * Wait for notification to disappear
   */
  async waitForNotificationToDisappear(text: string, timeout = 10000): Promise<void> {
    await this.getNotification(text).waitFor({ state: 'hidden', timeout });
  }

  /**
   * Get modal by title
   */
  getModal(title?: string): Locator {
    const modal = this.page.locator('.modal.modal-open');
    return title ? modal.filter({ hasText: title }) : modal;
  }

  /**
   * Wait for modal to open
   */
  async waitForModal(title?: string, timeout = 5000): Promise<void> {
    await this.getModal(title).waitFor({ state: 'visible', timeout });
  }

  /**
   * Wait for modal to close
   */
  async waitForModalToClose(timeout = 5000): Promise<void> {
    await this.page.locator('.modal.modal-open').waitFor({ state: 'hidden', timeout });
  }

  /**
   * Get current theme from localStorage
   */
  async getCurrentTheme(): Promise<string> {
    return await this.page.evaluate(() => {
      return localStorage.getItem('theme') || 'light';
    });
  }

  /**
   * Clear localStorage
   */
  async clearLocalStorage(): Promise<void> {
    await this.page.evaluate(() => localStorage.clear());
  }

  /**
   * Navigate and wait for Angular
   */
  async navigateTo(path: string): Promise<void> {
    await this.page.goto(path);
    await this.waitForAngular();
  }

  /**
   * Click and wait for navigation
   */
  async clickAndWaitForNavigation(locator: Locator): Promise<void> {
    await Promise.all([
      this.page.waitForURL('**/*'),
      locator.click(),
    ]);
    await this.waitForAngular();
  }

  /**
   * Get breadcrumb items
   */
  getBreadcrumbs(): Locator {
    return this.page.locator('.breadcrumbs li');
  }

  /**
   * Check if sidebar is collapsed
   */
  async isSidebarCollapsed(): Promise<boolean> {
    const drawer = this.page.locator('.drawer-side');
    const width = await drawer.evaluate(el => el.clientWidth);
    return width < 100; // Collapsed sidebar is typically much narrower
  }

  /**
   * Get table rows
   */
  getTableRows(): Locator {
    return this.page.locator('table tbody tr');
  }

  /**
   * Get table headers
   */
  getTableHeaders(): Locator {
    return this.page.locator('table thead th');
  }


  /**
   * Take a screenshot with a name
   */
  async screenshot(name: string): Promise<void> {
    await this.page.screenshot({ path: `e2e/screenshots/${name}.png`, fullPage: true });
  }

  /**
   * Assert element is visible
   */
  async assertVisible(locator: Locator): Promise<void> {
    await expect(locator).toBeVisible();
  }

  /**
   * Assert element is hidden
   */
  async assertHidden(locator: Locator): Promise<void> {
    await expect(locator).toBeHidden();
  }

  /**
   * Assert text content
   */
  async assertText(locator: Locator, text: string | RegExp): Promise<void> {
    await expect(locator).toHaveText(text);
  }

  /**
   * Assert element count
   */
  async assertCount(locator: Locator, count: number): Promise<void> {
    await expect(locator).toHaveCount(count);
  }
}
