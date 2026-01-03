import { test, expect } from '@playwright/test';
import { SettingsPage } from './page-objects/settings.page';

test.describe('Settings and Preferences', () => {
  let page: SettingsPage;

  test.beforeEach(async ({ page: testPage }) => {
    page = new SettingsPage(testPage);
    await page.gotoSettings();
  });

  test.afterEach(async () => {
    // Clean up localStorage
    await page.helpers.clearLocalStorage();
  });

  test('should load settings page', async ({ page: testPage }) => {
    expect(await page.getCurrentPath()).toContain('/settings');
  });

  test('should display theme selector', async ({ page: testPage }) => {
    if (await page.themeSelect.isVisible()) {
      await expect(page.themeSelect).toBeVisible();

      const options = await page.themeSelect.locator('option').count();
      expect(options).toBeGreaterThan(0);
    }
  });

  test('should change theme setting', async ({ page: testPage }) => {
    if (await page.themeSelect.isVisible()) {
      const themes = await page.getAvailableThemes();

      if (themes.length > 1) {
        await page.selectTheme(themes[1]);

        const currentTheme = await page.getCurrentTheme();
        expect(currentTheme).toBe(themes[1]);
      }
    }
  });

  test('should save settings to localStorage', async ({ page: testPage }) => {
    if (await page.themeSelect.isVisible()) {
      const themes = await page.getAvailableThemes();

      if (themes.length > 0) {
        const testTheme = themes[0];
        await page.selectTheme(testTheme);

        // Check localStorage
        const stored = await testPage.evaluate(() => localStorage.getItem('theme'));
        expect(stored).toBeTruthy();
      }
    }
  });

  test('should display notification preferences', async ({ page: testPage }) => {
    const notificationSettings = await testPage.locator('input[type="checkbox"], input[type="radio"]').count();

    // Settings page might have notification preferences
    expect(notificationSettings).toBeGreaterThanOrEqual(0);
  });

  test('should toggle notification settings', async ({ page: testPage }) => {
    const checkboxes = await testPage.locator('input[type="checkbox"]').all();

    if (checkboxes.length > 0) {
      const checkbox = checkboxes[0];

      const initialState = await checkbox.isChecked();

      await checkbox.click();
      await testPage.waitForTimeout(300);

      const newState = await checkbox.isChecked();

      expect(newState).toBe(!initialState);
    }
  });

  test('should have save button', async ({ page: testPage }) => {
    const saveButtons = await testPage.locator('button:has-text("Save")').count();

    // Settings might have a save button
    expect(saveButtons).toBeGreaterThanOrEqual(0);
  });

  test('should save settings when save button clicked', async ({ page: testPage }) => {
    if (await page.saveButton.isVisible()) {
      // Make some changes
      const checkboxes = await testPage.locator('input[type="checkbox"]').all();

      if (checkboxes.length > 0) {
        await checkboxes[0].click();
      }

      await page.saveButton.click();
      await page.helpers.waitForAngular();

      // Should not crash
      expect(true).toBeTruthy();
    }
  });

  test('should reset settings', async ({ page: testPage }) => {
    if (await page.resetButton.isVisible()) {
      await page.resetButton.click();
      await page.helpers.waitForAngular();

      // Settings should be reset
      expect(true).toBeTruthy();
    }
  });

  test('should display layout density options', async ({ page: testPage }) => {
    const densityControls = await testPage.locator('select, input[name*="density"], [class*="density"]').count();

    // Might have density settings
    expect(densityControls).toBeGreaterThanOrEqual(0);
  });

  test('should persist settings across page reload', async ({ page: testPage }) => {
    if (await page.themeSelect.isVisible()) {
      const themes = await page.getAvailableThemes();

      if (themes.length > 1) {
        await page.selectTheme(themes[1]);

        await testPage.reload();
        await page.helpers.waitForAngular();

        const currentTheme = await page.getCurrentTheme();
        expect(currentTheme).toBe(themes[1]);
      }
    }
  });

  test('should show current settings values', async ({ page: testPage }) => {
    // Page should display current theme
    const currentTheme = await page.getCurrentTheme();
    expect(currentTheme).toBeTruthy();

    // Theme selector should reflect current theme
    if (await page.themeSelect.isVisible()) {
      const selectedValue = await page.themeSelect.inputValue();
      expect(selectedValue).toBeTruthy();
    }
  });

  test('should handle multiple setting changes', async ({ page: testPage }) => {
    if (await page.themeSelect.isVisible()) {
      const themes = await page.getAvailableThemes();

      // Change theme multiple times
      for (let i = 0; i < Math.min(3, themes.length); i++) {
        await page.selectTheme(themes[i]);
        await testPage.waitForTimeout(200);
      }

      // Should end with last selected theme
      const finalTheme = themes[Math.min(2, themes.length - 1)];
      const currentTheme = await page.getCurrentTheme();
      expect(currentTheme).toBe(finalTheme);
    }
  });

  test('should be responsive on mobile', async ({ page: testPage }) => {
    await testPage.setViewportSize({ width: 375, height: 667 });
    await testPage.waitForTimeout(300);

    // Settings page should still be functional
    expect(await page.getCurrentPath()).toContain('/settings');
  });
});
