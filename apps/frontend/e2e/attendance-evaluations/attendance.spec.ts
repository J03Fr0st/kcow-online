import { test, expect } from '@playwright/test';

test.describe('Attendance Tab E2E Tests', () => {
  let studentId: string;

  test.beforeEach(async ({ page }) => {
    // Login as admin
    await page.goto('/login');
    await page.fill('input[name="email"]', 'admin@kcow.local');
    await page.fill('input[name="password"]', 'Admin123!');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL('/admin');

    // Create a test student with class group
    const timestamp = Date.now();
    const studentRef = `E2E${timestamp}`;
    studentId = studentRef;

    // Create school first
    const schoolResponse = await page.request.post('/api/schools', {
      data: {
        name: `E2E Test School ${timestamp}`,
        shortName: `E2E${timestamp}`,
        printInvoice: false,
        importFlag: false,
        isActive: true
      }
    });
    const school = await schoolResponse.json();

    // Create class group
    const classGroupResponse = await page.request.post('/api/class-groups', {
      data: {
        name: `E2E-CG-${timestamp}`,
        description: 'E2E Test Class Group',
        schoolId: school.id,
        dayOfWeek: 1,
        startTime: '08:00',
        endTime: '09:00',
        isActive: true
      }
    });
    const classGroup = await classGroupResponse.json();

    // Create student
    const studentResponse = await page.request.post('/api/students', {
      data: {
        reference: studentRef,
        firstName: 'E2E',
        lastName: `TestStudent${timestamp}`,
        dateOfBirth: '2010-01-01',
        classGroupId: classGroup.id,
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
  });

  test('User can navigate to Attendance tab', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await expect(page.locator('h1')).toContainText('Student Profile');

    // Click Attendance tab
    await page.click('button:has-text("Attendance")');
    await expect(page.locator('h2')).toContainText('Attendance');
  });

  test('Attendance list displays with proper format', async ({ page }) => {
    // Create attendance record via API
    await page.request.post('/api/attendance', {
      data: {
        studentId: parseInt(studentId),
        classGroupId: 1, // Using existing class group
        sessionDate: '2024-03-15',
        status: 'Present',
        notes: 'E2E test attendance'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Attendance")');

    // Verify attendance list
    await expect(page.locator('table')).toBeVisible();
    await expect(page.locator('text=Date')).toBeVisible();
    await expect(page.locator('text=Class Group')).toBeVisible();
    await expect(page.locator('text=Status')).toBeVisible();
    await expect(page.locator('text=Notes')).toBeVisible();
    await expect(page.locator('text=2024-03-15')).toBeVisible();
  });

  test('Status chips display with correct colors', async ({ page }) => {
    // Create attendance records with different statuses
    await page.request.post('/api/attendance', {
      data: {
        studentId: parseInt(studentId),
        classGroupId: 1,
        sessionDate: '2024-03-16',
        status: 'Present',
        notes: 'Present test'
      }
    });

    await page.request.post('/api/attendance', {
      data: {
        studentId: parseInt(studentId),
        classGroupId: 1,
        sessionDate: '2024-03-17',
        status: 'Absent',
        notes: 'Absent test'
      }
    });

    await page.request.post('/api/attendance', {
      data: {
        studentId: parseInt(studentId),
        classGroupId: 1,
        sessionDate: '2024-03-18',
        status: 'Late',
        notes: 'Late test'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Attendance")');

    // Verify status chip colors
    await expect(page.locator('.status-chip:has-text("Present")')).toHaveCSS('background-color', /rgb\(34, 197, 94\)|green/);
    await expect(page.locator('.status-chip:has-text("Absent")')).toHaveCSS('background-color', /rgb\(239, 68, 68\)|red/);
    await expect(page.locator('.status-chip:has-text("Late")')).toHaveCSS('background-color', /rgb\(234, 179, 8\)|yellow/);
  });

  test('User can edit attendance inline', async ({ page }) => {
    // Create initial attendance record
    await page.request.post('/api/attendance', {
      data: {
        studentId: parseInt(studentId),
        classGroupId: 1,
        sessionDate: '2024-03-19',
        status: 'Present',
        notes: 'Initial note'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Attendance")');

    // Click on attendance row to edit
    await page.click('table tbody tr:first-child');

    // Change status to Absent
    await page.selectOption('select[name="status"]', 'Absent');

    // Add notes
    await page.fill('input[name="notes"]', 'Updated via E2E test');

    // Save
    await page.click('button:has-text("Save")');

    // Verify success
    await expect(page.locator('text=Updated via E2E test')).toBeVisible();
    await expect(page.locator('.status-chip:has-text("Absent")')).toBeVisible();
  });

  test('User can add new attendance entry', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Attendance")');

    // Click Add button
    await page.click('button:has-text("Add Attendance")');

    // Fill form
    await page.fill('input[name="sessionDate"]', '2024-03-20');
    await page.selectOption('select[name="classGroupId"]', '1');
    await page.selectOption('select[name="status"]', 'Present');
    await page.fill('input[name="notes"]', 'New attendance entry');

    // Save
    await page.click('button:has-text("Save")');

    // Verify
    await expect(page.locator('text=New attendance entry')).toBeVisible();
    await expect(page.locator('text=2024-03-20')).toBeVisible();
  });

  test('Save confirms changes immediately', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Attendance")');

    await page.click('button:has-text("Add Attendance")');
    await page.fill('input[name="sessionDate"]', '2024-03-21');
    await page.selectOption('select[name="classGroupId"]', '1');
    await page.selectOption('select[name="status"]', 'Present');

    // Verify save button is enabled
    await expect(page.locator('button:has-text("Save")')).toBeEnabled();

    await page.click('button:has-text("Save")');

    // Verify success message
    await expect(page.locator('text=success')).toBeVisible();
  });
});