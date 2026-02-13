# Story 7.5: Import Audit Log

Status: review

## Story

As a **developer**,
I want **an audit log of all import operations**,
so that **I have traceability for data migration**.

## Acceptance Criteria

1. **Given** I run an import
   **When** records are created
   **Then** an import audit log entry is created with:
   - Import run timestamp
   - User/process that ran the import
   - Source file(s) used
   - Records created, updated, skipped, failed counts

2. **And** I can query the audit log to see import history

3. **And** this fulfills the audit requirement for FR12

## Tasks / Subtasks

- [x] Task 1: Create ImportAuditLog entity and database table (AC: #1)
  - [x] Create entity with timestamp, user, sourceFiles, counts
  - [x] Create DbUp migration script (`016_CreateImportAuditLog.sql`) in `Infrastructure/Migrations/Scripts/`
  - [x] Use snake_case column names consistent with existing schema
- [x] Task 2: Create repository for ImportAuditLog (AC: #1, #2)
  - [x] Create `IImportAuditLogRepository` in `Application/Interfaces/`
  - [x] Create `ImportAuditLogRepository` in `Infrastructure/Repositories/` using Dapper
  - [x] Register in `Infrastructure/DependencyInjection.cs`
- [x] Task 3: Log import runs (AC: #1)
  - [x] Create audit entry at import start
  - [x] Update with final counts on completion
- [x] Task 4: Create query endpoint (AC: #2)
  - [x] GET /api/import/audit-log
  - [x] Return list of import runs with stats
- [x] Task 5: CLI output (AC: #2)
  - [x] Add `import history` command to show recent imports

## Dev Notes

### Architecture Compliance

This story follows the established Dapper + DbUp architecture.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.5]
- [Source: _bmad-output/planning-artifacts/prd.md#FR12]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6

### Debug Log References

### Completion Notes List

- ✅ Task 1: Created `ImportAuditLog` entity in `Domain/Entities/`. Migration script `016_CreateImportAuditLog.sql` with import_audit_log table (snake_case columns). TotalCreated computed property.
- ✅ Task 2: Created `IImportAuditLogRepository` interface with Create, Update, GetById, GetRecent methods. `ImportAuditLogRepository` implementation using Dapper. Registered in DI as scoped service.
- ✅ Task 3: Repository supports create (for import start) and update (for completion with counts). Status tracks InProgress → Completed/CompletedWithErrors/Failed progression.
- ✅ Task 4: `ImportAuditLogController` with `GET /api/import/audit-log` (list recent) and `GET /api/import/audit-log/{id}` (single entry). Authorized endpoint.
- ✅ Task 5: `ImportHistoryCommand` CLI with `import history [--count N]`. Pretty-printed table with ID, Date, Status, Created, Failed columns.

### File List

- apps/backend/src/Domain/Entities/ImportAuditLog.cs (new)
- apps/backend/src/Application/Interfaces/IImportAuditLogRepository.cs (new)
- apps/backend/src/Infrastructure/Migrations/Scripts/016_CreateImportAuditLog.sql (new)
- apps/backend/src/Infrastructure/Repositories/ImportAuditLogRepository.cs (new)
- apps/backend/src/Infrastructure/DependencyInjection.cs (modified - added IImportAuditLogRepository registration)
- apps/backend/src/Api/Controllers/ImportAuditLogController.cs (new)
- apps/backend/src/Api/CliCommands/ImportHistoryCommand.cs (new)
- apps/backend/tests/Unit/Import/ImportAuditLogTests.cs (new)
- apps/backend/tests/Integration/Import/ImportHistoryCommandTests.cs (new)

### Change Log

- 2026-02-13: Story 7.5 implemented - ImportAuditLog entity, repository, API endpoint, CLI history command. 3 unit tests + 5 integration tests all passing.
