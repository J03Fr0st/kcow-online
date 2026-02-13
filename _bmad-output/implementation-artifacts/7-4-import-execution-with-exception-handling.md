# Story 7.4: Import Execution with Exception Handling

Status: done

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

- [x] Task 1: Implement import execution (AC: #1)
  - [x] Run parser and mapper
  - [x] Open connection via `IDbConnectionFactory` and begin `IDbTransaction`
  - [x] Insert valid records using Dapper with transaction support
  - [x] Commit transaction on success, rollback on critical error
- [x] Task 2: Handle record failures (AC: #1)
  - [x] Catch validation/insert errors per record
  - [x] Continue processing other records
  - [x] Track failed records
- [x] Task 3: Create exceptions file (AC: #1, #2)
  - [x] Define ImportException model
  - [x] Write exceptions to JSON file
  - [x] Include: entityType, legacyId, field, reason, originalValue
- [x] Task 4: Generate summary (AC: #2)
  - [x] Count imported, failed, skipped
  - [x] Print summary to console
  - [x] Report exceptions file location

## Dev Notes

### Architecture Compliance

This story follows the established Dapper + DbUp architecture:

- **Connection management:** Use `IDbConnectionFactory` to create connections
- **Repositories:** Uses direct Dapper SQL with transaction support (repos don't accept connection/transaction params)
- **Transactions:** Use `IDbTransaction` per entity type batch
- **No EF Core:** All database access via Dapper with parameterized SQL

### Previous Story Dependencies

- **Story 7.3** provides: Preview mode logic to reuse

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.4]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6

### Debug Log References

### Completion Notes List

- ✅ Task 1: Created `ImportExecutionService` implementing `IImportExecutionService`. Uses `IDbConnectionFactory.CreateAsync()` for shared connection. Inserts in dependency order: Schools → ClassGroups → Activities → Students. Each entity type uses its own `IDbTransaction` for batch atomicity. Direct Dapper SQL with parameterized queries.
- ✅ Task 2: Per-record try/catch around each insert. Failed records increment `Failed` counter and add `ImportException`. Processing continues to next record. Transaction commits if at least some records succeed; rolls back on critical (non-record) exceptions.
- ✅ Task 3: `ImportException` model with entityType, legacyId, field, reason, originalValue. `ImportExceptionWriter.WriteAsync()` writes structured JSON with importRun metadata, summary stats, and full exceptions list. Auto-generates dated filename via `GetDefaultPath()`.
- ✅ Task 4: `PrintImportResult()` outputs `=== IMPORT COMPLETE ===` with per-entity imported counts, per-entity failed counts (if any), and total processed/failed/success rate. Exception file location reported when exceptions exist.
- Note: Existing repository interfaces don't accept IDbConnection/IDbTransaction params, so ImportExecutionService uses direct Dapper SQL (same patterns as repos). Full import mode requires `IImportExecutionService` injection; CLI returns error with guidance if service unavailable.

### File List

- apps/backend/src/Application/Import/IImportExecutionService.cs (new)
- apps/backend/src/Application/Import/ImportExecutionResult.cs (new)
- apps/backend/src/Application/Import/ImportExceptionWriter.cs (new)
- apps/backend/src/Infrastructure/Import/ImportExecutionService.cs (new)
- apps/backend/src/Api/CliCommands/ImportRunCommand.cs (modified - added full import mode)
- apps/backend/tests/Unit/Import/ImportExecutionResultTests.cs (new)

### Change Log

- 2026-02-13: Story 7.4 implemented - ImportExecutionService with per-record exception handling, JSON exception file writer, CLI summary output. 8 unit tests + 11 integration tests all passing.

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
