# Story 7.5: Import Audit Log

Status: ready-for-dev

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

- [ ] Task 1: Create ImportAuditLog entity and database table (AC: #1)
  - [ ] Create entity with timestamp, user, sourceFiles, counts
  - [ ] Create DbUp migration script (`011_CreateImportAuditLog.sql`) in `Infrastructure/Migrations/Scripts/`
  - [ ] Use snake_case column names consistent with existing schema
- [ ] Task 2: Create repository for ImportAuditLog (AC: #1, #2)
  - [ ] Create `IImportAuditLogRepository` in `Application/Interfaces/`
  - [ ] Create `ImportAuditLogRepository` in `Infrastructure/Repositories/` using Dapper
  - [ ] Register in `Infrastructure/DependencyInjection.cs`
- [ ] Task 3: Log import runs (AC: #1)
  - [ ] Create audit entry at import start
  - [ ] Update with final counts on completion
- [ ] Task 4: Create query endpoint (AC: #2)
  - [ ] GET /api/import/audit-log
  - [ ] Return list of import runs with stats
- [ ] Task 5: CLI output (AC: #2)
  - [ ] Add `import history` command to show recent imports

## Dev Notes

### Architecture Compliance

This story follows the established Dapper + DbUp architecture:

- **Migration:** DbUp script in `Infrastructure/Migrations/Scripts/011_CreateImportAuditLog.sql`
- **Repository interface:** `IImportAuditLogRepository` in `Application/Interfaces/`
- **Repository implementation:** `ImportAuditLogRepository` in `Infrastructure/Repositories/` using `IDbConnectionFactory` and Dapper
- **DI registration:** Add `services.AddScoped<IImportAuditLogRepository, ImportAuditLogRepository>()` in `Infrastructure/DependencyInjection.cs`
- **No EF Core:** All database access via Dapper with parameterized SQL

### DbUp Migration Script (011_CreateImportAuditLog.sql)

```sql
CREATE TABLE IF NOT EXISTS import_audit_log (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    started_at TEXT NOT NULL,
    completed_at TEXT,
    run_by TEXT NOT NULL DEFAULT 'system',
    source_path TEXT NOT NULL,
    status TEXT NOT NULL DEFAULT 'InProgress',
    schools_created INTEGER NOT NULL DEFAULT 0,
    class_groups_created INTEGER NOT NULL DEFAULT 0,
    activities_created INTEGER NOT NULL DEFAULT 0,
    students_created INTEGER NOT NULL DEFAULT 0,
    total_failed INTEGER NOT NULL DEFAULT 0,
    total_skipped INTEGER NOT NULL DEFAULT 0,
    exceptions_file_path TEXT,
    notes TEXT
);
```

### ImportAuditLog Entity

```csharp
public class ImportAuditLog
{
    public int Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string RunBy { get; set; } = "system";
    public string SourcePath { get; set; } = string.Empty;
    public ImportStatus Status { get; set; }

    public int SchoolsCreated { get; set; }
    public int ClassGroupsCreated { get; set; }
    public int ActivitiesCreated { get; set; }
    public int StudentsCreated { get; set; }
    public int TotalFailed { get; set; }
    public int TotalSkipped { get; set; }

    public string? ExceptionsFilePath { get; set; }
    public string? Notes { get; set; }
}

public enum ImportStatus
{
    InProgress,
    Completed,
    CompletedWithErrors,
    Failed
}
```

### Repository Interface

```csharp
// Application/Interfaces/IImportAuditLogRepository.cs
public interface IImportAuditLogRepository
{
    Task<int> CreateAsync(ImportAuditLog auditLog);
    Task UpdateAsync(ImportAuditLog auditLog);
    Task<ImportAuditLog?> GetByIdAsync(int id);
    Task<IEnumerable<ImportAuditLog>> GetRecentAsync(int count = 10);
}
```

### Repository Implementation Pattern

```csharp
// Infrastructure/Repositories/ImportAuditLogRepository.cs
public class ImportAuditLogRepository : IImportAuditLogRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ImportAuditLogRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(ImportAuditLog auditLog)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO import_audit_log (started_at, run_by, source_path, status)
            VALUES (@StartedAt, @RunBy, @SourcePath, @Status);
            SELECT last_insert_rowid();";

        return await connection.ExecuteScalarAsync<int>(sql, auditLog);
    }

    public async Task<IEnumerable<ImportAuditLog>> GetRecentAsync(int count = 10)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT id, started_at, completed_at, run_by, source_path, status,
                   schools_created, class_groups_created, activities_created,
                   students_created, total_failed, total_skipped,
                   exceptions_file_path, notes
            FROM import_audit_log
            ORDER BY started_at DESC
            LIMIT @Count;";

        return await connection.QueryAsync<ImportAuditLog>(sql, new { Count = count });
    }
}
```

### CLI History Command

```bash
dotnet run --project apps/backend/src/Api import history

=== IMPORT HISTORY ===
ID  | Date       | Status              | Created | Failed
----+------------+---------------------+---------+--------
3   | 2026-01-03 | CompletedWithErrors | 410     | 3
2   | 2026-01-02 | Completed           | 408     | 0
1   | 2026-01-01 | Failed              | 0       | -
```

### Previous Story Dependencies

- **Story 7.4** provides: Import execution to log

### Existing Infrastructure

The next available migration script number is `011` (after existing `010_CreateAttendance.sql`). Repository follows the same pattern as `AttendanceRepository`, `StudentRepository`, etc.

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.5]
- [Source: _bmad-output/planning-artifacts/prd.md#FR12]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

