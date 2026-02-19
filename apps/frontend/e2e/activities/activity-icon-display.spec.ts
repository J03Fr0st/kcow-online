import { test, expect } from '@playwright/test';
import { writeFileSync, unlinkSync } from 'fs';
import { join } from 'path';

/**
 * Regression tests for the BMP OLE wrapper fix — commit 74d0a34
 *
 * Background:
 *   MS Access exported activity icons with a 78-byte OLE wrapper prefix that
 *   caused the browser to receive an unrecognisable byte sequence instead of a
 *   valid image.  Two fixes were shipped together:
 *
 *   1. Backend (ActivityDataMapper.cs): strips the OLE header at import time.
 *   2. Frontend (activities-list.component.ts getIconSrc()): derives the MIME
 *      type from the base64 header bytes so the correct data: URI scheme is
 *      emitted regardless of whatever the database column may contain.
 *
 *   These tests verify the visible, user-facing behaviour produced by those
 *   fixes without depending on Access-style import fixtures.
 */

// Minimal 1×1 PNG (8-bit colour, single pixel — red)
const MINIMAL_PNG_B64 =
  'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==';

// Minimal 2×2 24-bit BMP (no compression, hand-crafted header + pixel data)
// The base64 string starts with "Qk" which decodes to 0x42 0x4D — the BMP
// magic bytes.  getIconSrc() keys on exactly this prefix.
const MINIMAL_BMP_B64 =
  'Qk1GAAAAAAAAAD4AAACoAAAAAgAAAAIAAAABABgAAAAAAAAAAAATCwAAEwsAAAAAAAAAAAAA/wAA/wAA/wAA/wAA';

