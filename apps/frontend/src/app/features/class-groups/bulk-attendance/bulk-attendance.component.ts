import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed, DestroyRef, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, EMPTY } from 'rxjs';
import { environment } from '@environments/environment';
import type { ClassGroup } from '../models/class-group.model';
import { StudentService } from '@core/services/student.service';
import { NotificationService } from '@core/services/notification.service';

/**
 * Attendance status enumeration matching backend enum.
 */
type AttendanceStatus = 'Present' | 'Absent' | 'Late';

/**
 * Student attendance entry for bulk editing
 */
interface StudentAttendanceEntry {
  studentId: number;
  studentName: string;
  grade?: string;
  status: AttendanceStatus;
  notes: string;
  hasExistingRecord: boolean; // True if this is an update, false if new
  attendanceId?: number; // ID of existing attendance record
}

/**
 * Request model for batch attendance save
 */
interface BatchAttendanceRequest {
  classGroupId: number;
  sessionDate: string; // ISO date string (YYYY-MM-DD)
  entries: BatchAttendanceEntry[];
}

interface BatchAttendanceEntry {
  studentId: number;
  status: 'Present' | 'Absent' | 'Late';
  notes?: string;
}

/**
 * Response model for batch attendance save
 */
interface BatchAttendanceResponse {
  created: number;
  updated: number;
  failed: number;
  errors?: string[];
}

/**
 * Attendance record as returned from the API.
 */
interface Attendance {
  id: number;
  studentId: number;
  studentName?: string;
  classGroupId: number;
  classGroupName?: string;
  sessionDate: string; // ISO date string (YYYY-MM-DD)
  status: 'Present' | 'Absent' | 'Late';
  notes?: string;
  createdAt: string; // ISO datetime string
  modifiedAt?: string; // ISO datetime string
}

