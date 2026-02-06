# Story 7.4: Import Execution with Exception Handling

Status: ready-for-dev

## Story

As a **developer**,
I want **to run the full import with proper error handling**,
so that **valid records are imported and failures are tracked**.

## Acceptance Criteria

1. **Given** I run the import command without `--preview`
   **When** the import executes
   **Then** valid records are inserted into the database
   **And** failed records are logged to an exceptions file (JSON or CSV)
   **And** each exception includes: record ID, field, error reason, original value

2. **Given** some records fail validation
   **When** the import completes
   **Then** a summary shows: X imported, Y failed, Z skipped
   **And** the exceptions file is saved for review

## Tasks / Subtasks

- [ ] Task 1: Implement import execution (AC: #1)
  - [ ] Run parser and mapper
  - [ ] Open connection via `IDbConnectionFactory` and begin `IDbTransaction`
  - [ ] Insert valid records using existing Dapper repositories
  - [ ] Commit transaction on success, rollback on critical error
- [ ] Task 2: Handle record failures (AC: #1)
  - [ ] Catch validation/insert errors per record
  - [ ] Continue processing other records
  - [ ] Track failed records
- [ ] Task 3: Create exceptions file (AC: #1, #2)
  - [ ] Define ImportException model
  - [ ] Write exceptions to JSON file
  - [ ] Include: recordId, field, reason, originalValue
- [ ] Task 4: Generate summary (AC: #2)
  - [ ] Count imported, failed, skipped
  - [ ] Print summary to console
  - [ ] Report exceptions file location

## Dev Notes

### Architecture Compliance

This story follows the established Dapper + DbUp architecture:

- **Connection management:** Use `IDbConnectionFactory` to create connections
- **Repositories:** Use existing `ISchoolRepository`, `IClassGroupRepository`, `IStudentRepository`, `IActivityRepository` for inserts
- **Transactions:** Use `IDbTransaction` for batch atomicity
- **No EF Core:** All database access via Dapper with parameterized SQL

### Transaction Pattern (Dapper)

```csharp
public class ImportExecutionService
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IActivityRepository _activityRepository;

    public async Task<ImportResult> ExecuteImportAsync(ImportData data)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            // Insert schools first (other entities reference them)
            foreach (var school in data.Schools)
            {
                await _schoolRepository.CreateAsync(school, connection, transaction);
            }

            // Insert class groups, activities, students...
            // Each repository method accepts optional connection/transaction

            transaction.Commit();
            return ImportResult.Success(counts);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw;
        }
    }
}
```

### Batch Transaction Strategy

- Use transaction per entity type (schools, then class groups, then activities, then students)
- Or atomic per batch (100 records) for large datasets
- Rollback batch on critical error, log and continue to next batch
- Each repository insert uses parameterized SQL via Dapper

### Exception File Format

```json
{
  "importRun": {
    "timestamp": "2026-01-03T19:00:00Z",
    "inputPath": "docs/legacy"
  },
  "exceptions": [
    {
      "entityType": "Student",
      "legacyId": "123",
      "field": "FirstName",
      "reason": "Required field is empty",
      "originalValue": ""
    },
    {
      "entityType": "ClassGroup",
      "legacyId": "5",
      "field": "SchoolId",
      "reason": "Referenced school not found",
      "originalValue": "999"
    }
  ]
}
```

### Import Summary Output

```
=== IMPORT COMPLETE ===

Imported:
  - Schools: 15
  - Class Groups: 40
  - Activities: 8
  - Students: 347

Failed:
  - Students: 3

Exceptions saved to: import-exceptions-2026-01-03.json

Total: 410 processed, 3 failed (99.3% success rate)
```

### Previous Story Dependencies

- **Story 7.3** provides: Preview mode logic to reuse

### Existing Infrastructure

Repositories already exist for all entity types in `Infrastructure/Repositories/`:
- `SchoolRepository` (implements `ISchoolRepository`)
- `ClassGroupRepository` (implements `IClassGroupRepository`)
- `StudentRepository` (implements `IStudentRepository`)
- `ActivityRepository` (implements `IActivityRepository`)

All use `IDbConnectionFactory` and Dapper. The import service should reuse these repositories, potentially adding overloads that accept `IDbConnection` and `IDbTransaction` parameters for transactional batch inserts.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.4]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

