# Story 7.3: Import Preview Mode

Status: done

## Story

As a **developer**,
I want **to preview import results before committing**,
so that **I can verify the data before it goes live (FR12)**.

## Acceptance Criteria

1. **Given** I run the import command with `--preview` flag
   **When** the import runs
   **Then** it shows:
   - Total records to import by entity type
   - Sample of mapped records
   - Validation warnings and errors
   - Records that will be skipped

2. **And** NO data is written to the database

3. **And** I can review the preview output before running full import

## Tasks / Subtasks

- [x] Task 1: Add --preview flag to CLI (AC: #1)
  - [x] Parse --preview command line argument
  - [x] Branch logic based on flag
- [x] Task 2: Implement preview logic (AC: #1, #2)
  - [x] Run parser and mapper
  - [x] Collect statistics
  - [x] Skip database writes when preview=true
- [x] Task 3: Generate preview report (AC: #1)
  - [x] Count total records per entity type
  - [x] Show sample of first 5 records per type
  - [x] List all warnings and errors
  - [x] List records to be skipped with reasons
- [x] Task 4: Output preview to console/file (AC: #3)
  - [x] Pretty-print preview to console
  - [x] Optionally save to JSON file

## Dev Notes

### Architecture Note

Preview mode runs the parser and mapper pipeline but does NOT interact with the database at all. This story produces an in-memory report only. No repositories, connections, or transactions are involved.

### CLI Command

```bash
# Preview mode (no database writes)
dotnet run --project apps/backend/src/Api import run --preview

# Full import (writes to database)
dotnet run --project apps/backend/src/Api import run
```

### Preview Output Format

```
=== IMPORT PREVIEW ===

Record Counts:
  - Schools: 15
  - Class Groups: 42
  - Activities: 8
  - Students: 350

Sample Records (Schools):
  1. Greenwood Primary - 123 Main St
  2. Hillside Academy - 456 Oak Ave
  ...

Warnings (12 total):
  - Student #45: DateOfBirth missing, using null
  - Student #89: Invalid gender value "X", using null

Errors (3 records will be skipped):
  - Student #123: FirstName is required (skipped)
  - ClassGroup #5: Invalid SchoolId reference (skipped)

Summary:
  - Valid: 410 records
  - Warnings: 12 records
  - Skipped: 3 records

Run without --preview to import data.
```

### Previous Story Dependencies

- **Story 7.1** provides: Parser
- **Story 7.2** provides: Mapper

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.3]
- [Source: _bmad-output/planning-artifacts/prd.md#FR12]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6

### Debug Log References

### Completion Notes List

- ✅ Task 1: Created `ImportRunCommand` with `--preview` flag, `--input`, `--output`, `--help` options. Wired into `Program.cs` alongside existing `ImportParseCommand`.
- ✅ Task 2: Preview mode runs full parse→map pipeline using `ILegacyParser` and all 4 `IDataMapper` implementations. No database interaction at any point. `ParsedEntityResults` holds intermediate parse results.
- ✅ Task 3: `ImportPreviewReport` collects per-entity counts (parsed/mapped/skipped), sample records (first 5), all warnings and errors. Console output matches story format.
- ✅ Task 4: Pretty-printed console output with `=== IMPORT PREVIEW ===` header, record counts, sample records, warnings/errors (capped at 20 with "and X more"), summary. JSON file output via `--output` flag using `System.Text.Json`.
- Note: Full import mode (`import run` without `--preview`) shows record counts but defers DB writes to Story 7.4.

### File List

- apps/backend/src/Api/CliCommands/ImportRunCommand.cs (new)
- apps/backend/src/Api/Program.cs (modified - added ImportRunCommand handler)
- apps/backend/tests/Integration/Import/ImportPreviewCommandTests.cs (new)

### Change Log

- 2026-02-13: Story 7.3 implemented - ImportRunCommand with preview mode, 11 tests all passing.

## Senior Developer Review (AI)

**Reviewer:** Joe (AI-assisted) on 2026-02-13
**Outcome:** Approved with fixes applied

**Issues Found and Fixed (Epic-wide review):**
- [CRITICAL] Missing DI registrations for ILegacyParser and IImportExecutionService in DependencyInjection.cs — Fixed
- [CRITICAL] 37+ Student fields missing from INSERT/UPDATE SQL in ImportExecutionService — Fixed (all XSD fields now mapped)
- [CRITICAL] ImportAuditLogController returned entities directly — Fixed (created ImportAuditLogDto, added count validation)
- [CRITICAL] ClassGroupDataMapper.MapMany always reported Success=true — Fixed (now checks allErrors.Count == 0)
- [CRITICAL] LegacyParser directly instantiated concrete parsers — Fixed (added constructor injection)
- [HIGH] SQL injection risk via table name interpolation in FindByLegacyIdAsync — Fixed (added whitelist validation)
- [HIGH] ImportAuditLogRepository used blocking Create() and ignored CancellationToken — Fixed (uses CreateAsync + CommandDefinition)
- [HIGH] Missing UNIQUE constraint on legacy_id indexes — Fixed (partial unique indexes)
- [HIGH] ImportExceptionWriter had no null checks or path validation — Fixed (added ArgumentNullException, path canonicalization, CancellationToken)
- [HIGH] School INSERT/UPDATE missing school_description, phone, scheduling_notes, omsendbriewe columns — Fixed
- [MEDIUM] ImportHistoryCommand had no error handling for repository calls — Fixed (try-catch added)
- [MEDIUM] ImportHistoryCommand accepted negative count values — Fixed (validation added)

**Test Results:** 86 unit tests PASS, 126 integration tests PASS (2 pre-existing failures unrelated to Epic 7)
