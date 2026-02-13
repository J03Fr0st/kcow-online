import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Billing Audit Trail (FR14 - Critical)
 * Covers AC #5: Audit logging for billing operations
 * Covers AC #9: Financial audit trail requirements
 *
 * Note: This test suite focuses on creation audit logs since the backend
 * does not have PATCH/DELETE endpoints for invoices/payments.
 */
test.describe('Billing Audit Trail E2E Tests (FR14 - Critical)', () => {
  let studentId: string;
  let schoolId: string;
  let classGroupId: string;
  let invoiceId: string;
  let paymentId: string;

  test.beforeEach(async ({ page }) => {
    // Login as admin
    await page.goto('/login');
    await page.fill('input[name="email"]', 'admin@kcow.local');
    await page.fill('input[name="password"]', 'Admin123!');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL('/admin');

    const timestamp = Date.now();

    // Create school first
    const schoolResponse = await page.request.post('/api/schools', {
      data: {
        name: `E2E Audit Billing School ${timestamp}`,
        shortName: `E2EAB${timestamp}`,
        printInvoice: false,
        importFlag: false,
        isActive: true
      }
    });
    const school = await schoolResponse.json();
    schoolId = school.id.toString();

    // Create class group
    const classGroupResponse = await page.request.post('/api/class-groups', {
      data: {
        name: `E2E-Audit-Billing-CG-${timestamp}`,
        description: 'E2E Audit Billing Test Class Group',
        schoolId: school.id,
        dayOfWeek: 1,
        startTime: '08:00',
        endTime: '09:00',
        isActive: true
      }
    });
    const classGroup = await classGroupResponse.json();
    classGroupId = classGroup.id.toString();

    // Create student
    const studentResponse = await page.request.post('/api/students', {
      data: {
        reference: `E2EAUDIT${timestamp}`,
        firstName: 'Audit',
        lastName: `BillingTest${timestamp}`,
        dateOfBirth: '2010-01-01',
        classGroupId: classGroup.id,
        schoolId: school.id,
        isActive: true
      }
    });
    const student = await studentResponse.json();
    studentId = student.id.toString();

    // Create initial invoice
    const invoiceResponse = await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 500.00,
        dueDate: '2024-04-15',
        description: 'Initial audit test invoice'
      }
    });
    const invoice = await invoiceResponse.json();
    invoiceId = invoice.id.toString();

    // Create initial payment
    const paymentResponse = await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-20',
        amount: 200.00,
        paymentMethod: 0,
        notes: 'Initial audit test payment'
      }
    });
    const payment = await paymentResponse.json();
    paymentId = payment.id.toString();
  });

  test.afterEach(async ({ page }) => {
    // Clean up test data - only delete student which cascades
    if (studentId) {
      await page.request.delete(`/api/students/${studentId}`);
    }
    if (classGroupId) {
      await page.request.delete(`/api/class-groups/${classGroupId}`);
    }
    if (schoolId) {
      await page.request.delete(`/api/schools/${schoolId}`);
    }
  });

  test('Audit log entry is created on invoice creation', async ({ page }) => {
    // Check audit log was created
    const auditResponse = await page.request.get(`/api/audit-log?entityType=Invoice&entityId=${invoiceId}`);
    const auditLogs = await auditResponse.json();

    expect(auditLogs.length).toBeGreaterThan(0);

    const latestLog = auditLogs[0];
    expect(latestLog.changedAt).toBeDefined();
    expect(latestLog.field).toBeDefined();
    expect(latestLog.changedBy).toBeDefined();
  });

  test('Audit log entry is created on payment creation', async ({ page }) => {
    // Check audit log was created
    const auditResponse = await page.request.get(`/api/audit-log?entityType=Payment&entityId=${paymentId}`);
    const auditLogs = await auditResponse.json();

    expect(auditLogs.length).toBeGreaterThan(0);

    const latestLog = auditLogs[0];
    expect(latestLog.changedAt).toBeDefined();
    expect(latestLog.field).toBeDefined();
    expect(latestLog.changedBy).toBeDefined();
  });

  test('User can click "View History" on invoice record', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Wait for invoices table to load
    await expect(page.locator('h3:has-text("Invoices")')).toBeVisible();

    // Find and click View History button for invoice
    const historyButton = page.locator('button[title="View history"]').first();
    await expect(historyButton).toBeVisible();
    await historyButton.click();

    // Verify audit trail panel appears
    await expect(page.locator('[role="dialog"], .audit-trail-panel')).toBeVisible();
  });

  test('User can click "View History" on payment record', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Wait for payments table to load
    await expect(page.locator('h3:has-text("Payments")')).toBeVisible();

    // Find View History button for payment (second occurrence)
    const historyButtons = page.locator('button[title="View history"]');
    const count = await historyButtons.count();

    if (count > 1) {
      await historyButtons.nth(1).click();
    } else if (count === 1) {
      // If only one button, it might be for payments in a different layout
      await historyButtons.first().click();
    }

    // Verify audit trail panel appears
    await expect(page.locator('[role="dialog"], .audit-trail-panel')).toBeVisible();
  });

  test('Audit Trail Panel appears showing all changes', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Wait for invoices to load
    await expect(page.locator('h3:has-text("Invoices")')).toBeVisible();

    // Click View History
    await page.click('button[title="View history"]');

    // Verify panel shows audit entries
    await expect(page.locator('[role="dialog"], .audit-trail-panel')).toBeVisible();

    // Check for audit entries (look for timestamps)
    await expect(page.locator('text=/\\d{4}-\\d{2}-\\d{2}/')).toBeVisible();
  });

  test('Audit trail is read-only', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Wait for invoices to load
    await expect(page.locator('h3:has-text("Invoices")')).toBeVisible();

    // Click View History
    await page.click('button[title="View history"]');

    // Verify no edit controls in audit panel
    const auditPanel = page.locator('[role="dialog"], .audit-trail-panel');

    // Check no edit buttons exist
    await expect(auditPanel.locator('button:has-text("Edit"), button:has-text("Save")')).not.toBeVisible();
  });

  test('Audit log contains invoice creation details', async ({ page }) => {
    // Get audit log for invoice
    const auditResponse = await page.request.get(`/api/audit-log?entityType=Invoice&entityId=${invoiceId}`);
    const auditLogs = await auditResponse.json();

    expect(auditLogs.length).toBeGreaterThan(0);

    // Check that audit entry has the expected information
    const creationLog = auditLogs.find((log: { field: string }) =>
      log.field === 'Amount' || log.field === 'InvoiceDate' || log.field === 'Created'
    );

    expect(creationLog).toBeDefined();
  });

  test('FR14 compliance - Invoice creation creates audit entry', async ({ page }) => {
    // Verify audit log exists for invoice
    const auditResponse = await page.request.get(`/api/audit-log?entityType=Invoice&entityId=${invoiceId}`);
    const auditLogs = await auditResponse.json();

    expect(auditLogs.length).toBeGreaterThanOrEqual(1);
  });

  test('FR14 compliance - Payment creation creates audit entry', async ({ page }) => {
    // Verify audit log exists for payment
    const auditResponse = await page.request.get(`/api/audit-log?entityType=Payment&entityId=${paymentId}`);
    const auditLogs = await auditResponse.json();

    expect(auditLogs.length).toBeGreaterThanOrEqual(1);
  });

  test('Multiple invoices show separate audit histories', async ({ page }) => {
    // Create a second invoice
    const invoice2Response = await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-20',
        amount: 300.00,
        dueDate: '2024-04-20',
        description: 'Second invoice'
      }
    });
    const invoice2 = await invoice2Response.json();
    const invoice2Id = invoice2.id.toString();

    // Get audit logs for both invoices
    const audit1Response = await page.request.get(`/api/audit-log?entityType=Invoice&entityId=${invoiceId}`);
    const audit1 = await audit1Response.json();

    const audit2Response = await page.request.get(`/api/audit-log?entityType=Invoice&entityId=${invoice2Id}`);
    const audit2 = await audit2Response.json();

    // Both should have audit entries
    expect(audit1.length).toBeGreaterThanOrEqual(1);
    expect(audit2.length).toBeGreaterThanOrEqual(1);
  });

  test('Audit entry shows user who made the change', async ({ page }) => {
    // Get audit log
    const auditResponse = await page.request.get(`/api/audit-log?entityType=Invoice&entityId=${invoiceId}`);
    const auditLogs = await auditResponse.json();

    const log = auditLogs[0];
    expect(log.changedBy).toBeDefined();
    expect(log.changedBy.length).toBeGreaterThan(0);
  });
});
