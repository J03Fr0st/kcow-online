import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Financial Tab in Student Profile
 * Covers AC #1: Financial Tab display and functionality
 */
test.describe('Financial Tab E2E Tests', () => {
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
        name: `E2E Billing School ${timestamp}`,
        shortName: `E2EB${timestamp}`,
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
        name: `E2E-Billing-CG-${timestamp}`,
        description: 'E2E Billing Test Class Group',
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
        reference: `E2EBILL${timestamp}`,
        firstName: 'Billing',
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

  test('User can navigate to Financial tab', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await expect(page.locator('h1')).toContainText('Student Profile');

    // Click Financial tab
    await page.click('button:has-text("Financial")');
    await expect(page.locator('.financial-tab')).toBeVisible();
  });

  test('Billing summary shows current balance, last payment, outstanding invoices', async ({ page }) => {
    // Create an invoice
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 500.00,
        dueDate: '2024-04-15',
        description: 'Test invoice for summary'
      }
    });

    // Create a payment
    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-20',
        amount: 200.00,
        paymentMethod: 0,
        notes: 'Test payment for summary'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Verify summary section
    await expect(page.locator('h3:has-text("Financial Summary")')).toBeVisible();

    // Check current balance (500 - 200 = 300)
    await expect(page.locator('.stat-title:has-text("Current Balance")')).toBeVisible();
    await expect(page.locator('.stat-value')).toContainText('R 300');

    // Check last payment
    await expect(page.locator('.stat-title:has-text("Last Payment")')).toBeVisible();
    await expect(page.locator('.stat-value')).toContainText('R 200');

    // Check outstanding invoices
    await expect(page.locator('.stat-title:has-text("Outstanding Invoices")')).toBeVisible();
  });

  test('Invoices list displays with Date, Amount, Status', async ({ page }) => {
    // Create test invoices
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-01',
        amount: 350.00,
        dueDate: '2024-03-15',
        description: 'First test invoice'
      }
    });

    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-10',
        amount: 450.00,
        dueDate: '2024-03-25',
        description: 'Second test invoice'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Verify invoices table exists
    await expect(page.locator('h3:has-text("Invoices")')).toBeVisible();

    // Check table headers
    await expect(page.locator('th:has-text("Date")')).toBeVisible();
    await expect(page.locator('th:has-text("Amount")')).toBeVisible();
    await expect(page.locator('th:has-text("Status")')).toBeVisible();

    // Check invoice data
    await expect(page.locator('text=350.00')).toBeVisible();
    await expect(page.locator('text=450.00')).toBeVisible();
  });

  test('Payments list displays with Date, Amount, Receipt #', async ({ page }) => {
    // Create test payments
    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-05',
        amount: 150.00,
        paymentMethod: 0,
        notes: 'First test payment'
      }
    });

    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-12',
        amount: 250.00,
        paymentMethod: 1,
        notes: 'Second test payment'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Verify payments table exists
    await expect(page.locator('h3:has-text("Payments")')).toBeVisible();

    // Check table headers
    await expect(page.locator('th:has-text("Date")')).toBeVisible();
    await expect(page.locator('th:has-text("Amount")')).toBeVisible();
    await expect(page.locator('th:has-text("Receipt")')).toBeVisible();

    // Check payment data
    await expect(page.locator('text=150.00')).toBeVisible();
    await expect(page.locator('text=250.00')).toBeVisible();
  });

  test('Summary and lists update correctly after changes', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Initial state - no invoices
    await expect(page.locator('text=No invoices found')).toBeVisible();

    // Create an invoice via API
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 400.00,
        dueDate: '2024-04-15',
        description: 'Test invoice'
      }
    });

    // Refresh to see updated data
    await page.reload();
    await page.click('button:has-text("Financial")');

    // Verify invoice appears in list
    await expect(page.locator('text=400')).toBeVisible();
  });
});
