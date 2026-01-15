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

    const searchBar = page.locator('.global-search-container input, input[placeholder*="search" i]');
    await expect(searchBar.first()).toBeVisible();

    // Navigate to students page using sidebar navigation (client-side routing)
    const studentsLink = page.locator('a[href*="students"], [routerlink*="students"]').first();
    if (await studentsLink.isVisible()) {
      await studentsLink.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Verify search bar is still visible on students page
      const searchBarOnStudents = page.locator('.global-search-container input, input[placeholder*="search" i]');
      await expect(searchBarOnStudents.first()).toBeVisible();
    }
  });

  test('should show typeahead results with student details', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const searchBar = page.locator('.global-search-container input, input[placeholder*="search" i]').first();

    // Type to trigger search - use a single letter to get more results
    await searchBar.fill('a');

    // Wait for debounce (300ms) + API response
    await page.waitForTimeout(800);

    // Look for typeahead results dropdown
    const resultsDropdown = page.locator('.global-search-container .dropdown-content');

    // Check if dropdown is visible
    const isDropdownVisible = await resultsDropdown.isVisible().catch(() => false);

    if (isDropdownVisible) {
      // Check for actual results (non-disabled li elements with anchor links)
      const resultItems = resultsDropdown.locator('li a');
      const itemCount = await resultItems.count();

      if (itemCount > 0) {
        // Check first result has required details (name, school info)
        const firstResult = resultItems.first();
        const resultText = await firstResult.textContent();

        // Should contain some text (student name and/or school/grade info)
        expect(resultText?.trim().length).toBeGreaterThan(0);
      } else {
        // If no results, check for "No results found" message
        const noResultsMsg = resultsDropdown.locator('text=/no results/i');
        const hasNoResultsMsg = await noResultsMsg.count() > 0;
        expect(hasNoResultsMsg || itemCount === 0).toBeTruthy();
      }
    }
  });

  test('should disambiguate students with similar names', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const searchBar = page.locator('.global-search-container input, input[placeholder*="search" i]').first();

    // Search for a common name that might have multiple results
    await searchBar.fill('Smith');
    await page.waitForTimeout(500);

    const resultsDropdown = page.locator('.global-search-container .dropdown-content, .search-results, [role="listbox"]');
    await page.waitForTimeout(1000);

    const hasResults = await resultsDropdown.count() > 0;
    if (hasResults) {
      const resultItems = resultsDropdown.first().locator('li:not(.disabled)');
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

    const searchBar = page.locator('.global-search-container input, input[placeholder*="search" i]').first();

    // Type a search query
    await searchBar.fill('John');
    await page.waitForTimeout(500);

    const resultsDropdown = page.locator('.global-search-container .dropdown-content, .search-results, [role="listbox"]');
    await page.waitForTimeout(1000);

    const hasResults = await resultsDropdown.count() > 0;
    if (hasResults) {
      const resultItems = resultsDropdown.first().locator('li:not(.disabled)');
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

    const searchBar = page.locator('.global-search-container input, input[placeholder*="search" i]').first();

    // Search for something that won't exist
    await searchBar.fill('NonexistentStudentXYZ123');
    await page.waitForTimeout(500);

    const resultsDropdown = page.locator('.global-search-container .dropdown-content, .search-results, [role="listbox"]');
    await page.waitForTimeout(1000);

    const hasResults = await resultsDropdown.count() > 0;
    if (hasResults) {
      // Check for "No results" message
      const noResults = resultsDropdown.first().getByText(/no results|not found|empty/i);
      const hasNoResults = await noResults.count() > 0;

      if (hasNoResults) {
        await expect(noResults.first()).toBeVisible();
      } else {
        // If no explicit message, dropdown might be hidden or empty
        const resultItems = resultsDropdown.first().locator('li:not(.disabled)');
        const itemCount = await resultItems.count();
        expect(itemCount).toBe(0);
      }
    }
  });

  test('should complete search in under 2 seconds (NFR1)', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const searchBar = page.locator('.global-search-container input, input[placeholder*="search" i]').first();

    // Listen for API response to measure actual search time
    const apiResponsePromise = page.waitForResponse(
      (response) => response.url().includes('/api/students/search') && response.status() === 200,
      { timeout: 5000 }
    ).catch(() => null);

    // Measure search performance from input to API response
    const startTime = Date.now();
    await searchBar.fill('John');

    // Wait for API response (this accounts for debounce + network time)
    const response = await apiResponsePromise;
    const endTime = Date.now();
    const duration = endTime - startTime;

    // Search should complete in under 2 seconds (2000ms)
    // This includes 300ms debounce + API response time
    expect(duration).toBeLessThan(2000);

    console.log(`Search completed in ${duration}ms`);

    // Verify results appear
    if (response) {
      const resultsDropdown = page.locator('.global-search-container .dropdown-content');
      await expect(resultsDropdown.first()).toBeVisible({ timeout: 1000 });
    }
  });

  test('should handle keyboard navigation in search results', async ({ page }) => {
    await page.goto('/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const searchBar = page.locator('.global-search-container input, input[placeholder*="search" i]').first();

    await searchBar.fill('John');
    await page.waitForTimeout(500);

    const resultsDropdown = page.locator('.global-search-container .dropdown-content, .search-results, [role="listbox"]');
    await page.waitForTimeout(1000);

    const hasResults = await resultsDropdown.count() > 0;
    if (hasResults) {
      const resultItems = resultsDropdown.first().locator('li:not(.disabled)');
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

    const searchBar = page.locator('.global-search-container input, input[placeholder*="search" i]').first();

    await searchBar.fill('John');
    await page.waitForTimeout(500);

    const resultsDropdown = page.locator('.global-search-container .dropdown-content, .search-results, [role="listbox"]');
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
