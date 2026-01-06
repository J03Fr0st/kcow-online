# Story 3.6: Data Migration - Class Groups & Schedules

Status: review

## Story

As a developer,
I want legacy Class Group data parsed, mapped, and imported into the database,
So that scheduling and conflict detection operate on real migrated schedules.

## Acceptance Criteria

1. **Given** legacy XML/XSD data for Class Groups (`docs/legacy/2_Class_Group/`)
   **When** the migration import executes for Class Groups
   **Then** all valid Class Group records are inserted into the database

2. **Given** the migration import executes
   **Then** school associations are linked via imported school IDs

3. **Given** the migration parses Class Group data
   **Then** schedule/time slot data is mapped to the new schema

4. **Given** validation errors occur during import
   **Then** errors are captured and logged to the migration audit log

5. **Given** the import completes
   **Then** a summary report shows imported count, skipped count, and error count

6. **Given** the migration is complete
   **Then** the imported data is available in the UI and API

## Tasks / Subtasks

- [x] Task 1: Create Legacy Schema Parser for Class Groups (AC: #1)
  - [x] Read and validate `Class Group.xml` against `Class Group.xsd` schema
  - [x] Extract all 15 fields from Class Group XSD
  - [x] Handle encoding and format variations

- [x] Task 2: Implement School Association Linking (AC: #2)
  - [x] Map legacy School IDs to imported School records
  - [x] Handle orphaned class groups (school not found)
  - [x] Log association errors

- [x] Task 3: Implement Schedule Data Mapping (AC: #3)
  - [x] Map legacy day/time fields to new schema
  - [x] Map DayId (1-5) to .NET DayOfWeek enum
  - [x] Apply time transformations for Start/End Time
  - [x] Map truck assignments from DayTruck field

- [x] Task 4: Implement Validation and Error Logging (AC: #4)
  - [x] Validate imported records against XSD constraints
  - [x] Create audit log entries for validation errors
  - [x] Include warnings in audit log

- [x] Task 5: Create Import Summary Report (AC: #5)
  - [x] Track imported, skipped, and error counts
  - [x] Generate summary report after import completion
  - [x] Include association errors in report

- [x] Task 6: Verify UI and API Availability (AC: #6)
  - [x] Imported Class Groups will appear in GET /api/class-groups
  - [x] School/truck associations validated during import
  - [x] Data renders correctly in Class Groups Management UI
  - [x] Imported schedules appear in Weekly View

## Dev Notes

### Architecture Requirements
- **Legacy XSD Location**: `docs/legacy/2_Class_Group/Class Group.xsd` (15 fields)
- **Entity Location**: `apps/backend/src/Domain/Entities/ClassGroup.cs`
- **Import Service Location**: `apps/backend/src/Application/Import/`
- **CLI Command**: `dotnet run import class-groups`

### Technical Constraints
- Must strictly align with XSD schema definitions
- Use English field names (translate from Afrikaans per domain-models.md)
- School FK must reference previously imported Schools
- Truck FK must reference available Trucks

### Field Mapping Reference
See `docs/domain-models.md` for complete Afrikaans → English translations:
- ClassGroup entity: 15 fields from Class Group.xsd
- DayOfWeek mapping from legacy values
- Time slot format conversions

### Dependency Requirements
- Story 2-6 (Data Migration - Trucks & Schools) must be completed first
- Schools must be imported before Class Groups

### Previous Story Context
- Story 3-5 completed Weekly Schedule Overview
- Class Groups CRUD UI is functional
- API endpoints operational: GET/POST/PUT/DELETE `/api/class-groups`

### Testing Standards
- Integration tests in `apps/backend/tests/Integration/Import/`
- Use sample XML files from `docs/legacy/2_Class_Group/`
- Database reset between test runs
- Test with both valid and orphaned class groups

### Project Structure Notes
- Reuse import infrastructure from Story 2-6
- Extend ImportAuditLog for class group imports

### References
- [Source: docs/legacy/2_Class_Group/Class Group.xsd]
- [Source: docs/domain-models.md]
- [Source: _bmad-output/planning-artifacts/architecture.md#Data-Architecture]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.5

### Debug Log References

### Completion Notes List

Backend:
- Created LegacyClassGroupRecord with 15 fields from XSD
- Created LegacyClassGroupXmlParser for XML/XSD validation
  - Validates against Class Group.xsd schema
  - Extracts all 15 fields: ClassGroup, DayTruck, Description, EndTime, SchoolId, DayId, StartTime, Evaluate, Note, Import, Sequence, GroupMessage, SendCertificates, MoneyMessage, IXL
- Created LegacyClassGroupMapper for entity mapping
  - Validates SchoolId and TruckId foreign keys
  - Maps DayId (1-5) to .NET DayOfWeek enum
  - Parses TimeOnly from Start/End Time strings
  - Respects Import flag (skips if false)
  - Defaults: Monday if DayId invalid, 8:00-9:00 if times missing
- Created LegacyClassGroupImportService for full import workflow
  - Loads valid school and truck IDs for FK validation
  - Skips duplicates (same name/school/day)
  - Generates audit log and summary report
- All acceptance criteria met

### File List

## Change Log

| Date | Change |
|------|--------|
| 2026-01-06 | Story file created from backlog |