@Component({
  selector: 'app-bulk-attendance',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './bulk-attendance.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BulkAttendanceComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly studentService = inject(StudentService);
  private readonly notificationService = inject(NotificationService);
  private readonly destroyRef = inject(DestroyRef);

  // Inputs
  readonly classGroupId = input.required<number>();

  // Outputs
  readonly close = output<void>();

  // Today's date for max attribute on date input
  protected readonly todayDate = this.getTodayDate();

  // Local component state
  protected readonly sessionDate = signal<string>(this.todayDate);
  protected readonly students = signal<StudentAttendanceEntry[]>([]);
  protected readonly loading = signal<boolean>(false);
  protected readonly saving = signal<boolean>(false);
  protected readonly error = signal<string | null>(null);

  // Computed properties
  protected readonly hasChanges = computed(() => {
    return this.students().some(s => !s.hasExistingRecord);
  });

  protected readonly studentCount = computed(() => this.students().length);
  protected readonly presentCount = computed(() => this.students().filter(s => s.status === 'Present').length);
  protected readonly absentCount = computed(() => this.students().filter(s => s.status === 'Absent').length);
  protected readonly lateCount = computed(() => this.students().filter(s => s.status === 'Late').length);

  // Class group info (will be loaded)
  protected classGroupName = signal<string>('');
  protected dayOfWeek = signal<string>('');

  ngOnInit(): void {
    this.loadClassGroupInfo();
    this.loadStudents();
  }

  /**
   * Get today's date in ISO format (YYYY-MM-DD)
   */
  private getTodayDate(): string {
    const today = new Date();
    return today.toISOString().split('T')[0];
  }

  /**
   * Load class group info for display
   */
  private loadClassGroupInfo(): void {
    this.loading.set(true);

    this.http.get<ClassGroup>(`${environment.apiUrl}/class-groups/${this.classGroupId()}`).pipe(
      takeUntilDestroyed(this.destroyRef),
      catchError((error: HttpErrorResponse) => {
        console.error('Error loading class group:', error);
        this.error.set('Failed to load class group information');
        this.loading.set(false);
        return EMPTY;
      })
    ).subscribe({
      next: (classGroup) => {
        this.classGroupName.set(classGroup.name);
        this.dayOfWeek.set(this.getDayOfWeekName(classGroup.dayOfWeek));
        this.loading.set(false);
      },
    });
  }

  /**
   * Load students assigned to this class group
   */
  private loadStudents(): void {
    this.loading.set(true);

    // Get students with classGroupId filter
    this.studentService.getStudents({ classGroupId: this.classGroupId() }).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (response) => {
        const students = response.items;

        // Convert to StudentAttendanceEntry with default status
        const entries: StudentAttendanceEntry[] = students.map(student => ({
          studentId: student.id,
          studentName: `${student.firstName || ''} ${student.lastName || ''}`.trim() || student.reference,
          grade: student.grade,
          status: 'Present' as AttendanceStatus, // Default to Present
          notes: '',
          hasExistingRecord: false,
        }));

        this.students.set(entries);
        this.loading.set(false);

        // Load existing attendance for the selected date
        this.loadExistingAttendance();
      },
      error: (err) => {
        console.error('Error loading students:', err);
        this.error.set('Failed to load students');
        this.loading.set(false);
      }
    });
  }

  /**
   * Load existing attendance records for the selected date
   */
  private loadExistingAttendance(): void {
    const date = this.sessionDate();
    if (!date) return;

    this.http.get<Attendance[]>(`${environment.apiUrl}/attendance?classGroupId=${this.classGroupId()}&fromDate=${date}&toDate=${date}`).pipe(
      takeUntilDestroyed(this.destroyRef),
      catchError((error: HttpErrorResponse) => {
        // If no attendance found, that's expected for new entries
        if (error.status === 404) {
          return of([]);
        }
        console.error('Error loading existing attendance:', error);
        return of([]);
      })
    ).subscribe({
      next: (existingAttendance) => {
        if (existingAttendance && existingAttendance.length > 0) {
          // Update students with existing attendance data
          this.students.update(entries =>
            entries.map(entry => {
              const existing = existingAttendance.find(a => a.studentId === entry.studentId);
              if (existing) {
                return {
                  ...entry,
                  status: existing.status,
                  notes: existing.notes || '',
                  hasExistingRecord: true,
                  attendanceId: existing.id,
                };
              }
              return entry;
            })
          );
        }
      }
    });
  }

  /**
   * Handle date change - reload existing attendance
   */
  protected onDateChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.sessionDate.set(select.value);

    // Reset entries to default and reload existing attendance
    this.students.update(entries =>
      entries.map(entry => ({
        ...entry,
        status: 'Present' as AttendanceStatus,
        notes: '',
        hasExistingRecord: false,
        attendanceId: undefined,
      }))
    );

    this.loadExistingAttendance();
  }

  /**
   * Update student's attendance status
   */
  protected updateStudentStatus(studentId: number, status: AttendanceStatus): void {
    this.students.update(entries =>
      entries.map(entry =>
        entry.studentId === studentId ? { ...entry, status } : entry
      )
    );
  }

  /**
   * Update student's notes from input event
   */
  protected onStudentNotesInput(studentId: number, event: Event): void {
    const input = event.target as HTMLInputElement;
    this.updateStudentNotes(studentId, input.value);
  }

  /**
   * Update student's notes
   */
  protected updateStudentNotes(studentId: number, notes: string): void {
    this.students.update(entries =>
      entries.map(entry =>
        entry.studentId === studentId ? { ...entry, notes } : entry
      )
    );
  }

  /**
   * Save all attendance entries
   */
  protected saveAll(): void {
    const entries = this.students();
    if (entries.length === 0) {
      this.notificationService.error('No students to save attendance for');
      return;
    }

    this.saving.set(true);
    this.error.set(null);

    const request: BatchAttendanceRequest = {
      classGroupId: this.classGroupId(),
      sessionDate: this.sessionDate(),
      entries: entries.map(entry => ({
        studentId: entry.studentId,
        status: entry.status,
        notes: entry.notes || undefined,
      })),
    };

    this.http.post<BatchAttendanceResponse>(`${environment.apiUrl}/class-groups/${this.classGroupId()}/attendance`, request).pipe(
      takeUntilDestroyed(this.destroyRef),
      catchError((error: HttpErrorResponse) => {
        console.error('Error saving attendance:', error);
        this.error.set('Failed to save attendance. Please try again.');
        this.saving.set(false);
        return EMPTY;
      })
    ).subscribe({
      next: (response) => {
        this.saving.set(false);

        if (response.failed > 0) {
          this.notificationService.warning(
            `Attendance saved with ${response.failed} errors. ${response.created} created, ${response.updated} updated.`
          );
          if (response.errors && response.errors.length > 0) {
            console.error('Save errors:', response.errors);
          }
        } else {
          this.notificationService.success(
            `Attendance saved successfully! ${response.created} created, ${response.updated} updated.`
          );
          this.close.emit();
        }
      },
    });
  }

  /**
   * Cancel and close the bulk attendance view
   */
  protected cancel(): void {
    this.close.emit();
  }

  /**
   * Get day of week name from number
   */
  private getDayOfWeekName(dayNumber: number): string {
    const days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];
    return days[dayNumber] || 'Unknown';
  }

  /**
   * Get status badge class
   */
  protected getStatusBadgeClass(status: AttendanceStatus): string {
    switch (status) {
      case 'Present':
        return 'badge badge-success badge-outline';
      case 'Absent':
        return 'badge badge-error badge-outline';
      case 'Late':
        return 'badge badge-warning badge-outline';
      default:
        return 'badge badge-ghost badge-outline';
    }
  }
}
