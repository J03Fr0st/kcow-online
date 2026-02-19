import { CommonModule } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  type OnInit,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AttendanceService } from '@core/services/attendance.service';
import { ClassGroupService } from '@core/services/class-group.service';
import { NotificationService } from '@core/services/notification.service';
import type {
  Attendance,
  AttendanceStatus,
  CreateAttendanceRequest,
} from '@features/attendance/models/attendance.model';
import type { ClassGroup } from '@features/class-groups/models/class-group.model';
import { AuditTrailPanelComponent } from '../audit-trail-panel/audit-trail-panel.component';

interface EditingAttendance {
  id: number;
  status: AttendanceStatus;
  notes: string;
}

interface NewAttendanceForm {
  sessionDate: string;
  classGroupId: number;
  status: AttendanceStatus;
  notes: string;
}

@Component({
  selector: 'app-attendance-tab',
  standalone: true,
  imports: [CommonModule, AuditTrailPanelComponent],
  templateUrl: './attendance-tab.component.html',
  styleUrls: ['./attendance-tab.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AttendanceTabComponent implements OnInit {
  private readonly attendanceService = inject(AttendanceService);
  private readonly classGroupService = inject(ClassGroupService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  // Class groups signal
  readonly classGroups = signal<ClassGroup[]>([]);
  readonly isLoadingClassGroups = signal<boolean>(false);

  // Input signal for student ID
  readonly studentId = input.required<number>();

  // State signals
  readonly attendanceRecords = signal<Attendance[]>([]);
  readonly isLoading = signal<boolean>(false);
  readonly isSaving = signal<boolean>(false);
  readonly error = signal<string | null>(null);

  // Edit state
  readonly editingId = signal<number | null>(null);
  readonly editingForm = signal<EditingAttendance | null>(null);

  // Add form state
  readonly showAddForm = signal<boolean>(false);
  readonly newAttendanceForm = signal<NewAttendanceForm>({
    sessionDate: new Date().toISOString().split('T')[0], // Default to today
    classGroupId: 0,
    status: 'Present',
    notes: '',
  });

  // Audit trail panel state
  readonly auditPanelRecordId = signal<number | null>(null);

  // Computed: sorted attendance records by date descending
  readonly sortedRecords = computed(() => {
    return [...this.attendanceRecords()].sort((a, b) => {
      return new Date(b.sessionDate).getTime() - new Date(a.sessionDate).getTime();
    });
  });

  // Status options
  readonly statusOptions: AttendanceStatus[] = ['Present', 'Absent', 'Late'];

  ngOnInit(): void {
    this.loadAttendance();
    this.loadClassGroups();
  }

  /**
   * Load attendance records for the student
   */
  private loadAttendance(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.attendanceService
      .getAttendance({ studentId: this.studentId() })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (records) => {
          this.attendanceRecords.set(Array.isArray(records) ? records : []);
          this.isLoading.set(false);
        },
        error: (err) => {
          this.error.set('Failed to load attendance records');
          this.isLoading.set(false);
          console.error('Error loading attendance:', err);
        },
      });
  }

  /**
   * Load available class groups for the dropdown
   */
  private loadClassGroups(): void {
    this.isLoadingClassGroups.set(true);

    // Use the loading signal from the service
    this.classGroupService.loadClassGroups();
    // Copy the class groups from the service signal to our component signal
    this.classGroups.set(this.classGroupService.classGroups());
    this.isLoadingClassGroups.set(this.classGroupService.loading());
  }

  /**
   * Start editing an attendance record
   */
  startEdit(record: Attendance): void {
    this.editingId.set(record.id);
    this.editingForm.set({
      id: record.id,
      status: record.status,
      notes: record.notes || '',
    });
  }

  /**
   * Cancel editing
   */
  cancelEdit(): void {
    this.editingId.set(null);
    this.editingForm.set(null);
  }

  /**
   * Cancel add form
   */
  cancelAdd(): void {
    this.showAddForm.set(false);
    this.newAttendanceForm.set({
      sessionDate: new Date().toISOString().split('T')[0],
      classGroupId: 0,
      status: 'Present',
      notes: '',
    });
  }

  /**
   * Show add form
   */
  showAddAttendanceForm(): void {
    this.showAddForm.set(true);
  }

  /**
   * Save edited attendance
   */
  protected saveEdit(): void {
    const form = this.editingForm();
    if (!form) return;

    this.isSaving.set(true);

    this.attendanceService
      .updateAttendance(form.id, {
        status: form.status,
        notes: form.notes || undefined,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.updateRecordInList(updated);
          this.editingId.set(null);
          this.editingForm.set(null);
          this.isSaving.set(false);
          this.showSuccessMessage('Attendance updated successfully');
        },
        error: (err) => {
          this.isSaving.set(false);
          this.showErrorMessage('Failed to update attendance');
          console.error('Error updating attendance:', err);
        },
      });
  }

  /**
   * Add new attendance record
   */
  protected addAttendance(): void {
    const form = this.newAttendanceForm();

    if (!form.sessionDate || !form.classGroupId) {
      this.showErrorMessage('Please fill in all required fields');
      return;
    }

    // Check for duplicate attendance record
    const existingRecord = this.attendanceRecords().find(
      (r) => r.sessionDate === form.sessionDate && r.classGroupId === form.classGroupId,
    );

    if (existingRecord) {
      this.showErrorMessage(
        'An attendance record for this student, date, and class group already exists',
      );
      return;
    }

    this.isSaving.set(true);

    const request: CreateAttendanceRequest = {
      studentId: this.studentId(),
      classGroupId: form.classGroupId,
      sessionDate: form.sessionDate,
      status: form.status,
      notes: form.notes || undefined,
    };

    this.attendanceService
      .createAttendance(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (created) => {
          this.attendanceRecords.set([...this.attendanceRecords(), created]);
          this.cancelAdd();
          this.isSaving.set(false);
          this.showSuccessMessage('Attendance added successfully');
        },
        error: (err) => {
          this.isSaving.set(false);
          this.showErrorMessage('Failed to add attendance');
          console.error('Error adding attendance:', err);
        },
      });
  }

  /**
   * Update a record in the list after edit
   */
  private updateRecordInList(updated: Attendance): void {
    const current = this.attendanceRecords();
    const index = current.findIndex((r) => r.id === updated.id);
    if (index !== -1) {
      const updatedList = [...current];
      updatedList[index] = updated;
      this.attendanceRecords.set(updatedList);
    }
  }

  /**
   * Format date for display
   */
  formatDate(dateString: string): string {
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString();
    } catch {
      return dateString;
    }
  }

  /**
   * Get status class for chip styling
   */
  getStatusClass(status: AttendanceStatus): string {
    switch (status) {
      case 'Present':
        return 'badge-success';
      case 'Absent':
        return 'badge-error';
      case 'Late':
        return 'badge-warning';
      default:
        return '';
    }
  }

  /**
   * Update edit form field
   */
  updateEditForm(field: keyof EditingAttendance, value: string | number): void {
    const current = this.editingForm();
    if (current) {
      this.editingForm.set({ ...current, [field]: value });
    }
  }

  /**
   * Handle blur event during editing - save if form is valid
   */
  protected onEditBlur(): void {
    // Only save if edit mode is still active (prevents double-save with Enter key)
    if (this.editingId() !== null && this.isEditFormValid()) {
      this.saveEdit();
    }
  }

  /**
   * Update new attendance form field
   */
  updateNewForm(field: keyof NewAttendanceForm, value: string | number): void {
    this.newAttendanceForm.set({ ...this.newAttendanceForm(), [field]: value });
  }

  /**
   * Check if edit form is valid
   */
  isEditFormValid(): boolean {
    const form = this.editingForm();
    return (
      form !== null &&
      (form.status === 'Present' || form.status === 'Absent' || form.status === 'Late')
    );
  }

  /**
   * Check if new form is valid
   */
  isNewFormValid(): boolean {
    const form = this.newAttendanceForm();
    return form.sessionDate !== '' && form.classGroupId > 0 && !!form.status;
  }

  /**
   * Show success message
   */
  private showSuccessMessage(message: string): void {
    this.notificationService.success(message, undefined, 3000);
  }

  /**
   * Show error message
   */
  private showErrorMessage(message: string): void {
    this.notificationService.error(message, undefined, 5000);
  }

  /**
   * Open audit trail panel for a specific record
   */
  openAuditTrail(recordId: number): void {
    this.auditPanelRecordId.set(recordId);
  }

  /**
   * Close audit trail panel
   */
  closeAuditTrail(): void {
    this.auditPanelRecordId.set(null);
  }
}
