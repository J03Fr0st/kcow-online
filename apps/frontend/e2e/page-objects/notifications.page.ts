import { type Page, type Locator } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Page object for Notifications functionality
 */
export class NotificationsPage extends BasePage {
  readonly successButton: Locator;
  readonly errorButton: Locator;
  readonly warningButton: Locator;
  readonly infoButton: Locator;
  readonly customDurationButton: Locator;
  readonly stackedNotificationsButton: Locator;

  constructor(page: Page) {
    super(page);

    this.successButton = page.locator('button', { hasText: /success/i }).first();
    this.errorButton = page.locator('button', { hasText: /error/i }).first();
    this.warningButton = page.locator('button', { hasText: /warning/i }).first();
    this.infoButton = page.locator('button', { hasText: /info/i }).first();
    this.customDurationButton = page.locator('button', { hasText: /custom duration/i });
    this.stackedNotificationsButton = page.locator('button', { hasText: /stacked|multiple/i });
  }

  async gotoNotifications(): Promise<void> {
    await this.goto('/notifications');
  }

  async showSuccessNotification(): Promise<void> {
    await this.successButton.click();
  }

  async showErrorNotification(): Promise<void> {
    await this.errorButton.click();
  }

  async showWarningNotification(): Promise<void> {
    await this.warningButton.click();
  }

  async showInfoNotification(): Promise<void> {
    await this.infoButton.click();
  }

  async getNotificationByType(type: 'success' | 'error' | 'warning' | 'info'): Promise<Locator> {
    const classMap = {
      success: 'alert-success',
      error: 'alert-error',
      warning: 'alert-warning',
      info: 'alert-info',
    };
    return this.page.locator(`.alert.${classMap[type]}`);
  }

  async getNotificationCount(): Promise<number> {
    return await this.page.locator('.alert').count();
  }
}
