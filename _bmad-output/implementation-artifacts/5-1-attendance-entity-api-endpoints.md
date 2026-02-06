# Story 5.1: Attendance Entity & API Endpoints

Status: done

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

- [x] Task 1: Create Attendance entity (AC: #1)
  - [x] Create `Attendance.cs` in `Domain/Entities/`
  - [x] Add `AttendanceStatus` enum (Present, Absent, Late)
  - [x] Add foreign key fields (StudentId, ClassGroupId) - no navigation properties (Dapper)
- [x] Task 2: Create DbUp migration script (AC: #3)
  - [x] Create `011_CreateAttendance.sql` in `Infrastructure/Migrations/Scripts/`
  - [x] Add indexes on student_id, session_date, class_group_id
  - [x] Add composite index on (student_id, session_date)
  - [x] Note: `010_CreateAttendance.sql` already exists - verified; existing script already meets all requirements
- [x] Task 3: Create DTOs (AC: #4)
  - [x] `AttendanceDto` in `Application/Attendance/` with student/class group info
  - [x] `CreateAttendanceRequest`, `UpdateAttendanceRequest`
- [x] Task 4: Create repository interface and implementation (AC: #2)
  - [x] `IAttendanceRepository` in `Application/Interfaces/`
  - [x] `AttendanceRepository` in `Infrastructure/Repositories/` using Dapper
  - [x] Use `IDbConnectionFactory` for connections
  - [x] Use parameterized SQL with `QueryAsync<T>`, `QueryFirstOrDefaultAsync<T>`, `ExecuteAsync`
  - [x] Include filtering by StudentId, ClassGroupId, date range
  - [x] Note: `IAttendanceRepository` and `AttendanceRepository` already exist - verified and functional
- [x] Task 5: Create AttendanceService (AC: #4)
  - [x] `IAttendanceService` in `Application/Attendance/`
  - [x] `AttendanceService` in `Infrastructure/Attendance/` using `IAttendanceRepository`
  - [x] CRUD with filtering, track modifications for audit
- [x] Task 6: Create endpoints (AC: #4, #5)
  - [x] `AttendanceController` in `Api/Controllers/`
  - [x] Nested route on `StudentsController` for `/api/students/:id/attendance`
  - [x] Add `[Authorize]` attribute
  - [x] Return ProblemDetails for errors
- [x] Task 7: Register services in DI (AC: #2)
  - [x] Register `IAttendanceRepository`/`AttendanceRepository` in `DependencyInjection.cs`
  - [x] Register `IAttendanceService`/`AttendanceService` in `DependencyInjection.cs`
  - [x] Note: Registrations already existed - verified correct

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

Claude Opus 4.6

### Debug Log References

- Investigated integration test failures (5 of 12 failing) - root cause: Dapper `DefaultTypeMap.MatchNamesWithUnderscores` was `false`, preventing snake_case DB columns from mapping to PascalCase C# properties when re-fetching records via JOIN queries
- Fix: Added `DefaultTypeMap.MatchNamesWithUnderscores = true` in DependencyInjection.cs - improved overall test suite from 66/28 pass/fail to 77/17 pass/fail (fixed 11 additional tests beyond attendance)
- Removed navigation properties (Student, ClassGroup) from Attendance entity per Dapper pattern compliance (Task 1 spec: "no navigation properties")

### Completion Notes List

- All attendance implementation files were already present from a previous session
- Fixed critical Dapper snake_case mapping bug that prevented correct data retrieval from JOIN queries
- All 15 unit tests pass (AttendanceServiceTests)
- All 12 integration tests pass (AttendanceControllerTests) including: CRUD, filtering, auth, status validation, audit trail, nested route, all status types
- Remaining 17 integration test failures and 3 unit test failures are pre-existing (ClassGroups, Auth, Import, Families) - confirmed by running tests against clean main branch

### Senior Developer Review (AI)

**Reviewed:** 2026-02-06 | **Reviewer:** Joe (AI-assisted)
**Result:** Approved with fixes applied

**Issues Found & Fixed:**
1. [HIGH] Fragile enum cast in MapToDto - added Enum.IsDefined guard for out-of-range status int values
2. [HIGH] CancellationToken not propagated through service layer - added to IAttendanceService, AttendanceService, AttendanceController, StudentsController
3. [MEDIUM] [Required] on int value types (StudentId, ClassGroupId) always passes - changed to [Range(1, int.MaxValue)]
4. [MEDIUM] No SessionDate format validation - added [RegularExpression] for ISO date format (YYYY-MM-DD)

**Noted (not fixed - codebase-wide patterns):**
- Controller generic Exception catch blocks (matches existing pattern in ActivitiesController, etc.)
- Duplicated CreateServerErrorProblemDetails helper across controllers
- Story Dev Notes entity spec inconsistent with actual implementation types

**All 15 unit tests pass after fixes.**

### Change Log

- 2026-02-06: Code review fixes: CancellationToken propagation, enum cast guard, request validation improvements
- 2026-02-06: Fixed Dapper snake_case mapping (MatchNamesWithUnderscores=true), removed navigation properties from Attendance entity, verified all ACs met with passing tests

### File List

- `apps/backend/src/Domain/Entities/Attendance.cs` - Entity + AttendanceStatus enum (modified: removed navigation properties)
- `apps/backend/src/Application/Interfaces/IAttendanceRepository.cs` - Repository interface + AttendanceWithNames helper
- `apps/backend/src/Application/Attendance/IAttendanceService.cs` - Service interface
- `apps/backend/src/Application/Attendance/AttendanceDto.cs` - DTO with student/class group names
- `apps/backend/src/Application/Attendance/CreateAttendanceRequest.cs` - Create request with validation
- `apps/backend/src/Application/Attendance/UpdateAttendanceRequest.cs` - Update request with validation
- `apps/backend/src/Infrastructure/Repositories/AttendanceRepository.cs` - Dapper repository with JOINs and filtering
- `apps/backend/src/Infrastructure/Attendance/AttendanceService.cs` - Service with CRUD, status validation, audit tracking
- `apps/backend/src/Infrastructure/Migrations/Scripts/010_CreateAttendance.sql` - Migration with indexes
- `apps/backend/src/Infrastructure/DependencyInjection.cs` - DI registrations + Dapper MatchNamesWithUnderscores fix (modified)
- `apps/backend/src/Api/Controllers/AttendanceController.cs` - REST controller with [Authorize]
- `apps/backend/src/Api/Controllers/StudentsController.cs` - Nested route GET /api/students/{id}/attendance
- `apps/backend/tests/Unit/AttendanceServiceTests.cs` - 15 unit tests
- `apps/backend/tests/Integration/Attendance/AttendanceControllerTests.cs` - 12 integration tests
