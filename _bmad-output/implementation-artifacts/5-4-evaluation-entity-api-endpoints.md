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

2. **And** Activity entity exists with basic activity/curriculum info

3. **And** EF Core configuration and migrations

4. **And** `/api/evaluations` endpoints support CRUD

5. **And** GET `/api/students/:id/evaluations` returns evaluation history

## Tasks / Subtasks

- [ ] Task 1: Create Activity entity (AC: #2)
  - [ ] Create Activity.cs with Id, Name, Description, Category
  - [ ] Configure EF Core
- [ ] Task 2: Create Evaluation entity (AC: #1)
  - [ ] Create Evaluation.cs with all properties
  - [ ] Add FKs to Student and Activity
- [ ] Task 3: Configure EF Core and migrate (AC: #3)
  - [ ] Configure both tables
  - [ ] Create and apply migration
- [ ] Task 4: Create DTOs (AC: #4, #5)
  - [ ] EvaluationDto, CreateEvaluationRequest
  - [ ] ActivityDto
- [ ] Task 5: Create services and endpoints (AC: #4, #5)
  - [ ] EvaluationService, EvaluationController
  - [ ] ActivityService, ActivityController (basic CRUD)

## Dev Notes

### Activity Entity

```csharp
public class Activity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### Evaluation Entity

```csharp
public class Evaluation
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ActivityId { get; set; }
    public DateOnly EvaluationDate { get; set; }
    public int? Score { get; set; }
    public decimal? SpeedMetric { get; set; }
    public decimal? AccuracyMetric { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    
    public Student Student { get; set; } = null!;
    public Activity Activity { get; set; } = null!;
}
```

### Previous Story Dependencies

- **Story 4.1** provides: Student entity

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.4]
- [Source: docs/legacy/3_Activity/Activity.xsd]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
