# Story 5.1: Attendance Entity & API Endpoints

Status: in-progress

## Story

As a **developer**,
I want **the Attendance domain entity and REST API endpoints**,
so that **attendance data can be tracked per student per session**.

## Acceptance Criteria

1. **Given** the backend project with Student and ClassGroup entities
   **When** the Attendance entity is created
   **Then** the `Attendance` entity exists in `Domain/Entities/` with properties:
   - Id, StudentId (FK), ClassGroupId (FK), SessionDate, Status (Present/Absent/Late), Notes, CreatedAt, ModifiedAt

2. **And** Dapper repository with `IAttendanceRepository` interface in `Application/Interfaces/` and implementation in `Infrastructure/Repositories/`

3. **And** DbUp migration script creates the `attendance` table with indexes on StudentId, SessionDate, ClassGroupId

4. **And** `/api/attendance` endpoints support:
   - GET (list with student/class group/date filters)
   - GET `/:id` (single attendance record)
   - POST (create attendance entry)
   - PUT `/:id` (update attendance - triggers audit)
   - GET `/api/students/:id/attendance` (attendance history for a student)

5. **And** endpoints require authentication

## Tasks / Subtasks

- [ ] Task 1: Create Attendance entity (AC: #1)
  - [ ] Create `Attendance.cs` in `Domain/Entities/`
  - [ ] Add `AttendanceStatus` enum (Present, Absent, Late)
  - [ ] Add foreign key fields (StudentId, ClassGroupId) - no navigation properties (Dapper)
- [ ] Task 2: Create DbUp migration script (AC: #3)
  - [ ] Create `011_CreateAttendance.sql` in `Infrastructure/Migrations/Scripts/`
  - [ ] Add indexes on student_id, session_date, class_group_id
  - [ ] Add composite index on (student_id, session_date)
  - [ ] Note: `010_CreateAttendance.sql` already exists - verify and extend or replace as needed
- [ ] Task 3: Create DTOs (AC: #4)
  - [ ] `AttendanceDto` in `Application/Attendance/` with student/class group info
  - [ ] `CreateAttendanceRequest`, `UpdateAttendanceRequest`
- [ ] Task 4: Create repository interface and implementation (AC: #2)
  - [ ] `IAttendanceRepository` in `Application/Interfaces/`
  - [ ] `AttendanceRepository` in `Infrastructure/Repositories/` using Dapper
  - [ ] Use `IDbConnectionFactory` for connections
  - [ ] Use parameterized SQL with `QueryAsync<T>`, `QueryFirstOrDefaultAsync<T>`, `ExecuteAsync`
  - [ ] Include filtering by StudentId, ClassGroupId, date range
  - [ ] Note: `IAttendanceRepository` and `AttendanceRepository` already exist - verify and extend
- [ ] Task 5: Create AttendanceService (AC: #4)
  - [ ] `IAttendanceService` in `Application/Attendance/`
  - [ ] `AttendanceService` in `Infrastructure/Attendance/` using `IAttendanceRepository`
  - [ ] CRUD with filtering, track modifications for audit
- [ ] Task 6: Create endpoints (AC: #4, #5)
  - [ ] `AttendanceController` in `Api/Controllers/`
  - [ ] Nested route on `StudentsController` for `/api/students/:id/attendance`
  - [ ] Add `[Authorize]` attribute
  - [ ] Return ProblemDetails for errors
- [ ] Task 7: Register services in DI (AC: #2)
  - [ ] Register `IAttendanceRepository`/`AttendanceRepository` in `DependencyInjection.cs`
  - [ ] Register `IAttendanceService`/`AttendanceService` in `DependencyInjection.cs`
  - [ ] Note: Some registrations may already exist - verify

## Dev Notes

### Attendance Entity

```csharp
namespace Kcow.Domain.Entities;

public class Attendance
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ClassGroupId { get; set; }
    public string SessionDate { get; set; } = string.Empty; // ISO 8601 date string (TEXT in SQLite)
    public int Status { get; set; } // 0=Present, 1=Absent, 2=Late
    public string? Notes { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
    public string? ModifiedAt { get; set; }
}

public enum AttendanceStatus
{
    Present = 0,
    Absent = 1,
    Late = 2
}
```

### Repository Pattern (follow ActivityRepository)

```csharp
public class AttendanceRepository : IAttendanceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AttendanceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Attendance>> GetByStudentIdAsync(int studentId, CancellationToken ct = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, student_id, class_group_id, session_date, status, notes, created_at, modified_at
            FROM attendance
            WHERE student_id = @StudentId
            ORDER BY session_date DESC";
        return await connection.QueryAsync<Attendance>(sql, new { StudentId = studentId });
    }
}
```

### DbUp Migration Script (010_CreateAttendance.sql already exists)

Verify existing script at `Infrastructure/Migrations/Scripts/010_CreateAttendance.sql` matches schema requirements.

### API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/attendance` | List with filters |
| GET | `/api/attendance/{id}` | Single record |
| POST | `/api/attendance` | Create |
| PUT | `/api/attendance/{id}` | Update (audit logged) |
| GET | `/api/students/{id}/attendance` | History for student |

### Architecture Compliance

- **Repository Pattern**: Follow `ActivityRepository` / `IActivityRepository` pattern exactly
- **Service Pattern**: Follow `ActivityService` / `IActivityService` pattern
- **Controller Pattern**: Follow `ActivitiesController` pattern
- **DI Registration**: Register in `Infrastructure/DependencyInjection.cs`
- **SQL**: Use parameterized queries exclusively, `snake_case` column names
- **Connection**: `IDbConnectionFactory` creates connections, disposed after each operation

### Previous Story Dependencies

- **Story 4.1** provides: Student entity and repository
- **Story 3.1** provides: ClassGroup entity and repository
- **Story 0.1** provides: Dapper + DbUp infrastructure

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.1]
- [Source: _bmad-output/planning-artifacts/architecture.md#Data-Access-Patterns]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
