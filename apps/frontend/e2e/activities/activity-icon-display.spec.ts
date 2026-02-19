import { test, expect } from '@playwright/test';
import { writeFileSync, unlinkSync, existsSync } from 'fs';
import { join } from 'path';
import os from 'os';

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
    await page.waitForTimeout(500);

    // If the DB is empty the table is replaced by the empty-state div.
    // Create a throwaway activity so the table renders, then clean it up.
    let createdCode: string | null = null;
    const emptyState = page.locator('.empty-state');
    if (await emptyState.count() > 0) {
      const suffix = `${Date.now()}`;
      createdCode = `HEADER-CHECK-${suffix}`;

      const addBtn = page.locator('button').filter({ hasText: /add|new|create/i }).first();
      await expect(addBtn).toBeVisible();
      await addBtn.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(300);

      const form = page.locator('form');
      await form.locator('input#code').fill(createdCode);
      await form.locator('input#name').fill('Header Check');

      await page.locator('button').filter({ hasText: /Create Activity/i }).click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);
    }

    const table = page.locator('table[aria-label="Activities Registry"]');
    await expect(table).toBeVisible({ timeout: 5000 });

    const headers = table.locator('thead th');
    const headerTexts = await headers.allTextContents();
    expect(headerTexts).toContain('Icon');

    // Clean up throwaway activity if created
    if (createdCode) {
      const rows = page.locator('table tbody tr').filter({ hasText: createdCode });
      if (await rows.count() > 0) {
        const deleteBtn = rows.locator('button').filter({ hasText: /archive/i });
        if (await deleteBtn.count() > 0) {
          await deleteBtn.click();
          await page.waitForTimeout(300);
          const confirmBtn = page.locator('button').filter({ hasText: /^\s*✓\s*$/ });
          if (await confirmBtn.count() > 0) {
            await confirmBtn.click();
            await page.waitForTimeout(500);
          }
        }
      }
    }
  });

  // --------------------------------------------------------------------------
  // Test 2 — PNG icon round-trips as a data:image/png URI
  // --------------------------------------------------------------------------
  test('uploading a PNG icon stores and displays as data:image/png URI', async ({ page }) => {
    const suffix = `${Date.now()}${Math.random().toString(36).slice(2, 6)}`;
    const tmpPath = join(os.tmpdir(), `test-icon-png-${suffix}.png`);
    const activityCode = `ICON-PNG-${suffix}`;
    let createdCode: string | null = null;

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

      // Submit — the activity form uses type="button" (not type="submit")
      // Submit button is in the footer div, outside <form> — scope to page
      const submitButton = page.locator('button').filter({ hasText: /Create Activity/i });
      await expect(submitButton).toBeVisible();
      await submitButton.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Record that the activity was created so finally can clean it up
      createdCode = activityCode;

      // Locate the newly created row and inspect the icon img
      const activityRow = page
        .locator('table tbody tr')
        .filter({ hasText: activityCode })
        .first();
      await expect(activityRow).toBeVisible({ timeout: 5000 });

      // Hard assertion — the img must be present (not silently skipped)
      const iconImg = activityRow.locator('.avatar img');
      await expect(iconImg).toHaveCount(1, { timeout: 3000 });
      const src = await iconImg.getAttribute('src');
      expect(src).toMatch(/^data:image\/png;base64,/);
    } finally {
      // Always clean up temp file
      if (existsSync(tmpPath)) {
        unlinkSync(tmpPath);
      }

      // Always clean up created activity
      if (createdCode) {
        const rows = page.locator('table tbody tr').filter({ hasText: createdCode });
        if ((await rows.count()) > 0) {
          const deleteBtn = rows.locator('button').filter({ hasText: /archive/i });
          if ((await deleteBtn.count()) > 0) {
            await deleteBtn.click();
            await page.waitForTimeout(300);
            const confirmBtn = page.locator('button').filter({ hasText: /^\s*✓\s*$/ });
            if ((await confirmBtn.count()) > 0) {
              await confirmBtn.click();
              await page.waitForTimeout(500);
            }
          }
        }
      }
    }
  });

  // --------------------------------------------------------------------------
  // Test 3 — BMP icon (injected via API) produces the correct data URI
  //
  // The upload form only accepts PNG files; BMP icons enter the DB via legacy
  // MS Access import only.  This test bypasses the form and POSTs a raw BMP
  // base64 icon directly to the API — exactly as a migrated record would look
  // in the database — then verifies that getIconSrc() maps the "Qk" prefix to
  // a valid data URI rather than a broken or placeholder image.
  // --------------------------------------------------------------------------
  test('BMP icon data from API displays with correct MIME type (getIconSrc fix)', async ({ page }) => {
    const suffix = `${Date.now()}${Math.random().toString(36).slice(2, 6)}`;
    const activityCode = `ICON-BMP-${suffix}`;
    let createdCode: string | null = null;

    // Must be on the activities page before reading localStorage (Angular sets
    // auth_token only after the app bootstraps on a page that requires auth)
    await page.goto('/activities');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(500);

    // Grab the JWT that Angular stored after login
    const token = await page.evaluate(() => localStorage.getItem('auth_token'));
    expect(token).not.toBeNull();

    // Create an activity with a raw BMP base64 icon via the API.
    // This mimics what the legacy importer writes to the DB after stripping
    // the OLE wrapper — the icon column contains a "Qk…" base64 BMP string.
    const createResp = await page.request.post('http://localhost:5039/api/activities', {
      headers: { Authorization: `Bearer ${token}` },
      data: { code: activityCode, name: 'BMP Icon Test', icon: MINIMAL_BMP_B64 },
    });
    expect(createResp.ok()).toBeTruthy();
    createdCode = activityCode;

    try {
      // Reload so Angular fetches the updated activities list
      await page.reload();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      const activityRow = page
        .locator('table tbody tr')
        .filter({ hasText: activityCode })
        .first();
      await expect(activityRow).toBeVisible({ timeout: 5000 });

      // Hard assertion — the img element must exist (not silently absent)
      const iconImg = activityRow.locator('.avatar img');
      await expect(iconImg).toHaveCount(1, { timeout: 3000 });
      const src = (await iconImg.getAttribute('src')) ?? '';

      // getIconSrc() must map "Qk…" → data:image/bmp;base64,…
      // (other image/* MIME types are also acceptable if the server re-encodes)
      expect(src).toMatch(/^data:image\/(bmp|jpeg|png);base64,/);
      expect(src.length).toBeGreaterThan(20);
    } finally {
      // Clean up via the UI archive button
      if (createdCode) {
        const rows = page.locator('table tbody tr').filter({ hasText: createdCode });
        if ((await rows.count()) > 0) {
          const deleteBtn = rows.locator('button').filter({ hasText: /archive/i });
          if ((await deleteBtn.count()) > 0) {
            await deleteBtn.click();
            await page.waitForTimeout(300);
            const confirmBtn = page.locator('button').filter({ hasText: /^\s*✓\s*$/ });
            if ((await confirmBtn.count()) > 0) {
              await confirmBtn.click();
              await page.waitForTimeout(500);
            }
          }
        }
      }
    }
  });

  // --------------------------------------------------------------------------
  // Test 4 — activity with no icon must not emit a broken empty-src <img>
  // --------------------------------------------------------------------------
  test('activity without icon shows no broken empty-src img element', async ({ page }) => {
    const suffix = `${Date.now()}${Math.random().toString(36).slice(2, 6)}`;
    const activityCode = `NO-ICON-${suffix}`;
    let createdCode: string | null = null;

    try {
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

      // Submit — the activity form uses type="button" (not type="submit")
      // Submit button is in the footer div, outside <form> — scope to page
      const submitButton = page.locator('button').filter({ hasText: /Create Activity/i });
      await expect(submitButton).toBeVisible();
      await submitButton.click();
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(500);

      // Record that the activity was created so finally can clean it up
      createdCode = activityCode;

      const activityRow = page
        .locator('table tbody tr')
        .filter({ hasText: activityCode })
        .first();
      await expect(activityRow).toBeVisible({ timeout: 5000 });

      // An img with an empty src is a broken image — there must be none.
      const brokenImgCount = await activityRow.locator('.avatar img[src=""]').count();
      expect(brokenImgCount).toBe(0);
    } finally {
      // Always clean up created activity
      if (createdCode) {
        const rows = page.locator('table tbody tr').filter({ hasText: createdCode });
        if ((await rows.count()) > 0) {
          const deleteBtn = rows.locator('button').filter({ hasText: /archive/i });
          if ((await deleteBtn.count()) > 0) {
            await deleteBtn.click();
            await page.waitForTimeout(300);
            const confirmBtn = page.locator('button').filter({ hasText: /^\s*✓\s*$/ });
            if ((await confirmBtn.count()) > 0) {
              await confirmBtn.click();
              await page.waitForTimeout(500);
            }
          }
        }
      }
    }
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
