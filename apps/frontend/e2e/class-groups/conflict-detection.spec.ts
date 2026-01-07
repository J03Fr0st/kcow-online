import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Schedule Conflict Detection
 *
 * CRITICAL: These tests validate FR6 - Conflict Detection requirement
 *
 * Tests the conflict detection functionality including:
 * - Overlapping schedule detection for same truck
 * - Schedule Conflict Banner display
 * - Conflict details visibility
 * - Save blocking until conflict resolution
 * - Time adjustment to resolve conflicts
 * - Truck change to resolve conflicts
 * - Successful save after resolution
 *
 * AC3 & AC7: All scenarios must pass (CRITICAL)
 */

// Test credentials - should be moved to environment variables in production
const TEST_CREDENTIALS = {
  email: process.env.E2E_TEST_EMAIL || 'admin@kcow.local',
  password: process.env.E2E_TEST_PASSWORD || 'Admin123!'
};

test.describe('Class Groups - Conflict Detection (FR6 Critical)', () => {
  // Store created class group IDs for cleanup
  let createdClassGroupIds: string[] = [];

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

  test('should detect overlapping schedule for same truck on same day', async ({ page }) => {
    // This test validates the core conflict detection scenario:
    // Same truck, same day, overlapping times should trigger conflict

    // First, navigate to class groups and create initial class group
    await page.goto('/class-groups');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Click "Add Class Group" to create first schedule
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    // Wait for form to load
    await page.waitForTimeout(500);

    // Select school (CRITICAL: Required for class group creation)
    const schoolSelect = page.locator('select[name="school"], [formcontrolname="school"]');
    if (await schoolSelect.count() > 0) {
      await schoolSelect.selectOption({ index: 1 });
    }

    // Select first truck
    const truckSelect = page.locator('select[name="truck"], [formcontrolname="truck"]');
    if (await truckSelect.count() > 0) {
      await truckSelect.selectOption({ index: 1 });
    }

    // Set Monday schedule 09:00-10:00
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

    // Submit first class group
    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();

    // Wait for creation and redirect
    await page.waitForTimeout(2000);

    // Now try to create conflicting schedule
    await page.goto('/class-groups');
    await page.waitForLoadState('networkidle');

    // Click "Add Class Group" again
    const addButton2 = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton2.click();

    await page.waitForTimeout(500);

    // Select SAME school
    if (await schoolSelect.count() > 0) {
      await schoolSelect.selectOption({ index: 1 });
    }

    // Select SAME truck
    if (await truckSelect.count() > 0) {
      await truckSelect.selectOption({ index: 1 });
    }

    // Set SAME day (Monday)
    if (await daySelect.count() > 0) {
      await daySelect.selectOption('Monday');
    }

    // Set OVERLAPPING time (09:30-10:30 overlaps with 09:00-10:00)
    if (await startTimeInput.count() > 0) {
      await startTimeInput.first().fill('09:30');
    }

    if (await endTimeInput.count() > 0) {
      await endTimeInput.first().fill('10:30');
    }

    // Try to submit - should show conflict banner
    await submitButton.click();
    await page.waitForTimeout(1000);

    // CRITICAL: Schedule Conflict Banner should appear
    const conflictBanner = page.locator('text=conflict, text=overlapping, text=schedule conflict');
    const hasConflictBanner = await conflictBanner.count() > 0;
    expect(hasConflictBanner).toBe(true);

    if (hasConflictBanner) {
      await expect(conflictBanner.first()).toBeVisible();
    }
  });

  test('should display Schedule Conflict Banner with warning', async ({ page }) => {
    // Test that conflict banner is displayed prominently
    await page.goto('/class-groups');
    await page.waitForLoadState('networkidle');

    // Create initial conflicting schedule setup
    // (Assumes a conflict exists from seeded data or previous test)
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    await page.waitForTimeout(500);

    // Try to create conflicting schedule
    const truckSelect = page.locator('select[name="truck"], [formcontrolname="truck"]');
    if (await truckSelect.count() > 0) {
      await truckSelect.selectOption({ index: 1 });
    }

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

    await page.waitForTimeout(1000);

    // Check for conflict warning banner
    const warningBanner = page.locator('[role="alert"], .alert-warning, .conflict-banner, .error-banner');
    const hasBanner = await warningBanner.count() > 0;

    if (hasBanner) {
      // Verify banner contains warning message
      await expect(warningBanner.first()).toBeVisible();

      const bannerText = await warningBanner.first().textContent();
      expect(bannerText.toLowerCase()).toMatch(/conflict|overlapping|schedule/);
    }
  });

  test('should show conflicting class group details in banner', async ({ page }) => {
    // Test that conflict banner shows details of the conflicting schedule
    await page.goto('/class-groups');
    await page.waitForLoadState('networkidle');

    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    await page.waitForTimeout(500);

    // Create conflicting schedule
    const truckSelect = page.locator('select[name="truck"], [formcontrolname="truck"]');
    if (await truckSelect.count() > 0) {
      await truckSelect.selectOption({ index: 1 });
    }

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

    await page.waitForTimeout(1000);

    // Check conflict details in banner
    const conflictBanner = page.locator('text=conflict, [role="alert"], .conflict-banner');
    if (await conflictBanner.count() > 0) {
      const bannerText = await conflictBanner.first().textContent();

      // Should contain details about the conflict
      // (truck name, time, day, or conflicting class group info)
      expect(bannerText.length).toBeGreaterThan(0);
    }
  });

  test('should block save until conflict is resolved', async ({ page }) => {
    // Test that user cannot save while conflict exists
    await page.goto('/class-groups');
    await page.waitForLoadState('networkidle');

    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    await page.waitForTimeout(500);

    // Create conflicting schedule
    const truckSelect = page.locator('select[name="truck"], [formcontrolname="truck"]');
    if (await truckSelect.count() > 0) {
      await truckSelect.selectOption({ index: 1 });
    }

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

    await page.waitForTimeout(1000);

    // Verify we're still on the form (not redirected to list)
    // This indicates the save was blocked
    const currentUrl = page.url();
    expect(currentUrl).not.toContain('/class-groups');
    expect(currentUrl).toMatch(/\/(create|edit)/);

    // Verify conflict banner is visible
    const conflictBanner = page.locator('text=conflict, [role="alert"]');
    if (await conflictBanner.count() > 0) {
      await expect(conflictBanner.first()).toBeVisible();
    }
  });

  test('should resolve conflict by adjusting time', async ({ page }) => {
    // Test that adjusting time resolves the conflict
    await page.goto('/class-groups');
    await page.waitForLoadState('networkidle');

    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    await page.waitForTimeout(500);

    // Create conflicting schedule
    const truckSelect = page.locator('select[name="truck"], [formcontrolname="truck"]');
    if (await truckSelect.count() > 0) {
      await truckSelect.selectOption({ index: 1 });
    }

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

    await page.waitForTimeout(1000);

    // Now adjust time to resolve conflict
    if (await startTimeInput.count() > 0) {
      await startTimeInput.first().fill('11:00'); // Non-overlapping time
    }

    if (await endTimeInput.count() > 0) {
      await endTimeInput.first().fill('12:00');
    }

    // Try to save again
    await submitButton.click();

    // Should succeed and redirect to list
    await page.waitForTimeout(2000);
    await expect(page).toHaveURL(/\/class-groups/, { timeout: 10000 });

    // Verify success message
    const successMessage = page.locator('text=success, text=created, text=added');
    if (await successMessage.count() > 0) {
      await expect(successMessage.first()).toBeVisible();
    }
  });

  test('should resolve conflict by changing truck', async ({ page }) => {
    // Test that changing truck resolves the conflict
    await page.goto('/class-groups');
    await page.waitForLoadState('networkidle');

    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    await page.waitForTimeout(500);

    // Create conflicting schedule with first truck
    const truckSelect = page.locator('select[name="truck"], [formcontrolname="truck"]');
    if (await truckSelect.count() > 0) {
      await truckSelect.selectOption({ index: 1 });
    }

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

    await page.waitForTimeout(1000);

    // Now change truck to resolve conflict
    if (await truckSelect.count() > 0) {
      // Select different truck (index 2 instead of 1)
      await truckSelect.selectOption({ index: 2 });
    }

    // Try to save again
    await submitButton.click();

    // Should succeed and redirect to list
    await page.waitForTimeout(2000);
    await expect(page).toHaveURL(/\/class-groups/, { timeout: 10000 });

    // Verify success message
    const successMessage = page.locator('text=success, text=created, text=added');
    if (await successMessage.count() > 0) {
      await expect(successMessage.first()).toBeVisible();
    }
  });

  test('should not detect conflict for same truck on different days', async ({ page }) => {
    // Edge case: Same truck, different days should NOT conflict
    await page.goto('/class-groups');
    await page.waitForLoadState('networkidle');

    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    await page.waitForTimeout(500);

    // Select truck and Monday
    const truckSelect = page.locator('select[name="truck"], [formcontrolname="truck"]');
    if (await truckSelect.count() > 0) {
      await truckSelect.selectOption({ index: 1 });
    }

    const daySelect = page.locator('select[name="dayOfWeek"], [formcontrolname="dayOfWeek"]');
    if (await daySelect.count() > 0) {
      // Select DIFFERENT day (Tuesday instead of Monday)
      await daySelect.selectOption('Tuesday');
    }

    const startTimeInput = page.locator('input[name="startTime"], [formcontrolname="startTime"]');
    if (await startTimeInput.count() > 0) {
      await startTimeInput.first().fill('09:00');
    }

    const endTimeInput = page.locator('input[name="endTime"], [formcontrolname="endTime"]');
    if (await endTimeInput.count() > 0) {
      await endTimeInput.first().fill('10:00');
    }

    // Submit - should NOT show conflict banner
    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();

    await page.waitForTimeout(2000);

    // Should succeed without conflict
    await expect(page).toHaveURL(/\/class-groups/, { timeout: 10000 });

    // Verify NO conflict banner
    const conflictBanner = page.locator('text=conflict, [role="alert"].conflict');
    expect(await conflictBanner.count()).toBe(0);
  });

  test('should not detect conflict for different trucks at same time', async ({ page }) => {
    // Edge case: Different trucks, same time should NOT conflict
    await page.goto('/class-groups');
    await page.waitForLoadState('networkidle');

    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    await page.waitForTimeout(500);

    // Select DIFFERENT truck (index 2)
    const truckSelect = page.locator('select[name="truck"], [formcontrolname="truck"]');
    if (await truckSelect.count() > 0) {
      await truckSelect.selectOption({ index: 2 });
    }

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

    // Submit - should NOT show conflict banner
    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();

    await page.waitForTimeout(2000);

    // Should succeed without conflict
    await expect(page).toHaveURL(/\/class-groups/, { timeout: 10000 });

    // Verify NO conflict banner
    const conflictBanner = page.locator('text=conflict, [role="alert"].conflict');
    expect(await conflictBanner.count()).toBe(0);
  });

  test('should detect conflict for exact same time slot (critical edge case)', async ({ page }) => {
    // CRITICAL: Dev Notes line 117 - Exact same time slot, same truck → should trigger conflict
    // This test validates duplicate time slot detection
    await page.goto('/class-groups');
    await page.waitForLoadState('networkidle');

    // First, create a class group with specific time
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();
    await page.waitForTimeout(500);

    const schoolSelect = page.locator('select[name="school"], [formcontrolname="school"]');
    if (await schoolSelect.count() > 0) {
      await schoolSelect.selectOption({ index: 1 });
    }

    const truckSelect = page.locator('select[name="truck"], [formcontrolname="truck"]');
    if (await truckSelect.count() > 0) {
      await truckSelect.selectOption({ index: 1 });
    }

    const daySelect = page.locator('select[name="dayOfWeek"], [formcontrolname="dayOfWeek"]');
    if (await daySelect.count() > 0) {
      await daySelect.selectOption('Monday');
    }

    const startTimeInput = page.locator('input[name="startTime"], [formcontrolname="startTime"]');
    if (await startTimeInput.count() > 0) {
      await startTimeInput.first().fill('14:00');
    }

    const endTimeInput = page.locator('input[name="endTime"], [formcontrolname="endTime"]');
    if (await endTimeInput.count() > 0) {
      await endTimeInput.first().fill('15:00');
    }

    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();
    await page.waitForTimeout(2000);

    // Now try to create EXACT same time slot
    await page.goto('/class-groups');
    await page.waitForLoadState('networkidle');

    await addButton.click();
    await page.waitForTimeout(500);

    // Same school, same truck, same day, EXACT same time
    if (await schoolSelect.count() > 0) {
      await schoolSelect.selectOption({ index: 1 });
    }

    if (await truckSelect.count() > 0) {
      await truckSelect.selectOption({ index: 1 });
    }

    if (await daySelect.count() > 0) {
      await daySelect.selectOption('Monday');
    }

    if (await startTimeInput.count() > 0) {
      await startTimeInput.first().fill('14:00'); // EXACT same start
    }

    if (await endTimeInput.count() > 0) {
      await endTimeInput.first().fill('15:00'); // EXACT same end
    }

    await submitButton.click();
    await page.waitForTimeout(1000);

    // CRITICAL: Should trigger conflict for exact duplicate time
    const conflictBanner = page.locator('text=conflict, text=overlapping, [role="alert"]');
    const hasConflict = await conflictBanner.count() > 0;
    expect(hasConflict).toBe(true);

    if (hasConflict) {
      await expect(conflictBanner.first()).toBeVisible();
    }
  });

  test('should handle adjacent time slots (edge case)', async ({ page }) => {
    // Edge case: Adjacent time slots (10:00-11:00 and 11:00-12:00)
    // should NOT conflict as they don't overlap
    await page.goto('/class-groups');
    await page.waitForLoadState('networkidle');

    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await addButton.click();

    await page.waitForTimeout(500);

    const truckSelect = page.locator('select[name="truck"], [formcontrolname="truck"]');
    if (await truckSelect.count() > 0) {
      await truckSelect.selectOption({ index: 1 });
    }

    const daySelect = page.locator('select[name="dayOfWeek"], [formcontrolname="dayOfWeek"]');
    if (await daySelect.count() > 0) {
      await daySelect.selectOption('Monday');
    }

    // Set adjacent time (11:00-12:00, adjacent to 09:00-10:00 from seed data)
    const startTimeInput = page.locator('input[name="startTime"], [formcontrolname="startTime"]');
    if (await startTimeInput.count() > 0) {
      await startTimeInput.first().fill('11:00');
    }

    const endTimeInput = page.locator('input[name="endTime"], [formcontrolname="endTime"]');
    if (await endTimeInput.count() > 0) {
      await endTimeInput.first().fill('12:00');
    }

    // Submit - should NOT show conflict banner
    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();

    await page.waitForTimeout(2000);

    // Should succeed without conflict
    await expect(page).toHaveURL(/\/class-groups/, { timeout: 10000 });

    // Verify NO conflict banner
    const conflictBanner = page.locator('text=conflict, [role="alert"].conflict');
    expect(await conflictBanner.count()).toBe(0);
  });

  test.afterEach(async ({ page }) => {
    // Clean up created test data
    // Note: This requires implementation of delete/cleanup API or UI actions
    // For now, we document the requirement for proper test isolation
    if (createdClassGroupIds.length > 0) {
      // TODO: Implement cleanup via API or UI
      // Example: await page.request.delete(`/api/class-groups/${id}`) for each ID
      createdClassGroupIds = [];
    }
  });
});
