# Story 2.6: Data Migration - Trucks & Schools

Status: ready-for-dev

## Story

As a developer,
I want legacy Trucks/Schools data parsed, mapped, and imported into the database,
So that CRUD flows and E2E tests operate on real migrated records.

## Acceptance Criteria

1. **Given** legacy XML/XSD data for Schools (`docs/legacy/1_School/`)
   **When** the migration import executes for Schools
   **Then** all valid School records are inserted into the database

2. **Given** the migration import executes
   **Then** field mappings transform legacy data to new schema (e.g., contact fields, billing settings)

3. **Given** the migration needs Truck data
   **Then** Truck seed data is loaded for route assignments

4. **Given** validation errors occur during import
   **Then** errors are captured and logged to the migration audit log

5. **Given** the import completes
   **Then** a summary report shows imported count, skipped count, and error count

6. **Given** the migration is complete
   **Then** the imported data is available in the UI and API

## Tasks / Subtasks

- [ ] Task 1: Create Legacy Schema Parser for Schools (AC: #1)
  - [ ] Read and validate School.xml against School.xsd schema
  - [ ] Extract all 30 fields from School XSD
  - [ ] Handle encoding and format variations

- [ ] Task 2: Implement Data Mapping Service for Schools (AC: #2)
  - [ ] Map legacy School fields to domain entity properties
  - [ ] Translate Afrikaans field names to English per `docs/domain-models.md`
  - [ ] Apply data transformations (date formats, contact consolidation)
  - [ ] Flag missing or invalid values for review

- [ ] Task 3: Create Truck Seed Data (AC: #3)
  - [ ] Define standard truck seed data based on legacy system trucks
  - [ ] Ensure trucks are available for route assignment tests

- [ ] Task 4: Implement Validation and Error Logging (AC: #4)
  - [ ] Validate imported records against XSD constraints
  - [ ] Create audit log entries for validation errors
  - [ ] Include file/line information in error logs

- [ ] Task 5: Create Import Summary Report (AC: #5)
  - [ ] Track imported, skipped, and error counts
  - [ ] Generate summary report after import completion
  - [ ] Save report to configurable output location

- [ ] Task 6: Verify UI and API Availability (AC: #6)
  - [ ] Test that imported Schools appear in GET /api/schools
  - [ ] Test that imported data renders correctly in Schools Management UI

## Dev Notes

### Architecture Requirements
- **Legacy XSD Location**: `docs/legacy/1_School/School.xsd` (30 fields)
- **Entity Location**: `apps/backend/src/Domain/Entities/School.cs`
- **Import Service Location**: `apps/backend/src/Application/Import/`
- **CLI Command**: `dotnet run import parse` → `dotnet run import schools`

### Technical Constraints
- Must strictly align with XSD schema definitions
- Use English field names (translate from Afrikaans per domain-models.md)
- Database schemas must enforce XSD types and max lengths
- Validation must enforce XSD constraints (required fields, lengths)

### Field Mapping Reference
See `docs/domain-models.md` for complete Afrikaans → English translations:
- School entity: 30 fields from School.xsd
- Contact fields consolidation pattern

### Previous Story Context
- Story 2-5 completed School Billing Settings
- School CRUD UI is functional
- API endpoints operational: GET/POST/PUT/DELETE `/api/schools`

### Testing Standards
- Integration tests in `apps/backend/tests/Integration/Import/`
- Use sample XML files from `docs/legacy/1_School/`
- Database reset between test runs

### Project Structure Notes
- Import services in `Application/Import/`
- EF Core configurations in `Infrastructure/Data/`
- Align with existing School entity structure

### References
- [Source: docs/legacy/1_School/School.xsd]
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
