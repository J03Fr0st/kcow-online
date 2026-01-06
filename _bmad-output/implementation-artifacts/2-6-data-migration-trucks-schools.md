# Story 2.6: Data Migration - Trucks & Schools

Status: done

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

- [x] Task 1: Create Legacy Schema Parser for Schools (AC: #1)
  - [x] Read and validate School.xml against School.xsd schema
  - [x] Extract all 30 fields from School XSD
  - [x] Handle encoding and format variations

- [x] Task 2: Implement Data Mapping Service for Schools (AC: #2)
  - [x] Map legacy School fields to domain entity properties
  - [x] Translate Afrikaans field names to English per `docs/domain-models.md`
  - [x] Apply data transformations (date formats, contact consolidation)
  - [x] Flag missing or invalid values for review

- [x] Task 3: Create Truck Seed Data (AC: #3)
  - [x] Define standard truck seed data based on legacy system trucks
  - [x] Ensure trucks are available for route assignment tests

- [x] Task 4: Implement Validation and Error Logging (AC: #4)
  - [x] Validate imported records against XSD constraints
  - [x] Create audit log entries for validation errors
  - [x] Include file/line information in error logs

- [x] Task 5: Create Import Summary Report (AC: #5)
  - [x] Track imported, skipped, and error counts
  - [x] Generate summary report after import completion
  - [x] Save report to configurable output location

- [x] Task 6: Verify UI and API Availability (AC: #6)
  - [x] Test that imported Schools appear in GET /api/schools
  - [x] Test that imported data renders correctly in Schools Management UI

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

GPT-5

### Debug Log References

- `dotnet test apps/backend/tests/Integration/Kcow.Integration.Tests.csproj --filter FullyQualifiedName~LegacySchoolXmlParserTests`
- `dotnet test apps/backend/tests/Integration/Kcow.Integration.Tests.csproj --filter FullyQualifiedName~LegacySchoolMapperTests`
- `dotnet test apps/backend/tests/Integration/Kcow.Integration.Tests.csproj --filter FullyQualifiedName~LegacyTruckSeedDataTests`
- `dotnet test apps/backend/tests/Integration/Kcow.Integration.Tests.csproj --filter FullyQualifiedName~LegacyImportAuditLogTests`
- `dotnet test apps/backend/tests/Integration/Kcow.Integration.Tests.csproj --filter FullyQualifiedName~LegacyImportSummaryReportTests`
- `dotnet test apps/backend/tests/Integration/Kcow.Integration.Tests.csproj --filter FullyQualifiedName~LegacySchoolImportServiceTests`

### Completion Notes List

- Added legacy school XML parser with XSD validation and typed field extraction.
- Added integration tests for valid parsing and validation errors.
- Added legacy school mapping service and mapper tests with warning capture.
- Centralized standard truck seed data for legacy values and reused it in test seeding.
- Added audit log entry capture for validation errors with file/line metadata.
- Added legacy import summary report generator with file output.
- Added legacy school import service and API verification test for imported schools.
- Created comprehensive E2E tests for Schools Management UI to verify imported data rendering.
- E2E tests validate: schools list display, known imported schools presence, field rendering, navigation, search/filter, and data population.

### File List

- `apps/backend/src/Application/Import/LegacySchoolXmlParser.cs`
- `apps/backend/src/Application/Import/LegacySchoolMapper.cs`
- `apps/backend/src/Application/Import/LegacyTruckSeedData.cs`
- `apps/backend/src/Application/Import/LegacyImportAuditLog.cs`
- `apps/backend/src/Application/Import/LegacyImportSummaryReport.cs`
- `apps/backend/src/Infrastructure/Import/LegacySchoolImportService.cs`
- `apps/backend/tests/Integration/Import/LegacySchoolXmlParserTests.cs`
- `apps/backend/tests/Integration/Import/LegacySchoolMapperTests.cs`
- `apps/backend/tests/Integration/Import/LegacyTruckSeedDataTests.cs`
- `apps/backend/tests/Integration/Import/LegacyImportAuditLogTests.cs`
- `apps/backend/tests/Integration/Import/LegacyImportSummaryReportTests.cs`
- `apps/backend/tests/Integration/Import/LegacySchoolImportServiceTests.cs`
- `apps/backend/src/Infrastructure/Data/Seeders/TestDataSeeder.cs`
- `apps/frontend/e2e/schools.spec.ts`
## Change Log

| Date | Change |
|------|--------|
| 2026-01-06 | Story file created from backlog |
| 2026-01-06 | Implemented legacy school parser and parser tests |
| 2026-01-06 | Implemented legacy school mapping and mapper tests |
| 2026-01-06 | Added legacy truck seed data and tests |
| 2026-01-06 | Added audit log entries for legacy import validation |
| 2026-01-06 | Added legacy import summary report |
| 2026-01-06 | Added legacy school import service and API verification test |
| 2026-01-06 | Added comprehensive E2E tests for Schools Management UI (schools.spec.ts) |
| 2026-01-06 | **Code Review Fixes Applied:** |
| | • Added TruckId foreign key validation with warning system |
| | • Added duplicate school logging to audit trail |
| | • Enhanced API verification test with field mapping validation |
| | • Added environment variable support for E2E test credentials |
| | • All integration tests passing (5/5) |
