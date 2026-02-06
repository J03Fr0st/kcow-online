# Story 5.4: Evaluation Entity & API Endpoints

Status: ready-for-dev

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

- [ ] Task 1: Create Evaluation entity (AC: #1)
  - [ ] Create `Evaluation.cs` in `Domain/Entities/` with all properties
  - [ ] No navigation properties -- plain POCO entity (no `Student` or `Activity` references)
  - [ ] Note: Activity entity already exists from Epic 8 with Code, Name, Description, Folder, GradeLevel, Icon fields
- [ ] Task 2: Create DbUp migration script (AC: #2)
  - [ ] Create `Infrastructure/Migrations/Scripts/012_CreateEvaluation.sql`
  - [ ] Define `evaluations` table with FKs to `students` and `activities`
  - [ ] Add indexes on `student_id`, `activity_id`, and `evaluation_date`
- [ ] Task 3: Create Evaluation repository (AC: #3)
  - [ ] Create `IEvaluationRepository` interface in `Application/Interfaces/`
  - [ ] Create `EvaluationRepository` in `Infrastructure/Repositories/` using Dapper + `IDbConnectionFactory`
  - [ ] Methods: `GetAllAsync`, `GetByIdAsync`, `GetByStudentIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`
  - [ ] Use explicit SQL JOINs to fetch related Student/Activity names
  - [ ] Register in `Infrastructure/DependencyInjection.cs`
- [ ] Task 4: Create DTOs (AC: #3, #4)
  - [ ] `EvaluationDto` (includes student name, activity name from JOINs)
  - [ ] `CreateEvaluationRequest`, `UpdateEvaluationRequest`
- [ ] Task 5: Create EvaluationService and EvaluationController (AC: #3, #4)
  - [ ] `IEvaluationService` in `Application/Interfaces/`
  - [ ] `EvaluationService` in `Infrastructure/Evaluation/` using `IEvaluationRepository`
  - [ ] `EvaluationController` with full CRUD endpoints
  - [ ] GET `/api/students/{id}/evaluations` for student evaluation history
  - [ ] Register service in `Infrastructure/DependencyInjection.cs`

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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
