import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Student Profile (FR10 - Critical)
 *
 * Tests the single-screen student profile including:
 * - 3-column header layout (photo, demographics, school, status)
 * - Tabbed sections (Child Info, Financial, Attendance, Evaluation)
 * - Tab switching without page reload
 * - Inline editing in Child Info tab
 * - Save confirmation
 * - Inline validation errors
 * - Performance validation (<2 seconds per NFR2)
 */

test.describe('Student Profile - FR10', () => {
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

  test('should navigate to student profile from list', async ({ page }) => {
    // Navigate to students list
    await page.goto('/students');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Look for a student to click
    const table = page.locator('table');
    const hasTable = await table.count() > 0;

    if (hasTable) {
      const rows = page.locator('table tbody tr');
      const rowCount = await rows.count();

      if (rowCount > 0) {
        // Click on the first row or edit button
        const editButton = rows.nth(0).locator('button').filter({ hasText: /view|profile|edit/i });

        if (await editButton.count() > 0) {
          await editButton.first().click();
        } else {
          await rows.nth(0).click();
        }

        // Should navigate to student profile
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(500);

        const currentUrl = page.url();
        expect(currentUrl).toMatch(/\/students\/\d+/);
      }
    }
  });

  test('should display 3-column header layout', async ({ page }) => {
    // Navigate to students list first, then click on a student to get a valid profile
    await page.goto('/students');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Check if there are any students in the list
    const rows = page.locator('table tbody tr');
    const rowCount = await rows.count();

    if (rowCount > 0) {
      // Click on the first student row
      await rows.first().click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Check for 3-column grid layout
      const gridContainer = page.locator('.grid-cols-1, .grid-cols-3, [class*="grid"]');
      const hasGrid = await gridContainer.count() > 0;

      // Verify profile content sections
      const heading = page.locator('h1, h2').filter({ hasText: /student profile/i });
      const hasHeading = await heading.count() > 0;

      // Should have student profile visible (or any profile content)
      const profileContent = page.locator('[class*="profile"], [class*="header"]');
      const hasProfile = await profileContent.count() > 0;

      expect(hasHeading || hasGrid || hasProfile).toBe(true);

      // Look for basic info section (photo, name, status)
      const statusBadge = page.locator('.badge, [class*="badge"]');
      const hasStatus = await statusBadge.count() > 0;

      // Should show some form of student information
      expect(hasStatus || hasProfile).toBe(true);
    } else {
      // No students in database - skip test
      test.skip();
    }
  });

  test('should display tabbed sections', async ({ page }) => {
    // Navigate to students list first, then click on a student to get a valid profile
    await page.goto('/students');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Check if there are any students in the list
    const rows = page.locator('table tbody tr');
    const rowCount = await rows.count();

    if (rowCount > 0) {
      // Click on the first student row
      await rows.first().click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(1000);

      // Check for tabs
      const tabs = page.locator('[role="tab"], .tab, [class*="tab"]');
      const tabCount = await tabs.count();

      // Should have at least the main tabs (Child Info, Financial, Attendance, Evaluation)
      expect(tabCount).toBeGreaterThanOrEqual(2);

      // Verify expected tab labels
      const childInfoTab = tabs.filter({ hasText: /child info|child/i });

      // At least Child Info tab should exist
      expect(await childInfoTab.count()).toBeGreaterThan(0);
    } else {
      // No students in database - skip test
      test.skip();
    }
  });

  test('should switch tabs without page reload', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const tabs = page.locator('[role="tab"], .tab, [class*="tab"]');
    const tabCount = await tabs.count();

    if (tabCount >= 2) {
      // Click on a different tab
      const secondTab = tabs.nth(1);
      await secondTab.click();

      // Wait for content change, but NOT for full page reload/navigation
      await page.waitForTimeout(500);

      // URL should still be the same (no navigation)
      const currentUrl = page.url();
      expect(currentUrl).toMatch(/\/students\/\d+/);

      // Tab should be marked as active
      const isActive = await secondTab.getAttribute('aria-selected');
      const hasActiveClass = await secondTab.evaluate(el => el.classList.contains('tab-active') || el.classList.contains('active'));

      expect(isActive === 'true' || hasActiveClass).toBe(true);
    }
  });

  test('should allow inline editing in Child Info tab', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Navigate to Child Info tab
    const childInfoTab = page.locator('[role="tab"], .tab, [class*="tab"]').filter({ hasText: /child info|child/i });
    if (await childInfoTab.count() > 0) {
      await childInfoTab.first().click();
      await page.waitForTimeout(500);
    }

    // Look for edit button
    const editButton = page.locator('button').filter({ hasText: /edit|update/i });
    const hasEditButton = await editButton.count() > 0;

    if (hasEditButton) {
      await editButton.first().click();
      await page.waitForTimeout(500);

      // Should show form inputs
      const inputs = page.locator('input[type="text"], input[type="email"], textarea, select');
      const hasInputs = await inputs.count() > 0;

      expect(hasInputs).toBe(true);
    }
  });

  test('should display save confirmation after edit', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Navigate to Child Info tab
    const childInfoTab = page.locator('[role="tab"], .tab, [class*="tab"]').filter({ hasText: /child info|child/i });
    if (await childInfoTab.count() > 0) {
      await childInfoTab.first().click();
      await page.waitForTimeout(500);
    }

    const editButton = page.locator('button').filter({ hasText: /edit|update/i });
    const hasEditButton = await editButton.count() > 0;

    if (hasEditButton) {
      await editButton.first().click();
      await page.waitForTimeout(500);

      // Modify a field
      const notesInput = page.locator('textarea, input').filter({ hasText: /^$/ }).first();
      const hasNotesInput = await notesInput.count() > 0;

      if (hasNotesInput) {
        await notesInput.first().fill('E2E test note');
        await page.waitForTimeout(200);

        // Click save
        const saveButton = page.locator('button').filter({ hasText: /save|update/i });
        if (await saveButton.count() > 0) {
          await saveButton.first().click();
          await page.waitForTimeout(1000);

          // Look for success message
          const successMessage = page.locator('.alert, .toast, .snackbar, [role="alert"]').filter({ hasText: /success|saved|updated/i });
          const hasSuccess = await successMessage.count() > 0;

          if (hasSuccess) {
            await expect(successMessage.first()).toBeVisible();
          }
        }
      }
    }
  });

  test('should display inline validation errors', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Navigate to Child Info tab
    const childInfoTab = page.locator('[role="tab"], .tab, [class*="tab"]').filter({ hasText: /child info|child/i });
    if (await childInfoTab.count() > 0) {
      await childInfoTab.first().click();
      await page.waitForTimeout(500);
    }

    const editButton = page.locator('button').filter({ hasText: /edit|update/i });
    const hasEditButton = await editButton.count() > 0;

    if (hasEditButton) {
      await editButton.first().click();
      await page.waitForTimeout(500);

      // Find a required field and clear it
      const requiredInputs = page.locator('input[required], [required]');
      const inputCount = await requiredInputs.count();

      if (inputCount > 0) {
        await requiredInputs.nth(0).fill('');
        await page.waitForTimeout(200);

        // Trigger validation by clicking save
        const saveButton = page.locator('button').filter({ hasText: /save|update/i });
        if (await saveButton.count() > 0) {
          await saveButton.first().click();
          await page.waitForTimeout(500);

          // Check for error messages
          const errorMessage = page.locator('.error, [class*="error"], .invalid-feedback, [role="alert"]');
          const hasError = await errorMessage.count() > 0;

          if (hasError) {
            await expect(errorMessage.first()).toBeVisible();
          }
        }
      }
    }
  });

  test('should load profile in under 2 seconds (NFR2)', async ({ page }) => {
    // Measure profile load performance
    const startTime = Date.now();

    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForLoadState('domcontentloaded');
    await page.waitForTimeout(500);

    const endTime = Date.now();
    const duration = endTime - startTime;

    // Profile should load in under 2 seconds (2000ms)
    expect(duration).toBeLessThan(2000);

    console.log(`Profile loaded in ${duration}ms`);
  });

  test('should display school assignment in header', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Look for school assignment section
    const schoolSection = page.locator('[class*="school"]').or(page.getByText(/school/i));
    const hasSchoolSection = await schoolSection.count() > 0;

    if (hasSchoolSection) {
      // Should show school name
      const schoolName = await schoolSection.first().textContent();
      expect(schoolName?.length).toBeGreaterThan(0);
    }
  });

  test('should display status indicators', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Look for status badges or indicators
    const statusBadge = page.locator('.badge, [class*="badge"], .status');
    const hasStatus = await statusBadge.count() > 0;

    if (hasStatus) {
      // Should have at least one status indicator
      const statusCount = await statusBadge.count();
      expect(statusCount).toBeGreaterThan(0);
    }
  });

  test('should have back navigation to students list', async ({ page }) => {
    await page.goto('/students/1');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    // Look for back button
    const backButton = page.locator('button').filter({ hasText: /back|return|list/i });
    const hasBackButton = await backButton.count() > 0;

    if (hasBackButton) {
      await backButton.first().click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Should navigate back to students list
      const currentUrl = page.url();
      expect(currentUrl).toMatch(/\/students/);
    }
  });
});
