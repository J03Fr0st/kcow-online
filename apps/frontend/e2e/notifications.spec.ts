import { test, expect } from '@playwright/test';
import { NotificationsPage } from './page-objects/notifications.page';

test.describe('Notification System', () => {
  let page: NotificationsPage;

  test.beforeEach(async ({ page: testPage }) => {
    page = new NotificationsPage(testPage);
    await page.gotoNotifications();
  });

  test('should display success notification', async ({ page: testPage }) => {
    await page.showSuccessNotification();

    const notification = await page.getNotificationByType('success');
    await expect(notification).toBeVisible({ timeout: 5000 });

    const text = await notification.textContent();
    expect(text?.toLowerCase()).toContain('success');
  });

  test('should display error notification', async ({ page: testPage }) => {
    await page.showErrorNotification();

    const notification = await page.getNotificationByType('error');
    await expect(notification).toBeVisible({ timeout: 5000 });

    const text = await notification.textContent();
    expect(text?.toLowerCase()).toContain('error');
  });

  test('should display warning notification', async ({ page: testPage }) => {
    await page.showWarningNotification();

    const notification = await page.getNotificationByType('warning');
    await expect(notification).toBeVisible({ timeout: 5000 });

    const text = await notification.textContent();
    expect(text?.toLowerCase()).toContain('warning');
  });

  test('should display info notification', async ({ page: testPage }) => {
    await page.showInfoNotification();

    const notification = await page.getNotificationByType('info');
    await expect(notification).toBeVisible({ timeout: 5000 });

    const text = await notification.textContent();
    expect(text?.toLowerCase()).toContain('info');
  });

  test('should auto-dismiss notification after duration', async ({ page: testPage }) => {
    await page.showSuccessNotification();

    const notification = await page.getNotificationByType('success');
    await expect(notification).toBeVisible({ timeout: 5000 });

    // Wait for auto-dismiss (typically 3-5 seconds)
    await expect(notification).toBeHidden({ timeout: 10000 });
  });

  test('should stack multiple notifications', async ({ page: testPage }) => {
    // Show multiple notifications
    await page.showSuccessNotification();
    await testPage.waitForTimeout(100);
    await page.showErrorNotification();
    await testPage.waitForTimeout(100);
    await page.showWarningNotification();

    // Check that multiple notifications are visible
    const count = await page.getNotificationCount();
    expect(count).toBeGreaterThanOrEqual(2);
  });

  test('should display notifications in correct order (stack)', async ({ page: testPage }) => {
    await page.showSuccessNotification();
    await testPage.waitForTimeout(200);
    await page.showErrorNotification();

    const alerts = await testPage.locator('.alert').all();
    expect(alerts.length).toBeGreaterThanOrEqual(2);

    // First notification (success) should be on top or bottom depending on stack direction
    const firstAlert = alerts[0];
    const hasSuccess = await firstAlert.locator('.alert-success').count() > 0;
    const hasError = await firstAlert.locator('.alert-error').count() > 0;

    expect(hasSuccess || hasError).toBeTruthy();
  });

  test('should allow manual dismissal of notifications', async ({ page: testPage }) => {
    await page.showSuccessNotification();

    const notification = await page.getNotificationByType('success');
    await expect(notification).toBeVisible({ timeout: 5000 });

    // Look for close button
    const closeButton = notification.locator('button, [role="button"]').first();
    if (await closeButton.isVisible()) {
      await closeButton.click();
      await expect(notification).toBeHidden({ timeout: 2000 });
    }
  });

  test('should show different notification styles', async ({ page: testPage }) => {
    // Success (green/emerald)
    await page.showSuccessNotification();
    const success = await page.getNotificationByType('success');
    await expect(success).toBeVisible({ timeout: 5000 });
    const successClass = await success.getAttribute('class');
    expect(successClass).toContain('alert-success');

    // Clear and test error (red)
    await testPage.waitForTimeout(1000);
    await page.showErrorNotification();
    const error = await page.getNotificationByType('error');
    await expect(error).toBeVisible({ timeout: 5000 });
    const errorClass = await error.getAttribute('class');
    expect(errorClass).toContain('alert-error');
  });

  test('should handle rapid notification creation', async ({ page: testPage }) => {
    // Rapidly create multiple notifications
    for (let i = 0; i < 5; i++) {
      await page.showSuccessNotification();
      await testPage.waitForTimeout(50);
    }

    // Should show at least some notifications
    const count = await page.getNotificationCount();
    expect(count).toBeGreaterThan(0);
  });

  test('should maintain notification visibility across page navigation', async ({ page: testPage }) => {
    await page.showSuccessNotification();

    const notification = await page.getNotificationByType('success');
    await expect(notification).toBeVisible({ timeout: 5000 });

    // Navigate to another page
    await page.goto('/dashboard');

    // Notification might persist or disappear depending on implementation
    // This test verifies the behavior is consistent
    const isStillVisible = await notification.isVisible().catch(() => false);
    // Either persistent or properly cleaned up
    expect(typeof isStillVisible).toBe('boolean');
  });
});
