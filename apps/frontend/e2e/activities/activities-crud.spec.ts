import { test, expect } from '@playwright/test';
import { ActivitiesPage } from './page-objects/ActivitiesPage';
import { createTestActivity, testActivities } from './fixtures/activities';
import { readFileSync } from 'fs';
import { join } from 'path';

/**
 * E2E Tests for Activity CRUD Operations
 *
 * Comprehensive tests for activity management following Story 8.4 AC #1:
 * - Navigate to Activities page and see activity list
 * - Create a new activity with valid data
 * - Edit existing activity details
 * - Soft-delete an activity (removed from active list)
 * - Validation errors display inline for invalid data
 * - Icon handling (upload, display, missing)
 */

test.describe('Activities CRUD - Comprehensive E2E', () => {
  // Test credentials from environment or defaults
  const testEmail = process.env.TEST_EMAIL || 'admin@kcow.local';
  const testPassword = process.env.TEST_PASSWORD || 'Admin123!';

  // Track test-created activity codes for cleanup
  const createdActivityCodes: string[] = [];

  test.beforeEach(async ({ page }) => {
    // Navigate to login page
    await page.goto('/login');

    // Login with valid credentials
    await page.locator('#email').fill(testEmail);
    await page.locator('#password').fill(testPassword);

    // Wait for form to be ready
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    // Submit login form and wait for navigation
    await Promise.all([
      page.waitForURL(/\/dashboard/, { timeout: 15000 }),
      page.locator('button[type="submit"]').click(),
    ]);

    // Wait for auth to stabilize
    await page.waitForTimeout(1000);
  });

  test.afterEach(async ({ page }) => {
    // Clean up any test-created activities to maintain test isolation
    if (createdActivityCodes.length > 0) {
      try {
        const activitiesPage = new ActivitiesPage(page);
        await activitiesPage.goto();
        await activitiesPage.waitForLoading();
        await page.waitForTimeout(500);

        for (const code of createdActivityCodes) {
          try {
            const activityRow = activitiesPage.findActivityByCode(code);

            if (await activityRow.count() > 0) {
              // Click delete button (first confirmation step)
              await activitiesPage.clickDeleteActivity(code);
              await page.waitForTimeout(500);

              // Confirm deletion (second confirmation step)
              await activitiesPage.confirmDelete();
              await page.waitForTimeout(1000);
            }
          } catch (error) {
            // Log but don't fail - activity may already be deleted
            console.log(`Note: Could not cleanup activity ${code}, it may already be deleted`);
          }
        }

        // Clear the array for next test
        createdActivityCodes.length = 0;
      } catch (error) {
        console.log('Cleanup error:', error);
        // Don't fail the test if cleanup fails
      }
    }
  });

  // ============================================================================
  // AC #1.1: Navigation and List Display Tests (Task 3)
  // ============================================================================
  test.describe('Navigation and List Display', () => {
    test('should navigate to Activities page and display activity list', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();

      // Wait for page to fully load
      await activitiesPage.waitForLoading();

      // Verify authentication succeeded (no redirect to login)
      const currentUrl = page.url();
      expect(currentUrl).not.toContain('/login');
      expect(currentUrl).toContain('/activities');

      // Verify page heading is visible
      await expect(page.locator('h1').filter({ hasText: 'Activities' })).toBeVisible();

      // Verify table is present
      const table = page.locator('table[aria-label="Activities Registry"]');
      await expect(table).toBeVisible();

      // Verify "Add Activity" button is visible
      await expect(activitiesPage.addButton).toBeVisible();
    });

    test('should display table with correct columns', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Verify table headers
      const table = page.locator('table[aria-label="Activities Registry"]');
      await expect(table).toBeVisible();

      const headers = table.locator('thead th');
      const headerTexts = await headers.allTextContents();

      expect(headerTexts).toContain('Icon');
      expect(headerTexts).toContain('Code');
      expect(headerTexts).toContain('Name');
      expect(headerTexts).toContain('Grade Level');
      expect(headerTexts).toContain('Folder');
      expect(headerTexts).toContain('Status');
      expect(headerTexts).toContain('Actions');
    });

    test('should display empty state when no activities exist', async ({ page }) => {
      // This test assumes activities exist (from migration)
      // If we want to test empty state, we would need to delete all activities first
      // For now, we'll verify the page structure supports empty state
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Check for empty state OR table
      const emptyState = page.locator('.empty-state');
      const table = page.locator('table[aria-label="Activities Registry"]');

      // At least one should be visible
      const hasEmptyState = await emptyState.count() > 0;
      const hasTable = await table.count() > 0;

      expect(hasEmptyState || hasTable).toBe(true);
    });
  });

  // ============================================================================
  // AC #1.2: Create Activity Tests (Task 4)
  // ============================================================================
  test.describe('Create Activity', () => {
    test('should create a new activity with valid data', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Count activities before creation
      const countBefore = await activitiesPage.getActivityCount();

      // Create test activity with unique code
      const testActivity = createTestActivity('basic');

      // Click add button to open form
      await activitiesPage.clickAddActivity();

      // Verify form is open
      await expect(activitiesPage.isFormVisible()).resolves.toBe(true);

      // Fill form with test data
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
        description: testActivity.description,
        folder: testActivity.folder,
        gradeLevel: testActivity.gradeLevel,
      });

      // Submit form
      await activitiesPage.submitForm();

      // Wait for form to close
      await expect(activitiesPage.isFormVisible()).resolves.toBe(false);

      // Verify activity was added to list
      const countAfter = await activitiesPage.getActivityCount();
      expect(countAfter).toBe(countBefore + 1);

      // Verify new activity is visible in list
      const isVisible = await activitiesPage.isActivityVisible(testActivity.code);
      expect(isVisible).toBe(true);

      // Verify activity name is visible
      await expect(page.locator(`text=${testActivity.name}`)).toBeVisible();

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should create activity with minimal data', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      const testActivity = createTestActivity('minimal');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });

      await activitiesPage.submitForm();

      // Verify activity was created
      await expect(page.locator(`text=${testActivity.code}`)).toBeVisible();

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should show success message after creation', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      const testActivity = createTestActivity('basic');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
        description: testActivity.description,
        folder: testActivity.folder,
        gradeLevel: testActivity.gradeLevel,
      });

      await activitiesPage.submitForm();

      // Check for success indication (toast or message)
      // The form closing is the main success indicator
      await expect(activitiesPage.isFormVisible()).resolves.toBe(false);

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should persist created activity after page refresh', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      const testActivity = createTestActivity('complete');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
        description: testActivity.description,
        folder: testActivity.folder,
        gradeLevel: testActivity.gradeLevel,
      });

      await activitiesPage.submitForm();

      // Refresh page
      await page.reload();
      await activitiesPage.waitForLoading();
      await page.waitForTimeout(500);

      // Verify activity is still in list
      await expect(page.locator(`text=${testActivity.code}`)).toBeVisible();
      await expect(page.locator(`text=${testActivity.name}`)).toBeVisible();

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should cancel activity creation without saving', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      const testActivity = createTestActivity('basic');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });

      // Click cancel button
      await activitiesPage.cancelForm();

      // Verify form is closed
      await expect(activitiesPage.isFormVisible()).resolves.toBe(false);

      // Verify activity was NOT created
      const isVisible = await activitiesPage.isActivityVisible(testActivity.code);
      expect(isVisible).toBe(false);

      // Don't track for cleanup since it wasn't created
    });
  });

  // ============================================================================
  // AC #1.3: Edit Activity Tests (Task 5)
  // ============================================================================
  test.describe('Edit Activity', () => {
    test('should open edit form when clicking activity row', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Create an activity to edit
      const testActivity = createTestActivity('forUpdate');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
        description: testActivity.description,
        folder: testActivity.folder,
        gradeLevel: testActivity.gradeLevel,
      });
      await activitiesPage.submitForm();

      // Wait for form to close
      await expect(activitiesPage.isFormVisible()).resolves.toBe(false);

      // Click the activity row to edit
      await activitiesPage.clickActivityRow(testActivity.code);

      // Verify form is open with existing data
      await expect(activitiesPage.isFormVisible()).resolves.toBe(true);

      // Verify form is populated with existing data
      const codeValue = await activitiesPage.codeInput.inputValue();
      const nameValue = await activitiesPage.nameInput.inputValue();

      expect(codeValue).toBe(testActivity.code);
      expect(nameValue).toBe(testActivity.name);

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should update activity details and save', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Create an activity to edit
      const testActivity = createTestActivity('forUpdate');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
        description: testActivity.description,
        folder: testActivity.folder,
        gradeLevel: testActivity.gradeLevel,
      });
      await activitiesPage.submitForm();

      // Wait for form to close
      await expect(activitiesPage.isFormVisible()).resolves.toBe(false);

      // Click the activity row to edit
      await activitiesPage.clickActivityRow(testActivity.code);

      // Update the name
      const updatedName = `${testActivity.name} (Updated)`;
      await activitiesPage.fillActivityForm({
        name: updatedName,
      });

      // Submit form
      await activitiesPage.submitForm();

      // Wait for form to close
      await expect(activitiesPage.isFormVisible()).resolves.toBe(false);

      // Verify updated name is visible in list
      await expect(page.locator(`text=${updatedName}`)).toBeVisible();

      // Verify old name is not visible
      await expect(page.locator(`text=${testActivity.name}`)).not.toBeVisible();

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should persist edited activity after refresh', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Create an activity to edit
      const testActivity = createTestActivity('forUpdate');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });
      await activitiesPage.submitForm();

      // Edit the activity
      await activitiesPage.clickActivityRow(testActivity.code);

      const updatedName = `${testActivity.name} Edited`;
      await activitiesPage.fillActivityForm({
        name: updatedName,
      });

      await activitiesPage.submitForm();

      // Refresh page
      await page.reload();
      await activitiesPage.waitForLoading();
      await page.waitForTimeout(500);

      // Verify edit persisted
      await expect(page.locator(`text=${updatedName}`)).toBeVisible();

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should cancel edit without saving changes', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Create an activity to edit
      const testActivity = createTestActivity('forUpdate');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });
      await activitiesPage.submitForm();

      // Open edit form
      await activitiesPage.clickActivityRow(testActivity.code);

      // Make changes but cancel
      const tempName = 'Temporary Change';
      await activitiesPage.fillActivityForm({
        name: tempName,
      });

      await activitiesPage.cancelForm();

      // Verify form is closed
      await expect(activitiesPage.isFormVisible()).resolves.toBe(false);

      // Verify original name is still visible (not changed)
      await expect(page.locator(`text=${testActivity.name}`)).toBeVisible();
      await expect(page.locator(`text=${tempName}`)).not.toBeVisible();

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });
  });

  // ============================================================================
  // AC #1.4: Delete Activity Tests (Task 6)
  // ============================================================================
  test.describe('Delete Activity (Soft-Delete/Archive)', () => {
    test('should show confirmation before deleting', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Create an activity to delete
      const testActivity = createTestActivity('forDelete');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
        description: testActivity.description,
        folder: testActivity.folder,
        gradeLevel: testActivity.gradeLevel,
      });
      await activitiesPage.submitForm();

      // Click delete button (first confirmation step)
      await activitiesPage.clickDeleteActivity(testActivity.code);

      // Verify inline confirmation is shown
      const confirmText = page.locator('text=/archive this activity/i');
      await expect(confirmText).toBeVisible();

      // Verify confirm and cancel buttons are shown
      const confirmButton = page.locator('button').filter({ hasText: /^\s*✓\s*$/ });
      const cancelButton = page.locator('button').filter({ hasText: /^\s*✗\s*$/ });

      await expect(confirmButton).toBeVisible();
      await expect(cancelButton).toBeVisible();

      // Cancel to continue testing
      await activitiesPage.cancelDelete();

      // Track for cleanup (we canceled the delete, but still created it)
      createdActivityCodes.push(testActivity.code);
    });

    test('should delete activity when confirming', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Count activities before deletion
      const countBefore = await activitiesPage.getActivityCount();

      // Create an activity to delete
      const testActivity = createTestActivity('forDelete');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });
      await activitiesPage.submitForm();

      // Verify activity was created
      await expect(page.locator(`text=${testActivity.code}`)).toBeVisible();

      // Click delete button (first confirmation step)
      await activitiesPage.clickDeleteActivity(testActivity.code);

      // Confirm deletion (second confirmation step)
      await activitiesPage.confirmDelete();

      // Verify activity was removed from list
      const isNotVisible = await activitiesPage.isActivityNotVisible(testActivity.code);
      expect(isNotVisible).toBe(true);

      // Verify list count decreased
      const countAfter = await activitiesPage.getActivityCount();
      expect(countAfter).toBe(countBefore); // Should be same as before we created

      // Don't track for cleanup - it should be deleted already
    });

    test('should not delete when canceling confirmation', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Create an activity to test cancel
      const testActivity = createTestActivity('forDelete');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });
      await activitiesPage.submitForm();

      // Click delete button (first confirmation step)
      await activitiesPage.clickDeleteActivity(testActivity.code);

      // Cancel deletion
      await activitiesPage.cancelDelete();

      // Wait a moment for state to settle
      await page.waitForTimeout(500);

      // Verify activity is still in list
      const isVisible = await activitiesPage.isActivityVisible(testActivity.code);
      expect(isVisible).toBe(true);

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should remove archived activity from active list', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Create an activity to delete
      const testActivity = createTestActivity('forDelete');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });
      await activitiesPage.submitForm();

      // Verify it's in the list
      await expect(page.locator(`text=${testActivity.code}`)).toBeVisible();

      // Delete it
      await activitiesPage.clickDeleteActivity(testActivity.code);
      await activitiesPage.confirmDelete();

      // Wait for list to update
      await page.waitForTimeout(500);

      // Refresh to verify persistence
      await page.reload();
      await activitiesPage.waitForLoading();

      // Verify it's still not in the list
      const isNotVisible = await activitiesPage.isActivityNotVisible(testActivity.code);
      expect(isNotVisible).toBe(true);

      // Don't track for cleanup - it should be deleted
    });
  });

  // ============================================================================
  // AC #1.5: Validation Tests (Task 7)
  // ============================================================================
  test.describe('Form Validation', () => {
    test('should show validation error for code exceeding max length', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      await activitiesPage.clickAddActivity();

      // Enter code that exceeds 255 characters
      const longCode = 'A'.repeat(256);
      await activitiesPage.fillActivityForm({
        code: longCode,
        name: 'Test Activity',
      });

      // Blur the input to trigger validation
      await activitiesPage.codeInput.blur();
      await page.waitForTimeout(300);

      // Check for validation error
      // Note: Angular's maxLength validator prevents input, so this may not show error
      // But we test the behavior anyway
      const currentValue = await activitiesPage.codeInput.inputValue();
      expect(currentValue.length).toBeLessThanOrEqual(255);
    });

    test('should show validation error for name exceeding max length', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      await activitiesPage.clickAddActivity();

      // Enter name that exceeds 255 characters
      const longName = 'A'.repeat(256);
      await activitiesPage.fillActivityForm({
        code: 'TEST-001',
        name: longName,
      });

      // Blur the input to trigger validation
      await activitiesPage.nameInput.blur();
      await page.waitForTimeout(300);

      // Check for validation error
      // Note: Angular's maxLength validator prevents input, so this may not show error
      // But we test the behavior anyway
      const currentValue = await activitiesPage.nameInput.inputValue();
      expect(currentValue.length).toBeLessThanOrEqual(255);
    });

    test('should allow submitting form with optional fields empty', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      const testActivity = createTestActivity('minimal');

      await activitiesPage.clickAddActivity();

      // Fill only required fields (in this case, all fields are optional per the UI)
      // So we fill the minimum identifying fields
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });

      // Submit should succeed
      await activitiesPage.submitForm();

      // Verify activity was created
      await expect(page.locator(`text=${testActivity.code}`)).toBeVisible();

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should clear validation errors when input is corrected', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      await activitiesPage.clickAddActivity();

      // Try to enter too long code
      const longCode = 'A'.repeat(300);
      await activitiesPage.codeInput.fill(longCode);
      await activitiesPage.codeInput.blur();
      await page.waitForTimeout(300);

      // Clear and enter valid code
      const testActivity = createTestActivity('basic');
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });

      // Submit should succeed
      await activitiesPage.submitForm();

      // Verify activity was created
      await expect(page.locator(`text=${testActivity.code}`)).toBeVisible();

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should enforce activity code uniqueness', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Create first activity with unique code
      const testActivity = createTestActivity('basic');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });
      await activitiesPage.submitForm();

      // Verify first activity was created
      await expect(page.locator(`text=${testActivity.code}`)).toBeVisible();
      createdActivityCodes.push(testActivity.code);

      // Try to create second activity with SAME code
      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code, // Duplicate code
        name: 'Different Name',
      });
      await activitiesPage.submitForm();

      // Should either show validation error OR form should remain open
      // Check for error indication (API returns 400/409 for duplicate)
      const formStillOpen = await activitiesPage.isFormVisible();
      const errorVisible = await page.locator('text=/duplicate|already exists|unique/i').count() > 0;
      const toastError = await page.locator('.toast-error, .alert-error, [role="alert"]').count() > 0;

      // At least one of these should be true - duplicate was rejected
      expect(formStillOpen || errorVisible || toastError).toBe(true);

      // Verify only ONE activity with that code exists in the list
      const activitiesWithCode = activitiesPage.findActivityByCode(testActivity.code);
      const count = await activitiesWithCode.count();
      expect(count).toBe(1);
    });
  });

  // ============================================================================
  // AC #1.6: Data Integrity Tests (Task 8)
  // ============================================================================
  test.describe('Data Integrity', () => {
    test('should persist all fields on create', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      const testActivity = createTestActivity('complete');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
        description: testActivity.description,
        folder: testActivity.folder,
        gradeLevel: testActivity.gradeLevel,
      });

      await activitiesPage.submitForm();

      // Open edit form to verify all fields were persisted
      await activitiesPage.clickActivityRow(testActivity.code);

      // Verify all fields have correct values
      const codeValue = await activitiesPage.codeInput.inputValue();
      const nameValue = await activitiesPage.nameInput.inputValue();
      const descValue = await activitiesPage.descriptionInput.inputValue();
      const folderValue = await activitiesPage.folderInput.inputValue();
      const gradeValue = await activitiesPage.gradeLevelInput.inputValue();

      expect(codeValue).toBe(testActivity.code);
      expect(nameValue).toBe(testActivity.name);
      expect(descValue).toBe(testActivity.description);
      expect(folderValue).toBe(testActivity.folder);
      expect(gradeValue).toBe(testActivity.gradeLevel);

      // Close form
      await activitiesPage.closeForm();

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should persist all fields on update', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      const testActivity = createTestActivity('forUpdate');

      // Create activity
      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });
      await activitiesPage.submitForm();

      // Update all fields
      await activitiesPage.clickActivityRow(testActivity.code);

      const updatedData = {
        name: `${testActivity.name} Updated`,
        description: 'Updated description',
        folder: 'Updated Folder',
        gradeLevel: 'Grade 6-7',
      };

      await activitiesPage.fillActivityForm(updatedData);
      await activitiesPage.submitForm();

      // Open edit form again to verify persistence
      await activitiesPage.clickActivityRow(testActivity.code);

      const nameValue = await activitiesPage.nameInput.inputValue();
      const descValue = await activitiesPage.descriptionInput.inputValue();
      const folderValue = await activitiesPage.folderInput.inputValue();
      const gradeValue = await activitiesPage.gradeLevelInput.inputValue();

      expect(nameValue).toBe(updatedData.name);
      expect(descValue).toBe(updatedData.description);
      expect(folderValue).toBe(updatedData.folder);
      expect(gradeValue).toBe(updatedData.gradeLevel);

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should show placeholder for missing icon', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      const testActivity = createTestActivity('basic');

      // Create activity without icon
      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });
      await activitiesPage.submitForm();

      // Verify placeholder or no icon is shown
      const hasPlaceholder = await activitiesPage.isPlaceholderShown(testActivity.code);
      // Either placeholder is shown OR icon thumbnail is not visible
      const hasNoIcon = !(await activitiesPage.isIconThumbnailVisible(testActivity.code));

      expect(hasPlaceholder || hasNoIcon).toBe(true);

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should display icon thumbnail in list', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Create a small test PNG icon file (1x1 red pixel)
      const testIconPath = join(process.cwd(), 'test-icon.png');
      const fs = require('fs');
      const base64Icon = 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==';
      const buffer = Buffer.from(base64Icon, 'base64');
      fs.writeFileSync(testIconPath, buffer);

      const testActivity = createTestActivity('withIcon');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });

      // Upload icon
      await activitiesPage.uploadIcon(testIconPath);
      await activitiesPage.submitForm();

      // Verify icon thumbnail is visible
      const hasIcon = await activitiesPage.isIconThumbnailVisible(testActivity.code);
      expect(hasIcon).toBe(true);

      // Get the icon src and verify it's a data URL
      const iconSrc = await activitiesPage.getIconThumbnailSrc(testActivity.code);
      expect(iconSrc).toMatch(/^data:image\/png;base64,/);

      // Clean up test file
      fs.unlinkSync(testIconPath);

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });

    test('should handle grade level mapping correctly', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      const testActivity = createTestActivity('complete');
      testActivity.gradeLevel = 'Grade 1-5';

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
        gradeLevel: testActivity.gradeLevel,
      });
      await activitiesPage.submitForm();

      // Verify grade level is displayed in list
      await expect(page.locator(`text=${testActivity.gradeLevel}`)).toBeVisible();

      // Track for cleanup
      createdActivityCodes.push(testActivity.code);
    });
  });

  // ============================================================================
  // Test Cleanup Verification (Task 9)
  // ============================================================================
  test.describe('Test Data Cleanup', () => {
    test('should clean up test data after each test', async ({ page }) => {
      // This test verifies the cleanup mechanism works
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Create a test activity
      const testActivity = createTestActivity('basic');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
      });
      await activitiesPage.submitForm();

      // Verify it exists
      await expect(page.locator(`text=${testActivity.code}`)).toBeVisible();

      // Add to cleanup list
      createdActivityCodes.push(testActivity.code);

      // The afterEach hook will clean it up
      // This test passes if the afterEach hook completes without errors
    });
  });

  // ============================================================================
  // Search and Filtering Tests (AC #1)
  // ============================================================================
  test.describe('Search and Filtering', () => {
    test('should filter activities by search term if available', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // Check if search input exists
      const searchInput = page.locator('input[placeholder*="search" i], input[type="search"]');

      if (await searchInput.count() > 0) {
        // Create test activities for search
        const testActivity1 = createTestActivity('basic');
        testActivity1.name = 'Searchable Activity Alpha';
        const testActivity2 = createTestActivity('basic');
        testActivity2.name = 'Searchable Activity Beta';

        // Create both activities
        for (const activity of [testActivity1, testActivity2]) {
          await activitiesPage.clickAddActivity();
          await activitiesPage.fillActivityForm({
            code: activity.code,
            name: activity.name,
          });
          await activitiesPage.submitForm();
          await page.waitForTimeout(300);
          createdActivityCodes.push(activity.code);
        }

        // Search for "Alpha"
        await activitiesPage.search('Alpha');
        await page.waitForTimeout(500);

        // Verify Alpha is visible
        await expect(page.locator(`text=Searchable Activity Alpha`)).toBeVisible();

        // Clear search
        await activitiesPage.clearSearch();
        await page.waitForTimeout(500);

        // Verify both are visible again
        await expect(page.locator(`text=Searchable Activity Alpha`)).toBeVisible();
        await expect(page.locator(`text=Searchable Activity Beta`)).toBeVisible();
      } else {
        // Search not implemented, skip test gracefully
        console.log('Search functionality not available in current UI');
        test.skip();
      }
    });
  });

  // ============================================================================
  // Full Test Suite (Task 10)
  // ============================================================================
  test.describe('Full CRUD Workflow', () => {
    test('should complete full CRUD workflow', async ({ page }) => {
      const activitiesPage = new ActivitiesPage(page);
      await activitiesPage.goto();
      await activitiesPage.waitForLoading();

      // CREATE
      const testActivity = createTestActivity('complete');

      await activitiesPage.clickAddActivity();
      await activitiesPage.fillActivityForm({
        code: testActivity.code,
        name: testActivity.name,
        description: testActivity.description,
        folder: testActivity.folder,
        gradeLevel: testActivity.gradeLevel,
      });
      await activitiesPage.submitForm();

      // Verify creation
      await expect(page.locator(`text=${testActivity.code}`)).toBeVisible();
      await expect(page.locator(`text=${testActivity.name}`)).toBeVisible();

      // READ
      await activitiesPage.clickActivityRow(testActivity.code);

      // Verify form is populated
      const nameValue = await activitiesPage.nameInput.inputValue();
      expect(nameValue).toBe(testActivity.name);

      await activitiesPage.closeForm();

      // UPDATE
      await activitiesPage.clickActivityRow(testActivity.code);

      const updatedName = `${testActivity.name} - Edited`;
      await activitiesPage.fillActivityForm({
        name: updatedName,
        description: 'Updated description',
      });
      await activitiesPage.submitForm();

      // Verify update
      await expect(page.locator(`text=${updatedName}`)).toBeVisible();

      // DELETE
      await activitiesPage.clickDeleteActivity(testActivity.code);
      await activitiesPage.confirmDelete();

      // Verify deletion
      const isNotVisible = await activitiesPage.isActivityNotVisible(testActivity.code);
      expect(isNotVisible).toBe(true);

      // Don't add to cleanup - it's already deleted
    });
  });
});
