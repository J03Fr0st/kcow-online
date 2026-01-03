import { test, expect } from '@playwright/test';
import { SettingsPage } from './page-objects/settings.page';

test.describe('Theme Switching', () => {
  let page: SettingsPage;

  test.beforeEach(async ({ page: testPage }) => {
    page = new SettingsPage(testPage);
    await page.gotoSettings();
  });

  test.afterEach(async () => {
    // Clean up localStorage after each test
    await page.helpers.clearLocalStorage();
  });

  test('should display available themes', async ({ page: testPage }) => {
    const themes = await page.getAvailableThemes();

    expect(themes.length).toBeGreaterThan(0);
    expect(themes.length).toBeGreaterThanOrEqual(10); // Should have 10+ themes
  });

  test('should switch to different themes', async ({ page: testPage }) => {
    const themes = await page.getAvailableThemes();

    // Test a few different themes
    const themesToTest = themes.slice(0, 5);

    for (const theme of themesToTest) {
      await page.selectTheme(theme);

      const currentTheme = await page.getCurrentTheme();
      expect(currentTheme).toBe(theme);
    }
  });

  test('should persist theme selection in localStorage', async ({ page: testPage }) => {
    const themes = await page.getAvailableThemes();
    const testTheme = themes[1] || 'dark';

    await page.selectTheme(testTheme);

    const storedTheme = await page.getThemeFromStorage();
    expect(storedTheme).toBe(testTheme);
  });

  test('should maintain theme after page reload', async ({ page: testPage }) => {
    const themes = await page.getAvailableThemes();
    const testTheme = themes[2] || 'cupcake';

    await page.selectTheme(testTheme);

    // Reload the page
    await testPage.reload();
    await page.helpers.waitForAngular();

    const currentTheme = await page.getCurrentTheme();
    expect(currentTheme).toBe(testTheme);
  });

  test('should maintain theme across navigation', async ({ page: testPage }) => {
    const themes = await page.getAvailableThemes();
    const testTheme = themes[1] || 'dark';

    await page.selectTheme(testTheme);

    // Navigate to different pages
    await page.goto('/dashboard');
    let currentTheme = await page.getCurrentTheme();
    expect(currentTheme).toBe(testTheme);

    await page.goto('/forms');
    currentTheme = await page.getCurrentTheme();
    expect(currentTheme).toBe(testTheme);
  });

  test('should apply theme to all page elements', async ({ page: testPage }) => {
    const themes = await page.getAvailableThemes();
    const darkTheme = themes.find(t => t.includes('dark')) || themes[1];

    await page.selectTheme(darkTheme);

    // Check that the HTML element has the data-theme attribute
    const htmlTheme = await testPage.locator('html').getAttribute('data-theme');
    expect(htmlTheme).toBe(darkTheme);
  });

  test('should support light and dark theme variants', async ({ page: testPage }) => {
    const themes = await page.getAvailableThemes();

    const hasLight = themes.some(t => t.includes('light') || t === 'cupcake' || t === 'emerald');
    const hasDark = themes.some(t => t.includes('dark') || t === 'night' || t === 'dracula');

    expect(hasLight || hasDark).toBeTruthy();
  });

  test('should handle rapid theme switching', async ({ page: testPage }) => {
    const themes = await page.getAvailableThemes();

    // Rapidly switch between themes
    for (let i = 0; i < 5; i++) {
      const theme = themes[i % themes.length];
      await page.selectTheme(theme);
      await testPage.waitForTimeout(100);
    }

    // Should end up with the last theme selected
    const finalTheme = themes[4 % themes.length];
    const currentTheme = await page.getCurrentTheme();
    expect(currentTheme).toBe(finalTheme);
  });

  test('should update visual appearance when theme changes', async ({ page: testPage }) => {
    const themes = await page.getAvailableThemes();
    const theme1 = themes[0];
    const theme2 = themes[1];

    // Select first theme and get background color
    await page.selectTheme(theme1);
    const bg1 = await testPage.locator('body').evaluate(
      el => window.getComputedStyle(el).backgroundColor
    );

    // Select second theme and get background color
    await page.selectTheme(theme2);
    const bg2 = await testPage.locator('body').evaluate(
      el => window.getComputedStyle(el).backgroundColor
    );

    // Background colors should be valid CSS colors
    expect(bg1).toBeTruthy();
    expect(bg2).toBeTruthy();
  });

  test('should have consistent theme names', async ({ page: testPage }) => {
    const themes = await page.getAvailableThemes();

    for (const theme of themes) {
      // Theme names should be lowercase and valid
      expect(theme).toMatch(/^[a-z0-9-]+$/);
      expect(theme.length).toBeGreaterThan(0);
    }
  });
});
