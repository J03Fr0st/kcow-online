# Story 5.1: Attendance Entity & API Endpoints

Status: ready-for-dev

## Story

As a **developer**,
I want **the Attendance domain entity and REST API endpoints**,
so that **attendance data can be tracked per student per session**.

## Acceptance Criteria

1. **Given** the backend project with Student and ClassGroup entities
   **When** the Attendance entity is created
   **Then** the `Attendance` entity exists with properties:
   - Id, StudentId (FK), ClassGroupId (FK), SessionDate, Status (Present/Absent/Late), Notes, CreatedAt, ModifiedAt

2. **And** EF Core configuration with foreign key relationships

3. **And** migration creates the `attendance` table

4. **And** `/api/attendance` endpoints support:
   - GET (list with student/class group/date filters)
   - GET `/:id` (single attendance record)
   - POST (create attendance entry)
   - PUT `/:id` (update attendance - triggers audit)
   - GET `/api/students/:id/attendance` (attendance history for a student)

5. **And** endpoints require authentication

## Tasks / Subtasks

- [ ] Task 1: Create Attendance entity (AC: #1)
  - [ ] Create Attendance.cs with properties
  - [ ] Add AttendanceStatus enum (Present, Absent, Late)
  - [ ] Add foreign keys and navigation
- [ ] Task 2: Configure EF Core (AC: #2, #3)
  - [ ] Configure attendance table
  - [ ] Add indexes on StudentId, SessionDate
  - [ ] Create and apply migration
- [ ] Task 3: Create DTOs (AC: #4)
  - [ ] AttendanceDto with student/class group info
  - [ ] CreateAttendanceRequest, UpdateAttendanceRequest
- [ ] Task 4: Create AttendanceService (AC: #4)
  - [ ] CRUD with filtering
  - [ ] Track modifications for audit
- [ ] Task 5: Create endpoints (AC: #4, #5)
  - [ ] AttendanceController
  - [ ] Nested route on StudentsController

## Dev Notes

### Attendance Entity

```csharp
public class Attendance
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ClassGroupId { get; set; }
    public DateOnly SessionDate { get; set; }
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    
    public Student Student { get; set; } = null!;
    public ClassGroup ClassGroup { get; set; } = null!;
}

public enum AttendanceStatus
{
    Present,
    Absent,
    Late
}
```

### API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/attendance` | List with filters |
| GET | `/api/attendance/{id}` | Single record |
| POST | `/api/attendance` | Create |
| PUT | `/api/attendance/{id}` | Update (audit logged) |
| GET | `/api/students/{id}/attendance` | History for student |

### Previous Story Dependencies

- **Story 4.1** provides: Student entity
- **Story 3.1** provides: ClassGroup entity

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.1]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
