# Story 5.4: Evaluation Entity & API Endpoints

Status: done

## Story

As a **developer**,
I want **the Evaluation domain entity and REST API endpoints**,
so that **student progress evaluations can be recorded**.

## Acceptance Criteria

1. **Given** the backend project with Student and Activity entities
   **When** the Evaluation entity is created
   **Then** the `Evaluation` entity exists with properties:
   - Id, StudentId (FK), ActivityId (FK), EvaluationDate, Score, SpeedMetric, AccuracyMetric, Notes, CreatedAt, ModifiedAt

2. **And** a DbUp migration script creates the `evaluations` table with foreign keys to `students` and `activities`

3. **And** `/api/evaluations` endpoints support CRUD via Dapper repository

4. **And** GET `/api/students/:id/evaluations` returns evaluation history

## Tasks / Subtasks

- [x] Task 1: Create Evaluation entity (AC: #1)
  - [x] Create `Evaluation.cs` in `Domain/Entities/` with all properties
  - [x] No navigation properties -- plain POCO entity (no `Student` or `Activity` references)
  - [x] Note: Activity entity already exists from Epic 8 with Code, Name, Description, Folder, GradeLevel, Icon fields
- [x] Task 2: Create DbUp migration script (AC: #2)
  - [x] Create `Infrastructure/Migrations/Scripts/012_CreateEvaluation.sql`
  - [x] Define `evaluations` table with FKs to `students` and `activities`
  - [x] Add indexes on `student_id`, `activity_id`, and `evaluation_date`
- [x] Task 3: Create Evaluation repository (AC: #3)
  - [x] Create `IEvaluationRepository` interface in `Application/Interfaces/`
  - [x] Create `EvaluationRepository` in `Infrastructure/Repositories/` using Dapper + `IDbConnectionFactory`
  - [x] Methods: `GetAllAsync`, `GetByIdAsync`, `GetByStudentIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`
  - [x] Use explicit SQL JOINs to fetch related Student/Activity names
  - [x] Register in `Infrastructure/DependencyInjection.cs`
- [x] Task 4: Create DTOs (AC: #3, #4)
  - [x] `EvaluationDto` (includes student name, activity name from JOINs)
  - [x] `CreateEvaluationRequest`, `UpdateEvaluationRequest`
- [x] Task 5: Create EvaluationService and EvaluationController (AC: #3, #4)
  - [x] `IEvaluationService` in `Application/Interfaces/`
  - [x] `EvaluationService` in `Infrastructure/Evaluation/` using `IEvaluationRepository`
  - [x] `EvaluationController` with full CRUD endpoints
  - [x] GET `/api/students/{id}/evaluations` for student evaluation history
  - [x] Register service in `Infrastructure/DependencyInjection.cs`

## Dev Notes

### Architecture Compliance

This story follows the project's **Dapper + DbUp** architecture (established in Story 0.1):
- **No EF Core** -- all data access uses Dapper via `IDbConnectionFactory`
- **Repository pattern**: `IEvaluationRepository` in `Application/Interfaces/`, `EvaluationRepository` in `Infrastructure/Repositories/`
- **DbUp migration**: SQL script in `Infrastructure/Migrations/Scripts/` with sequential numbering
- **Parameterized SQL only**, snake_case column names
- **No navigation properties** -- use explicit SQL JOINs to fetch related entity names
- **DI registration** in `Infrastructure/DependencyInjection.cs`

### Activity Entity (Already Exists -- DO NOT CREATE)

The Activity entity was created in Epic 8 and already exists at `Domain/Entities/Activity.cs` with fields:
- Id, Code, Name, Description, Folder, GradeLevel, Icon, IsActive, CreatedAt, UpdatedAt

The `IActivityRepository` and `ActivityRepository` also already exist. No Activity work is needed in this story.

### DbUp Migration Script (012_CreateEvaluation.sql)

```sql
-- Create Evaluation table for tracking student progress evaluations
CREATE TABLE IF NOT EXISTS "evaluations" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_evaluations" PRIMARY KEY AUTOINCREMENT,
    "student_id" INTEGER NOT NULL,
    "activity_id" INTEGER NOT NULL,
    "evaluation_date" TEXT NOT NULL,
    "score" INTEGER NULL,
    "speed_metric" REAL NULL,
    "accuracy_metric" REAL NULL,
    "notes" TEXT NULL,
    "created_at" TEXT NOT NULL DEFAULT (datetime('now')),
    "modified_at" TEXT NULL,
    CONSTRAINT "FK_evaluations_students_student_id" FOREIGN KEY ("student_id") REFERENCES "students" ("id") ON DELETE CASCADE,
    CONSTRAINT "FK_evaluations_activities_activity_id" FOREIGN KEY ("activity_id") REFERENCES "activities" ("id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_evaluations_student_id" ON "evaluations" ("student_id");
CREATE INDEX IF NOT EXISTS "IX_evaluations_activity_id" ON "evaluations" ("activity_id");
CREATE INDEX IF NOT EXISTS "IX_evaluations_evaluation_date" ON "evaluations" ("evaluation_date");
CREATE INDEX IF NOT EXISTS "IX_evaluations_student_activity" ON "evaluations" ("student_id", "activity_id");
```

### Evaluation Entity (Plain POCO -- no navigation properties)

```csharp
namespace Kcow.Domain.Entities;

public class Evaluation
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ActivityId { get; set; }
    public string EvaluationDate { get; set; } = string.Empty; // Stored as ISO date string (YYYY-MM-DD)
    public int? Score { get; set; }
    public decimal? SpeedMetric { get; set; }
    public decimal? AccuracyMetric { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
```

### Repository Pattern (Dapper)

```csharp
// Application/Interfaces/IEvaluationRepository.cs
public interface IEvaluationRepository
{
    Task<IEnumerable<EvaluationWithNames>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EvaluationWithNames?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<EvaluationWithNames>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(Evaluation evaluation, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Evaluation evaluation, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}

// Projection class for JOIN results
public class EvaluationWithNames
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string? StudentFirstName { get; set; }
    public string? StudentLastName { get; set; }
    public int ActivityId { get; set; }
    public string? ActivityName { get; set; }
    public string EvaluationDate { get; set; } = string.Empty;
    public int? Score { get; set; }
    public decimal? SpeedMetric { get; set; }
    public decimal? AccuracyMetric { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
```

### Example Repository Query (Dapper pattern)

```csharp
public async Task<EvaluationWithNames?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
{
    using var connection = _connectionFactory.Create();
    const string sql = @"
        SELECT e.id, e.student_id, s.first_name AS student_first_name, s.last_name AS student_last_name,
               e.activity_id, a.name AS activity_name,
               e.evaluation_date, e.score, e.speed_metric, e.accuracy_metric,
               e.notes, e.created_at, e.modified_at
        FROM evaluations e
        INNER JOIN students s ON e.student_id = s.id
        INNER JOIN activities a ON e.activity_id = a.id
        WHERE e.id = @Id";
    return await connection.QueryFirstOrDefaultAsync<EvaluationWithNames>(sql, new { Id = id });
}
```

### Previous Story Dependencies

- **Story 4.1** provides: Student entity
- **Epic 8** provides: Activity entity, IActivityRepository, ActivityRepository (already complete)

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.4]
- [Source: docs/legacy/3_Activity/Activity.xsd]

## Dev Agent Record

### Agent Model Used

glm-4.7 (Claude Opus 4.6 compatible)

### Debug Log References

### Completion Notes List

**Implementation Summary:**
- Created Evaluation entity as plain POCO with all required properties (Id, StudentId, ActivityId, EvaluationDate, Score, SpeedMetric, AccuracyMetric, Notes, CreatedAt, ModifiedAt)
- Created DbUp migration script (012_CreateEvaluation.sql) with proper FKs to students and activities tables, plus indexes for performance
- Implemented IEvaluationRepository and EvaluationRepository following Dapper pattern with explicit SQL JOINs
- Created EvaluationDto, CreateEvaluationRequest, and UpdateEvaluationRequest DTOs with proper validation
- Implemented IEvaluationService and EvaluationService with audit logging support for create and update operations
- Created EvaluationController with full CRUD endpoints and student evaluation history endpoint
- Registered repository and service in DependencyInjection.cs
- Wrote 19 unit tests for EvaluationService covering all CRUD operations and edge cases

**All acceptance criteria met:**
1. ✅ Evaluation entity exists with all specified properties
2. ✅ DbUp migration script creates evaluations table with FKs to students and activities
3. ✅ /api/evaluations endpoints support CRUD via Dapper repository
4. ✅ GET /api/students/{id}/evaluations returns evaluation history

**Tests Passing:** 21/21 EvaluationServiceTests (added 2 new tests for FK validation)

### Code Review Fixes Applied

**Issues Found and Fixed:**
1. ✅ **HIGH:** Added audit logging to DeleteAsync - now logs who deleted which evaluation record
2. ✅ **MEDIUM:** Added FK validation before create - now throws InvalidOperationException with clear message when StudentId or ActivityId don't exist
3. ✅ **MEDIUM:** Skip update when no changes detected - now skips database write if no actual changes

**Changes Made:**
- Updated `IEvaluationService.DeleteAsync` signature to include `deletedBy` parameter
- Updated `EvaluationService` to inject `IStudentRepository` and `IActivityRepository` for FK validation
- Added `InvalidOperationException` handling in `EvaluationController.Create` for proper 400 responses
- Modified `UpdateAsync` to return early when `changes.Count == 0`
- Updated 21 unit tests to include new dependencies and validation tests

### File List

**New Files Created:**
- apps/backend/src/Domain/Entities/Evaluation.cs
- apps/backend/src/Infrastructure/Migrations/Scripts/012_CreateEvaluation.sql
- apps/backend/src/Application/Interfaces/IEvaluationRepository.cs
- apps/backend/src/Infrastructure/Repositories/EvaluationRepository.cs
- apps/backend/src/Application/Evaluations/EvaluationDto.cs
- apps/backend/src/Application/Evaluations/CreateEvaluationRequest.cs
- apps/backend/src/Application/Evaluations/UpdateEvaluationRequest.cs
- apps/backend/src/Application/Evaluations/IEvaluationService.cs
- apps/backend/src/Infrastructure/Evaluations/EvaluationService.cs
- apps/backend/src/Api/Controllers/EvaluationController.cs
- apps/backend/tests/Unit/EvaluationServiceTests.cs

**Modified Files:**
- apps/backend/src/Infrastructure/DependencyInjection.cs
