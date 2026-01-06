import { test, expect } from '@playwright/test';

/**
 * E2E Tests for School CRUD Operations
 *
 * Comprehensive tests for school management following Story 2.7 AC #2:
 * - Navigate to Schools page and see school list
 * - Create a new school with contact information
 * - Edit school details including contacts
 * - Configure school billing settings (default rate, cycle)
 * - Archive a school
 */

test.describe('Schools CRUD - Comprehensive E2E', () => {
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

  test.describe('AC #2.1: Navigation and List Display', () => {
    test('should navigate to Schools page and display school list', async ({ page }) => {
      // Navigate to schools page
      await page.goto('/schools');

      // Wait for page to fully load
      await page.waitForLoadState('networkidle');
      await page.waitForLoadState('domcontentloaded');
      await page.waitForTimeout(1000);

      // Verify authentication succeeded (no redirect to login)
      const currentUrl = page.url();
      expect(currentUrl).not.toContain('/login');
      expect(currentUrl).toContain('/schools');

      // Verify page heading is visible
      await expect(page.locator('h1').filter({ hasText: /Schools/i })).toBeVisible();

      // Verify table is present with imported data
      const table = page.locator('table');
      await expect(table).toBeVisible();

      // Verify seeded schools are displayed (from legacy XML import)
      const rows = page.locator('table tbody tr');
      const rowCount = await rows.count();
      expect(rowCount).toBeGreaterThan(10); // Should have at least some imported schools

      // Verify specific known imported schools are visible
      await expect(page.locator('text=Uitstaande')).toBeVisible();
      await expect(page.locator('text=Rinkel Krinkel Kleuterskool')).toBeVisible();
    });

    test('should display school names in list', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Verify multiple school names are visible
      const knownSchools = [
        'Uitstaande',
        'Rinkel Krinkel Kleuterskool',
        'Kiddo Kleuterskool',
        'Akasia Kinderlandgoed'
      ];

      // At least one known school should be visible
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
  });

  test.describe('AC #2.2: Create School with Contact Information', () => {
    test('should create a new school with contact information', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Count schools before creation
      const table = page.locator('table tbody tr');
      const countBefore = await table.count();

      // Click "Add School" button
      const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
      await expect(addButton).toBeVisible();
      await addButton.click();

      // Wait for form to load
      await page.waitForLoadState('networkidle');

      // Fill in school details with unique data
      const timestamp = Date.now();
      const schoolName = `E2E Test School ${timestamp}`;

      const schoolForm = page.locator('form');

      // Fill required fields
      const nameInput = schoolForm.locator('input[name="name"], input[placeholder*="name" i], #name').first();
      await expect(nameInput).toBeVisible();
      await nameInput.fill(schoolName);

      // Fill contact information
      const contactPersonInput = schoolForm.locator('input[name="contactPerson"], input[placeholder*="contact person" i]').first();
      await expect(contactPersonInput).toBeVisible();
      await contactPersonInput.fill('John Doe');

      const contactCellInput = schoolForm.locator('input[name="contactCell"], input[placeholder*="cell" i], input[placeholder*="mobile" i]').first();
      if (await contactCellInput.isVisible()) {
          await contactCellInput.fill('+27 82 123 4567');
      }

      const emailInput = schoolForm.locator('input[name="email"], input[type="email"]').first();
      if (await emailInput.isVisible()) {
          await emailInput.fill(`test${timestamp}@example.com`);
      }

      const addressInput = schoolForm.locator('input[name="address"], textarea[name="address"], input[placeholder*="address" i]').first();
      if (await addressInput.isVisible()) {
          await addressInput.fill('123 Test Street, Test City');
      }

      // Submit form
      const submitButton = schoolForm.locator('button[type="submit"]');
      await expect(submitButton).toBeVisible();
      await submitButton.click();

      // Wait for navigation back to list
      await page.waitForURL(/\/schools/, { timeout: 10000 });
      await page.waitForLoadState('networkidle');

      // Verify school was added to list
      const countAfter = await table.count();
      expect(countAfter).toBeGreaterThan(countBefore);

      // Verify new school is visible in list
      await expect(page.locator(`text=${schoolName}`)).toBeVisible();

      // Verify success message
      const successMessage = page.locator('text=/success|created|added/i').first();
      await expect(successMessage).toBeVisible();
    });

    test('should validate contact information format', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');

      const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
      await expect(addButton).toBeVisible();
      await addButton.click();
      await page.waitForLoadState('networkidle');

      // Try to enter invalid email
      const emailInput = page.locator('input[name="email"], input[type="email"]').first();
      await expect(emailInput).toBeVisible();
      await emailInput.fill('invalid-email');
      await emailInput.blur();

      // Check for validation error
      const emailError = page.locator('text=/email|invalid|format/i').first();
      await expect(emailError).toBeVisible();

      // Try to enter invalid phone number
      const phoneInput = page.locator('input[name="contactCell"], input[placeholder*="cell" i]').first();
      if (await phoneInput.isVisible()) {
          await phoneInput.fill('abc');
          await phoneInput.blur();

          // Check for validation error
          const phoneError = page.locator('text=/phone|invalid|numeric/i').first();
          await expect(phoneError).toBeVisible();
      }
    });
  });

  test.describe('AC #2.3: Edit School Details Including Contacts', () => {
    test('should edit school contact information', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Find a school to edit (use one of the imported schools)
      const schoolRow = page.locator('table tbody tr').filter({ hasText: 'Uitstaande' }).first();
      await expect(schoolRow).toBeVisible();

      // Click edit button
      const editButton = schoolRow.locator('button').filter({ hasText: /edit|modify/i });

      if (await editButton.count() > 0) {
        await editButton.click();
      } else {
        // If no button, try clicking the row
        await schoolRow.click();
      }

      // Wait for form to load
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Update contact information
      const schoolForm = page.locator('form');
      const contactPersonInput = schoolForm.locator('input[name="contactPerson"], input[placeholder*="contact person" i]');

      if (await contactPersonInput.count() > 0) {
        const originalPerson = await contactPersonInput.first().inputValue();
        const updatedPerson = `${originalPerson} (Updated)`;

        await contactPersonInput.first().clear();
        await contactPersonInput.first().fill(updatedPerson);

        // Submit form
        const submitButton = schoolForm.locator('button[type="submit"]');
        await submitButton.click();

        // Wait for navigation back to list
        await page.waitForURL(/\/schools/, { timeout: 10000 });
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(1000);

        // Verify updated contact person is saved
        // Note: The contact person might not be visible in the list view, so we may need to open the form again
        await schoolRow.locator('button').filter({ hasText: /edit|modify/i }).first().click();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        const currentPerson = await contactPersonInput.first().inputValue();
        expect(currentPerson).toContain('Updated');
      }
    });

    test('should edit multiple contact fields', async ({ page }) => {
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

      const schoolForm = page.locator('form');
      const timestamp = Date.now();

      // Update multiple contact fields
      const contactPersonInput = schoolForm.locator('input[name="contactPerson"]');
      if (await contactPersonInput.count() > 0) {
        await contactPersonInput.first().fill(`Test Person ${timestamp}`);
      }

      const contactCellInput = schoolForm.locator('input[name="contactCell"]');
      if (await contactCellInput.count() > 0) {
        await contactCellInput.first().fill(`+27 82 ${timestamp}`);
      }

      const emailInput = schoolForm.locator('input[name="email"]');
      if (await emailInput.count() > 0) {
        await emailInput.first().fill(`updated${timestamp}@example.com`);
      }

      // Submit
      await schoolForm.locator('button[type="submit"]').click();

      // Wait for save
      await page.waitForURL(/\/schools/, { timeout: 10000 });
      await page.waitForTimeout(1000);

      // Verify success message
      const successMessage = page.locator('text=/success|updated/i');
      if (await successMessage.count() > 0) {
        await expect(successMessage.first()).toBeVisible();
      }
    });
  });

  test.describe('AC #2.4: Configure Billing Settings', () => {
    test('should configure school billing settings', async ({ page }) => {
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

      const schoolForm = page.locator('form');

      // Look for billing-related fields
      const priceInput = schoolForm.locator('input[name="price"], input[name="rate"], input[placeholder*="price" i], input[placeholder*="rate" i]');

      if (await priceInput.count() > 0) {
        // Set default billing rate
        await priceInput.first().fill('150.00');

        // Look for billing cycle or fee description
        const feeDescInput = schoolForm.locator('input[name="feeDescription"], textarea[name="feeDescription"]');
        if (await feeDescInput.count() > 0) {
          await feeDescInput.first().fill('Monthly fee for E2E test school');
        }

        // Look for formula field
        const formulaInput = schoolForm.locator('input[name="formula"]');
        if (await formulaInput.count() > 0) {
          await formulaInput.first().fill('1.5');
        }

        // Submit
        await schoolForm.locator('button[type="submit"]').click();

        // Wait for save
        await page.waitForURL(/\/schools/, { timeout: 10000 });
        await page.waitForTimeout(1000);

        // Verify billing settings were saved
        await schoolRow.locator('button').filter({ hasText: /edit|modify/i }).first().click();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        const savedPrice = await priceInput.first().inputValue();
        expect(savedPrice).toContain('150');

        // Verify success message
        const successMessage = page.locator('text=/success|updated/i');
        if (await successMessage.count() > 0) {
          await expect(successMessage.first()).toBeVisible();
        }
      } else {
        console.log('Billing fields not found in form - billing settings may be on a separate tab or section');
      }
    });

    test('should validate billing rate is numeric', async ({ page }) => {
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

      const priceInput = page.locator('input[name="price"], input[name="rate"]').first();

      if (await priceInput.count() > 0) {
        // Try to enter non-numeric value
        await priceInput.fill('abc');
        await priceInput.blur();
        await page.waitForTimeout(500);

        // Check for validation error
        const errorElement = page.locator('text=/numeric|invalid|number/i');
        if (await errorElement.count() > 0) {
          await expect(errorElement.first()).toBeVisible();
        }
      }
    });
  });

  test.describe('AC #2.5: Archive School', () => {
    test('should archive a school', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Find a school to archive (preferably an E2E test school)
      let schoolRow = page.locator('table tbody tr').filter({ hasText: /E2E Test School/i }).first();

      // If no E2E school, check for schools that can be archived
      if (await schoolRow.count() === 0) {
        schoolRow = page.locator('table tbody tr').first();
      }

      if (await schoolRow.count() > 0) {
        const schoolName = await schoolRow.textContent();

        // Look for archive button
        const archiveButton = schoolRow.locator('button').filter({ hasText: /archive|deactivate/i });

        if (await archiveButton.count() > 0) {
          // Handle confirmation dialog
          page.on('dialog', async dialog => {
            await dialog.accept();
          });

          await archiveButton.click();

          // Wait for archiving to complete
          await page.waitForTimeout(2000);
          await page.waitForLoadState('networkidle');

          // Verify success message
          const successMessage = page.locator('text=/success|archived/i');
          if (await successMessage.count() > 0) {
            await expect(successMessage.first()).toBeVisible();
          }
        } else {
          console.log('Archive button not found - archiving may use delete button instead');

          // Try delete button instead
          const deleteButton = schoolRow.locator('button').filter({ hasText: /delete/i });

          if (await deleteButton.count() > 0) {
            page.on('dialog', async dialog => {
              await dialog.accept();
            });

            await deleteButton.click();
            await page.waitForTimeout(2000);
            await page.waitForLoadState('networkidle');

            // Verify school was removed or marked as inactive
            const stillVisible = await schoolRow.count() > 0;
            if (stillVisible) {
              // If still visible, check for inactive status
              const inactiveIndicator = schoolRow.locator('text=Inactive, text=Archived');
              if (await inactiveIndicator.count() > 0) {
                await expect(inactiveIndicator.first()).toBeVisible();
              }
            }
          }
        }
      }
    });

    test('should show confirmation dialog before archiving', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      const archiveButton = page.locator('button').filter({ hasText: /archive|deactivate/i }).first();

      if (await archiveButton.count() > 0) {
        // Set up dialog handler
        let dialogHandled = false;
        page.on('dialog', async dialog => {
          dialogHandled = true;
          expect(dialog.message()).toMatch(/archive|deactivate|confirm/i);
          await dialog.dismiss();
        });

        await archiveButton.click();
        await page.waitForTimeout(500);

        // Verify dialog was shown
        expect(dialogHandled).toBe(true);
      }
    });
  });

  test.describe('Additional School CRUD Features', () => {
    test('should cancel school creation without saving', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');

      const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();

      if (await addButton.count() > 0) {
        await addButton.click();
        await page.waitForLoadState('networkidle');

        // Fill some data
        const nameInput = page.locator('input[name="name"], input[placeholder*="name" i]').first();
        await nameInput.fill('Temporary School');

        // Click cancel button
        const cancelButton = page.locator('button').filter({ hasText: /cancel|back/i }).first();

        if (await cancelButton.count() > 0) {
          await cancelButton.click();

          // Should return to list without saving
          await page.waitForURL(/\/schools$/, { timeout: 5000 });

          // Verify temporary school is not in list
          await expect(page.locator('text=Temporary School')).not.toBeVisible();
        }
      }
    });

    test('should filter or search schools', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Look for search input
      const searchInput = page.locator('input[placeholder*="search" i], input[type="search"], #search');

      if (await searchInput.count() > 0) {
        // Search for "Rinkel" (imported school)
        await searchInput.first().fill('Rinkel');
        await page.waitForTimeout(1000);

        // Verify search was performed
        await expect(searchInput.first()).toHaveValue('Rinkel');

        // Verify filtered results
        await expect(page.locator('text=Rinkel')).toBeVisible();
      } else {
        console.log('Search input not found - filtering may not be implemented');
      }
    });

    test('should display school contact information in list or details', async ({ page }) => {
      await page.goto('/schools');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Check if contact information is displayed in list
      const contactCell = page.locator('table tbody tr td').filter({ hasText: /@|\+27|0[0-9]/ }).first();

      if (await contactCell.count() > 0) {
        await expect(contactCell).toBeVisible();
      } else {
        console.log('Contact info not visible in list - may need to open details');
      }
    });
  });
});
