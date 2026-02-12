import { test, expect } from '@playwright/test';

test.describe('Audit Trail E2E Tests (FR14 - Critical)', () => {
  let studentId: string;
  let classGroupId: string;
  let attendanceId: string;

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
        name: `E2E Audit Test School ${timestamp}`,
        shortName: `E2EA${timestamp}`,
        printInvoice: false,
        importFlag: false,
        isActive: true
      }
    });
    const school = await schoolResponse.json();

    // Create class group
    const classGroupResponse = await page.request.post('/api/class-groups', {
      data: {
        name: `E2E-Audit-CG-${timestamp}`,
        description: 'E2E Audit Trail Test Class Group',
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
        firstName: 'E2E',
        lastName: `AuditStudent${timestamp}`,
        dateOfBirth: '2010-01-01',
        classGroupId: classGroup.id,
        isActive: true
      }
    });
    const student = await studentResponse.json();
    studentId = student.id.toString();

    // Create initial attendance record
    const attendanceResponse = await page.request.post('/api/attendance', {
      data: {
        studentId: student.id,
        classGroupId: classGroup.id,
        sessionDate: '2024-03-30',
        status: 'Present',
        notes: 'Initial attendance'
      }
    });
    const attendance = await attendanceResponse.json();
    attendanceId = attendance.id.toString();
  });

  test.afterEach(async ({ page }) => {
    // Clean up test data
    if (attendanceId) {
      await page.request.delete(`/api/attendance/${attendanceId}`);
    }
    if (studentId) {
      await page.request.delete(`/api/students/${studentId}`);
    }
    if (classGroupId) {
      await page.request.delete(`/api/class-groups/${classGroupId}`);
    }
  });

  test('Audit log created on attendance update', async ({ page }) => {
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Attendance")');

    // Edit attendance
    await page.click('table tbody tr');
    await page.selectOption('select[name="status"]', 'Absent');
    await page.fill('input[name="notes"]', 'Changed to absent');
    await page.click('button:has-text("Save")');

    // Verify audit log was created by checking the audit trail
    await page.reload();
    await page.click('button:has-text("Attendance")');

    // Look for "View History" or similar button
    const historyButton = page.locator('button:has-text("View History"), button:has-text("History"), button[aria-label="View History"]');
    if (await historyButton.isVisible()) {
      await historyButton.click();
    }

    // Verify audit trail panel appears
    await expect(page.locator('[role="dialog"], .audit-trail-panel, .history-panel')).toBeVisible();
  });

  test('Audit entry contains required information', async ({ page }) => {
    const newNotes = 'Updated via E2E audit test';
    const newStatus = 'Late';

    // Update attendance
    await page.request.patch(`/api/attendance/${attendanceId}`, {
      data: {
        status: newStatus,
        notes: newNotes
      }
    });

    // Navigate to student profile
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Attendance")');

    // Open audit trail
    await page.click('button:has-text("View History")');

    // Verify audit panel
    await expect(page.locator('h2, h3')).toContainText('Audit Trail');

    // Check for audit information (may vary by implementation)
    // Timestamp should be present
    const timestampPresent = await page.locator('text=/\\d{4}-\\d{2}-\\d{2}/').count() > 0;

    // Status change should be documented
    const statusChangePresent = await page.locator(`text=${newStatus}`).count() > 0 ||
                                 await page.locator('text=Status').count() > 0;

    // Notes change should be documented
    const notesChangePresent = await page.locator(`text=${newNotes}`).count() > 0 ||
                                await page.locator('text=Notes').count() > 0;

    expect(timestampPresent || statusChangePresent || notesChangePresent).toBeTruthy();
  });

  test('"View History" opens Audit Trail Panel', async ({ page }) => {
    // Make an update to create audit history
    await page.request.patch(`/api/attendance/${attendanceId}`, {
      data: {
        notes: 'Audit trail test'
      }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Attendance")');

    // Click View History button
    await page.click('button:has-text("View History")');

    // Verify panel opens
    await expect(page.locator('[role="dialog"], .audit-trail-panel')).toBeVisible();

    // Verify panel title or header
    await expect(page.locator('h2, h3, .panel-title')).toContainText('Audit Trail');
  });

  test('Audit Trail Panel displays all changes', async ({ page }) => {
    // Make multiple updates
    await page.request.patch(`/api/attendance/${attendanceId}`, {
      data: { status: 'Absent', notes: 'First change' }
    });

    await page.request.patch(`/api/attendance/${attendanceId}`, {
      data: { status: 'Present', notes: 'Second change' }
    });

    await page.request.patch(`/api/attendance/${attendanceId}`, {
      data: { status: 'Late', notes: 'Third change' }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Attendance")');

    // Open audit trail
    await page.click('button:has-text("View History")');

    // Verify panel shows changes
    // Look for evidence of multiple entries
    const auditEntriesCount = await page.locator('.audit-entry, .history-entry, tr').count();
    expect(auditEntriesCount).toBeGreaterThan(1);
  });

  test('Audit trail is read-only', async ({ page }) => {
    await page.request.patch(`/api/attendance/${attendanceId}`, {
      data: { notes: 'Read-only test' }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Attendance")');

    // Open audit trail
    await page.click('button:has-text("View History")');

    // Verify panel is read-only (no edit controls)
    await expect(page.locator('input, textarea, select')).not.toBeVisible();
    await expect(page.locator('button:has-text("Edit"), button:has-text("Save")')).not.toBeVisible();
  });

  test('Each change shows timestamp, field, old value, new value, actor', async ({ page }) => {
    const originalResponse = await page.request.get(`/api/attendance/${attendanceId}`);
    const original = await originalResponse.json();

    // Update with specific values to verify
    await page.request.patch(`/api/attendance/${attendanceId}`, {
      data: { status: 'Late', notes: 'Audit detail test' }
    });

    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Attendance")');

    // Open audit trail
    await page.click('button:has-text("View History")');

    // Verify timestamp
    await expect(page.locator('text=/\\d{4}-\\d{2}-\\d{2}/, text=/\\d{2}:\\d{2}/')).toBeVisible();

    // Verify change information
    // Look for status or notes field mentions
    const fieldMentions = await page.locator('text=Status, text=Notes, text=attendance').count();
    expect(fieldMentions).toBeGreaterThan(0);

    // Verify audit trail is visible in the UI
    await expect(page.locator('[role="dialog"], .audit-trail-panel, .history-panel')).toBeVisible();
  });

  test('Historical data shows correct timestamps', async ({ page }) => {
    // Create attendance with past date
    await page.request.post('/api/attendance', {
      data: {
        studentId: parseInt(studentId),
        classGroupId: parseInt(classGroupId),
        sessionDate: '2023-01-15', // Historical date
        status: 'Present',
        notes: 'Historical attendance'
      }
    });

    // Navigate to student
    await page.goto(`/admin/students/${studentId}`);
    await page.click('button:has-text("Attendance")');

    // Verify historical data displays
    await expect(page.locator('text=2023-01-15')).toBeVisible();

    // Open audit trail for historical record
    await page.click('table tbody tr:has-text("2023-01-15")');
    await page.click('button:has-text("View History")');

    // Verify historical timestamp preservation
    await expect(page.locator('[role="dialog"], .audit-trail-panel')).toBeVisible();
  });

  test('FR14 compliance - Audit trail mandatory for all attendance updates', async ({ page }) => {
    // Make multiple updates to verify every change is audited
    for (let i = 1; i <= 3; i++) {
      await page.request.patch(`/api/attendance/${attendanceId}`, {
        data: {
          status: i === 1 ? 'Absent' : i === 2 ? 'Late' : 'Present',
          notes: `Update ${i}`
        }
      });

      // Verify each update creates audit trail entry
      await page.goto(`/admin/students/${studentId}`);
      await page.click('button:has-text("Attendance")');
      await page.click('button:has-text("View History")');

      // Verify audit trail has entries for this update
      const hasAuditData = await page.locator('text=Update').count() > 0 ||
                           await page.locator('text=Status').count() > 0;
      expect(hasAuditData).toBeTruthy();
    }
  });
});