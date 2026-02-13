import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AttendanceService } from '@core/services/attendance.service';
import { ClassGroupService } from '@core/services/class-group.service';
import type { Attendance, AttendanceQueryParams } from '@features/attendance/models/attendance.model';

@Component({
  selector: 'app-attendance-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './attendance-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AttendanceListComponent implements OnInit {
  private readonly attendanceService = inject(AttendanceService);
  protected readonly classGroupService = inject(ClassGroupService);

  protected readonly records = signal<Attendance[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly error = signal<string | null>(null);

  // Filter state
  protected readonly filterClassGroupId = signal<number | undefined>(undefined);
  protected readonly filterFromDate = signal('');
  protected readonly filterToDate = signal('');

  protected readonly sortedRecords = computed(() => {
    return [...this.records()].sort(
      (a, b) => new Date(b.sessionDate).getTime() - new Date(a.sessionDate).getTime()
    );
  });

  ngOnInit(): void {
    this.classGroupService.loadClassGroups();
    this.loadAttendance();
  }

  protected loadAttendance(): void {
    this.isLoading.set(true);
    this.error.set(null);

    const params: AttendanceQueryParams = {};
    if (this.filterClassGroupId()) {
      params.classGroupId = this.filterClassGroupId();
    }
    if (this.filterFromDate()) {
      params.fromDate = this.filterFromDate();
    }
    if (this.filterToDate()) {
      params.toDate = this.filterToDate();
    }

    this.attendanceService.getAttendance(params).subscribe({
      next: (records) => {
        this.records.set(records);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set(err.detail || err.message || 'Failed to load attendance records');
        this.isLoading.set(false);
      },
    });
  }

  protected onClassGroupChange(value: string): void {
    this.filterClassGroupId.set(value ? Number(value) : undefined);
  }

  protected onFromDateChange(value: string): void {
    this.filterFromDate.set(value);
  }

  protected onToDateChange(value: string): void {
    this.filterToDate.set(value);
  }

  protected applyFilters(): void {
    this.loadAttendance();
  }

  protected clearFilters(): void {
    this.filterClassGroupId.set(undefined);
    this.filterFromDate.set('');
    this.filterToDate.set('');
    this.loadAttendance();
  }

  protected getStatusBadgeClass(status: string): string {
    switch (status) {
      case 'Present':
        return 'badge-success';
      case 'Absent':
        return 'badge-error';
      case 'Late':
        return 'badge-warning';
      default:
        return 'badge-ghost';
    }
  }
}
