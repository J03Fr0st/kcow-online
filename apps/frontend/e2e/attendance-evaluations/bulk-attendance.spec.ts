import { test, expect } from '@playwright/test';

test.describe('Bulk Attendance Entry E2E Tests', () => {
  let schoolId: string;
  let classGroupId: string;
  const students: number[] = [];

  test.beforeEach(async ({ page }) => {
    // Login as admin
    await page.goto('/login');
    await page.fill('input[name="email"]', 'admin@kcow.local');
    await page.fill('input[name="password"]', 'Admin123!');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL('/admin');

    const timestamp = Date.now();

    // Create school
    const schoolResponse = await page.request.post('/api/schools', {
      data: {
        name: `E2E Bulk Test School ${timestamp}`,
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
        name: `E2E-Bulk-CG-${timestamp}`,
        description: 'E2E Bulk Attendance Test Class Group',
        schoolId: school.id,
        dayOfWeek: 1,
        startTime: '08:00',
        endTime: '09:00',
        isActive: true
      }
    });
    const classGroup = await classGroupResponse.json();
    classGroupId = classGroup.id.toString();

    // Create multiple test students
    for (let i = 1; i <= 5; i++) {
      const studentResponse = await page.request.post('/api/students', {
        data: {
          reference: `E2EBULK${timestamp}-${i}`,
          firstName: `BulkStudent`,
          lastName: `${i}`,
          dateOfBirth: '2010-01-01',
          classGroupId: classGroup.id,
          isActive: true
        }
      });
      const student = await studentResponse.json();
      students.push(student.id);
    }
  });

  test.afterEach(async ({ page }) => {
    // Clean up test data
    for (const studentId of students) {
      await page.request.delete(`/api/students/${studentId}`);
    }
    if (classGroupId) {
      await page.request.delete(`/api/class-groups/${classGroupId}`);
    }
    if (schoolId) {
      await page.request.delete(`/api/schools/${schoolId}`);
    }
  });

  test('Navigate to bulk attendance from Class Groups page', async ({ page }) => {
    await page.goto('/admin/class-groups');
    await expect(page.locator('h1')).toContainText('Class Groups');

    // Click on class group
    await page.click(`[data-testid="class-group-${classGroupId}"]`);

    // Verify "Take Attendance" button is visible
    await expect(page.locator('button:has-text("Take Attendance")')).toBeVisible();
  });

  test('"Take Attendance" opens bulk entry modal', async ({ page }) => {
    await page.goto('/admin/class-groups');
    await page.click(`[data-testid="class-group-${classGroupId}"]`);

    // Click Take Attendance button
    await page.click('button:has-text("Take Attendance")');

    // Verify modal opens
    await expect(page.locator('[role="dialog"]')).toBeVisible();
    await expect(page.locator('h2')).toContainText('Take Attendance');
  });

  test('All enrolled students display with status toggles', async ({ page }) => {
    await page.goto('/admin/class-groups');
    await page.click(`[data-testid="class-group-${classGroupId}"]`);
    await page.click('button:has-text("Take Attendance")');

    // Verify all students are listed
    for (let i = 1; i <= 5; i++) {
      await expect(page.locator(`text=BulkStudent ${i}`)).toBeVisible();
    }

    // Verify each student has a status toggle
    for (let i = 0; i < 5; i++) {
      await expect(page.locator(`table tbody tr:nth-child(${i + 1}) select[name="status"]`)).toBeVisible();
    }
  });

  test('User can mark attendance for all students', async ({ page }) => {
    await page.goto('/admin/class-groups');
    await page.click(`[data-testid="class-group-${classGroupId}"]`);
    await page.click('button:has-text("Take Attendance")');

    // Mark first student as Present
    await page.selectOption('table tbody tr:nth-child(1) select[name="status"]', 'Present');

    // Mark second student as Absent
    await page.selectOption('table tbody tr:nth-child(2) select[name="status"]', 'Absent');

    // Mark third student as Late
    await page.selectOption('table tbody tr:nth-child(3) select[name="status"]', 'Late');

    // Verify selections are made
    await expect(page.locator('table tbody tr:nth-child(1) select[name="status"]')).toHaveValue('Present');
    await expect(page.locator('table tbody tr:nth-child(2) select[name="status"]')).toHaveValue('Absent');
    await expect(page.locator('table tbody tr:nth-child(3) select[name="status"]')).toHaveValue('Late');
  });

  test('"Save All" creates attendance records', async ({ page }) => {
    await page.goto('/admin/class-groups');
    await page.click(`[data-testid="class-group-${classGroupId}"]`);
    await page.click('button:has-text("Take Attendance")');

    // Mark all students as Present
    for (let i = 1; i <= 5; i++) {
      await page.selectOption(`table tbody tr:nth-child(${i}) select[name="status"]`, 'Present');
    }

    // Set attendance date
    await page.fill('input[name="sessionDate"]', '2024-03-25');

    // Click Save All
    await page.click('button:has-text("Save All")');

    // Verify success message
    await expect(page.locator('text=Attendance saved successfully')).toBeVisible();

    // Modal should close
    await expect(page.locator('[role="dialog"]')).not.toBeVisible();

    // Navigate to student profile to verify
    await page.goto(`/admin/students/${students[0]}`);
    await page.click('button:has-text("Attendance")');

    // Verify attendance was recorded
    await expect(page.locator('text=2024-03-25')).toBeVisible();
  });

  test('Existing attendance pre-fills with current values', async ({ page }) => {
    const sessionDate = '2024-03-26';

    // Create existing attendance record
    await page.request.post('/api/attendance', {
      data: {
        studentId: students[0],
        classGroupId: parseInt(classGroupId),
        sessionDate: sessionDate,
        status: 'Present',
        notes: 'Pre-existing'
      }
    });

    await page.goto('/admin/class-groups');
    await page.click(`[data-testid="class-group-${classGroupId}"]`);
    await page.click('button:has-text("Take Attendance")');

    // Set same date
    await page.fill('input[name="sessionDate"]', sessionDate);

    // First student should have "Present" pre-selected
    await expect(page.locator('table tbody tr:nth-child(1) select[name="status"]')).toHaveValue('Present');
  });

  test('Success confirmation appears after save', async ({ page }) => {
    await page.goto('/admin/class-groups');
    await page.click(`[data-testid="class-group-${classGroupId}"]`);
    await page.click('button:has-text("Take Attendance")');

    // Mark all students
    for (let i = 1; i <= 5; i++) {
      await page.selectOption(`table tbody tr:nth-child(${i}) select[name="status"]`, 'Present');
    }

    await page.fill('input[name="sessionDate"]', '2024-03-27');
    await page.click('button:has-text("Save All")');

    // Verify success notification
    await expect(page.locator('.toast-success, .alert-success, text=Success')).toBeVisible();
  });

  test('Bulk attendance handles mixed statuses correctly', async ({ page }) => {
    await page.goto('/admin/class-groups');
    await page.click(`[data-testid="class-group-${classGroupId}"]`);
    await page.click('button:has-text("Take Attendance")');

    // Set mixed statuses
    await page.selectOption('table tbody tr:nth-child(1) select[name="status"]', 'Present');
    await page.selectOption('table tbody tr:nth-child(2) select[name="status"]', 'Absent');
    await page.selectOption('table tbody tr:nth-child(3) select[name="status"]', 'Late');
    await page.selectOption('table tbody tr:nth-child(4) select[name="status"]', 'Present');
    await page.selectOption('table tbody tr:nth-child(5) select[name="status"]', 'Absent');

    await page.fill('input[name="sessionDate"]', '2024-03-28');
    await page.click('button:has-text("Save All")');

    // Verify all statuses saved correctly
    await expect(page.locator('text=Attendance saved successfully')).toBeVisible();

    // Verify in one student's profile
    await page.goto(`/admin/students/${students[2]}`); // Student with Late status
    await page.click('button:has-text("Attendance")');
    await expect(page.locator('text=2024-03-28')).toBeVisible();
    await expect(page.locator('.status-chip:has-text("Late")')).toBeVisible();
  });
});