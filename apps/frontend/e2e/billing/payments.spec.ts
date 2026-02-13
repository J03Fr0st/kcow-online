import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Record Payment functionality
 * Covers AC #2: Payment recording and validation
 */
test.describe('Record Payment E2E Tests', () => {
  let studentId: string;
  let schoolId: string;
  let classGroupId: string;
  let invoiceId: string;

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
        name: `E2E Payment School ${timestamp}`,
        shortName: `E2EP${timestamp}`,
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
        name: `E2E-Payment-CG-${timestamp}`,
        description: 'E2E Payment Test Class Group',
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
        reference: `E2EPAY${timestamp}`,
        firstName: 'Payment',
        lastName: `TestStudent${timestamp}`,
        dateOfBirth: '2010-01-01',
        classGroupId: classGroup.id,
        schoolId: school.id,
        isActive: true
      }
    });
    const student = await studentResponse.json();
    studentId = student.id.toString();

    // Create a pending invoice for payment application tests
    const invoiceResponse = await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-01',
        amount: 500.00,
        dueDate: '2024-04-01',
        description: 'Test invoice for payment'
      }
    });
    const invoice = await invoiceResponse.json();
    invoiceId = invoice.id.toString();
  });

  test.afterEach(async ({ page }) => {
    // Clean up test data
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

  test('User can click "Record Payment" to open inline form', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Click Record Payment button
    await page.click('button:has-text("Record Payment")');

    // Verify form appears
    await expect(page.locator('h4:has-text("Record Payment")')).toBeVisible();
  });

  test('Form displays amount input, payment method dropdown, optional invoice, notes', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');
    await page.click('button:has-text("Record Payment")');

    // Verify form fields exist
    await expect(page.locator('label:has-text("Amount")')).toBeVisible();
    await expect(page.locator('label:has-text("Payment Method")')).toBeVisible();
    await expect(page.locator('label:has-text("Apply to Invoice")')).toBeVisible();
    await expect(page.locator('label:has-text("Notes")')).toBeVisible();

    // Verify input elements
    await expect(page.locator('input[type="number"]')).toBeVisible();
    await expect(page.locator('select')).toBeVisible();
  });

  test('User can submit valid payment', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');
    await page.click('button:has-text("Record Payment")');

    // Fill payment form
    await page.fill('input[type="number"]', '250.00');
    await page.selectOption('select', '0'); // Cash

    // Submit
    await page.click('button:has-text("Save Payment")');

    // Verify success - form closes and payment appears
    await expect(page.locator('h4:has-text("Record Payment")')).not.toBeVisible();
  });

  test('Payment appears in payments list', async ({ page }) => {
    // Create payment via API
    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-15',
        amount: 300.00,
        paymentMethod: 0,
        notes: 'E2E test payment'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Verify payment in list
    await expect(page.locator('text=300.00')).toBeVisible();
    await expect(page.locator('text=Cash')).toBeVisible();
  });

  test('Balance is updated correctly after payment', async ({ page }) => {
    // Check initial balance (500 from invoice)
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Initial balance should be 500
    await expect(page.locator('.stat-value')).toContainText('R 500');

    // Record a payment via API
    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-20',
        amount: 200.00,
        paymentMethod: 0,
        notes: 'Partial payment'
      }
    });

    // Refresh page
    await page.reload();
    await page.click('button:has-text("Financial")');

    // New balance should be 300
    await expect(page.locator('.stat-value')).toContainText('R 300');
  });

  test('Receipt number is generated automatically', async ({ page }) => {
    // Create payment via API
    const response = await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-15',
        amount: 150.00,
        paymentMethod: 0,
        notes: 'Test receipt generation'
      }
    });
    const payment = await response.json();

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Verify receipt number exists (format: RCP-YYYY-NNNN or similar)
    await expect(page.locator('td.font-mono')).toBeVisible();
    expect(payment.receiptNumber).toBeDefined();
    expect(payment.receiptNumber.length).toBeGreaterThan(0);
  });

  test('If applied to invoice, invoice status updates', async ({ page }) => {
    // Create payment applied to invoice (full payment)
    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        invoiceId: parseInt(invoiceId),
        paymentDate: '2024-03-15',
        amount: 500.00, // Full payment
        paymentMethod: 0,
        notes: 'Full payment for invoice'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Check invoice status changed to Paid
    await expect(page.locator('.badge:has-text("Paid")')).toBeVisible();
  });

  test('Partial payment reduces invoice balance', async ({ page }) => {
    // Create partial payment
    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        invoiceId: parseInt(invoiceId),
        paymentDate: '2024-03-15',
        amount: 200.00, // Partial payment
        paymentMethod: 1,
        notes: 'Partial payment'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Balance should be 300 (500 - 200)
    await expect(page.locator('.stat-value')).toContainText('R 300');
  });

  test('Payment method options are available', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');
    await page.click('button:has-text("Record Payment")');

    // Check payment method dropdown has expected options
    const select = page.locator('select').first();
    await expect(select.locator('option[value="0"]')).toContainText('Cash');
    await expect(select.locator('option[value="1"]')).toContainText('Card');
    await expect(select.locator('option[value="2"]')).toContainText('Transfer');
  });

  test('Invoice dropdown shows pending invoices', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');
    await page.click('button:has-text("Record Payment")');

    // Check invoice dropdown has pending invoice option
    const invoiceSelect = page.locator('select').nth(1);
    await expect(invoiceSelect).toBeVisible();

    // Should contain the test invoice
    await expect(invoiceSelect.locator('option')).toContainText(/500/);
  });
});
