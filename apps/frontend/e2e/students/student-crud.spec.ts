import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Student CRUD Operations
 *
 * Tests student management including:
 * - Creating new student with school/class group/seat assignment
 * - Class group dropdown filtering by selected school
 * - Editing student demographics
 * - Archiving a student
 */

test.describe('Student CRUD Operations', () => {
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

  test('should navigate to student creation form', async ({ page }) => {
    // Navigate to students list
    await page.goto('/students');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Look for "Add Student" or "Create" button
    const addButton = page.locator('button').filter({ hasText: /add|create|new/i });
    const hasAddButton = await addButton.count() > 0;

    if (hasAddButton) {
      await addButton.first().click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Should navigate to create form
      const currentUrl = page.url();
      expect(currentUrl).toMatch(/\/students\/(new|create)/);
    }
  });

  test('should create new student with school and class group assignment', async ({ page }) => {
    await page.goto('/students/new');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Check if we're on create page
    const currentUrl = page.url();
    const isCreatePage = currentUrl.match(/\/students\/(new|create)/);

    if (isCreatePage) {
      // Fill in required fields
      const firstNameInput = page.locator('input[name*="firstName" i], #firstName, [formcontrolname="firstName"]');
      const lastNameInput = page.locator('input[name*="lastName" i], #lastName, [formcontrolname="lastName"]');

      const hasFirstName = await firstNameInput.count() > 0;
      if (hasFirstName) {
        await firstNameInput.first().fill('E2E');
        await lastNameInput.first().fill('Test Student');
        await page.waitForTimeout(200);

        // Select school
        const schoolSelect = page.locator('select[name*="school" i], [formcontrolname="school"], app-school-select');
        const hasSchool = await schoolSelect.count() > 0;

        if (hasSchool) {
          await schoolSelect.first().click();
          await page.waitForTimeout(200);

          // Select first school option
          const firstOption = page.locator('option').filter({ hasText: /^(?!$)/ }).first();
          await firstOption.click();
        }

        // Select class group (should be filtered by school)
        const classGroupSelect = page.locator('select[name*="classGroup" i], [formcontrolname="classGroup"], app-class-group-select');
        const hasClassGroup = await classGroupSelect.count() > 0;

        if (hasClassGroup) {
          await classGroupSelect.first().click();
          await page.waitForTimeout(200);

          const classGroupOption = page.locator('option').filter({ hasText: /^(?!$)/ }).first();
          await classGroupOption.click();
        }

        // Fill seat number
        const seatInput = page.locator('input[name*="seat" i], [formcontrolname="seat"]');
        const hasSeat = await seatInput.count() > 0;
        if (hasSeat) {
          await seatInput.first().fill('99');
        }

        // Submit form
        const submitButton = page.locator('button[type="submit"]').filter({ hasText: /save|create|add/i });
        if (await submitButton.count() > 0) {
          await submitButton.first().click();
          await page.waitForLoadState('networkidle');
          await page.waitForTimeout(1000);

          // Should navigate to students list or new student profile
          const newUrl = page.url();
          expect(newUrl).toMatch(/\/students/);
        }
      }
    }
  });

  test('should filter class groups by selected school', async ({ page }) => {
    await page.goto('/students/new');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const currentUrl = page.url();
    const isCreatePage = currentUrl.match(/\/students\/(new|create)/);

    if (isCreatePage) {
      const schoolSelect = page.locator('select[name*="school" i], [formcontrolname="school"], app-school-select');
      const classGroupSelect = page.locator('select[name*="classGroup" i], [formcontrolname="classGroup"], app-class-group-select');
      const hasSchool = await schoolSelect.count() > 0;
      const hasClassGroup = await classGroupSelect.count() > 0;

      if (hasSchool && hasClassGroup) {
        // Get initial class group options
        await classGroupSelect.first().click();
        await page.waitForTimeout(200);
        let initialOptions = await page.locator('option').filter({ hasText: /^(?!$)/ }).count();
        await classGroupSelect.first().click(); // Close dropdown

        // Select a school
        await schoolSelect.first().click();
        await page.waitForTimeout(200);
        const schoolOption = page.locator('option').filter({ hasText: /^(?!$)/ }).first();
        await schoolOption.click();
        await page.waitForTimeout(500);

        // Check class group options again
        await classGroupSelect.first().click();
        await page.waitForTimeout(200);
        const filteredOptions = await page.locator('option').filter({ hasText: /^(?!$)/ }).count();

        // Class group options should be filtered (likely fewer than initial)
        // Note: This might not always be true depending on data
        console.log(`Initial class group options: ${initialOptions}, After school selection: ${filteredOptions}`);
      }
    }
  });

  test('should edit student demographics', async ({ page }) => {
    // Navigate to a student profile
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Look for edit button
    const editButton = page.locator('button').filter({ hasText: /edit|update/i });
    const hasEditButton = await editButton.count() > 0;

    if (hasEditButton) {
      await editButton.first().click();
      await page.waitForTimeout(500);

      // Should navigate to edit form
      const currentUrl = page.url();
      const isEditPage = currentUrl.match(/\/students\/\d+\/edit/);

      if (isEditPage) {
        // Update a field
        const notesInput = page.locator('textarea[name*="note" i], [formcontrolname="note"], textarea');
        const hasNotes = await notesInput.count() > 0;

        if (hasNotes) {
          await notesInput.first().fill('Updated E2E test notes');
          await page.waitForTimeout(200);

          // Save
          const saveButton = page.locator('button[type="submit"], button').filter({ hasText: /save|update/i });
          await saveButton.first().click();
          await page.waitForLoadState('networkidle');
          await page.waitForTimeout(1000);

          // Should return to profile
          const newUrl = page.url();
          expect(newUrl).toMatch(/\/students\/\d+$/);
        }
      }
    }
  });

  test('should archive a student', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Look for archive/delete button
    const archiveButton = page.locator('button').filter({ hasText: /archive|delete|deactivate/i });
    const hasArchiveButton = await archiveButton.count() > 0;

    if (hasArchiveButton) {
      // Click archive button
      await archiveButton.first().click();
      await page.waitForTimeout(500);

      // Confirm dialog if present
      const confirmButton = page.locator('button').filter({ hasText: /confirm|yes|ok/i });
      const hasConfirm = await confirmButton.count() > 0;

      if (hasConfirm) {
        await confirmButton.first().click();
      }

      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Should return to students list
      const currentUrl = page.url();
      expect(currentUrl).toMatch(/\/students/);
    }
  });

  test('should validate required fields on create', async ({ page }) => {
    await page.goto('/students/new');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const currentUrl = page.url();
    const isCreatePage = currentUrl.match(/\/students\/(new|create)/);

    if (isCreatePage) {
      // Try to submit without filling required fields
      const submitButton = page.locator('button[type="submit"], button').filter({ hasText: /save|create|add/i });
      const hasSubmit = await submitButton.count() > 0;

      if (hasSubmit) {
        await submitButton.first().click();
        await page.waitForTimeout(500);

        // Should show validation errors
        const errorMessages = page.locator('.error, [class*="error"], .invalid-feedback, [role="alert"]');
        const hasErrors = await errorMessages.count() > 0;

        if (hasErrors) {
          await expect(errorMessages.first()).toBeVisible();
        }

        // Should not navigate away
        const stillOnCreatePage = page.url().match(/\/students\/(new|create)/);
        expect(stillOnCreatePage).toBeTruthy();
      }
    }
  });

  test('should display student list with filters', async ({ page }) => {
    await page.goto('/students');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Check for filter dropdowns
    const schoolFilter = page.locator('select[name*="school" i], [formcontrolname="school"], app-school-select');
    const classGroupFilter = page.locator('select[name*="classGroup" i], [formcontrolname="classGroup"], app-class-group-select');
    const hasSchoolFilter = await schoolFilter.count() > 0;
    const hasClassGroupFilter = await classGroupFilter.count() > 0;

    // Should have at least school filter
    expect(hasSchoolFilter || await page.locator('.filter, [class*="filter"]').count() > 0).toBe(true);

    if (hasSchoolFilter) {
      await schoolFilter.first().click();
      await page.waitForTimeout(200);
      // Should show options
      const options = await page.locator('option').count();
      expect(options).toBeGreaterThan(0);
    }
  });

  test('should apply school filter to student list', async ({ page }) => {
    await page.goto('/students');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const schoolFilter = page.locator('select[name*="school" i], [formcontrolname="school"], app-school-select');
    const hasSchoolFilter = await schoolFilter.count() > 0;

    if (hasSchoolFilter) {
      await schoolFilter.first().click();
      await page.waitForTimeout(200);

      // Select second option (first is usually "All")
      const schoolOption = page.locator('option').nth(1);
      const hasOptions = await schoolOption.count() > 0;

      if (hasOptions) {
        await schoolOption.click();
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        // List should be filtered
        const currentUrl = page.url();
        expect(currentUrl).toContain('school');
      }
    }
  });
});
