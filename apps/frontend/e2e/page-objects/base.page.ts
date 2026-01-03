import { type Page, type Locator } from '@playwright/test';
import { TestHelpers } from '../helpers/test-helpers';

/**
 * Base page object with common elements and helpers
 */
export class BasePage {
  protected helpers: TestHelpers;

  // Common elements
  readonly sidebar: Locator;
  readonly sidebarToggle: Locator;
  readonly breadcrumbs: Locator;
  readonly notificationContainer: Locator;
  readonly modalContainer: Locator;

  constructor(protected page: Page) {
    this.helpers = new TestHelpers(page);

    // Initialize common elements
    this.sidebar = page.locator('.drawer-side');
    this.sidebarToggle = page.locator('[data-testid="sidebar-toggle"], .btn-ghost').first();
    this.breadcrumbs = page.locator('.breadcrumbs');
    this.notificationContainer = page.locator('[data-testid="notification-container"], .toast');
    this.modalContainer = page.locator('[data-testid="modal-container"]');
  }

  /**
   * Navigate to a specific route
   */
  async goto(path: string): Promise<void> {
    await this.helpers.navigateTo(path);
  }

  /**
   * Click sidebar menu item
   */
  async clickSidebarItem(text: string): Promise<void> {
    await this.sidebar.locator('a', { hasText: text }).click();
    await this.helpers.waitForAngular();
  }

  /**
   * Get current URL path
   */
  async getCurrentPath(): Promise<string> {
    return new URL(this.page.url()).pathname;
  }

  /**
   * Toggle sidebar
   */
  async toggleSidebar(): Promise<void> {
    await this.sidebarToggle.click();
    await this.page.waitForTimeout(300); // Wait for animation
  }
}
