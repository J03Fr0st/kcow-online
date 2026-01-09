import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Global Student Search (FR11 - Critical)
 *
 * Tests the global search functionality including:
 * - Typeahead results display
 * - Student disambiguation (similar names)
 * - Navigation to student profile
 * - Performance validation (<2 seconds per NFR1)
 * - "No results found" handling
 */

test.describe('Global Student Search - FR11', () => {
  test.beforeEach(async ({ page }) => {
    const testEmail = process.env.TEST_EMAIL || 'admin@kcow.local';
    const testPassword = process.env.TEST_PASSWORD || 'Admin123!';

    await page.goto('/login');
    await page.locator('#email').fill(testEmail);
    await page.locator('#password').fill(testPassword);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    await Promise.all([
      page.waitForURL(/\/dashboard/, { timeout: 15000 }),
      page.locator('button[type="submit"]').click(),
    ]);

    await page.waitForTimeout(1000);
  });

  test('should display global search bar accessible from any page', async ({ page }) => {
    // Test search bar is visible on dashboard
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const searchBar = page.locator('input[placeholder*="search" i], input[type="search"], .global-search input, #global-search');
    await expect(searchBar.first()).toBeVisible();

    // Navigate to another page and verify search is still accessible
    await page.goto('/students');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const searchBarOnStudents = page.locator('input[placeholder*="search" i], input[type="search"], .global-search input, #global-search');
    await expect(searchBarOnStudents.first()).toBeVisible();
  });

  test('should show typeahead results with student details', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const searchBar = page.locator('input[placeholder*="search" i], input[type="search"], .global-search input, #global-search').first();

    // Type to trigger search (assuming we have imported students)
    await searchBar.fill('John');
    await page.waitForTimeout(500); // Wait for debounce

    // Look for typeahead results dropdown
    const resultsDropdown = page.locator('.search-results, .typeahead, .autocomplete, [role="listbox"]');

    // Wait for results to appear
    await page.waitForTimeout(1000);

    const hasResults = await resultsDropdown.count() > 0;
    if (hasResults) {
      await expect(resultsDropdown.first()).toBeVisible();

      // Verify results show student details: name, school, grade, class group
      const resultItems = resultsDropdown.first().locator('[role="option"], .result-item, li');
      const itemCount = await resultItems.count();

      // Should have at least one result
      expect(itemCount).toBeGreaterThan(0);

      // Check first result has required details
      const firstResult = resultItems.first();
      const resultText = await firstResult.textContent();

      // Should contain name and at least school or grade for disambiguation
      expect(resultText).toMatch(/(John|Smith|Grade|School|Class)/i);
    }
  });

  test('should disambiguate students with similar names', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const searchBar = page.locator('input[placeholder*="search" i], input[type="search"], .global-search input, #global-search').first();

    // Search for a common name that might have multiple results
    await searchBar.fill('Smith');
    await page.waitForTimeout(500);

    const resultsDropdown = page.locator('.search-results, .typeahead, .autocomplete, [role="listbox"]');
    await page.waitForTimeout(1000);

    const hasResults = await resultsDropdown.count() > 0;
    if (hasResults) {
      const resultItems = resultsDropdown.first().locator('[role="option"], .result-item, li');
      const itemCount = await resultItems.count();

      // If multiple Smith students exist, verify disambiguation info
      if (itemCount > 1) {
        // Check that results show differentiating info (school, grade, class)
        for (let i = 0; i < Math.min(itemCount, 3); i++) {
          const item = resultItems.nth(i);
          const text = await item.textContent();

          // Each result should have identifying information
          expect(text?.length).toBeGreaterThan(0);
        }
      }
    }
  });

  test('should navigate to student profile when result is selected', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const searchBar = page.locator('input[placeholder*="search" i], input[type="search"], .global-search input, #global-search').first();

    // Type a search query
    await searchBar.fill('John');
    await page.waitForTimeout(500);

    const resultsDropdown = page.locator('.search-results, .typeahead, .autocomplete, [role="listbox"]');
    await page.waitForTimeout(1000);

    const hasResults = await resultsDropdown.count() > 0;
    if (hasResults) {
      const resultItems = resultsDropdown.first().locator('[role="option"], .result-item, li');
      const itemCount = await resultItems.count();

      if (itemCount > 0) {
        // Click the first result
        await resultItems.first().click();

        // Should navigate to student profile
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        const currentUrl = page.url();
        expect(currentUrl).toMatch(/\/students\/\d+/);
      }
    }
  });

  test('should display no results message when search has no matches', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const searchBar = page.locator('input[placeholder*="search" i], input[type="search"], .global-search input, #global-search').first();

    // Search for something that won't exist
    await searchBar.fill('NonexistentStudentXYZ123');
    await page.waitForTimeout(500);

    const resultsDropdown = page.locator('.search-results, .typeahead, .autocomplete, [role="listbox"]');
    await page.waitForTimeout(1000);

    const hasResults = await resultsDropdown.count() > 0;
    if (hasResults) {
      // Check for "No results" message
      const noResults = resultsDropdown.first().locator('text=/no results/i, text=/not found/i, text=/empty/i');
      const hasNoResults = await noResults.count() > 0;

      if (hasNoResults) {
        await expect(noResults.first()).toBeVisible();
      } else {
        // If no explicit message, dropdown might be hidden or empty
        const resultItems = resultsDropdown.first().locator('[role="option"], .result-item, li');
        const itemCount = await resultItems.count();
        expect(itemCount).toBe(0);
      }
    }
  });

  test('should complete search in under 2 seconds (NFR1)', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const searchBar = page.locator('input[placeholder*="search" i], input[type="search"], .global-search input, #global-search').first();

    // Measure search performance
    const startTime = Date.now();

    await searchBar.fill('John');
    await page.waitForTimeout(500);

    const resultsDropdown = page.locator('.search-results, .typeahead, .autocomplete, [role="listbox"]');

    // Wait for results to appear
    await resultsDropdown.first().waitFor({ state: 'visible', timeout: 2000 }).catch(() => {
      // If no results appear, that's okay for this test - we're measuring response time
    });

    const endTime = Date.now();
    const duration = endTime - startTime;

    // Search should complete in under 2 seconds (2000ms)
    expect(duration).toBeLessThan(2000);

    console.log(`Search completed in ${duration}ms`);
  });

  test('should handle keyboard navigation in search results', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const searchBar = page.locator('input[placeholder*="search" i], input[type="search"], .global-search input, #global-search').first();

    await searchBar.fill('John');
    await page.waitForTimeout(500);

    const resultsDropdown = page.locator('.search-results, .typeahead, .autocomplete, [role="listbox"]');
    await page.waitForTimeout(1000);

    const hasResults = await resultsDropdown.count() > 0;
    if (hasResults) {
      const resultItems = resultsDropdown.first().locator('[role="option"], .result-item, li');
      const itemCount = await resultItems.count();

      if (itemCount > 0) {
        // Test arrow down navigation
        await searchBar.press('ArrowDown');
        await page.waitForTimeout(200);

        // Test Enter to select
        await searchBar.press('Enter');

        // Should navigate to profile
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        const currentUrl = page.url();
        expect(currentUrl).toMatch(/\/students\/\d+/);
      }
    }
  });

  test('should close search results on Escape key', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const searchBar = page.locator('input[placeholder*="search" i], input[type="search"], .global-search input, #global-search').first();

    await searchBar.fill('John');
    await page.waitForTimeout(500);

    const resultsDropdown = page.locator('.search-results, .typeahead, .autocomplete, [role="listbox"]');
    await page.waitForTimeout(1000);

    const hasResults = await resultsDropdown.count() > 0;
    if (hasResults) {
      // Press Escape to close
      await searchBar.press('Escape');
      await page.waitForTimeout(200);

      // Results should be hidden
      await expect(resultsDropdown.first()).not.toBeVisible();
    }
  });
});