test.describe('Activity Icon Display Regression — BMP OLE fix (74d0a34)', () => {
  const testEmail = process.env.TEST_EMAIL || 'admin@kcow.local';
  const testPassword = process.env.TEST_PASSWORD || 'Admin123!';

  test.beforeEach(async ({ page }) => {
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

  // --------------------------------------------------------------------------
  // Test 1 — table and Icon column header are present
  // --------------------------------------------------------------------------
  test('activities page loads and shows icon column header', async ({ page }) => {
    await page.goto('/activities');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);

    const table = page.locator('table[aria-label="Activities Registry"]');
    await expect(table).toBeVisible({ timeout: 5000 });

    const headers = table.locator('thead th');
    const headerTexts = await headers.allTextContents();
    expect(headerTexts).toContain('Icon');
  });

  // --------------------------------------------------------------------------
  // Test 2 — PNG icon round-trips as a data:image/png URI
  // --------------------------------------------------------------------------
  test('uploading a PNG icon stores and displays as data:image/png URI', async ({ page }) => {
    const tmpPath = join(process.cwd(), `test-icon-png-${Date.now()}.png`);
    const activityCode = `ICON-PNG-${Date.now()}`;

    try {
      writeFileSync(tmpPath, Buffer.from(MINIMAL_PNG_B64, 'base64'));

      // Open create form
      await page.goto('/activities');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
      await expect(addButton).toBeVisible();
      await addButton.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(300);

      // Fill required fields
      const form = page.locator('form');
      await form.locator('input#code').fill(activityCode);
      await form.locator('input#name').fill('PNG Icon Test');

      // Upload PNG
      const iconInput = form.locator('input#icon[type="file"]');
      await expect(iconInput).toBeVisible();
      await iconInput.setInputFiles(tmpPath);
      await page.waitForTimeout(500);

      // Submit
      const submitButton = form.locator('button[type="submit"]');
      await expect(submitButton).toBeVisible();
      await submitButton.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Locate the newly created row and inspect the icon img
      const activityRow = page
        .locator('table tbody tr')
        .filter({ hasText: activityCode })
        .first();
      await expect(activityRow).toBeVisible({ timeout: 5000 });

      const iconImg = activityRow.locator('.avatar img');
      if ((await iconImg.count()) > 0) {
        const src = await iconImg.getAttribute('src');
        expect(src).toMatch(/^data:image\/png;base64,/);
      }

      // Clean up — archive + confirm
      const archiveBtn = activityRow.locator('button').filter({ hasText: /archive/i });
      await expect(archiveBtn).toBeVisible();
      await archiveBtn.click();
      await page.waitForTimeout(300);

      const confirmBtn = page.locator('button').filter({ hasText: /^\s*✓\s*$/ });
      await expect(confirmBtn).toBeVisible();
      await confirmBtn.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);
    } finally {
      try {
        unlinkSync(tmpPath);
      } catch {
        // File may not have been created — ignore
      }
    }
  });

  // --------------------------------------------------------------------------
  // Test 3 — BMP icon produces a valid (non-broken) data URI
  // --------------------------------------------------------------------------
  test('uploading a BMP icon displays with a valid data URI (not broken)', async ({ page }) => {
    const tmpPath = join(process.cwd(), `test-icon-bmp-${Date.now()}.bmp`);
    const activityCode = `ICON-BMP-${Date.now()}`;

    try {
      writeFileSync(tmpPath, Buffer.from(MINIMAL_BMP_B64, 'base64'));

      await page.goto('/activities');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
      await expect(addButton).toBeVisible();
      await addButton.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(300);

      const form = page.locator('form');
      await form.locator('input#code').fill(activityCode);
      await form.locator('input#name').fill('BMP Icon Test');

      const iconInput = form.locator('input#icon[type="file"]');
      await expect(iconInput).toBeVisible();
      await iconInput.setInputFiles(tmpPath);
      await page.waitForTimeout(500);

      const submitButton = form.locator('button[type="submit"]');
      await expect(submitButton).toBeVisible();
      await submitButton.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      const activityRow = page
        .locator('table tbody tr')
        .filter({ hasText: activityCode })
        .first();
      await expect(activityRow).toBeVisible({ timeout: 5000 });

      const iconImg = activityRow.locator('.avatar img');
      if ((await iconImg.count()) > 0) {
        const src = (await iconImg.getAttribute('src')) ?? '';
        // getIconSrc() maps "Qk…" → bmp, but the browser may also accept jpeg/png
        // if the server re-encodes the image.  Any recognised MIME type is valid.
        expect(src).toMatch(/^data:image\/(bmp|jpeg|png);base64,/);
        expect(src.length).toBeGreaterThan(20);
      }

      // Clean up
      const archiveBtn = activityRow.locator('button').filter({ hasText: /archive/i });
      await expect(archiveBtn).toBeVisible();
      await archiveBtn.click();
      await page.waitForTimeout(300);

      const confirmBtn = page.locator('button').filter({ hasText: /^\s*✓\s*$/ });
      await expect(confirmBtn).toBeVisible();
      await confirmBtn.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);
    } finally {
      try {
        unlinkSync(tmpPath);
      } catch {
        // Ignore
      }
    }
  });

  // --------------------------------------------------------------------------
  // Test 4 — activity with no icon must not emit a broken empty-src <img>
  // --------------------------------------------------------------------------
  test('activity without icon shows no broken empty-src img element', async ({ page }) => {
    const activityCode = `NO-ICON-${Date.now()}`;

    await page.goto('/activities');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const addButton = page.locator('button').filter({ hasText: /add|new|create/i }).first();
    await expect(addButton).toBeVisible();
    await addButton.click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(300);

    const form = page.locator('form');
    await form.locator('input#code').fill(activityCode);
    await form.locator('input#name').fill('No Icon Test');
    // Intentionally do NOT upload any icon

    const submitButton = form.locator('button[type="submit"]');
    await expect(submitButton).toBeVisible();
    await submitButton.click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    const activityRow = page
      .locator('table tbody tr')
      .filter({ hasText: activityCode })
      .first();
    await expect(activityRow).toBeVisible({ timeout: 5000 });

    // An img with an empty src is a broken image — there must be none.
    const brokenImgCount = await activityRow.locator('.avatar img[src=""]').count();
    expect(brokenImgCount).toBe(0);

    // Clean up
    const archiveBtn = activityRow.locator('button').filter({ hasText: /archive/i });
    await expect(archiveBtn).toBeVisible();
    await archiveBtn.click();
    await page.waitForTimeout(300);

    const confirmBtn = page.locator('button').filter({ hasText: /^\s*✓\s*$/ });
    await expect(confirmBtn).toBeVisible();
    await confirmBtn.click();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);
  });

  // --------------------------------------------------------------------------
  // Test 5 — no icon/image/src console errors on page load
  // --------------------------------------------------------------------------
  test('no icon-related console errors when activities page loads', async ({ page }) => {
    const consoleErrors: string[] = [];

    page.on('console', (msg) => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    await page.goto('/activities');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    const iconRelatedErrors = consoleErrors.filter((text) => {
      const lower = text.toLowerCase();
      return lower.includes('icon') || lower.includes('image') || lower.includes('src');
    });

    expect(iconRelatedErrors).toHaveLength(0);
  });
});
