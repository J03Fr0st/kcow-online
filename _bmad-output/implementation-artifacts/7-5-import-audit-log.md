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

- [ ] Task 1: Create ImportAuditLog entity (AC: #1)
  - [ ] Create entity with timestamp, user, sourceFiles, counts
  - [ ] Configure EF Core
  - [ ] Apply migration
- [ ] Task 2: Log import runs (AC: #1)
  - [ ] Create audit entry at import start
  - [ ] Update with final counts on completion
- [ ] Task 3: Create query endpoint (AC: #2)
  - [ ] GET /api/import/audit-log
  - [ ] Return list of import runs with stats
- [ ] Task 4: CLI output (AC: #2)
  - [ ] Add `import history` command to show recent imports

## Dev Notes

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

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.5]
- [Source: _bmad-output/planning-artifacts/prd.md#FR12]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
