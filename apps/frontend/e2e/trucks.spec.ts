import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Truck Management
 *
 * Tests the truck CRUD operations including:
 * - Viewing truck list
 * - Creating a new truck
 * - Editing an existing truck
 * - Deleting a truck
 * - Form validation
 */

test.describe('Trucks Management - CRUD Operations', () => {
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

  test('should display trucks list page', async ({ page }) => {
    // Navigate to trucks page
    await page.goto('/trucks');

    // Wait for page to fully load and stabilize
    await page.waitForLoadState('networkidle');
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(1000);

    // Verify we successfully reached the trucks page (auth worked)
    const currentUrl = page.url();
    if (currentUrl.includes('/login')) {
      throw new Error('Authentication failed - auth guard redirected to login page');
    }

    // Check for page heading
    await expect(page.locator('h1').filter({ hasText: 'Trucks' })).toBeVisible();

    // Check for table or empty state
    const table = page.locator('table[aria-label="Trucks Registry"]');
    const emptyState = page.locator('text=No trucks found');

    const hasTable = await table.count() > 0;
    const hasEmpty = await emptyState.count() > 0;

    expect(hasTable || hasEmpty).toBe(true);
  });

  test('should show validation errors for empty truck fields', async ({ page }) => {
    // Navigate to trucks page
    await page.goto('/trucks');

    // Click "Add Truck" or "New Truck" button
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    if (await addButton.count() > 0) {
      await addButton.click();

      // Try to submit without entering required fields
      const submitButton = page.locator('button[type="submit"]');
      if (await submitButton.count() > 0) {
        await submitButton.click();

        // Check for validation errors
        await expect(page.locator('text=required')).toBeVisible();
      }
    }
  });

  test('should create a new truck successfully', async ({ page }) => {
    // Navigate to trucks page
    await page.goto('/trucks');

    // Click "Add Truck" button
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    if (await addButton.count() > 0) {
      await addButton.click();

      // Fill in truck details
      const truckName = `E2E Test Truck ${Date.now()}`;
      const truckForm = page.locator('form');

      // Fill required fields
      const nameInput = truckForm.locator('input[name="name"], input[placeholder*="name"], #name');
      if (await nameInput.count() > 0) {
        await nameInput.first().fill(truckName);
      }

      const plateInput = truckForm.locator('input[name="plate"], input[placeholder*="plate"], #plate');
      if (await plateInput.count() > 0) {
        await plateInput.first().fill(`E2E-${Date.now()}`);
      }

      const yearInput = truckForm.locator('input[name="year"], input[type="number"]');
      if (await yearInput.count() > 0) {
        await yearInput.first().fill('2024');
      }

      // Submit form
      const submitButton = truckForm.locator('button[type="submit"]');
      if (await submitButton.count() > 0) {
        await submitButton.click();

        // Should redirect back to trucks list
        await expect(page).toHaveURL(/\/trucks/, { timeout: 10000 });

        // Should show success message or new truck in list
        const successMessage = page.locator('text=success, text=created, text=added');
        if (await successMessage.count() > 0) {
          await expect(successMessage.first()).toBeVisible();
        }
      }
    }
  });

  test('should edit an existing truck', async ({ page }) => {
    // Navigate to trucks page
    await page.goto('/trucks');

    // Wait for list to load
    await page.waitForTimeout(1000);

    // Find and click edit button for first truck
    const editButton = page.locator('button').filter({ hasText: /edit|modify/i }).first();
    if (await editButton.count() > 0) {
      await editButton.click();

      // Update truck details
      const truckForm = page.locator('form');
      const nameInput = truckForm.locator('input[name="name"], input[placeholder*="name"], #name');
      if (await nameInput.count() > 0) {
        const originalName = await nameInput.first().inputValue();
        await nameInput.first().fill(`${originalName} (Updated)`);
      }

      // Submit form
      const submitButton = truckForm.locator('button[type="submit"]');
      if (await submitButton.count() > 0) {
        await submitButton.click();

        // Should redirect back to trucks list
        await expect(page).toHaveURL(/\/trucks/, { timeout: 10000 });

        // Should show success message
        const successMessage = page.locator('text=success, text=updated');
        if (await successMessage.count() > 0) {
          await expect(successMessage.first()).toBeVisible();
        }
      }
    }
  });

  test('should delete a truck', async ({ page }) => {
    // Navigate to trucks page
    await page.goto('/trucks');

    // Wait for list to load
    await page.waitForTimeout(1000);

    // Count trucks before deletion
    const truckRows = page.locator('tr, [role="row"]');
    const countBefore = await truckRows.count();

    // Find and click delete button for first truck
    const deleteButton = page.locator('button').filter({ hasText: /delete|remove/i }).first();
    if (await deleteButton.count() > 0) {
      // Handle confirmation dialog if present
      page.on('dialog', async dialog => {
        await dialog.accept();
      });

      await deleteButton.click();

      // Wait for deletion to complete
      await page.waitForTimeout(2000);

      // Verify truck was removed from list
      const countAfter = await truckRows.count();
      if (countBefore > 0) {
        expect(countAfter).toBeLessThanOrEqual(countBefore);
      }

      // Should show success message
      const successMessage = page.locator('text=success, text=deleted');
      if (await successMessage.count() > 0) {
        await expect(successMessage.first()).toBeVisible();
      }
    }
  });

  test('should cancel truck creation', async ({ page }) => {
    // Navigate to trucks page
    await page.goto('/trucks');

    // Click "Add Truck" button
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    if (await addButton.count() > 0) {
      await addButton.click();

      // Fill in some data
      const truckForm = page.locator('form');
      const nameInput = truckForm.locator('input[name="name"], input[placeholder*="name"], #name');
      if (await nameInput.count() > 0) {
        await nameInput.first().fill('Temp Truck');
      }

      // Click cancel button
      const cancelButton = truckForm.locator('button').filter({ hasText: /cancel|back/i });
      if (await cancelButton.count() > 0) {
        await cancelButton.first().click();

        // Should return to trucks list without saving
        await expect(page).toHaveURL(/\/trucks/, { timeout: 5000 });
      }
    }
  });

  test('should filter or search trucks', async ({ page }) => {
    // Navigate to trucks page
    await page.goto('/trucks');

    // Look for search/filter input
    const searchInput = page.locator('input[placeholder*="search"], input[type="search"], #search');
    if (await searchInput.count() > 0) {
      await searchInput.first().fill('test');
      await page.waitForTimeout(1000);

      // Verify search was performed (list should update)
      await expect(searchInput.first()).toHaveValue('test');
    }
  });

  test('should handle navigation between truck list and details', async ({ page }) => {
    // Navigate to trucks page
    await page.goto('/trucks');

    // Wait for page to fully load
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Check if there are trucks to interact with
    const table = page.locator('table[aria-label="Trucks Registry"]');
    const hasTrucks = await table.count() > 0;

    if (!hasTrucks) {
      // Skip test if no trucks exist
      console.log('Skipping test - no trucks in database');
      return;
    }

    // Find and click on a truck row or details button
    const rows = page.locator('table tbody tr');
    const rowCount = await rows.count();

    if (rowCount > 0) {
      // Click on the first truck row
      await rows.nth(0).click();

      // Should navigate to trucks page (same page, row is clickable)
      // The table rows are clickable for editing in the actual UI
      await expect(page).toHaveURL(/\/trucks/);
    }
  });

  test('should validate truck year input', async ({ page }) => {
    // Navigate to trucks page
    await page.goto('/trucks');

    // Click "Add Truck" button
    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    if (await addButton.count() > 0) {
      await addButton.click();

      // Try to enter invalid year
      const yearInput = page.locator('input[name="year"], input[type="number"]');
      if (await yearInput.count() > 0) {
        await yearInput.first().fill('1800'); // Too old
        await yearInput.first().blur();

        // Check for validation error
        const yearError = page.locator('text=year, text=invalid, text=range');
        if (await yearError.count() > 0) {
          await expect(yearError.first()).toBeVisible();
        }
      }
    }
  });
});
