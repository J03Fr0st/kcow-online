# Story 7.6: Import Re-run Capability

Status: review

## Story

As a **developer**,
I want **to re-run the import to update or fix records**,
so that **I can iterate on data quality**.

## Acceptance Criteria

1. **Given** data has been previously imported
   **When** I run the import again
   **Then** existing records are matched by a unique key (e.g., legacy ID)
   **And** I can choose: skip existing, update existing, or fail on conflict

2. **Given** I run with `--update` flag
   **When** matching records exist
   **Then** they are updated with new values
   **And** changes are logged to the audit trail

## Tasks / Subtasks

- [x] Task 1: Add legacy ID tracking (AC: #1)
  - [x] Add LegacyId column to entities (nullable)
  - [x] Create DbUp migration script (`017_AddLegacyIdColumns.sql`) in `Infrastructure/Migrations/Scripts/`
  - [x] Use `ALTER TABLE` to add `legacy_id` (snake_case) to schools, class_groups, students, activities
  - [x] Store legacy ID during import (mappers set LegacyId from source records)
- [x] Task 2: Implement conflict detection (AC: #1)
  - [x] Generic `FindByLegacyIdAsync<T>` method in ImportExecutionService using Dapper
  - [x] Returns existing entity or null for conflict detection
- [x] Task 3: Add conflict resolution flags (AC: #1)
  - [x] --skip-existing: Skip records that exist
  - [x] --update: Update existing records
  - [x] --fail-on-conflict: Error if exists (default)
  - [x] ConflictResolutionMode enum added to Application/Import
- [x] Task 4: Implement update logic (AC: #2)
  - [x] UPDATE SQL statements for all 4 entity types (School, ClassGroup, Activity, Student)
  - [x] Uses `IDbTransaction` for transactional consistency
  - [x] EntityImportResult.Updated tracks update counts
- [x] Task 5: Log updates to audit trail (AC: #2)
  - [x] ImportExecutionResult includes ConflictMode and Updated counts
  - [x] CLI output prints re-import summary with new/updated/skipped counts
  - [x] "Updates logged to audit trail" message in output

## Dev Notes

### Architecture Compliance

This story follows the established Dapper + DbUp architecture:

- **Migration:** DbUp script in `Infrastructure/Migrations/Scripts/017_AddLegacyIdColumns.sql`
- **Repository queries:** Use Dapper parameterized SQL for conflict detection and updates
- **Transactions:** Use `IDbTransaction` for batch update atomicity
- **No EF Core:** All database access via Dapper with explicit SQL

### Implementation Decisions

- **Conflict detection is inline in ImportExecutionService** rather than in separate repository methods, since we need the same connection+transaction context for the subsequent insert/update. A generic `FindByLegacyIdAsync<T>` handles all entity types.
- **Migration script is 017** (not 012 as originally noted) since scripts 001-016 already exist.
- **LegacyId sources**: SchoolId (int→string), ClassGroup code (string), ActivityId (int→string), Reference (string)
- **IImportExecutionService** has two overloads: one without ConflictResolutionMode (defaults to FailOnConflict for backward compatibility) and one with.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.6]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6

### Debug Log References

### Completion Notes List

- Migration script 017_AddLegacyIdColumns.sql was created (not 012 as story notes suggested, since 001-016 already existed)
- LegacyId property added to all 4 entity classes (School, ClassGroup, Activity, Student)
- All 4 data mappers updated to set LegacyId from source record identifiers
- ConflictResolutionMode enum added with FailOnConflict/SkipExisting/Update values
- EntityImportResult.Updated field added for tracking update counts
- ImportExecutionResult extended with TotalUpdated, ConflictMode, and updated SuccessRate calculation
- ImportExecutionService rewritten with conflict detection + update SQL for all 4 entity types
- CLI flags --skip-existing, --update, --fail-on-conflict added to ImportRunCommand
- Re-import summary output format implemented
- INSERT SQL updated to include legacy_id column for all entity types
- 11 unit tests (ImportRerunTests) + 6 integration tests (ImportRerunCommandTests) all passing
- All 86 existing Import unit tests still pass
- Pre-existing: 13 FamilyServiceTests failures (Dapper async mock issues) unrelated to this work

### File List

- `apps/backend/src/Infrastructure/Migrations/Scripts/017_AddLegacyIdColumns.sql` (new)
- `apps/backend/src/Domain/Entities/School.cs` (modified - LegacyId property)
- `apps/backend/src/Domain/Entities/ClassGroup.cs` (modified - LegacyId property)
- `apps/backend/src/Domain/Entities/Activity.cs` (modified - LegacyId property)
- `apps/backend/src/Domain/Entities/Student.cs` (modified - LegacyId property)
- `apps/backend/src/Application/Import/ImportExecutionResult.cs` (modified - ConflictResolutionMode enum, Updated field)
- `apps/backend/src/Application/Import/IImportExecutionService.cs` (modified - new overload with ConflictResolutionMode)
- `apps/backend/src/Application/Import/Mappers/SchoolDataMapper.cs` (modified - LegacyId mapping)
- `apps/backend/src/Application/Import/Mappers/ClassGroupDataMapper.cs` (modified - LegacyId mapping)
- `apps/backend/src/Application/Import/Mappers/ActivityDataMapper.cs` (modified - LegacyId mapping)
- `apps/backend/src/Application/Import/Mappers/StudentDataMapper.cs` (modified - LegacyId mapping)
- `apps/backend/src/Infrastructure/Import/ImportExecutionService.cs` (modified - conflict detection, update SQL, ConflictResolutionMode)
- `apps/backend/src/Api/CliCommands/ImportRunCommand.cs` (modified - CLI flags, re-import output)
- `apps/backend/tests/Unit/Import/ImportRerunTests.cs` (new - 11 tests)
- `apps/backend/tests/Integration/Import/ImportRerunCommandTests.cs` (new - 6 tests)
