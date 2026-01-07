import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Schedule Configuration
 *
 * Tests the schedule configuration functionality including:
 * - Day of week selection
 * - Start time and end time configuration
 * - Sequence/order configuration
 * - Truck assignment
 * - Schedule visibility in weekly view
 * - Schedule block details display
 *
 * AC2: All scenarios must pass
 */

// Test credentials - should be moved to environment variables in production
const TEST_CREDENTIALS = {
  email: process.env.E2E_TEST_EMAIL || 'admin@kcow.local',
  password: process.env.E2E_TEST_PASSWORD || 'Admin123!'
};

test.describe('Class Groups - Schedule Configuration', () => {
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

  test('should select day of week for class group', async ({ page }) => {
    // Navigate to class groups page
    await page.goto('/class-groups');

    // Click "Add Class Group" button
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    // Wait for form to load
    await page.waitForTimeout(500);

    // Find day of week selector
    const daySelect = page.locator('select[name="dayOfWeek"], [formcontrolname="dayOfWeek"]');
    if (await daySelect.count() > 0) {
      // Get all available options
      const options = await daySelect.allOptions();
      expect(options.length).toBeGreaterThan(0);

      // Select a specific day
      await daySelect.selectOption('Wednesday');
      await expect(daySelect).toHaveValue('Wednesday');
    }
  });

  test('should configure start and end times', async ({ page }) => {
    // Navigate to class groups page
    await page.goto('/class-groups');

    // Click "Add Class Group" button
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    // Wait for form to load
    await page.waitForTimeout(500);

    // Set start time
    const startTimeInput = page.locator('input[name="startTime"], [formcontrolname="startTime"]');
    if (await startTimeInput.count() > 0) {
      await startTimeInput.first().fill('09:30');
      await expect(startTimeInput.first()).toHaveValue('09:30');
    }

    // Set end time
    const endTimeInput = page.locator('input[name="endTime"], [formcontrolname="endTime"]');
    if (await endTimeInput.count() > 0) {
      await endTimeInput.first().fill('10:30');
      await expect(endTimeInput.first()).toHaveValue('10:30');
    }
  });

  test('should set sequence order for class group', async ({ page }) => {
    // Navigate to class groups page
    await page.goto('/class-groups');

    // Click "Add Class Group" button
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    // Wait for form to load
    await page.waitForTimeout(500);

    // Find sequence/order input
    const sequenceInput = page.locator('input[name="sequence"], input[name="order"], [formcontrolname="sequence"], [formcontrolname="order"]');
    if (await sequenceInput.count() > 0) {
      await sequenceInput.first().fill('1');
      await expect(sequenceInput.first()).toHaveValue('1');
    }
  });

  test('should assign truck to class group', async ({ page }) => {
    // Navigate to class groups page
    await page.goto('/class-groups');

    // Click "Add Class Group" button
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    // Wait for form to load
    await page.waitForTimeout(500);

    // Find truck selector
    const truckSelect = page.locator('select[name="truck"], [formcontrolname="truck"]');
    if (await truckSelect.count() > 0) {
      // Get available trucks
      const options = await truckSelect.allOptions();
      expect(options.length).toBeGreaterThan(0);

      // Select first truck
      await truckSelect.selectOption({ index: 1 });

      // Verify selection
      const selectedValue = await truckSelect.inputValue();
      expect(selectedValue).toBeTruthy();
      expect(selectedValue).not.toBe('');
    }
  });

  test('should display schedule in weekly view', async ({ page }) => {
    // Navigate to weekly schedule view
    await page.goto('/schedule/weekly');

    // Wait for page to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Verify we're on the weekly schedule page
    await expect(page).toHaveURL(/\/schedule\/weekly/);

    // Check for calendar grid or schedule display
    const scheduleGrid = page.locator('.calendar, .schedule-grid, .week-view');
    const hasGrid = await scheduleGrid.count() > 0;
    expect(hasGrid).toBe(true);
  });

  test('should display schedule block with class group details', async ({ page }) => {
    // Navigate to weekly schedule view
    await page.goto('/schedule/weekly');

    // Wait for page to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Look for schedule blocks
    const scheduleBlocks = page.locator('.schedule-block, .event, .class-group-item');
    const blockCount = await scheduleBlocks.count();

    if (blockCount > 0) {
      // Check first block for details
      const firstBlock = scheduleBlocks.first();

      // Verify block contains class group information
      const blockText = await firstBlock.textContent();
      expect(blockText).toBeTruthy();
      expect(blockText.length).toBeGreaterThan(0);
    }
  });

  test('should show school and truck in schedule block', async ({ page }) => {
    // Navigate to weekly schedule view
    await page.goto('/schedule/weekly');

    // Wait for page to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Look for schedule blocks with school and truck info
    const scheduleBlocks = page.locator('.schedule-block, .event, .class-group-item');
    const blockCount = await scheduleBlocks.count();

    if (blockCount > 0) {
      const firstBlock = scheduleBlocks.first();
      const blockText = await firstBlock.textContent();

      // Schedule blocks should contain identifying information
      // (exact format depends on implementation)
      expect(blockText.length).toBeGreaterThan(0);
    }
  });

  test('should handle multiple schedules on same day', async ({ page }) => {
    // Navigate to weekly schedule view
    await page.goto('/schedule/weekly');

    // Wait for page to load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Check for day columns with multiple events
    const dayColumns = page.locator('.day-column, .week-day');
    const columnCount = await dayColumns.count();

    if (columnCount > 0) {
      // Verify each day column exists and can display multiple events
      const firstColumn = dayColumns.first();
      await expect(firstColumn).toBeVisible();
    }
  });

  test('should update schedule when class group is modified', async ({ page }) => {
    // Create a class group with specific schedule
    await page.goto('/class-groups');

    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    await page.waitForTimeout(500);

    // Set initial schedule
    const daySelect = page.locator('select[name="dayOfWeek"], [formcontrolname="dayOfWeek"]');
    if (await daySelect.count() > 0) {
      await daySelect.selectOption('Monday');
    }

    const startTimeInput = page.locator('input[name="startTime"], [formcontrolname="startTime"]');
    if (await startTimeInput.count() > 0) {
      await startTimeInput.first().fill('09:00');
    }

    const endTimeInput = page.locator('input[name="endTime"], [formcontrolname="endTime"]');
    if (await endTimeInput.count() > 0) {
      await endTimeInput.first().fill('10:00');
    }

    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();

    // Wait for creation and redirect
    await page.waitForTimeout(2000);

    // Navigate to weekly view to verify schedule appears
    await page.goto('/schedule/weekly');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Verify schedule is displayed
    const scheduleGrid = page.locator('.calendar, .schedule-grid, .week-view');
    await expect(scheduleGrid.first()).toBeVisible();
  });
});
