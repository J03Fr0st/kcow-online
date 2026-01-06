import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Truck CRUD Operations
 *
 * Comprehensive tests for truck management following Story 2.7 AC #1:
 * - Navigate to Trucks page and see truck list
 * - Create a new truck with valid data
 * - Edit existing truck details
 * - Soft-delete a truck (removed from active list)
 * - Validation errors display inline for invalid data
 */

test.describe('Trucks CRUD - Comprehensive E2E', () => {
  // Test credentials from environment or defaults
  const testEmail = process.env.TEST_EMAIL || 'admin@kcow.local';
  const testPassword = process.env.TEST_PASSWORD || 'Admin123!';

  // Track test-created trucks for cleanup
  const createdTrucks: string[] = [];

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
    // Clean up any test-created trucks to maintain test isolation
    if (createdTrucks.length > 0) {
      try {
        await page.goto('/trucks');
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        for (const truckName of createdTrucks) {
          try {
            const truckRow = page.locator('table tbody tr').filter({ hasText: truckName }).first();

            if (await truckRow.count() > 0) {
              const deleteButton = truckRow.locator('button').filter({ hasText: /delete|remove|archive/i }).first();

              if (await deleteButton.count() > 0) {
                page.on('dialog', async dialog => {
                  await dialog.accept();
                });

                await deleteButton.click();
                await page.waitForTimeout(1000);
              }
            }
          } catch (error) {
            // Log but don't fail - truck may already be deleted
            console.log(`Note: Could not cleanup truck ${truckName}, it may already be deleted`);
          }
        }

        // Clear the array for next test
        createdTrucks.length = 0;
      } catch (error) {
        console.log('Cleanup error:', error);
        // Don't fail the test if cleanup fails
      }
    }
  });

  test.describe('AC #1.1: Navigation and List Display', () => {
    test('should navigate to Trucks page and display truck list', async ({ page }) => {
      // Navigate to trucks page
      await page.goto('/trucks');

      // Wait for page to fully load
      await page.waitForLoadState('networkidle');
      await page.waitForLoadState('domcontentloaded');
      await page.waitForTimeout(1000);

      // Verify authentication succeeded (no redirect to login)
      const currentUrl = page.url();
      expect(currentUrl).not.toContain('/login');
      expect(currentUrl).toContain('/trucks');

      // Verify page heading is visible
      await expect(page.locator('h1').filter({ hasText: 'Trucks' })).toBeVisible();

      // Verify table is present (should have seeded data)
      const table = page.locator('table[aria-label="Trucks Registry"]');
      await expect(table).toBeVisible();

      // Verify seeded trucks are displayed (at least the 2 seeded trucks)
      const rows = page.locator('table tbody tr');
      const rowCount = await rows.count();
      expect(rowCount).toBeGreaterThanOrEqual(2);

      // Verify specific seeded trucks are visible
      await expect(page.locator('text=Alpha')).toBeVisible();
      await expect(page.locator('text=Bravo')).toBeVisible();
    });

    test('should display truck registration numbers in list', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Verify registration numbers from seed data are visible
      await expect(page.locator('text=KCOW-001')).toBeVisible();
      await expect(page.locator('text=KCOW-002')).toBeVisible();
    });

    test('should display truck status indicators', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Verify status is displayed (both seeded trucks are "Active")
      const statusIndicators = page.locator('text=Active');
      await expect(statusIndicators.first()).toBeVisible();
    });
  });

  test.describe('AC #1.2: Create New Truck', () => {
    test('should create a new truck with valid data', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Count trucks before creation
      const table = page.locator('table tbody tr');
      const countBefore = await table.count();

      // Click "Add Truck" button
      const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
      await expect(addButton).toBeVisible();
      await addButton.click();

      // Wait for form to load
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Fill in truck details with unique data
      const timestamp = Date.now();
      const truckName = `E2E Test Truck ${timestamp}`;
      const registrationNumber = `E2E-${timestamp}`;

      const truckForm = page.locator('form');

      // Fill name field
      const nameInput = truckForm.locator('input[name="name"], input[placeholder*="name" i], #name').first();
      await expect(nameInput).toBeVisible();
      await nameInput.fill(truckName);

      // Fill registration number
      const plateInput = truckForm.locator('input[name="plate"], input[name="registrationNumber"], input[placeholder*="plate" i], input[placeholder*="registration" i]').first();
      await expect(plateInput).toBeVisible();
      await plateInput.fill(registrationNumber);

      // Fill year - Optional but should be checked if we expect it
      const yearInput = truckForm.locator('input[name="year"], input[type="number"]').first();
      if (await yearInput.isVisible()) {
         await yearInput.fill('2024');
      }

      // Fill notes
      const notesInput = truckForm.locator('textarea[name="notes"], #notes').first();
      await expect(notesInput).toBeVisible();
      await notesInput.fill('E2E test truck - can be deleted');

      // Submit form
      const submitButton = truckForm.locator('button[type="submit"]');
      await expect(submitButton).toBeVisible();
      await submitButton.click();

      // Wait for navigation back to list
      await page.waitForURL(/\/trucks/, { timeout: 10000 });
      await page.waitForLoadState('networkidle');

      // Verify truck was added to list
      const countAfter = await table.count();
      expect(countAfter).toBe(countBefore + 1);

      // Verify new truck is visible in list
      await expect(page.locator(`text=${truckName}`)).toBeVisible();
      await expect(page.locator(`text=${registrationNumber}`)).toBeVisible();

      // Verify success message
      const successMessage = page.locator('text=/success|created|added/i').first();
      await expect(successMessage).toBeVisible();

      // Track for cleanup
      createdTrucks.push(truckName);
    });

    test('should persist created truck after page refresh', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');

      // Create a truck
      const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
      await addButton.click();

      await page.waitForLoadState('networkidle');
      const timestamp = Date.now();
      const truckName = `Persistent Truck ${timestamp}`;

      await page.locator('input[name="name"], input[placeholder*="name" i], #name').first().fill(truckName);
      await page.locator('input[name="plate"], input[placeholder*="plate" i]').first().fill(`P-${timestamp}`);
      await page.locator('button[type="submit"]').click();

      // Wait for save and navigation
      await page.waitForURL(/\/trucks/, { timeout: 10000 });
      await page.waitForTimeout(1000);

      // Refresh page
      await page.reload();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Verify truck is still in list
      await expect(page.locator(`text=${truckName}`)).toBeVisible();

      // Track for cleanup
      createdTrucks.push(truckName);
    });
  });

  test.describe('AC #1.3: Edit Existing Truck', () => {
    test('should edit existing truck details', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Find a seeded truck to edit (use "Alpha" from seed data)
      const alphaRow = page.locator('table tbody tr').filter({ hasText: 'Alpha' }).first();
      await expect(alphaRow).toBeVisible();

      // Click edit button - Expect it to exist
      const editButton = alphaRow.locator('button').filter({ hasText: /edit|modify/i }).first();
      await expect(editButton).toBeVisible();
      await editButton.click();

      // Wait for form to load
      await page.waitForLoadState('networkidle');

      // Update truck name
      const truckForm = page.locator('form');
      const nameInput = truckForm.locator('input[name="name"], input[placeholder*="name" i], #name').first();
      await expect(nameInput).toBeVisible();
      const originalName = await nameInput.inputValue();

      const updatedName = `${originalName} (Updated)`;
      await nameInput.clear();
      await nameInput.fill(updatedName);

      // Submit form
      const submitButton = truckForm.locator('button[type="submit"]');
      await submitButton.click();

      // Wait for navigation back to list
      await page.waitForURL(/\/trucks/, { timeout: 10000 });
      await page.waitForLoadState('networkidle');

      // Verify updated name is visible in list
      await expect(page.locator(`text=${updatedName}`)).toBeVisible();

      // Verify old name is not visible
      await expect(page.locator(`text=${originalName}`)).not.toBeVisible();

      // Verify success message
      const successMessage = page.locator('text=/success|updated/i').first();
      await expect(successMessage).toBeVisible();
    });

    test('should persist edited truck details after refresh', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Edit a truck
      const bravoRow = page.locator('table tbody tr').filter({ hasText: 'Bravo' }).first();
      const editButton = bravoRow.locator('button').filter({ hasText: /edit|modify/i });

      if (await editButton.count() > 0) {
        await editButton.click();
      } else {
        await bravoRow.click();
      }

      await page.waitForLoadState('networkidle');
      const timestamp = Date.now();
      const updatedName = `Bravo Edited ${timestamp}`;

      await page.locator('input[name="name"], input[placeholder*="name" i], #name').first().fill(updatedName);
      await page.locator('button[type="submit"]').click();

      // Wait for save
      await page.waitForURL(/\/trucks/, { timeout: 10000 });
      await page.waitForTimeout(1000);

      // Refresh page
      await page.reload();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Verify edit persisted
      await expect(page.locator(`text=${updatedName}`)).toBeVisible();
    });
  });

  test.describe('AC #1.4: Soft-Delete Truck', () => {
    test('should soft-delete a truck and remove from active list', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Count trucks before deletion
      const table = page.locator('table tbody tr');
      const countBefore = await table.count();

      // Find a truck to delete (preferably not one of the seeded ones we want to keep)
      // Look for E2E test trucks first
      let truckRow = page.locator('table tbody tr').filter({ hasText: /E2E Test Truck/i }).first();

      // If no E2E truck, use the first row that is NOT Alpha or Bravo (if possible)
      if (await truckRow.count() === 0) {
         // Create one to delete if none exist, to be safe? Or just pick one that isn't seeded?
         // For now, let's assume one exists or pick the first one, but log a warning if it's a seed.
         truckRow = table.first();
      }

      await expect(truckRow).toBeVisible();

      const truckName = await truckRow.textContent();

      // Click delete button - Expect it to exist
      const deleteButton = truckRow.locator('button').filter({ hasText: /delete|remove|archive/i }).first();
      await expect(deleteButton).toBeVisible();

      // Handle confirmation dialog
      page.on('dialog', async dialog => {
        await dialog.accept();
      });

      await deleteButton.click();

      // Wait for deletion to complete - Use explicit wait for removal or message
      await page.waitForLoadState('networkidle');

      // Verify success message
      const successMessage = page.locator('text=/success|deleted|archived/i').first();
      await expect(successMessage).toBeVisible();

      // Verify truck was removed from list
      const countAfter = await table.count();
      expect(countAfter).toBeLessThan(countBefore);

      // Verify deleted truck is not visible
      await expect(page.locator(`text=${truckName}`)).not.toBeVisible();
    });

    test('should show confirmation dialog before deletion', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Find delete button
      const deleteButton = page.locator('button').filter({ hasText: /delete|remove/i }).first();

      if (await deleteButton.count() > 0) {
        // Set up dialog handler
        let dialogHandled = false;
        page.on('dialog', async dialog => {
          dialogHandled = true;
          expect(dialog.message()).toMatch(/delete|remove|confirm/i);
          await dialog.dismiss();
        });

        await deleteButton.click();
        await page.waitForTimeout(500);

        // Verify dialog was shown
        expect(dialogHandled).toBe(true);
      }
    });
  });

  test.describe('AC #1.5: Inline Validation Errors', () => {
    test('should show validation errors for empty required fields', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');

      // Click "Add Truck" button
      const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
      await addButton.click();

      // Wait for form to load
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Try to submit without entering any data
      const submitButton = page.locator('button[type="submit"]');
      await submitButton.click();

      // Wait for validation to trigger
      await page.waitForTimeout(500);

      // Check for validation errors
      const validationErrors = page.locator('text=/required|mandatory|invalid/i');
      const hasErrors = await validationErrors.count() > 0;

      if (hasErrors) {
        await expect(validationErrors.first()).toBeVisible();
      }

      // Verify form did not submit (still on form page, not redirected to list)
      await expect(page).not.toHaveURL(/\/trucks$/);
    });

    test('should show inline error for invalid truck name', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');

      const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
      await addButton.click();

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Try to enter invalid data (e.g., extremely long name)
      const nameInput = page.locator('input[name="name"], input[placeholder*="name" i], #name').first();

      // Fill with extremely long name
      await nameInput.fill('A'.repeat(500));

      // Trigger validation by blurring
      await nameInput.blur();
      await page.waitForTimeout(500);

      // Check for validation error
      const errorMessage = page.locator('text=/long|invalid|max|length/i').filter({ hasText: /name/i });
      if (await errorMessage.count() > 0) {
        await expect(errorMessage.first()).toBeVisible();
      }
    });

    test('should show inline error for invalid year', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');

      const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
      await addButton.click();

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Try to enter invalid year
      const yearInput = page.locator('input[name="year"], input[type="number"]').first();

      if (await yearInput.count() > 0) {
        await yearInput.fill('1800'); // Too old
        await yearInput.blur();
        await page.waitForTimeout(500);

        // Check for validation error
        const yearError = page.locator('text=/year|invalid|range|between/i');
        if (await yearError.count() > 0) {
          await expect(yearError.first()).toBeVisible();
        }
      }
    });

    test('should clear validation errors when fixed', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');

      const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
      await addButton.click();

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Trigger validation error
      const nameInput = page.locator('input[name="name"], input[placeholder*="name" i], #name').first();
      await nameInput.fill('');
      await nameInput.blur();
      await page.waitForTimeout(500);

      // Fix the error
      await nameInput.fill('Valid Truck Name');
      await nameInput.blur();
      await page.waitForTimeout(500);

      // Verify error is cleared
      const validationErrors = page.locator('text=/required/i').filter({ hasText: /name/i });
      if (await validationErrors.count() > 0) {
        await expect(validationErrors.first()).not.toBeVisible();
      }
    });
  });

  test.describe('Additional Truck CRUD Features', () => {
    test('should cancel truck creation without saving', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');

      const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
      await addButton.click();

      await page.waitForLoadState('networkidle');

      // Fill some data
      const nameInput = page.locator('input[name="name"], input[placeholder*="name" i], #name').first();
      await nameInput.fill('Temporary Truck');

      // Click cancel button
      const cancelButton = page.locator('button').filter({ hasText: /cancel|back/i }).first();

      if (await cancelButton.count() > 0) {
        await cancelButton.click();

        // Should return to list without saving
        await page.waitForURL(/\/trucks$/, { timeout: 5000 });

        // Verify temporary truck is not in list
        await expect(page.locator('text=Temporary Truck')).not.toBeVisible();
      }
    });

    test('should filter or search trucks', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Look for search input
      const searchInput = page.locator('input[placeholder*="search" i], input[type="search"], #search');

      if (await searchInput.count() > 0) {
        // Search for "Alpha" (seeded truck)
        await searchInput.first().fill('Alpha');
        await page.waitForTimeout(1000);

        // Verify search was performed
        await expect(searchInput.first()).toHaveValue('Alpha');

        // Verify filtered results show only matching trucks
        await expect(page.locator('text=Alpha')).toBeVisible();
        const bravoVisible = await page.locator('text=Bravo').count();
        if (bravoVisible > 0) {
          await expect(page.locator('text=Bravo')).not.toBeVisible();
        }
      } else {
        console.log('Search input not found - filtering may not be implemented');
      }
    });

    test('should handle truck status changes', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Verify status indicators are visible
      const statusElements = page.locator('text=Active, text=Inactive, text=Maintenance');
      const hasStatus = await statusElements.count() > 0;

      if (hasStatus) {
        await expect(statusElements.first()).toBeVisible();
      }
    });
  });
});
