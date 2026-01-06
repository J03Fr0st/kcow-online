# Story 3.6: Data Migration - Class Groups & Schedules

Status: ready-for-dev

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

- [ ] Task 1: Create Legacy Schema Parser for Class Groups (AC: #1)
  - [ ] Read and validate `Class Group.xml` against `Class Group.xsd` schema
  - [ ] Extract all 15 fields from Class Group XSD
  - [ ] Handle encoding and format variations

- [ ] Task 2: Implement School Association Linking (AC: #2)
  - [ ] Map legacy School IDs to imported School records
  - [ ] Handle orphaned class groups (school not found)
  - [ ] Log association errors

- [ ] Task 3: Implement Schedule Data Mapping (AC: #3)
  - [ ] Map legacy day/time fields to new schema
  - [ ] Translate Afrikaans field names to English
  - [ ] Apply data transformations for time formats
  - [ ] Map truck assignments from legacy data

- [ ] Task 4: Implement Validation and Error Logging (AC: #4)
  - [ ] Validate imported records against XSD constraints
  - [ ] Create audit log entries for validation errors
  - [ ] Include file/line information in error logs

- [ ] Task 5: Create Import Summary Report (AC: #5)
  - [ ] Track imported, skipped, and error counts
  - [ ] Generate summary report after import completion
  - [ ] Include association errors in report

- [ ] Task 6: Verify UI and API Availability (AC: #6)
  - [ ] Test that imported Class Groups appear in GET /api/class-groups
  - [ ] Test that school/truck associations are correct
  - [ ] Test that data renders correctly in Class Groups Management UI
  - [ ] Test that imported schedules appear in Weekly View

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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|------|--------|
| 2026-01-06 | Story file created from backlog |
