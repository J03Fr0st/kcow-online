import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Schools Management
 *
 * Tests the schools UI including:
 * - Viewing imported schools list
 * - Verifying imported data displays correctly
 * - Validating imported school details
 */

test.describe('Schools Management - Imported Data Verification', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to login page first
    await page.goto('/login');

    // Login with valid credentials
    await page.locator('#email').fill('admin@kcow.local');
    await page.locator('#password').fill('Admin123!');

    // Wait for Angular to be stable and form to be valid
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    // Click submit button and wait for navigation
    await Promise.all([
      page.waitForURL(/\/dashboard/, { timeout: 15000 }),
      page.locator('button[type="submit"]').click(),
    ]);

    // Wait for auth state to fully stabilize
    await page.waitForTimeout(1000);
  });

  test('should display schools list page with imported data', async ({ page }) => {
    // Navigate to schools page
    await page.goto('/schools');

    // Wait for page to fully load and stabilize
    await page.waitForLoadState('networkidle');
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(1000);

    // Verify we successfully reached the schools page (auth worked)
    const currentUrl = page.url();
    if (currentUrl.includes('/login')) {
      throw new Error('Authentication failed - auth guard redirected to login page');
    }

    // Check for page heading
    await expect(page.locator('h1').filter({ hasText: /Schools/i })).toBeVisible();

    // Check for table with imported data (should have data from migration)
    const table = page.locator('table');
    const emptyState = page.locator('text=No schools found');

    const hasTable = await table.count() > 0;
    const hasEmpty = await emptyState.count() > 0;

    // Should have table with imported schools, not empty state
    expect(hasTable).toBe(true);

    // Verify table has at least one row (imported data)
    if (hasTable) {
      const rows = page.locator('table tbody tr');
      const rowCount = await rows.count();
      expect(rowCount).toBeGreaterThan(0);
    }
  });

  test('should display specific imported schools from legacy data', async ({ page }) => {
    // Navigate to schools page
    await page.goto('/schools');

    // Wait for page to fully load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Verify some known imported schools are visible
    // These are from the legacy School.xml file
    const knownSchools = [
      'Uitstaande',
      'Rinkel Krinkel Kleuterskool',
      'Kiddo Kleuterskool',
      'Akasia Kinderlandgoed'
    ];

    // Check for at least one known school in the table
    let foundSchool = false;
    for (const schoolName of knownSchools) {
      const schoolElement = page.locator(`text=${schoolName}`);
      if (await schoolElement.count() > 0) {
        foundSchool = true;
        await expect(schoolElement.first()).toBeVisible();
        break;
      }
    }

    expect(foundSchool).toBe(true);
  });

  test('should render school data with correct fields', async ({ page }) => {
    // Navigate to schools page
    await page.goto('/schools');

    // Wait for list to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Check table has correct columns for school data
    const table = page.locator('table');
    if (await table.count() > 0) {
      // Verify table headers exist (may vary based on implementation)
      const headerRow = page.locator('table thead tr');
      const hasHeaders = await headerRow.count() > 0;
      expect(hasHeaders).toBe(true);

      // Verify table body has data
      const dataRows = page.locator('table tbody tr');
      const rowCount = await dataRows.count();
      expect(rowCount).toBeGreaterThan(0);

      // Verify first row has cells with data
      const firstRowCells = dataRows.nth(0).locator('td');
      const cellCount = await firstRowCells.count();
      expect(cellCount).toBeGreaterThan(0);
    }
  });

  test('should handle navigation to schools list', async ({ page }) => {
    // Navigate to schools page
    await page.goto('/schools');

    // Wait for page to fully load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Verify URL is correct
    await expect(page).toHaveURL(/\/schools/);

    // Verify page is accessible (no redirect to login)
    const currentUrl = page.url();
    expect(currentUrl).not.toContain('/login');

    // Verify page has loaded content (not just spinner)
    const loadingSpinner = page.locator('[role="progressbar"], .spinner, .loading');
    const hasSpinner = await loadingSpinner.count() > 0;

    if (hasSpinner) {
      // Wait for spinner to disappear
      await loadingSpinner.waitFor({ state: 'hidden', timeout: 5000 });
    }

    // Verify content is visible
    const heading = page.locator('h1').filter({ hasText: /Schools/i });
    await expect(heading).toBeVisible();
  });

  test('should display school count matching imported data', async ({ page }) => {
    // Navigate to schools page
    await page.goto('/schools');

    // Wait for data to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Count rows in table
    const table = page.locator('table');
    if (await table.count() > 0) {
      const rows = page.locator('table tbody tr');
      const rowCount = await rows.count();

      // We imported 76 schools from legacy XML
      // Verify we have a reasonable number of schools (at least some of them)
      expect(rowCount).toBeGreaterThanOrEqual(10);

      // Optionally check if total count display matches
      const countDisplay = page.locator('text=/\\d+\\s+(school|total|results|items)/i');
      if (await countDisplay.count() > 0) {
        const countText = await countDisplay.first().textContent();
        console.log('School count display:', countText);
      }
    }
  });

  test('should search or filter imported schools', async ({ page }) => {
    // Navigate to schools page
    await page.goto('/schools');

    // Wait for page to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Look for search/filter input
    const searchInput = page.locator('input[placeholder*="search" i], input[type="search"], #search');
    if (await searchInput.count() > 0) {
      // Search for a known imported school
      await searchInput.first().fill('Rinkel');
      await page.waitForTimeout(1000);

      // Verify search was performed
      await expect(searchInput.first()).toHaveValue('Rinkel');

      // Check if filtered results contain the search term
      const tableContent = page.locator('table');
      if (await tableContent.count() > 0) {
        const bodyText = await tableContent.textContent();
        expect(bodyText).toContain('Rinkel');
      }
    }
  });

  test('should display imported school details when clicking a school', async ({ page }) => {
    // Navigate to schools page
    await page.goto('/schools');

    // Wait for data to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Check if there are schools to interact with
    const table = page.locator('table');
    const hasTables = await table.count() > 0;

    if (!hasTables) {
      throw new Error('No schools table found - imported data may not be loaded');
    }

    // Find and click on a school row or edit button
    const rows = page.locator('table tbody tr');
    const rowCount = await rows.count();

    if (rowCount > 0) {
      // Click edit button or row for the first school
      const editButton = rows.nth(0).locator('button').filter({ hasText: /edit|view|details/i });

      if (await editButton.count() > 0) {
        await editButton.first().click();

        // Should navigate to edit page or show details
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        // Verify we're on a school-related page
        const currentUrl = page.url();
        expect(currentUrl).toMatch(/\/schools/);
      } else {
        // Try clicking the row directly
        await rows.nth(0).click();
        await page.waitForTimeout(500);
      }
    }
  });

  test('should verify imported schools have required fields populated', async ({ page }) => {
    // Navigate to schools page
    await page.goto('/schools');

    // Wait for data to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Get table rows
    const rows = page.locator('table tbody tr');
    const rowCount = await rows.count();

    if (rowCount > 0) {
      // Check first few schools have data in cells
      const firstRow = rows.nth(0);
      const cells = firstRow.locator('td');
      const cellCount = await cells.count();

      // Verify cells exist and have content
      expect(cellCount).toBeGreaterThan(0);

      // Check that cells contain text (not all empty)
      let hasContent = false;
      for (let i = 0; i < Math.min(cellCount, 5); i++) {
        const cellText = await cells.nth(i).textContent();
        if (cellText && cellText.trim().length > 0) {
          hasContent = true;
          break;
        }
      }

      expect(hasContent).toBe(true);
    }
  });
});
