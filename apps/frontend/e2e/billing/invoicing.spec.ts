import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Create Invoice functionality
 * Covers AC #3: Invoice creation and validation
 */
test.describe('Create Invoice E2E Tests', () => {
  let studentId: string;
  let schoolId: string;
  let classGroupId: string;

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
        name: `E2E Invoice School ${timestamp}`,
        shortName: `E2EI${timestamp}`,
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
        name: `E2E-Invoice-CG-${timestamp}`,
        description: 'E2E Invoice Test Class Group',
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
        reference: `E2EINV${timestamp}`,
        firstName: 'Invoice',
        lastName: `TestStudent${timestamp}`,
        dateOfBirth: '2010-01-01',
        classGroupId: classGroup.id,
        schoolId: school.id,
        isActive: true
      }
    });
    const student = await studentResponse.json();
    studentId = student.id.toString();
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

  test('User can click "Create Invoice" to open inline form', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Click Create Invoice button
    await page.click('button:has-text("Create Invoice")');

    // Verify form appears
    await expect(page.locator('h4:has-text("Create Invoice")')).toBeVisible();
  });

  test('Form displays amount input, due date picker, description/notes', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');
    await page.click('button:has-text("Create Invoice")');

    // Verify form fields exist
    await expect(page.locator('label:has-text("Amount")')).toBeVisible();
    await expect(page.locator('label:has-text("Due Date")')).toBeVisible();
    await expect(page.locator('label:has-text("Description")')).toBeVisible();
    await expect(page.locator('label:has-text("Notes")')).toBeVisible();

    // Verify input elements
    await expect(page.locator('input[type="number"]')).toBeVisible();
    await expect(page.locator('input[type="date"]')).toBeVisible();
  });

  test('User can submit valid invoice', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');
    await page.click('button:has-text("Create Invoice")');

    // Fill invoice form
    const amountInput = page.locator('input[type="number"]').first();
    await amountInput.fill('450.00');

    const dateInputs = page.locator('input[type="date"]');
    await dateInputs.first().fill('2024-04-15');

    // Submit
    await page.click('button:has-text("Create Invoice"):not(:has(button))');

    // Verify success - form closes
    await expect(page.locator('h4:has-text("Create Invoice")')).not.toBeVisible();
  });

  test('Invoice appears in invoices list', async ({ page }) => {
    // Create invoice via API
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 375.00,
        dueDate: '2024-04-15',
        description: 'E2E test invoice'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Verify invoice in list
    await expect(page.locator('text=375.00')).toBeVisible();
    await expect(page.locator('text=E2E test invoice')).toBeVisible();
  });

  test('Balance is updated after invoice creation', async ({ page }) => {
    // Initial balance should be 0
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Create invoice via API
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 550.00,
        dueDate: '2024-04-15',
        description: 'Test invoice'
      }
    });

    // Refresh to see updated data
    await page.reload();
    await page.click('button:has-text("Financial")');

    // New balance should be 550
    await expect(page.locator('.stat-value')).toContainText('R 550');
  });

  test('Initial status is set to "Pending"', async ({ page }) => {
    // Create invoice via API
    const response = await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 250.00,
        dueDate: '2024-04-15',
        description: 'Status test invoice'
      }
    });
    const invoice = await response.json();

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Check status badge
    await expect(page.locator('.badge:has-text("Pending")')).toBeVisible();

    // Verify API returned Pending status (0)
    expect(invoice.status).toBe(0);
  });

  test('Invoice due date is captured correctly', async ({ page }) => {
    const dueDate = '2024-05-20';

    // Create invoice with specific due date
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 300.00,
        dueDate: dueDate,
        description: 'Due date test'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Verify due date in table
    await expect(page.locator('text=2024-05-20')).toBeVisible();
  });

  test('Can cancel invoice creation', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');
    await page.click('button:has-text("Create Invoice")');

    // Verify form is visible
    await expect(page.locator('h4:has-text("Create Invoice")')).toBeVisible();

    // Click cancel
    await page.click('button:has-text("Cancel")');

    // Verify form is hidden
    await expect(page.locator('h4:has-text("Create Invoice")')).not.toBeVisible();
  });

  test('Amount validation - negative amounts not allowed', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');
    await page.click('button:has-text("Create Invoice")');

    // Try to enter negative amount
    const amountInput = page.locator('input[type="number"]').first();
    await amountInput.fill('-100.00');

    // Submit button should be disabled
    const submitButton = page.locator('button:has-text("Create Invoice"):not(:has(button))');
    await expect(submitButton).toBeDisabled();
  });

  test('Multiple invoices can be created', async ({ page }) => {
    // Create multiple invoices via API
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-01',
        amount: 100.00,
        dueDate: '2024-04-01',
        description: 'First invoice'
      }
    });

    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-10',
        amount: 200.00,
        dueDate: '2024-04-10',
        description: 'Second invoice'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Verify both invoices appear
    await expect(page.locator('text=100.00')).toBeVisible();
    await expect(page.locator('text=200.00')).toBeVisible();

    // Total balance should be 300
    await expect(page.locator('.stat-value')).toContainText('R 300');
  });
});
