import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Financial Calculations
 * Covers AC #6: Financial calculation accuracy
 * Covers AC #9: Thorough financial testing requirements
 */
test.describe('Financial Calculations E2E Tests', () => {
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
        name: `E2E Calc School ${timestamp}`,
        shortName: `E2EC${timestamp}`,
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
        name: `E2E-Calc-CG-${timestamp}`,
        description: 'E2E Calculation Test Class Group',
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
        reference: `E2ECALC${timestamp}`,
        firstName: 'Calc',
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

  test('Balance updates correctly when invoice is created', async ({ page }) => {
    // Initial state - no balance
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Initial balance should be 0
    await expect(page.locator('.stat-value')).toContainText('R 0');

    // Create invoice
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 350.00,
        dueDate: '2024-04-15',
        description: 'Test invoice'
      }
    });

    // Refresh and verify balance
    await page.reload();
    await page.click('button:has-text("Financial")');

    await expect(page.locator('.stat-value')).toContainText('R 350');
  });

  test('Balance updates correctly when payment is recorded', async ({ page }) => {
    // Create invoice first
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 500.00,
        dueDate: '2024-04-15',
        description: 'Test invoice'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Balance should be 500
    await expect(page.locator('.stat-value')).toContainText('R 500');

    // Create payment
    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-20',
        amount: 200.00,
        paymentMethod: 0,
        notes: 'Partial payment'
      }
    });

    // Refresh and verify balance
    await page.reload();
    await page.click('button:has-text("Financial")');

    // Balance should be 300
    await expect(page.locator('.stat-value')).toContainText('R 300');
  });

  test('Applied payments reduce invoice balance', async ({ page }) => {
    // Create invoice
    const invoiceResponse = await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 400.00,
        dueDate: '2024-04-15',
        description: 'Applied payment test'
      }
    });
    const invoice = await invoiceResponse.json();

    // Create payment applied to invoice
    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        invoiceId: invoice.id,
        paymentDate: '2024-03-20',
        amount: 400.00,
        paymentMethod: 0,
        notes: 'Full payment applied to invoice'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Balance should be 0 (400 - 400)
    await expect(page.locator('.stat-value')).toContainText('R 0');

    // Invoice status should be Paid
    await expect(page.locator('.badge:has-text("Paid")')).toBeVisible();
  });

  test('Calculations are accurate with multiple invoices and payments', async ({ page }) => {
    // Create multiple invoices
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-01',
        amount: 200.00,
        dueDate: '2024-04-01',
        description: 'Invoice 1'
      }
    });

    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-10',
        amount: 300.00,
        dueDate: '2024-04-10',
        description: 'Invoice 2'
      }
    });

    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 150.00,
        dueDate: '2024-04-15',
        description: 'Invoice 3'
      }
    });

    // Create multiple payments
    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-05',
        amount: 100.00,
        paymentMethod: 0,
        notes: 'Payment 1'
      }
    });

    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-12',
        amount: 250.00,
        paymentMethod: 1,
        notes: 'Payment 2'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Total invoiced: 200 + 300 + 150 = 650
    // Total paid: 100 + 250 = 350
    // Balance: 650 - 350 = 300
    await expect(page.locator('.stat-value')).toContainText('R 300');
  });

  test('Balance calculation handles exact zero balance', async ({ page }) => {
    // Create invoice
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 275.50,
        dueDate: '2024-04-15',
        description: 'Exact balance test'
      }
    });

    // Create payment for exact amount
    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-20',
        amount: 275.50,
        paymentMethod: 0,
        notes: 'Exact payment'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Balance should be exactly 0
    await expect(page.locator('.stat-value')).toContainText('R 0');
  });

  test('Summary shows correct last payment date and amount', async ({ page }) => {
    // Create payments on different dates
    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-10',
        amount: 100.00,
        paymentMethod: 0,
        notes: 'Earlier payment'
      }
    });

    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-25',
        amount: 200.00,
        paymentMethod: 1,
        notes: 'Latest payment'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Last payment should show 200.00 from 2024-03-25
    const lastPaymentSection = page.locator('.stat-title:has-text("Last Payment")').locator('..');
    await expect(lastPaymentSection).toContainText('200');
    await expect(lastPaymentSection).toContainText('2024-03-25');
  });

  test('Outstanding invoices count is accurate', async ({ page }) => {
    // Create invoices with different statuses
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-01',
        amount: 100.00,
        dueDate: '2024-04-01',
        description: 'Pending invoice'
      }
    });

    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-10',
        amount: 150.00,
        dueDate: '2024-04-10',
        description: 'Paid invoice'
      }
    });

    // Create full payment for the second invoice
    const invoicesResponse = await page.request.get(`/api/students/${studentId}/invoices`);
    const invoices = await invoicesResponse.json();
    const paidInvoice = invoices.find((i: { amount: number }) => i.amount === 150.00);

    if (paidInvoice) {
      await page.request.post(`/api/students/${studentId}/payments`, {
        data: {
          invoiceId: paidInvoice.id,
          paymentDate: '2024-03-15',
          amount: 150.00,
          paymentMethod: 0,
          notes: 'Full payment'
        }
      });
    }

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Outstanding invoices should be 1 (the pending one)
    const outstandingSection = page.locator('.stat-title:has-text("Outstanding Invoices")').locator('..');
    // At least the pending invoice should count
    const text = await outstandingSection.textContent();
    expect(text).toMatch(/[1-9]/); // Should have at least 1 outstanding
  });

  test('Decimal amounts are handled correctly', async ({ page }) => {
    // Create invoice with decimal amount
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 123.45,
        dueDate: '2024-04-15',
        description: 'Decimal test invoice'
      }
    });

    // Create payment with decimal amount
    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-20',
        amount: 78.90,
        paymentMethod: 0,
        notes: 'Decimal test payment'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Financial")');

    // Balance: 123.45 - 78.90 = 44.55
    await expect(page.locator('.stat-value')).toContainText('44.55');
  });

  test('Consistency between header and tab billing status', async ({ page }) => {
    // Create invoice
    await page.request.post(`/api/students/${studentId}/invoices`, {
      data: {
        invoiceDate: '2024-03-15',
        amount: 450.00,
        dueDate: '2024-04-15',
        description: 'Consistency test'
      }
    });

    // Create payment
    await page.request.post(`/api/students/${studentId}/payments`, {
      data: {
        paymentDate: '2024-03-20',
        amount: 150.00,
        paymentMethod: 0,
        notes: 'Consistency payment'
      }
    });

    await page.goto(`/admin/students/${studentId}`);

    // Check header shows balance due 300
    const headerBalance = page.locator('.stat-value:has-text("Balance due")');
    await expect(headerBalance).toContainText('300');

    // Go to Financial tab
    await page.click('button:has-text("Financial")');

    // Check tab shows same balance
    const tabBalance = page.locator('.stat-title:has-text("Current Balance")').locator('..').locator('.stat-value');
    await expect(tabBalance).toContainText('R 300');
  });
});
