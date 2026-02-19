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
 * Form selector: form (first instance)
 * First-name input: [formcontrolname="firstName"]
 * Save button: button[type="submit"] (text "Save Changes", disabled when form.invalid || isSaving)
 */

const LOGIN_EMAIL = process.env.TEST_EMAIL || 'admin@kcow.local';
const LOGIN_PASSWORD = process.env.TEST_PASSWORD || 'Admin123!';

test.describe('Student Edit Form Regression — OnPush CD fix (5218fcf)', () => {
  let studentEditUrl: string | null = null;

  test.beforeEach(async ({ page }) => {
    // Login
    await page.goto('/login');
    await page.locator('#email').fill(LOGIN_EMAIL);
    await page.locator('#password').fill(LOGIN_PASSWORD);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);
    await Promise.all([
      page.waitForURL(/\/dashboard/, { timeout: 15000 }),
      page.locator('button[type="submit"]').click(),
    ]);
    await page.waitForTimeout(1000);

    // Resolve a real student edit URL dynamically
    await page.goto('/students');
    await page.waitForLoadState('networkidle');
    await expect(page.locator('table tbody')).toBeVisible({ timeout: 10000 });

    const firstRow = page.locator('table tbody tr').first();
    const rowCount = await page.locator('table tbody tr').count();
    if (rowCount === 0) {
      studentEditUrl = null;
      return;
    }

    // Try to find edit link in the row, otherwise extract ID from profile link
    const editLink = firstRow.locator('a[href*="/edit"]').first();
    if (await editLink.count() > 0) {
      studentEditUrl = await editLink.getAttribute('href');
    } else {
      const profileLink = firstRow.locator('a[href*="/students/"]').first();
      if (await profileLink.count() > 0) {
        const href = await profileLink.getAttribute('href');
        const match = href?.match(/\/students\/(\d+)/);
        if (match) {
          studentEditUrl = `/students/${match[1]}/edit`;
        }
      }
    }

    // Fallback: click first row, get URL, derive edit URL
    if (!studentEditUrl) {
      await firstRow.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);
      const url = page.url();
      const match = url.match(/\/students\/(\d+)/);
      if (match) {
        studentEditUrl = `/students/${match[1]}/edit`;
      }
    }
  });

  test('edit form renders without getting stuck on loading spinner', async ({ page }) => {
    if (!studentEditUrl) { test.skip(); return; }

    await page.goto(studentEditUrl);
    await page.waitForLoadState('networkidle');

    // THE CRITICAL ASSERTION: spinner must disappear within 5s.
    // Before the fix the spinner would stay on screen indefinitely due to
    // OnPush + missing markForCheck().
    await expect(page.locator('.loading.loading-spinner.loading-lg')).not.toBeVisible({ timeout: 5000 });

    // Form must be visible (not blank page or stuck spinner)
    await expect(page.locator('form').first()).toBeVisible({ timeout: 3000 });

    // First name input must be present (rendered by the @else branch)
    await expect(page.locator('[formcontrolname="firstName"]')).toBeVisible({ timeout: 3000 });
  });

  test('edit form populates fields with student data', async ({ page }) => {
    if (!studentEditUrl) { test.skip(); return; }

    await page.goto(studentEditUrl);
    await page.waitForLoadState('networkidle');

    // Wait for spinner to disappear before inspecting field values
    await expect(page.locator('.loading.loading-spinner.loading-lg')).not.toBeVisible({ timeout: 5000 });

    // firstName must be populated with a non-empty value from the API response.
    // If markForCheck() were missing, the form itself would not render and this
    // locator would not exist at all.
    const firstNameInput = page.locator('[formcontrolname="firstName"]');
    await expect(firstNameInput).toBeVisible({ timeout: 3000 });

    const firstNameValue = await firstNameInput.inputValue();
    expect(firstNameValue.length).toBeGreaterThan(0);
  });

  test('save button is enabled after form loads with valid data', async ({ page }) => {
    if (!studentEditUrl) { test.skip(); return; }

    await page.goto(studentEditUrl);
    await page.waitForLoadState('networkidle');

    // Wait for the loading spinner to clear
    await expect(page.locator('.loading.loading-spinner.loading-lg')).not.toBeVisible({ timeout: 5000 });

    // The form must be visible
    await expect(page.locator('form').first()).toBeVisible({ timeout: 3000 });

    // The submit button ("Save Changes") must be present.
    // When the form is loaded with valid seeded data (firstName, lastName,
    // reference, schoolId are all required and should be populated), the
    // button should not be disabled.
    const saveButton = page.locator('button[type="submit"]');
    await expect(saveButton).toBeVisible({ timeout: 3000 });

    // Verify the button is not disabled — i.e., the form loaded valid data
    // and Angular's CD correctly reflected the state into the DOM.
    await expect(saveButton).not.toBeDisabled();
  });
});
