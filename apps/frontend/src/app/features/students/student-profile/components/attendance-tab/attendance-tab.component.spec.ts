import { HttpClient } from '@angular/common/http';
import { DestroyRef } from '@angular/core';
import { type ComponentFixture, TestBed } from '@angular/core/testing';
import { AttendanceService } from '@core/services/attendance.service';
import { ClassGroupService } from '@core/services/class-group.service';
import { NotificationService } from '@core/services/notification.service';
import type { Attendance, AttendanceStatus } from '@features/attendance/models/attendance.model';
import { of, throwError } from 'rxjs';
import { AttendanceTabComponent } from './attendance-tab.component';

describe('AttendanceTabComponent', () => {
  let component: AttendanceTabComponent;
  let fixture: ComponentFixture<AttendanceTabComponent>;
  let attendanceServiceSpy: any;
  let classGroupServiceSpy: any;
  let notificationServiceSpy: any;

  const mockAttendanceRecords: Attendance[] = [
    {
      id: 1,
      studentId: 1,
      classGroupId: 1,
      classGroupName: 'Class 5A',
      sessionDate: '2026-01-02',
      status: 'Present',
      notes: 'On time',
      createdAt: '2026-01-02T10:00:00Z',
    },
    {
      id: 2,
      studentId: 1,
      classGroupId: 1,
      classGroupName: 'Class 5A',
      sessionDate: '2026-01-01',
      status: 'Late',
      notes: 'Arrived 10min late',
      createdAt: '2026-01-01T10:10:00Z',
    },
    {
      id: 3,
      studentId: 1,
      classGroupId: 1,
      classGroupName: 'Class 5A',
      sessionDate: '2025-12-31',
      status: 'Absent',
      notes: 'Sick',
      createdAt: '2025-12-31T10:00:00Z',
    },
  ];

  beforeEach(async () => {
    attendanceServiceSpy = {
      getAttendance: jest.fn(),
      updateAttendance: jest.fn(),
      createAttendance: jest.fn(),
    };

    classGroupServiceSpy = {
      loadClassGroups: jest.fn(),
      classGroups: (() => []) as any,
      loading: (() => false) as any,
    };

    notificationServiceSpy = {
      success: jest.fn(),
      error: jest.fn(),
      notifications$: (() => []) as any,
    };

    await TestBed.configureTestingModule({
      imports: [AttendanceTabComponent],
      providers: [
        { provide: AttendanceService, useValue: attendanceServiceSpy },
        { provide: ClassGroupService, useValue: classGroupServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy },
        { provide: HttpClient, useValue: {} },
        { provide: DestroyRef, useValue: { destroy: () => {} } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AttendanceTabComponent);
    component = fixture.componentInstance;

    // Set the required input signal
    // For input signals in tests, we set the value via the component instance
    (component as any).studentId = () => 1; // Make it act as a signal function
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Loading attendance records', () => {
    beforeEach(() => {
      (component as any).studentId = () => 1; // Ensure studentId signal is set
    });

    it('should load attendance records on init', () => {
      attendanceServiceSpy.getAttendance.mockReturnValue(of(mockAttendanceRecords));

      component.ngOnInit();
      fixture.detectChanges();

      expect(attendanceServiceSpy.getAttendance).toHaveBeenCalledWith({ studentId: 1 });
      expect(component.attendanceRecords()).toEqual(mockAttendanceRecords);
      expect(component.isLoading()).toBe(false);
    });

    it('should handle loading error', () => {
      attendanceServiceSpy.getAttendance.mockReturnValue(throwError(() => new Error('Error')));

      component.ngOnInit();
      fixture.detectChanges();

      expect(component.attendanceRecords()).toEqual([]);
      expect(component.isLoading()).toBe(false);
      expect(component.error()).toBe('Failed to load attendance records');
    });

    it('should handle empty response', () => {
      attendanceServiceSpy.getAttendance.mockReturnValue(of([]));

      component.ngOnInit();
      fixture.detectChanges();

      expect(component.attendanceRecords()).toEqual([]);
      expect(component.isLoading()).toBe(false);
    });
  });

  describe('Sorting records', () => {
    beforeEach(() => {
      attendanceServiceSpy.getAttendance.mockReturnValue(of(mockAttendanceRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should sort records by date descending', () => {
      const sorted = component.sortedRecords();
      expect(sorted[0].sessionDate).toBe('2026-01-02');
      expect(sorted[1].sessionDate).toBe('2026-01-01');
      expect(sorted[2].sessionDate).toBe('2025-12-31');
    });
  });

  describe('Status styling', () => {
    beforeEach(() => {
      attendanceServiceSpy.getAttendance.mockReturnValue(of(mockAttendanceRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should return correct class for Present status', () => {
      expect(component.getStatusClass('Present')).toBe('badge-success');
    });

    it('should return correct class for Absent status', () => {
      expect(component.getStatusClass('Absent')).toBe('badge-error');
    });

    it('should return correct class for Late status', () => {
      expect(component.getStatusClass('Late')).toBe('badge-warning');
    });
  });

  describe('Date formatting', () => {
    beforeEach(() => {
      attendanceServiceSpy.getAttendance.mockReturnValue(of(mockAttendanceRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should format date string correctly', () => {
      const formatted = component.formatDate('2026-01-02');
      expect(formatted).toContain('2026');
    });

    it('should handle invalid date string', () => {
      const formatted = component.formatDate('invalid-date');
      expect(formatted).toBe('Invalid Date');
    });
  });

  describe('Edit functionality', () => {
    beforeEach(() => {
      attendanceServiceSpy.getAttendance.mockReturnValue(of(mockAttendanceRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should start edit mode for a record', () => {
      component.startEdit(mockAttendanceRecords[0]);

      expect(component.editingId()).toBe(1);
      expect(component.editingForm()).toEqual({
        id: 1,
        status: 'Present',
        notes: 'On time',
      });
    });

    it('should cancel edit mode', () => {
      component.startEdit(mockAttendanceRecords[0]);
      component.cancelEdit();

      expect(component.editingId()).toBeNull();
      expect(component.editingForm()).toBeNull();
    });

    it('should update edit form field', () => {
      component.startEdit(mockAttendanceRecords[0]);
      component.updateEditForm('status', 'Absent');

      expect(component.editingForm()?.status).toBe('Absent');
    });
  });

  describe('Add functionality', () => {
    beforeEach(() => {
      attendanceServiceSpy.getAttendance.mockReturnValue(of(mockAttendanceRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should show add form', () => {
      component.showAddAttendanceForm();

      expect(component.showAddForm()).toBe(true);
    });

    it('should cancel add form', () => {
      component.showAddAttendanceForm();
      component.newAttendanceForm.set({
        sessionDate: '2026-01-01',
        classGroupId: 1,
        status: 'Present',
        notes: 'test',
      });
      component.cancelAdd();

      expect(component.showAddForm()).toBe(false);
      expect(component.newAttendanceForm().sessionDate).toBe(
        new Date().toISOString().split('T')[0],
      );
      expect(component.newAttendanceForm().classGroupId).toBe(0);
    });

    it('should validate new form', () => {
      component.newAttendanceForm.set({
        sessionDate: '2026-01-01',
        classGroupId: 1,
        status: 'Present',
        notes: '',
      });

      expect(component.isNewFormValid()).toBe(true);

      component.newAttendanceForm.set({
        sessionDate: '',
        classGroupId: 0,
        status: '' as AttendanceStatus,
        notes: '',
      });

      expect(component.isNewFormValid()).toBe(false);
    });
  });

  describe('Edit validation', () => {
    beforeEach(() => {
      attendanceServiceSpy.getAttendance.mockReturnValue(of(mockAttendanceRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should validate edit form', () => {
      component.editingForm.set({
        id: 1,
        status: 'Present',
        notes: 'test',
      });

      expect(component.isEditFormValid()).toBe(true);

      component.editingForm.set({
        id: 1,
        status: '' as AttendanceStatus,
        notes: '',
      });

      expect(component.isEditFormValid()).toBe(false);
    });
  });

  describe('Duplicate attendance detection', () => {
    beforeEach(() => {
      attendanceServiceSpy.getAttendance.mockReturnValue(of(mockAttendanceRecords));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should prevent adding duplicate attendance for same date and class group', () => {
      const existingRecord = mockAttendanceRecords[0];
      component.newAttendanceForm.set({
        sessionDate: existingRecord.sessionDate,
        classGroupId: existingRecord.classGroupId,
        status: 'Present',
        notes: '',
      });

      (component as any).addAttendance();

      expect(notificationServiceSpy.error).toHaveBeenCalledWith(
        'An attendance record for this student, date, and class group already exists',
        undefined,
        5000,
      );
      expect(attendanceServiceSpy.createAttendance).not.toHaveBeenCalled();
    });

    it('should allow adding attendance for different date or class group', () => {
      attendanceServiceSpy.createAttendance.mockReturnValue(
        of({ ...mockAttendanceRecords[0], id: 999 }),
      );

      component.newAttendanceForm.set({
        sessionDate: '2026-02-01', // Different date
        classGroupId: mockAttendanceRecords[0].classGroupId,
        status: 'Present',
        notes: '',
      });

      (component as any).addAttendance();

      expect(attendanceServiceSpy.createAttendance).toHaveBeenCalled();
    });
  });

  describe('Blur and enter key save', () => {
    beforeEach(() => {
      attendanceServiceSpy.getAttendance.mockReturnValue(of(mockAttendanceRecords));
      attendanceServiceSpy.updateAttendance.mockReturnValue(of(mockAttendanceRecords[0]));
      component.ngOnInit();
      fixture.detectChanges();
    });

    it('should save on blur when form is valid', () => {
      component.startEdit(mockAttendanceRecords[0]);

      // Trigger blur
      (component as any).onEditBlur();

      expect(attendanceServiceSpy.updateAttendance).toHaveBeenCalled();
    });

    it('should not save on blur when form is invalid', () => {
      component.startEdit(mockAttendanceRecords[0]);
      const currentForm = component.editingForm();
      if (currentForm) {
        component.editingForm.set({ ...currentForm, status: '' as AttendanceStatus });
      }

      // Trigger blur
      (component as any).onEditBlur();

      expect(attendanceServiceSpy.updateAttendance).not.toHaveBeenCalled();
    });
  });
});
