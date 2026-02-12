# Story 5.7: Data Migration - Attendance & Evaluations

Status: done

## Story

As a developer,
I want legacy attendance and evaluation data parsed, mapped, and imported into the database,
So that tracking flows and audit trails operate on real historical records.

## Acceptance Criteria

1. **Given** legacy XML/XSD data for Activities (`docs/legacy/3_Activity/`)
   **When** the migration import executes for Attendance and Evaluations
   **Then** all valid attendance records are inserted with correct student and class group links

2. **Given** the migration imports evaluation data
   **Then** evaluation/assessment records are imported with proper date and score mappings

3. **Given** the migration processes historical data
   **Then** historical data maintains accurate timestamps for audit trail purposes

4. **Given** validation errors occur during import
   **Then** errors are captured and logged to the migration audit log

5. **Given** the import completes
   **Then** a summary report shows imported count, skipped count, and error count

6. **Given** the migration is complete
   **Then** the imported data appears correctly in the attendance tab and evaluation tab

## Tasks / Subtasks

- [x] Task 1: Create Legacy Schema Parser for Activities (AC: #1, #2)
  - [x] Read and validate Activity.xml against Activity.xsd schema
  - [x] Extract all 7 fields from Activity XSD
  - [x] Parse attendance and evaluation records
  - [x] Handle date/time format variations

- [x] Task 2: Implement Attendance Record Import (AC: #1)
  - [x] Map legacy attendance data to Attendance entity
  - [x] Link StudentId and ClassGroupId from imported data
  - [x] Map status values (Present, Absent, Late)
  - [x] Handle orphaned attendance (student/class not found)
  - [x] Use existing `IAttendanceRepository` for data insertion via Dapper

- [x] Task 3: Implement Evaluation Record Import (AC: #2)
  - [x] Map legacy evaluation/assessment data to Evaluation entity
  - [x] Link StudentId and ActivityId
  - [x] Map score, speed, and accuracy metrics
  - [x] Translate field names from Afrikaans to English
  - [x] Use existing `IEvaluationRepository` for data insertion via Dapper

- [x] Task 4: Preserve Historical Timestamps (AC: #3)
  - [x] Map original CreatedAt timestamps from legacy data
  - [x] Preserve ModifiedAt dates for audit accuracy
  - [x] Ensure audit trail reflects actual historical dates
  - [x] Use parameterized SQL with explicit timestamp columns (snake_case)

- [x] Task 5: Implement Validation and Error Logging (AC: #4)
  - [x] Validate imported records against XSD constraints
  - [x] Create audit log entries for validation errors
  - [x] Include file/line information in error logs

- [x] Task 6: Create Import Summary Report (AC: #5)
  - [x] Track imported, skipped, and error counts
  - [x] Separate counts for Attendance and Evaluations
  - [x] Include association errors in report

- [x] Task 7: Verify Tab UI Display (AC: #6)
  - [x] Test that imported Attendance appears in student profile Attendance tab
  - [x] Test that imported Evaluations appear in Evaluation tab
  - [x] Test status chips display correctly
  - [x] Test audit trail shows historical data

## Dev Notes

### Architecture Requirements (Dapper + DbUp)

- **Legacy XSD Location**: `docs/legacy/3_Activity/Activity.xsd` (7 fields)
- **Entity Locations**:
  - `apps/backend/src/Domain/Entities/Attendance.cs`
  - `apps/backend/src/Domain/Entities/Evaluation.cs`
  - `apps/backend/src/Domain/Entities/Activity.cs`
- **Repository Interfaces**:
  - `apps/backend/src/Application/Interfaces/IAttendanceRepository.cs`
  - `apps/backend/src/Application/Interfaces/IEvaluationRepository.cs`
- **Repository Implementations**:
  - `apps/backend/src/Infrastructure/Repositories/AttendanceRepository.cs` (Dapper + `IDbConnectionFactory`)
  - `apps/backend/src/Infrastructure/Repositories/EvaluationRepository.cs` (Dapper + `IDbConnectionFactory`)
- **Import Service Location**: `apps/backend/src/Application/Import/`
- **CLI Command**: `dotnet run import attendance-evaluations`
- **DI Registration**: `apps/backend/src/Infrastructure/DependencyInjection.cs`

### Data Access Pattern

- All data insertion uses existing Dapper-based repositories (`IAttendanceRepository`, `IEvaluationRepository`)
- Bulk inserts should use `IDbTransaction` for batch atomicity
- SQL uses parameterized queries with snake_case column names
- No EF Core -- all data access is via Dapper with explicit SQL
- No navigation properties -- use explicit SQL JOINs for related data lookups

### Field Mapping Reference
See `docs/domain-models.md` for complete Afrikaans to English translations:
- Activity entity: 7 fields from Activity.xsd
- Attendance record extraction from activity data
- Evaluation/score mapping

### Dependency Requirements
- Story 4-9 (Students migration) must be completed first
- Story 3-6 (Class Groups migration) must be completed first
- Students and Class Groups must exist before importing attendance

### Import Order
1. Schools (Story 2-6)
2. Class Groups (Story 3-6)
3. Students & Families (Story 4-9)
4. Activities, Attendance & Evaluations (this story)

### Timestamp Preservation
Critical for audit compliance (FR14):
- Original dates must be preserved
- Do not overwrite with import timestamps
- Audit trail must reflect actual historical activity

### Previous Story Context
- Story 5-6 completed Bulk Attendance Entry
- Attendance and Evaluation tabs are functional
- Audit Trail Panel is implemented

### Testing Standards
- Integration tests in `apps/backend/tests/Integration/Import/`
- Use sample XML files from `docs/legacy/3_Activity/`
- Test timestamp preservation
- Test audit trail accuracy

### Project Structure Notes
- Reuse import infrastructure from previous migration stories
- Extend ImportAuditLog for attendance/evaluation imports

### References
- [Source: docs/legacy/3_Activity/Activity.xsd]
- [Source: docs/domain-models.md]
- [Source: _bmad-output/planning-artifacts/architecture.md#Data-Architecture]

## Dev Agent Record

### Agent Model Used

minimax/minimax-m2

### Debug Log References

- **XML Parser**: LegacyAttendanceEvaluationXmlParser.cs - parses Activity.xml against XSD schema
- **Attendance Mapper**: LegacyAttendanceMapper.cs - maps legacy data to Attendance entities with validation
- **Evaluation Mapper**: LegacyEvaluationMapper.cs - maps legacy data to Evaluation entities with validation
- **Import Service**: LegacyAttendanceEvaluationImportService.cs - orchestrates full import process with error handling
- **Integration Tests**: LegacyAttendanceEvaluationImportServiceTests.cs (8 comprehensive tests)

### Completion Notes List

1. **Task 1 - Schema Parser**: Implemented LegacyAttendanceEvaluationXmlParser that reads and validates Activity.xml against Activity.xsd schema, extracting all 7 fields from XSD with proper date/time format handling
2. **Task 2 - Attendance Import**: Implemented LegacyAttendanceMapper with validation for StudentId and ClassGroupId foreign keys, status mapping (Present/Absent/Late), and orphaned record handling
3. **Task 3 - Evaluation Import**: Implemented LegacyEvaluationMapper with validation for StudentId and ActivityId, score/speed/accuracy metrics mapping, and Afrikaans to English field translations
4. **Task 4 - Historical Timestamps**: Implemented timestamp preservation using OriginalCreatedAt and OriginalModifiedAt from legacy data with snake_case SQL column mapping
5. **Task 5 - Validation & Logging**: Implemented comprehensive validation and error logging with XSD constraints, audit log entries, and file/line information
6. **Task 6 - Summary Report**: Implemented LegacyAttendanceEvaluationImportSummary with separate counts for Attendance and Evaluations, association errors, and audit trail
7. **Task 7 - UI Verification**: Verified integration with existing Attendance/Evaluation tabs and audit trail functionality

All 8 integration tests passing successfully, including timestamp preservation tests.

### File List

**Backend Implementation:**
- apps/backend/src/Application/Import/LegacyAttendanceEvaluationXmlParser.cs
- apps/backend/src/Application/Import/LegacyAttendanceMapper.cs
- apps/backend/src/Application/Import/LegacyEvaluationMapper.cs
- apps/backend/src/Infrastructure/Import/LegacyAttendanceEvaluationImportService.cs

**Backend Tests:**
- apps/backend/tests/Integration/Import/LegacyAttendanceEvaluationImportServiceTests.cs
- apps/backend/tests/Integration/Import/LegacyAttendanceEvaluationXmlParserTests.cs
- apps/backend/tests/Integration/Import/LegacyAttendanceMapperTests.cs
- apps/backend/tests/Integration/Import/LegacyEvaluationMapperTests.cs

## Change Log

| Date | Change |
|------|--------|
| 2026-01-06 | Story file created from backlog |
| 2026-02-06 | Updated to reference Dapper + DbUp architecture and existing repositories |
| 2026-02-10 | Code implementation completed - All 8 tests passing - Story marked done |
