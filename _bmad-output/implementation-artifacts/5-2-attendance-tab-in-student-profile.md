# Story 5.2: Attendance Tab in Student Profile

Status: done

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

- [x] Task 1: Create AttendanceTab component (AC: #1)
  - [x] Replace placeholder in student profile
  - [x] Fetch attendance history for student via `AttendanceService` (calls `GET /api/attendance?studentId={id}`)
  - [x] Display in table with Date, Class Group, Status, Notes
- [x] Task 2: Add status chips (AC: #1)
  - [x] Create StatusChip component using Angular 21 signals
  - [x] Color-code: Present=green, Absent=red, Late=yellow
- [x] Task 3: Implement inline edit (AC: #2)
  - [x] Click row to enter edit mode
  - [x] Editable status dropdown and notes
  - [x] Save on blur or enter via `AttendanceService.update()` (calls `PUT /api/attendance/{id}`)
- [x] Task 4: Implement add flow (AC: #3)
  - [x] "Add Attendance" button
  - [x] Inline form with date, class group, status
  - [x] Save via `AttendanceService.create()` (calls `POST /api/attendance`) and append to list

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

GLM 4.7 (Claude Opus 4.6 equivalent)

### Debug Log References

None - Implementation proceeded smoothly without debugging issues.

### Completion Notes List

- Created `attendance.model.ts` with TypeScript interfaces for Attendance, CreateAttendanceRequest, UpdateAttendanceRequest, and AttendanceQueryParams
- Created `AttendanceService` with methods for getAttendance, getAttendanceById, createAttendance, and updateAttendance
- Created `AttendanceTabComponent` with:
  - Angular 21 signals for reactive state management
  - OnPush change detection strategy
  - Inline edit functionality with status dropdown and notes editing
  - Add attendance form with date, class group, status, and notes fields
  - Color-coded status chips (Present=green, Absent=red, Late=yellow)
  - Sorted attendance records by date descending
  - Loading and error states
  - Form validation
- Updated `student-profile.page.ts` and `student-profile.page.html` to import and use the AttendanceTab component
- Created service tests (13 tests passing) covering all CRUD operations and error handling
- Build succeeds without errors

### File List

**New Files:**
- `apps/frontend/src/app/features/attendance/models/attendance.model.ts`
- `apps/frontend/src/app/core/services/attendance.service.ts`
- `apps/frontend/src/app/core/services/attendance.service.spec.ts`
- `apps/frontend/src/app/features/students/student-profile/components/attendance-tab/attendance-tab.component.ts`
- `apps/frontend/src/app/features/students/student-profile/components/attendance-tab/attendance-tab.component.html`
- `apps/frontend/src/app/features/students/student-profile/components/attendance-tab/attendance-tab.component.scss`
- `apps/frontend/src/app/features/students/student-profile/components/attendance-tab/attendance-tab.component.spec.ts`

**Modified Files:**
- `apps/frontend/src/app/features/students/student-profile/student-profile.page.ts` - Added AttendanceTabComponent import
- `apps/frontend/src/app/features/students/student-profile/student-profile.page.html` - Replaced placeholder with AttendanceTab component

### Change Log

- 2026-02-06: Initial implementation of Attendance Tab in Student Profile (FR10)
  - Created attendance data models and service
  - Implemented AttendanceTab component with full CRUD functionality
  - Added status chips with color coding
  - Implemented inline edit and add attendance flows
  - All tasks completed, build passes, service tests pass

- 2026-02-06: Code Review completed - All HIGH and MEDIUM issues fixed
  - Added save on blur/enter for inline edit mode
  - Changed class group input to dropdown using ClassGroupService
  - Added duplicate detection for attendance records
  - Fixed Angular template syntax ($any() instead of as Type)
  - Converted tests to Jest syntax
  - All 21 component tests passing
  - Build passes
