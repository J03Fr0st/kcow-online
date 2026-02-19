import { test, expect } from '@playwright/test';

/**
 * Regression Tests for Student Avatar — OLE Photo Decode Fix
 *
 * Regression for commit 5218fcf: StudentAvatarComponent gained a
 * `resolvedPhotoSrc` computed signal that strips OLE headers from legacy
 * MS Access base64 photo exports, scans for JPEG/PNG/BMP magic bytes, and
 * returns a proper `data:image/...;base64,...` URI — or `null` to trigger
 * the initials fallback.
 *
 * Before the fix, raw OLE-wrapped base64 was passed straight to `<img src>`,
 * producing a broken empty-src element (or a garbage data URI that fired the
 * `(error)` handler and logged `[StudentAvatar] Failed to load image for …`).
 *
 * These tests guard that no `img[src=""]` elements exist inside
 * `app-student-avatar` and that the `[StudentAvatar]` console warning is
 * never emitted during normal page rendering.
 *
 * Routes under test:
 *   /students          — list page (avatars in table rows)
 *   /students/:id/edit — edit page (avatar in student form)
 *   /students/create   — create page (avatar driven by typed name)
 */

const LOGIN_EMAIL = process.env.TEST_EMAIL || 'admin@kcow.local';
const LOGIN_PASSWORD = process.env.TEST_PASSWORD || 'Admin123!';

test.describe('Student Avatar Regression — OLE photo decode fix (5218fcf)', () => {
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
  });

  test('student list shows no broken empty-src img inside avatars', async ({ page }) => {
    await page.goto('/students');
    await page.waitForLoadState('networkidle');

    await expect(page.locator('table tbody')).toBeVisible({ timeout: 10000 });

    const rowCount = await page.locator('table tbody tr').count();
    if (rowCount === 0) {
      test.skip();
      return;
    }

    // Any img with src="" inside an avatar component indicates that
    // resolvedPhotoSrc() returned an empty string (broken OLE path before fix).
    // The fixed component only renders <img> when src is non-null/non-empty.
    expect(await page.locator('app-student-avatar img[src=""]').count()).toBe(0);
  });

  test('student edit form shows avatar without broken img tag', async ({ page }) => {
    // ---- Resolve a real student edit URL (3-tier approach) ----
    let studentEditUrl: string | null = null;

    await page.goto('/students');
    await page.waitForLoadState('networkidle');
    await expect(page.locator('table tbody')).toBeVisible({ timeout: 10000 });

    const rowCount = await page.locator('table tbody tr').count();
    if (rowCount === 0) {
      test.skip();
      return;
    }

    const firstRow = page.locator('table tbody tr').first();

    // Tier 1: look for an explicit /edit href in the row
    const editLink = firstRow.locator('a[href*="/edit"]').first();
    if (await editLink.count() > 0) {
      studentEditUrl = await editLink.getAttribute('href');
    } else {
      // Tier 2: extract student ID from a profile href and build /edit URL
      const profileLink = firstRow.locator('a[href*="/students/"]').first();
      if (await profileLink.count() > 0) {
        const href = await profileLink.getAttribute('href');
        const match = href?.match(/\/students\/(\d+)/);
        if (match) studentEditUrl = `/students/${match[1]}/edit`;
      }
    }

    // Tier 3: click the row, derive the ID from the resulting URL
    if (!studentEditUrl) {
      await firstRow.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);
      const url = page.url();
      const match = url.match(/\/students\/(\d+)/);
      if (match) studentEditUrl = `/students/${match[1]}/edit`;
    }

    if (!studentEditUrl) {
      test.skip();
      return;
    }

    // ---- Navigate to the edit page and assert avatar health ----
    await page.goto(studentEditUrl);
    await page.waitForLoadState('networkidle');

    // Spinner must clear (OnPush CD fix is in the same commit — belt-and-suspenders)
    await expect(
      page.locator('.loading.loading-spinner.loading-lg'),
    ).not.toBeVisible({ timeout: 5000 });

    // Form must be visible
    await expect(page.locator('form').first()).toBeVisible({ timeout: 3000 });

    // Avatar component itself must be present
    await expect(page.locator('app-student-avatar').first()).toBeVisible();

    // No img with empty src — broken OLE path would produce this before the fix
    expect(await page.locator('app-student-avatar img[src=""]').count()).toBe(0);
  });

  test('student create form shows initials avatar (no broken img) when name is typed', async ({ page }) => {
    await page.goto('/students/create');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    // Form must be present on the create page
    await expect(page.locator('form').first()).toBeVisible({ timeout: 3000 });

    // Type a name — Angular CD should update initials in app-student-avatar
    await page.locator('input[placeholder="First Name"]').fill('Jane');
    await page.locator('input[placeholder="Last Name"]').fill('Doe');

    // Give Angular 300 ms for the computed signal to propagate
    await page.waitForTimeout(300);

    // Avatar must render (showing initials "JD" since no photo is provided)
    await expect(page.locator('app-student-avatar').first()).toBeVisible();

    // No img with empty src — on create there is no photo so the @if branch
    // should never render an <img> at all, let alone one with src=""
    expect(await page.locator('app-student-avatar img[src=""]').count()).toBe(0);
  });

  test('no StudentAvatar console warnings when viewing student list', async ({ page }) => {
    // Capture all console messages BEFORE navigating so none are missed
    const avatarWarnings: string[] = [];
    page.on('console', msg => {
      const text = msg.text();
      if (text.includes('[StudentAvatar]') || text.includes('Failed to load image')) {
        avatarWarnings.push(text);
      }
    });

    await page.goto('/students');
    await page.waitForLoadState('networkidle');
    // Extra wait to allow lazy-loaded avatars to attempt rendering
    await page.waitForTimeout(2000);

    // Any message matching the StudentAvatar warning prefix means resolvedPhotoSrc()
    // returned a value that the browser then rejected — i.e. the OLE fix did not fire.
    expect(avatarWarnings).toHaveLength(0);
  });
});
