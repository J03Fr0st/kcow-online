import { test, expect } from '@playwright/test';

/**
 * Regression Tests for Student Edit Form - OnPush Change Detection Fix
 *
 * Regression for commit 5218fcf: StudentFormComponent uses
 * ChangeDetectionStrategy.OnPush. Before the fix, loadStudent()'s observable
 * callback mutated `isLoading` without calling ChangeDetectorRef.markForCheck(),
 * so Angular never re-evaluated the @if (isLoading) block and the spinner was
 * displayed forever. These tests guard against that regression.
 *
 * Route: /students/:id/edit  (uses student-form.component.ts)
 * Spinner selector: .loading.loading-spinner.loading-lg
 * Form selector: form[formGroup] / form
 * First-name input: [formcontrolname="firstName"] / input[placeholder="First Name"]
 * Save button: button[type="submit"] (text "Save Changes", disabled when form.invalid || isSaving)
 */

test.describe('Student Edit Form - OnPush CD regression', () => {
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

  test('edit form renders without getting stuck on loading spinner', async ({ page }) => {
    // Guard: skip if no students exist in the database
    await page.goto('/students');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const rows = page.locator('table tbody tr');
    const rowCount = await rows.count();
    if (rowCount === 0) {
      test.skip();
      return;
    }

    // Navigate directly to the edit form for student 1
    await page.goto('/students/1/edit');
    await page.waitForLoadState('networkidle');

    // The spinner MUST NOT be visible once the API call resolves.
    // This is the primary regression assertion: before the fix the spinner
    // would stay on screen indefinitely due to OnPush + missing markForCheck().
    const spinner = page.locator('.loading.loading-spinner.loading-lg');
    await expect(spinner).not.toBeVisible({ timeout: 5000 });

    // The form must have rendered in place of the spinner
    const form = page.locator('form');
    await expect(form).toBeVisible({ timeout: 3000 });

    // The firstName input must be present (rendered by the @else branch)
    const firstNameInput = page.locator('[formcontrolname="firstName"], input[placeholder="First Name"]');
    await expect(firstNameInput.first()).toBeVisible();
  });

  test('edit form populates fields with student data', async ({ page }) => {
    // Guard: skip if no students exist in the database
    await page.goto('/students');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const rows = page.locator('table tbody tr');
    const rowCount = await rows.count();
    if (rowCount === 0) {
      test.skip();
      return;
    }

    await page.goto('/students/1/edit');
    await page.waitForLoadState('networkidle');

    // Wait for spinner to disappear before inspecting field values
    const spinner = page.locator('.loading.loading-spinner.loading-lg');
    await expect(spinner).not.toBeVisible({ timeout: 5000 });

    // firstName must be populated with a non-empty value from the API response.
    // If markForCheck() were missing, the form itself would not render and this
    // locator would not exist at all.
    const firstNameInput = page.locator('[formcontrolname="firstName"]').first();
    await expect(firstNameInput).toBeVisible({ timeout: 3000 });

    const value = await firstNameInput.inputValue();
    expect(value.length).toBeGreaterThan(0);
  });

  test('save button is enabled after form loads with valid data', async ({ page }) => {
    // Navigate directly - no student-count guard needed here, but we still
    // check that the form actually loaded (not the spinner) before asserting
    // button state.
    await page.goto('/students/1/edit');
    await page.waitForLoadState('networkidle');

    // Wait for the loading spinner to clear
    const spinner = page.locator('.loading.loading-spinner.loading-lg');
    await expect(spinner).not.toBeVisible({ timeout: 5000 });

    // The form must be visible
    const form = page.locator('form');
    await expect(form).toBeVisible({ timeout: 3000 });

    // The submit button ("Save Changes") must be present.
    // When the form is loaded with valid seeded data (firstName, lastName,
    // reference, schoolId are all required and should be populated), the
    // button should not be disabled.
    const saveButton = page.locator('button[type="submit"]');
    await expect(saveButton).toBeVisible({ timeout: 3000 });

    // Verify the button is not disabled - i.e., the form loaded valid data
    // and Angular's CD correctly reflected the state into the DOM.
    await expect(saveButton).not.toBeDisabled();
  });
});
