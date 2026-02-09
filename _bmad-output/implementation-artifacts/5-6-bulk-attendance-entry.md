# Story 5.6: Bulk Attendance Entry

Status: done

## Story

As an **admin**,
I want **to record attendance for an entire class group session at once**,
so that **I can efficiently mark attendance for a class**.

## Acceptance Criteria

1. **Given** I am on the Class Groups page
   **When** I click "Take Attendance" on a class group
   **Then** I see a list of all students in the class
   **And** each student has a status toggle (Present/Absent/Late)

2. **Given** I mark attendance for all students
   **When** I click "Save All"
   **Then** attendance records are created for all students
   **And** I see a success confirmation

3. **And** existing attendance for the same date shows pre-filled values

## Tasks / Subtasks

- [x] Task 1: Add "Take Attendance" action (AC: #1)
  - [x] Add button to class group row or detail view
  - [x] Navigate to bulk attendance view
- [x] Task 2: Create BulkAttendance component (AC: #1)
  - [x] Load students in class group via Angular service
  - [x] Display list with name and status toggle using Angular Signals for state
  - [x] Default all to "Present"
- [x] Task 3: Check for existing attendance (AC: #3)
  - [x] Load attendance for selected date via Attendance API
  - [x] Pre-fill existing statuses using Signal-based form state
  - [x] Indicate which are updates vs new
- [x] Task 4: Implement batch attendance endpoint (AC: #2)
  - [x] Create batch endpoint in `AttendanceController`
  - [x] Implement batch insert/update in `IAttendanceRepository` using Dapper
  - [x] Use `IDbTransaction` for atomic batch operations (all-or-nothing save)
  - [x] Validate all entries before committing transaction
  - [x] Register any new services in `DependencyInjection.cs`
- [x] Task 5: Implement bulk save on frontend (AC: #2)
  - [x] Call batch attendance endpoint with all student entries
  - [x] Show progress indicator and success confirmation
  - [x] Handle partial failure responses

## Dev Notes

### Bulk Attendance Layout

```
+-------------------------------------------------------------+
| Take Attendance: Class 5A - Monday 2026-01-03                |
| Date: [2026-01-03 v]                                         |
+-------------------------------------------------------------+
| Student          | Present | Absent | Late | Notes           |
| John Smith       |   (o)   |   o    |  o   | [_______]       |
| Jane Doe         |   o     |  (o)   |  o   | [Sick_____]     |
| Tom Wilson       |   o     |   o    | (o)  | [Late 10min]    |
+-------------------------------------------------------------+
|                                           [Cancel] [Save All] |
+-------------------------------------------------------------+
```

### Batch Attendance API

```
POST /api/class-groups/{id}/attendance
Body: { sessionDate: "2026-01-03", entries: [...] }
```

### Backend Architecture (Dapper + DbUp)

- **Repository**: `IAttendanceRepository` in `Application/Interfaces/` with batch method
- **Implementation**: `AttendanceRepository` in `Infrastructure/Repositories/` using `IDbConnectionFactory` and Dapper
- **Transaction handling**: Use `IDbTransaction` for atomic batch insert/update
  ```csharp
  using var connection = _connectionFactory.CreateConnection();
  connection.Open();
  using var transaction = connection.BeginTransaction();
  // Dapper operations with transaction parameter
  // INSERT INTO attendance (student_id, class_group_id, session_date, status, notes, ...)
  transaction.Commit();
  ```
- **SQL**: Use parameterized queries with snake_case column names
- **No EF Core**: All data access via Dapper with explicit SQL

### Frontend Architecture (Angular 21)

- **State management**: Use Angular Signals for bulk form state
- **Toggle state**: Signal array tracking each student's attendance status
- **API calls**: RxJS-based service with `toSignal()` where appropriate

### Previous Story Dependencies

- **Story 5.1** provides: Attendance API (Dapper-based with `IAttendanceRepository`)
- **Story 3.2** provides: Class groups with student roster

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.6]

## Dev Agent Record

### Agent Model Used

claude-opus-4-6

### Debug Log References

### Completion Notes List

- Implemented bulk attendance entry feature with Angular Signals for state management
- Added "Take Attendance" button to class groups list
- Created BulkAttendance component with date selector, student list, and status toggles
- Implemented batch attendance API endpoint with atomic transaction support
- Added validation for status values and error handling
- Supports both creating new attendance records and updating existing ones
- Shows pre-filled values when attendance already exists for the selected date
- Uses OnPush change detection for optimal performance
- Integrated with existing audit logging system

### File List

**Frontend:**
- apps/frontend/src/app/features/class-groups/class-groups-list/class-groups-list.component.ts (added takingAttendanceId signal and openTakeAttendance/closeTakeAttendance methods)
- apps/frontend/src/app/features/class-groups/class-groups-list/class-groups-list.component.html (added Take Attendance button and bulk attendance modal)
- apps/frontend/src/app/features/class-groups/bulk-attendance/bulk-attendance.component.ts (new)
- apps/frontend/src/app/features/class-groups/bulk-attendance/bulk-attendance.component.html (new)

**Backend:**
- apps/backend/src/Application/Attendance/BatchAttendanceRequest.cs (new)
- apps/backend/src/Application/Attendance/IAttendanceService.cs (added BatchSaveAsync method)
- apps/backend/src/Application/Interfaces/IAttendanceRepository.cs (added BatchSaveAsync method)
- apps/backend/src/Infrastructure/Attendance/AttendanceService.cs (implemented BatchSaveAsync with validation and audit logging)
- apps/backend/src/Infrastructure/Repositories/AttendanceRepository.cs (implemented BatchSaveAsync with transaction support and CancellationToken forwarding)
- apps/backend/src/Api/Controllers/ClassGroupsController.cs (added batch attendance endpoint POST {id}/attendance)

**Tests:**
- apps/backend/tests/Unit/AttendanceServiceTests.cs (added 4 BatchSaveAsync unit tests)
- apps/backend/tests/Integration/Attendance/AttendanceControllerTests.cs (added 4 batch attendance integration tests)

## Change Log

- 2026-02-06: Implemented bulk attendance entry feature (Story 5.6)
- 2026-02-09: Code review fixes - removed $any(), fixed ChangeDetectionStrategy import, removed duplicate error alert, removed unused FormsModule, fixed catchError returning of() to EMPTY, forwarded CancellationToken to Dapper CommandDefinition, added batch attendance unit and integration tests
