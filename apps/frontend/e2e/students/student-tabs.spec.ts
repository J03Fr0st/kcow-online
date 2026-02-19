import { test, expect } from '@playwright/test';

/**
 * Regression Tests for Student Form Tab Switching
 *
 * Regression for commit 3609804: StudentFormComponent tab switching was broken.
 * Before the fix, clicking a tab did not switch the visible content. The component
 * uses @if (activeTab === '...') blocks controlled by (click)="activeTab = '...'"
 * bindings on [role="tab"] anchors.
 *
 * These tests guard against that regression by verifying:
 *   1. All 7 tabs are present in the tab bar
 *   2. Child Info is active by default and shows the correct content
 *   3. Each clickable tab switches to the correct content panel
 *   4. Tab switching does not trigger a full page navigation
 *
 * Route: /students/:id/edit  (uses student-form.component.ts)
 * Tab selector: [role="tab"] inside [role="tablist"]
 * Active tab class: tab-active
 */

const LOGIN_EMAIL = process.env.TEST_EMAIL || 'admin@kcow.local';
const LOGIN_PASSWORD = process.env.TEST_PASSWORD || 'Admin123!';

const TAB_NAMES = [
  'Child Info',
  'Child Financial',
  'Class Groups',
  'Child Attendance',
  'Class Groups Attendance',
  'Child Evaluation',
  'Class Groups Evaluation',
];

test.describe('Student Form Tab Switching Regression — tab fix (3609804)', () => {
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

    // Resolve dynamic edit URL
    await page.goto('/students');
    await page.waitForLoadState('networkidle');
    await expect(page.locator('table tbody')).toBeVisible({ timeout: 10000 });

    const rowCount = await page.locator('table tbody tr').count();
    if (rowCount === 0) {
      studentEditUrl = null;
      return;
    }

    const firstRow = page.locator('table tbody tr').first();

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

    // Navigate to edit page and wait for form to load
    if (studentEditUrl) {
      await page.goto(studentEditUrl);
      await page.waitForLoadState('networkidle');
      await expect(page.locator('.loading.loading-spinner.loading-lg')).not.toBeVisible({ timeout: 5000 });
      await expect(page.locator('form').first()).toBeVisible({ timeout: 3000 });
    }
  });

  test('all 7 tabs are visible in the tab bar', async ({ page }) => {
    if (!studentEditUrl) { test.skip(); return; }

    await expect(page.locator('[role="tablist"]')).toBeVisible();

    for (const tabName of TAB_NAMES) {
      await expect(
        page.locator(`[role="tab"]:has-text("${tabName}")`)
      ).toBeVisible();
    }
  });

  test('Child Info tab is active by default and shows address fields', async ({ page }) => {
    if (!studentEditUrl) { test.skip(); return; }

    // Child Info tab must carry the tab-active class on load
    const infoTab = page.locator('[role="tab"]:has-text("Child Info")');
    await expect(infoTab).toHaveClass(/tab-active/);

    // Address fields must be visible (rendered by @if (activeTab === 'info'))
    await expect(page.locator('input[placeholder="Address Line 1"]')).toBeVisible();
    await expect(page.locator('textarea[placeholder="General Notes"]')).toBeVisible();
  });

  test('clicking Child Financial tab switches content', async ({ page }) => {
    if (!studentEditUrl) { test.skip(); return; }

    const financialTab = page.locator('[role="tab"]:has-text("Child Financial")');
    await financialTab.click();

    // Tab must now be marked active
    await expect(financialTab).toHaveClass(/tab-active/);

    // Financial panel content must be visible
    await expect(
      page.locator('.alert.alert-info').filter({ hasText: /Detailed financial history/i })
    ).toBeVisible();

    // Info tab content must be gone
    await expect(page.locator('input[placeholder="Address Line 1"]')).not.toBeVisible();
  });

  test('clicking Class Groups tab shows academic fields', async ({ page }) => {
    if (!studentEditUrl) { test.skip(); return; }

    const classGroupsTab = page.locator('[role="tab"]:has-text("Class Groups")').first();
    await classGroupsTab.click();

    // Tab must now be marked active
    await expect(classGroupsTab).toHaveClass(/tab-active/);

    // Class Groups panel content must be visible
    await expect(page.locator('h4').filter({ hasText: 'Academic & Transportation Details' })).toBeVisible();
    await expect(page.locator('h5').filter({ hasText: 'Assignment' })).toBeVisible();
  });

  test('clicking Child Attendance tab shows attendance or save-first message', async ({ page }) => {
    if (!studentEditUrl) { test.skip(); return; }

    // Use .first() — "Child Attendance" appears before "Class Groups Attendance"
    const attendanceTab = page.locator('[role="tab"]:has-text("Child Attendance")').first();
    await attendanceTab.click();

    // Tab must now be marked active
    await expect(attendanceTab).toHaveClass(/tab-active/);

    // Either the attendance component or the save-first alert must be present
    const hasAttendanceComponent = await page.locator('app-attendance-tab').count() > 0;
    const hasSaveFirstAlert = await page.locator('.alert.alert-info').filter({ hasText: /Save the student first/i }).count() > 0;

    expect(hasAttendanceComponent || hasSaveFirstAlert).toBeTruthy();
  });

  test('clicking Child Evaluation tab shows evaluation or save-first message', async ({ page }) => {
    if (!studentEditUrl) { test.skip(); return; }

    // Use .first() — "Child Evaluation" appears before "Class Groups Evaluation"
    const evaluationTab = page.locator('[role="tab"]:has-text("Child Evaluation")').first();
    await evaluationTab.click();

    // Tab must now be marked active
    await expect(evaluationTab).toHaveClass(/tab-active/);

    // Either the evaluation component or the save-first alert must be present
    const hasEvaluationComponent = await page.locator('app-evaluation-tab').count() > 0;
    const hasSaveFirstAlert = await page.locator('.alert.alert-info').filter({ hasText: /Save the student first/i }).count() > 0;

    expect(hasEvaluationComponent || hasSaveFirstAlert).toBeTruthy();
  });

  test('tab switching does NOT navigate to a different page', async ({ page }) => {
    if (!studentEditUrl) { test.skip(); return; }

    const originalUrl = page.url();

    // Click Child Financial — URL must stay the same
    await page.locator('[role="tab"]:has-text("Child Financial")').click();
    expect(page.url()).toBe(originalUrl);

    // Click Class Groups — URL must stay the same
    await page.locator('[role="tab"]:has-text("Class Groups")').first().click();
    expect(page.url()).toBe(originalUrl);

    // Click Child Info — URL must stay the same and info content returns
    await page.locator('[role="tab"]:has-text("Child Info")').click();
    expect(page.url()).toBe(originalUrl);

    // Info panel must be visible again (confirms tab switch back worked)
    await expect(page.locator('input[placeholder="Address Line 1"]')).toBeVisible();
  });
});
