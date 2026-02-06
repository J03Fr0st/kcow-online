# Story 7.6: Import Re-run Capability

Status: ready-for-dev

## Story

As a **developer**,
I want **to re-run the import to update or fix records**,
so that **I can iterate on data quality**.

## Acceptance Criteria

1. **Given** data has been previously imported
   **When** I run the import again
   **Then** existing records are matched by a unique key (e.g., legacy ID)
   **And** I can choose: skip existing, update existing, or fail on conflict

2. **Given** I run with `--update` flag
   **When** matching records exist
   **Then** they are updated with new values
   **And** changes are logged to the audit trail

## Tasks / Subtasks

- [ ] Task 1: Add legacy ID tracking (AC: #1)
  - [ ] Add LegacyId column to entities (nullable)
  - [ ] Create DbUp migration script (`012_AddLegacyIdColumns.sql`) in `Infrastructure/Migrations/Scripts/`
  - [ ] Use `ALTER TABLE` to add `legacy_id` (snake_case) to schools, class_groups, students, activities
  - [ ] Store legacy ID during import
- [ ] Task 2: Implement conflict detection (AC: #1)
  - [ ] Add repository methods to find records by `legacy_id` using Dapper
  - [ ] Return conflict status
- [ ] Task 3: Add conflict resolution flags (AC: #1)
  - [ ] --skip-existing: Skip records that exist
  - [ ] --update: Update existing records
  - [ ] --fail-on-conflict: Error if exists (default)
- [ ] Task 4: Implement update logic (AC: #2)
  - [ ] Update existing records with new data using repository update methods
  - [ ] Use `IDbTransaction` for transactional consistency
  - [ ] Track what changed
- [ ] Task 5: Log updates to audit trail (AC: #2)
  - [ ] Use existing `IImportAuditLogRepository` from Story 7.5
  - [ ] Log old/new values for updated fields

## Dev Notes

### Architecture Compliance

This story follows the established Dapper + DbUp architecture:

- **Migration:** DbUp script in `Infrastructure/Migrations/Scripts/012_AddLegacyIdColumns.sql`
- **Repository queries:** Use Dapper parameterized SQL for conflict detection and updates
- **Transactions:** Use `IDbTransaction` for batch update atomicity
- **No EF Core:** All database access via Dapper with explicit SQL

### DbUp Migration Script (012_AddLegacyIdColumns.sql)

```sql
-- Add legacy_id column to entities for re-import matching
ALTER TABLE schools ADD COLUMN legacy_id TEXT;
ALTER TABLE class_groups ADD COLUMN legacy_id TEXT;
ALTER TABLE activities ADD COLUMN legacy_id TEXT;
ALTER TABLE students ADD COLUMN legacy_id TEXT;

-- Create indexes for efficient conflict detection
CREATE INDEX IF NOT EXISTS idx_schools_legacy_id ON schools(legacy_id);
CREATE INDEX IF NOT EXISTS idx_class_groups_legacy_id ON class_groups(legacy_id);
CREATE INDEX IF NOT EXISTS idx_activities_legacy_id ON activities(legacy_id);
CREATE INDEX IF NOT EXISTS idx_students_legacy_id ON students(legacy_id);
```

### Conflict Detection Pattern (Dapper)

```csharp
// Add to existing repository interfaces, e.g. ISchoolRepository
Task<School?> GetByLegacyIdAsync(string legacyId);

// Repository implementation using Dapper
public async Task<School?> GetByLegacyIdAsync(string legacyId)
{
    using var connection = _connectionFactory.CreateConnection();
    const string sql = "SELECT * FROM schools WHERE legacy_id = @LegacyId;";
    return await connection.QuerySingleOrDefaultAsync<School>(sql, new { LegacyId = legacyId });
}
```

### CLI Flags

```bash
# Skip existing records
dotnet run --project apps/backend/src/Api import run --skip-existing

# Update existing records
dotnet run --project apps/backend/src/Api import run --update

# Fail on conflict (default)
dotnet run --project apps/backend/src/Api import run --fail-on-conflict
```

### Legacy ID Tracking

Add `LegacyId` to each entity:
```csharp
public class Student
{
    // ... existing properties ...

    /// <summary>
    /// Original ID from legacy Access database import.
    /// Used for re-import matching and audit trail.
    /// </summary>
    public string? LegacyId { get; set; }
}
```

### Re-import Summary

```
=== RE-IMPORT COMPLETE ===

Results:
  - New records: 5
  - Updated: 12
  - Skipped (unchanged): 393
  - Failed: 0

Updates logged to audit trail.
```

### Previous Story Dependencies

- **Story 7.4** provides: Basic import execution
- **Story 7.5** provides: Audit logging (`IImportAuditLogRepository`)
- **Story 5.3** provides: Audit trail service (reuse for update logging)

### Existing Infrastructure

- Repositories for all entity types already exist in `Infrastructure/Repositories/`
- `IDbConnectionFactory` provides connection management
- Next available migration script number is `012` (after `011_CreateImportAuditLog.sql` from Story 7.5)

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 7.6]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

