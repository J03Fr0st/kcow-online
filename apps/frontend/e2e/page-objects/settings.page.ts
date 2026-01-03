import { type Page, type Locator } from '@playwright/test';
import { BasePage } from './base.page';

/**
 * Page object for Settings functionality
 */
export class SettingsPage extends BasePage {
  readonly themeSelect: Locator;
  readonly densitySelect: Locator;
  readonly notificationToggle: Locator;
  readonly saveButton: Locator;
  readonly resetButton: Locator;
  readonly themePreview: Locator;

  constructor(page: Page) {
    super(page);

    this.themeSelect = page.locator('select[data-testid="theme-select"], select').first();
    this.densitySelect = page.locator('select').filter({ hasText: /density/i });
    this.notificationToggle = page.locator('input[type="checkbox"]').filter({ hasText: /notification/i });
    this.saveButton = page.locator('button', { hasText: /save/i });
    this.resetButton = page.locator('button', { hasText: /reset/i });
    this.themePreview = page.locator('[data-theme]');
  }

  async gotoSettings(): Promise<void> {
    await this.goto('/settings');
  }

  async selectTheme(theme: string): Promise<void> {
    await this.themeSelect.selectOption(theme);
    await this.helpers.waitForAngular();
  }

  async getCurrentTheme(): Promise<string | null> {
    return await this.page.getAttribute('html', 'data-theme');
  }

  async getThemeFromStorage(): Promise<string> {
    return await this.helpers.getCurrentTheme();
  }

  async selectDensity(density: string): Promise<void> {
    await this.densitySelect.selectOption(density);
    await this.helpers.waitForAngular();
  }

  async toggleNotifications(): Promise<void> {
    await this.notificationToggle.click();
  }

  async saveSettings(): Promise<void> {
    await this.saveButton.click();
    await this.helpers.waitForAngular();
  }

  async resetSettings(): Promise<void> {
    await this.resetButton.click();
    await this.helpers.waitForAngular();
  }

  async getAvailableThemes(): Promise<string[]> {
    const options = await this.themeSelect.locator('option').all();
    const themes: string[] = [];
    for (const option of options) {
      const value = await option.getAttribute('value');
      if (value) themes.push(value);
    }
    return themes;
  }
}
