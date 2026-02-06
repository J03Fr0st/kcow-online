# Story 5.6: Bulk Attendance Entry

Status: ready-for-dev

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

- [ ] Task 1: Add "Take Attendance" action (AC: #1)
  - [ ] Add button to class group row or detail view
  - [ ] Navigate to bulk attendance view
- [ ] Task 2: Create BulkAttendance component (AC: #1)
  - [ ] Load students in class group via Angular service
  - [ ] Display list with name and status toggle using Angular Signals for state
  - [ ] Default all to "Present"
- [ ] Task 3: Check for existing attendance (AC: #3)
  - [ ] Load attendance for selected date via Attendance API
  - [ ] Pre-fill existing statuses using Signal-based form state
  - [ ] Indicate which are updates vs new
- [ ] Task 4: Implement batch attendance endpoint (AC: #2)
  - [ ] Create batch endpoint in `AttendanceController`
  - [ ] Implement batch insert/update in `IAttendanceRepository` using Dapper
  - [ ] Use `IDbTransaction` for atomic batch operations (all-or-nothing save)
  - [ ] Validate all entries before committing transaction
  - [ ] Register any new services in `DependencyInjection.cs`
- [ ] Task 5: Implement bulk save on frontend (AC: #2)
  - [ ] Call batch attendance endpoint with all student entries
  - [ ] Show progress indicator and success confirmation
  - [ ] Handle partial failure responses

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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
