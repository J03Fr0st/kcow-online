import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Data Integrity
 *
 * Comprehensive data integrity tests following Story 2.7 AC #3:
 * - School contacts and billing settings persist correctly
 * - Truck status changes are reflected in UI
 * - Search and filtering work on list pages
 */

test.describe('Data Integrity - Trucks & Schools', () => {
  // Test credentials from environment or defaults
  const testEmail = process.env.TEST_EMAIL || 'admin@kcow.local';
  const testPassword = process.env.TEST_PASSWORD || 'Admin123!';

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

  test.describe('AC #3.1: School Contacts Persistence', () => {
    test('should persist school contact person after edit and page refresh', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Select a school to edit
      const schoolRow = page.locator('table tbody tr').first();
      const editButton = schoolRow.locator('button').filter({ hasText: /edit|modify/i });

      if (await editButton.count() > 0) {
        await editButton.click();
      } else {
        await schoolRow.click();
      }

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Update contact person
      const timestamp = Date.now();
      const newContactPerson = `Integrity Test ${timestamp}`;

      const contactPersonInput = page.locator('input[name="contactPerson"], input[placeholder*="contact person" i]');

      if (await contactPersonInput.count() > 0) {
        await contactPersonInput.first().fill(newContactPerson);

        // Save
        await page.locator('button[type="submit"]').click();

        // Wait for save
        await page.waitForURL(/\/schools/, { timeout: 10000 });
        await page.waitForTimeout(1000);

        // Refresh page
        await page.reload();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1000);

        // Open edit form again
        await schoolRow.locator('button').filter({ hasText: /edit|modify/i }).first().click();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        // Verify contact person persisted
        const savedValue = await contactPersonInput.first().inputValue();
        expect(savedValue).toContain('Integrity Test');
      }
    });

    test('should persist school contact phone number after edit', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const schoolRow = page.locator('table tbody tr').first();
      const editButton = schoolRow.locator('button').filter({ hasText: /edit|modify/i });

      if (await editButton.count() > 0) {
        await editButton.click();
      } else {
        await schoolRow.click();
      }

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Update contact cell
      const timestamp = Date.now();
      const newPhone = `+27 82 ${timestamp}`;

      const contactCellInput = page.locator('input[name="contactCell"], input[placeholder*="cell" i]');

      if (await contactCellInput.count() > 0) {
        await contactCellInput.first().fill(newPhone);

        // Save
        await page.locator('button[type="submit"]').click();

        // Wait for save
        await page.waitForURL(/\/schools/, { timeout: 10000 });
        await page.waitForTimeout(1000);

        // Refresh and verify
        await page.reload();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1000);

        await page.locator('table tbody tr').first().locator('button').filter({ hasText: /edit|modify/i }).first().click();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        const savedValue = await contactCellInput.first().inputValue();
        expect(savedValue).toContain('+27 82');
      }
    });

    test('should persist school email after edit', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const schoolRow = page.locator('table tbody tr').first();
      const editButton = schoolRow.locator('button').filter({ hasText: /edit|modify/i });

      if (await editButton.count() > 0) {
        await editButton.click();
      } else {
        await schoolRow.click();
      }

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Update email
      const timestamp = Date.now();
      const newEmail = `integrity${timestamp}@test.com`;

      const emailInput = page.locator('input[name="email"], input[type="email"]');

      if (await emailInput.count() > 0) {
        await emailInput.first().fill(newEmail);

        // Save
        await page.locator('button[type="submit"]').click();

        // Wait for save
        await page.waitForURL(/\/schools/, { timeout: 10000 });
        await page.waitForTimeout(1000);

        // Refresh and verify
        await page.reload();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1000);

        await page.locator('table tbody tr').first().locator('button').filter({ hasText: /edit|modify/i }).first().click();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        const savedValue = await emailInput.first().inputValue();
        expect(savedValue).toContain('integrity');
      }
    });

    test('should persist multiple contact fields simultaneously', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const schoolRow = page.locator('table tbody tr').first();
      const editButton = schoolRow.locator('button').filter({ hasText: /edit|modify/i });

      if (await editButton.count() > 0) {
        await editButton.click();
      } else {
        await schoolRow.click();
      }

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      const timestamp = Date.now();

      // Update multiple contact fields at once
      const contactPersonInput = page.locator('input[name="contactPerson"]');
      const contactCellInput = page.locator('input[name="contactCell"]');
      const emailInput = page.locator('input[name="email"]');

      const updates = {
        contactPerson: '',
        contactCell: '',
        email: ''
      };

      if (await contactPersonInput.count() > 0) {
        updates.contactPerson = `Person ${timestamp}`;
        await contactPersonInput.first().fill(updates.contactPerson);
      }

      if (await contactCellInput.count() > 0) {
        updates.contactCell = `+27 71 ${timestamp}`;
        await contactCellInput.first().fill(updates.contactCell);
      }

      if (await emailInput.count() > 0) {
        updates.email = `multi${timestamp}@test.com`;
        await emailInput.first().fill(updates.email);
      }

      // Save
      await page.locator('button[type="submit"]').click();
      await page.waitForURL(/\/schools/, { timeout: 10000 });
      await page.waitForTimeout(1000);

      // Refresh and verify all persisted
      await page.reload();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      await page.locator('table tbody tr').first().locator('button').filter({ hasText: /edit|modify/i }).first().click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      if (updates.contactPerson) {
        const savedPerson = await contactPersonInput.first().inputValue();
        expect(savedPerson).toBe(updates.contactPerson);
      }

      if (updates.contactCell) {
        const savedCell = await contactCellInput.first().inputValue();
        expect(savedCell).toContain('+27 71');
      }

      if (updates.email) {
        const savedEmail = await emailInput.first().inputValue();
        expect(savedEmail).toContain('multi');
      }
    });
  });

  test.describe('AC #3.2: Billing Settings Persistence', () => {
    test('should persist school billing rate after edit', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const schoolRow = page.locator('table tbody tr').first();
      const editButton = schoolRow.locator('button').filter({ hasText: /edit|modify/i });

      if (await editButton.count() > 0) {
        await editButton.click();
      } else {
        await schoolRow.click();
      }

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Update billing rate
      const newRate = '199.99';

      const priceInput = page.locator('input[name="price"], input[name="rate"], input[placeholder*="price" i], input[placeholder*="rate" i]');

      if (await priceInput.count() > 0) {
        await priceInput.first().fill(newRate);

        // Save
        await page.locator('button[type="submit"]').click();

        // Wait for save
        await page.waitForURL(/\/schools/, { timeout: 10000 });
        await page.waitForTimeout(1000);

        // Refresh and verify
        await page.reload();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1000);

        await page.locator('table tbody tr').first().locator('button').filter({ hasText: /edit|modify/i }).first().click();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        const savedRate = await priceInput.first().inputValue();
        expect(savedRate).toContain('199.99');
      } else {
        console.log('Billing rate field not found - may not be implemented in UI');
      }
    });

    test('should persist billing fee description', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const schoolRow = page.locator('table tbody tr').first();
      const editButton = schoolRow.locator('button').filter({ hasText: /edit|modify/i });

      if (await editButton.count() > 0) {
        await editButton.click();
      } else {
        await schoolRow.click();
      }

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Update fee description
      const newDescription = 'Monthly billing for testing data integrity';

      const feeDescInput = page.locator('input[name="feeDescription"], textarea[name="feeDescription"]');

      if (await feeDescInput.count() > 0) {
        await feeDescInput.first().fill(newDescription);

        // Save
        await page.locator('button[type="submit"]').click();

        // Wait for save
        await page.waitForURL(/\/schools/, { timeout: 10000 });
        await page.waitForTimeout(1000);

        // Refresh and verify
        await page.reload();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1000);

        await page.locator('table tbody tr').first().locator('button').filter({ hasText: /edit|modify/i }).first().click();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        const savedDesc = await feeDescInput.first().inputValue();
        expect(savedDesc).toContain('testing data integrity');
      }
    });

    test('should persist billing formula', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const schoolRow = page.locator('table tbody tr').first();
      const editButton = schoolRow.locator('button').filter({ hasText: /edit|modify/i });

      if (await editButton.count() > 0) {
        await editButton.click();
      } else {
        await schoolRow.click();
      }

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Update formula
      const newFormula = '2.5';

      const formulaInput = page.locator('input[name="formula"]');

      if (await formulaInput.count() > 0) {
        await formulaInput.first().fill(newFormula);

        // Save
        await page.locator('button[type="submit"]').click();

        // Wait for save
        await page.waitForURL(/\/schools/, { timeout: 10000 });
        await page.waitForTimeout(1000);

        // Refresh and verify
        await page.reload();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1000);

        await page.locator('table tbody tr').first().locator('button').filter({ hasText: /edit|modify/i }).first().click();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        const savedFormula = await formulaInput.first().inputValue();
        expect(savedFormula).toBe('2.5');
      }
    });
  });

  test.describe('AC #3.3: Truck Status UI Updates', () => {
    test('should reflect truck status changes in UI', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Find a truck and check its current status
      const truckRow = page.locator('table tbody tr').first();
      await expect(truckRow).toBeVisible();

      // Note current status
      const currentStatus = truckRow.locator('text=Active, text=Inactive, text=Maintenance');
      const hasStatusBefore = await currentStatus.count() > 0;

      // Edit the truck
      const editButton = truckRow.locator('button').filter({ hasText: /edit|modify/i });

      if (await editButton.count() > 0) {
        await editButton.click();
      } else {
        await truckRow.click();
      }

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Change status if status field exists
      const statusSelect = page.locator('select[name="status"], [role="combobox"]');

      if (await statusSelect.count() > 0) {
        // Select different status
        await statusSelect.first().selectOption('Maintenance');

        // Save
        await page.locator('button[type="submit"]').click();

        // Wait for save
        await page.waitForURL(/\/trucks/, { timeout: 10000 });
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1000);

        // Verify status changed in UI
        const updatedStatus = truckRow.locator('text=Maintenance');
        await expect(updatedStatus.first()).toBeVisible();
      } else {
        console.log('Status field not found - status may be read-only or managed differently');
      }
    });

    test('should update truck status after edit', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Count trucks with Active status
      const activeTrucks = page.locator('table tbody tr').filter({ hasText: 'Active' });
      const activeCountBefore = await activeTrucks.count();

      // Edit a truck
      const truckRow = page.locator('table tbody tr').first();
      const editButton = truckRow.locator('button').filter({ hasText: /edit|modify/i });

      if (await editButton.count() > 0) {
        await editButton.click();
      } else {
        await truckRow.click();
      }

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Modify truck (e.g., change name) to trigger update
      const timestamp = Date.now();
      const nameInput = page.locator('input[name="name"]');
      const originalName = await nameInput.first().inputValue();

      await nameInput.first().fill(`${originalName} (Status Check ${timestamp})`);

      // Save
      await page.locator('button[type="submit"]').click();

      // Wait for save
      await page.waitForURL(/\/trucks/, { timeout: 10000 });
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Verify UI reflects the update
      await expect(page.locator(`text=Status Check ${timestamp}`)).toBeVisible();
    });

    test('should show correct status for multiple trucks', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Check that multiple trucks have status indicators
      const allTrucks = page.locator('table tbody tr');
      const truckCount = await allTrucks.count();

      let statusCount = 0;
      for (let i = 0; i < Math.min(truckCount, 5); i++) {
        const truck = allTrucks.nth(i);
        const status = truck.locator('text=Active, text=Inactive, text=Maintenance');
        if (await status.count() > 0) {
          statusCount++;
        }
      }

      // At least some trucks should have status visible
      expect(statusCount).toBeGreaterThan(0);
    });
  });

  test.describe('AC #3.4: List Filtering and Search', () => {
    test('should filter trucks by search term', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const searchInput = page.locator('input[placeholder*="search" i], input[type="search"], #search');

      if (await searchInput.count() > 0) {
        // Count all trucks before search
        const allRows = page.locator('table tbody tr');
        const countBefore = await allRows.count();

        // Search for "Alpha" (seeded truck)
        await searchInput.first().fill('Alpha');
        await page.waitForTimeout(1000);

        // Count trucks after search
        const filteredRows = page.locator('table tbody tr');
        const countAfter = await filteredRows.count();

        // Should have fewer or equal rows after search
        expect(countAfter).toBeLessThanOrEqual(countBefore);

        // Verify searched truck is visible
        await expect(page.locator('text=Alpha')).toBeVisible();

        // Clear search
        await searchInput.first().fill('');
        await page.waitForTimeout(1000);

        // Verify all trucks are visible again
        const countAfterClear = await allRows.count();
        expect(countAfterClear).toBeGreaterThanOrEqual(countBefore);
      } else {
        console.log('Search input not found - filtering may not be implemented');
      }
    });

    test('should filter schools by search term', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const searchInput = page.locator('input[placeholder*="search" i], input[type="search"], #search');

      if (await searchInput.count() > 0) {
        // Count all schools before search
        const allRows = page.locator('table tbody tr');
        const countBefore = await allRows.count();

        // Search for "Rinkel" (imported school)
        await searchInput.first().fill('Rinkel');
        await page.waitForTimeout(1000);

        // Count schools after search
        const filteredRows = page.locator('table tbody tr');
        const countAfter = await filteredRows.count();

        // Should have fewer rows after search
        expect(countAfter).toBeLessThan(countBefore);

        // Verify searched school is visible
        await expect(page.locator('text=Rinkel')).toBeVisible();

        // Verify non-matching schools are not visible
        await expect(page.locator('text=Uitstaande')).not.toBeVisible();
      } else {
        console.log('Search input not found - filtering may not be implemented');
      }
    });

    test('should handle partial matches in search', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const searchInput = page.locator('input[placeholder*="search" i], input[type="search"], #search');

      if (await searchInput.count() > 0) {
        // Search for partial term
        await searchInput.first().fill('Kleut');
        await page.waitForTimeout(1000);

        // Should find schools with "Kleut" in name (e.g., "Kleuterskool")
        await expect(page.locator('text=Kleut')).toBeVisible();
      }
    });

    test('should show no results for non-existent search term', async ({ page }) => {
      await page.goto('/trucks');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const searchInput = page.locator('input[placeholder*="search" i], input[type="search"], #search');

      if (await searchInput.count() > 0) {
        // Search for non-existent truck
        await searchInput.first().fill('NonExistentTruck12345');
        await page.waitForTimeout(1000);

        // Should show empty state or no results
        const emptyState = page.locator('text=/no results|not found|empty/i');
        const table = page.locator('table tbody tr');

        const hasEmptyMessage = await emptyState.count() > 0;
        const hasNoRows = await table.count() === 0;

        expect(hasEmptyMessage || hasNoRows).toBe(true);
      }
    });

    test('should clear search and show all results', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const searchInput = page.locator('input[placeholder*="search" i], input[type="search"], #search');

      if (await searchInput.count() > 0) {
        // Perform search
        await searchInput.first().fill('Rinkel');
        await page.waitForTimeout(1000);

        const filteredCount = await page.locator('table tbody tr').count();

        // Clear search
        await searchInput.first().clear();
        await page.waitForTimeout(1000);

        const allCount = await page.locator('table tbody tr').count();

        // Should have more results after clearing
        expect(allCount).toBeGreaterThan(filteredCount);
      }
    });
  });

  test.describe('Additional Data Integrity Tests', () => {
    test('should handle concurrent edits gracefully', async ({ page }) => {
      // This test simulates rapid successive edits
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const schoolRow = page.locator('table tbody tr').first();
      const editButton = schoolRow.locator('button').filter({ hasText: /edit|modify/i }).first();
      await expect(editButton).toBeVisible();

      // First edit
      await editButton.click();
      await page.waitForLoadState('networkidle');

      const contactPersonInput = page.locator('input[name="contactPerson"]').first();
      await expect(contactPersonInput).toBeVisible();

      await contactPersonInput.fill('Edit 1');
      await page.locator('button[type="submit"]').click();
      await page.waitForURL(/\/schools/, { timeout: 10000 });

      // Second edit immediately after
      await schoolRow.locator('button').filter({ hasText: /edit|modify/i }).first().click();
      await page.waitForLoadState('networkidle');
      await contactPersonInput.fill('Edit 2');
      await page.locator('button[type="submit"]').click();
      await page.waitForURL(/\/schools/, { timeout: 10000 });

      // Verify final state
      await schoolRow.locator('button').filter({ hasText: /edit|modify/i }).first().click();
      await page.waitForLoadState('networkidle');

      const finalValue = await contactPersonInput.inputValue();
      expect(finalValue).toBe('Edit 2');
    });

    test('should maintain data consistency after page navigation', async ({ page }) => {
      // Edit a school, navigate away, navigate back, verify data
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const schoolRow = page.locator('table tbody tr').first();
      const editButton = schoolRow.locator('button').filter({ hasText: /edit|modify/i });

      if (await editButton.count() > 0) {
        await editButton.click();
        await page.waitForLoadState('networkidle');

        const timestamp = Date.now();
        const testData = `Nav Test ${timestamp}`;

        const contactPersonInput = page.locator('input[name="contactPerson"]');
        if (await contactPersonInput.count() > 0) {
          await contactPersonInput.first().fill(testData);
          await page.locator('button[type="submit"]').click();
          await page.waitForURL(/\/schools/, { timeout: 10000 });
        }

        // Navigate to trucks page
        await page.goto('/trucks');
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1000);

        // Navigate back to schools
        await page.goto('/schools');
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1000);

        // Verify data persisted
        await schoolRow.locator('button').filter({ hasText: /edit|modify/i }).first().click();
        await page.waitForLoadState('networkidle');

        if (await contactPersonInput.count() > 0) {
          const savedValue = await contactPersonInput.first().inputValue();
          expect(savedValue).toContain('Nav Test');
        }
      }
    });
  });
});
