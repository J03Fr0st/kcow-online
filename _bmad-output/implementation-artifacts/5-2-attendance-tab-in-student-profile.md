# Story 5.2: Attendance Tab in Student Profile

Status: ready-for-dev

## Story

As an **admin**,
I want **to view and manage attendance in the student profile**,
so that **I can track and update attendance without leaving the profile (FR10)**.

## Acceptance Criteria

1. **Given** I am on the student profile
   **When** I select the "Attendance" tab
   **Then** I see a list of recent attendance records with Date, Class Group, Status, Notes columns
   **And** status is shown with color-coded chips (Present=green, Absent=red, Late=yellow)

2. **Given** I click on an attendance row
   **When** inline edit mode activates
   **Then** I can change the status or add notes
   **And** save the change immediately

3. **Given** I click "Add Attendance"
   **When** the inline form appears
   **Then** I can select date, class group, and status
   **And** the new record appears in the list

## Tasks / Subtasks

- [ ] Task 1: Create AttendanceTab component (AC: #1)
  - [ ] Replace placeholder in student profile
  - [ ] Fetch attendance history for student via `AttendanceService` (calls `GET /api/attendance?studentId={id}`)
  - [ ] Display in table with Date, Class Group, Status, Notes
- [ ] Task 2: Add status chips (AC: #1)
  - [ ] Create StatusChip component using Angular 21 signals
  - [ ] Color-code: Present=green, Absent=red, Late=yellow
- [ ] Task 3: Implement inline edit (AC: #2)
  - [ ] Click row to enter edit mode
  - [ ] Editable status dropdown and notes
  - [ ] Save on blur or enter via `AttendanceService.update()` (calls `PUT /api/attendance/{id}`)
- [ ] Task 4: Implement add flow (AC: #3)
  - [ ] "Add Attendance" button
  - [ ] Inline form with date, class group, status
  - [ ] Save via `AttendanceService.create()` (calls `POST /api/attendance`) and append to list

## Dev Notes

### Architecture Note

This is a **frontend-only story**. The backend Attendance API already exists (Story 5.1) with:
- `AttendanceController` serving REST endpoints
- `IAttendanceRepository` / `AttendanceRepository` using Dapper + `IDbConnectionFactory`
- `010_CreateAttendance.sql` DbUp migration script

All API calls go through the existing attendance service which uses the Dapper repository on the backend. No backend changes are needed for this story.

### Attendance Tab Layout

```
+-------------------------------------------------------------+
| Attendance History                        [Add Attendance]   |
+-------------------------------------------------------------+
| Date       | Class Group | Status      | Notes              |
| 2026-01-02 | Class 5A    | [Present]   | On time            |
| 2026-01-01 | Class 5A    | [Late]      | Arrived 10min late |
| 2025-12-31 | Class 5A    | [Absent]    | Sick               |
+-------------------------------------------------------------+
```

### Status Chip Component (Angular 21 Signals Pattern)

```typescript
import { Component, input, computed } from '@angular/core';
import { AttendanceStatus } from '../models/attendance.model';

@Component({
  selector: 'app-status-chip',
  standalone: true,
  template: `
    <span class="badge" [class]="statusClass()">
      {{ status() }}
    </span>
  `
})
export class StatusChipComponent {
  status = input.required<AttendanceStatus>();

  statusClass = computed(() => {
    switch (this.status()) {
      case 'Present': return 'badge-success';
      case 'Absent': return 'badge-error';
      case 'Late': return 'badge-warning';
      default: return '';
    }
  });
}
```

### Previous Story Dependencies

- **Story 4.6** provides: Profile tabs
- **Story 5.1** provides: Attendance API (Dapper + DbUp backend)

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.2]
- [Source: _bmad-output/planning-artifacts/ux-design-specification.md#Status Chips]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
