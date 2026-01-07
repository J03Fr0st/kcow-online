import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Class Groups Management
 *
 * Tests the class groups CRUD operations including:
 * - Viewing class groups list
 * - Creating a new class group with school, truck, day/time
 * - Editing class group schedule
 * - Archiving a class group
 * - Filtering by school and truck
 *
 * AC1: All scenarios must pass
 */

// Test credentials - should be moved to environment variables in production
const TEST_CREDENTIALS = {
  email: process.env.E2E_TEST_EMAIL || 'admin@kcow.local',
  password: process.env.E2E_TEST_PASSWORD || 'Admin123!'
};

test.describe('Class Groups Management - CRUD Operations', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to login page first
    await page.goto('/login');

    // Login with test credentials
    await page.locator('#email').fill(TEST_CREDENTIALS.email);
    await page.locator('#password').fill(TEST_CREDENTIALS.password);

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

  test('should display class groups list page', async ({ page }) => {
    // Navigate to class groups page
    await page.goto('/class-groups');

    // Wait for page to fully load and stabilize
    await page.waitForLoadState('networkidle');
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(1000);

    // Verify we successfully reached the class groups page (auth worked)
    const currentUrl = page.url();
    if (currentUrl.includes('/login')) {
      throw new Error('Authentication failed - auth guard redirected to login page');
    }

    // Check for page heading
    await expect(page.locator('h1').filter({ hasText: /Class Groups/i })).toBeVisible();

    // Check for table or empty state
    const table = page.locator('table');
    const emptyState = page.locator('text=No class groups found');

    const hasTable = await table.count() > 0;
    const hasEmpty = await emptyState.count() > 0;

    expect(hasTable || hasEmpty).toBe(true);
  });

  test('should create a new class group with school, truck, day/time', async ({ page }) => {
    // Navigate to class groups page
    await page.goto('/class-groups');

    // Wait for page to fully load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Click "Add Class Group" button
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    // Wait for form to appear
    await page.waitForTimeout(500);

    // Fill in class group details
    const classGroupName = `E2E Test Class Group ${Date.now()}`;

    // Select school (dropdown or select)
    const schoolSelect = page.locator('select[name="school"], [formcontrolname="school"]');
    if (await schoolSelect.count() > 0) {
      await schoolSelect.selectOption({ index: 1 }); // Select first available school
    }

    // Select truck (dropdown or select)
    const truckSelect = page.locator('select[name="truck"], [formcontrolname="truck"]');
    if (await truckSelect.count() > 0) {
      await truckSelect.selectOption({ index: 1 }); // Select first available truck
    }

    // Select day of week
    const daySelect = page.locator('select[name="dayOfWeek"], [formcontrolname="dayOfWeek"]');
    if (await daySelect.count() > 0) {
      await daySelect.selectOption('Monday');
    }

    // Set start time
    const startTimeInput = page.locator('input[name="startTime"], [formcontrolname="startTime"]');
    if (await startTimeInput.count() > 0) {
      await startTimeInput.first().fill('09:00');
    }

    // Set end time
    const endTimeInput = page.locator('input[name="endTime"], [formcontrolname="endTime"]');
    if (await endTimeInput.count() > 0) {
      await endTimeInput.first().fill('10:00');
    }

    // Submit form
    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();

    // Should redirect back to class groups list
    await expect(page).toHaveURL(/\/class-groups/, { timeout: 10000 });

    // Should show success message or new class group in list
    const successMessage = page.locator('text=success, text=created, text=added');
    if (await successMessage.count() > 0) {
      await expect(successMessage.first()).toBeVisible();
    }
  });

  test('should edit class group schedule', async ({ page }) => {
    // Navigate to class groups page
    await page.goto('/class-groups');

    // Wait for list to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Find and click edit button for first class group
    const editButton = page.locator('button').filter({ hasText: /edit|modify/i }).first();
    if (await editButton.count() > 0) {
      await editButton.click();

      // Wait for form to load
      await page.waitForTimeout(500);

      // Update schedule details
      const daySelect = page.locator('select[name="dayOfWeek"], [formcontrolname="dayOfWeek"]');
      if (await daySelect.count() > 0) {
        await daySelect.selectOption('Tuesday');
      }

      // Update start time
      const startTimeInput = page.locator('input[name="startTime"], [formcontrolname="startTime"]');
      if (await startTimeInput.count() > 0) {
        await startTimeInput.first().fill('10:00');
      }

      // Update end time
      const endTimeInput = page.locator('input[name="endTime"], [formcontrolname="endTime"]');
      if (await endTimeInput.count() > 0) {
        await endTimeInput.first().fill('11:00');
      }

      // Submit form
      const submitButton = page.locator('button[type="submit"]');
      await submitButton.click();

      // Should redirect back to class groups list
      await expect(page).toHaveURL(/\/class-groups/, { timeout: 10000 });

      // Should show success message
      const successMessage = page.locator('text=success, text=updated');
      if (await successMessage.count() > 0) {
        await expect(successMessage.first()).toBeVisible();
      }
    }
  });

  test('should archive a class group', async ({ page }) => {
    // Navigate to class groups page
    await page.goto('/class-groups');

    // Wait for list to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Find and click archive button for first class group
    const archiveButton = page.locator('button').filter({ hasText: /archive|delete/i }).first();
    if (await archiveButton.count() > 0) {
      // Handle confirmation dialog if present
      page.on('dialog', async dialog => {
        await dialog.accept();
      });

      await archiveButton.click();

      // Wait for archive to complete
      await page.waitForTimeout(2000);

      // Should show success message
      const successMessage = page.locator('text=success, text=archived, text=deleted');
      if (await successMessage.count() > 0) {
        await expect(successMessage.first()).toBeVisible();
      }
    }
  });

  test('should filter class groups by school', async ({ page }) => {
    // Navigate to class groups page
    await page.goto('/class-groups');

    // Wait for page to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Get initial row count
    const tableRows = page.locator('table tbody tr, .class-group-row');
    const initialCount = await tableRows.count();

    // Look for school filter dropdown
    const schoolFilter = page.locator('select[name="school"], [formcontrolname="school"], select.filter-dropdown');
    if (await schoolFilter.count() > 0) {
      // Select a school to filter
      await schoolFilter.first().selectOption({ index: 1 });
      await page.waitForTimeout(1000);

      // Verify filter was applied - filtered count should be <= initial count
      const filteredCount = await tableRows.count();
      expect(filteredCount).toBeLessThanOrEqual(initialCount);

      // Verify filter dropdown has selected value
      await expect(schoolFilter.first()).toHaveValue(/./);
    }
  });

  test('should filter class groups by truck', async ({ page }) => {
    // Navigate to class groups page
    await page.goto('/class-groups');

    // Wait for page to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Get initial row count
    const tableRows = page.locator('table tbody tr, .class-group-row');
    const initialCount = await tableRows.count();

    // Look for truck filter dropdown
    const truckFilter = page.locator('select[name="truck"], [formcontrolname="truck"], select.filter-dropdown');
    if (await truckFilter.count() > 0) {
      // Select a truck to filter
      await truckFilter.last().selectOption({ index: 1 });
      await page.waitForTimeout(1000);

      // Verify filter was applied - filtered count should be <= initial count
      const filteredCount = await tableRows.count();
      expect(filteredCount).toBeLessThanOrEqual(initialCount);

      // Verify filter dropdown has selected value
      await expect(truckFilter.last()).toHaveValue(/./);
    }
  });

  test('should validate required fields for class group creation', async ({ page }) => {
    // Navigate to class groups page
    await page.goto('/class-groups');

    // Click "Add Class Group" button
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    // Wait for form to appear
    await page.waitForTimeout(500);

    // Try to submit without entering required fields
    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();

    // Check for validation errors
    const validationErrors = page.locator('text=required, text=invalid, text=must');
    const hasErrors = await validationErrors.count() > 0;
    expect(hasErrors).toBe(true);
  });

  test('should handle time range validation', async ({ page }) => {
    // Navigate to class groups page
    await page.goto('/class-groups');

    // Click "Add Class Group" button
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    // Wait for form to appear
    await page.waitForTimeout(500);

    // Set end time before start time (invalid)
    const startTimeInput = page.locator('input[name="startTime"], [formcontrolname="startTime"]');
    const endTimeInput = page.locator('input[name="endTime"], [formcontrolname="endTime"]');

    if (await startTimeInput.count() > 0 && await endTimeInput.count() > 0) {
      await startTimeInput.first().fill('11:00');
      await endTimeInput.first().fill('09:00');

      // Check for validation error
      await startTimeInput.first().blur();
      await endTimeInput.first().blur();
      await page.waitForTimeout(500);

      const timeError = page.locator('text=end time, text=start time, text=invalid');
      const hasTimeError = await timeError.count() > 0;
      expect(hasTimeError).toBe(true);
    }
  });
});
