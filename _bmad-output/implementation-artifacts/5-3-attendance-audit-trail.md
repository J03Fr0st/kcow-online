# Story 5.3: Attendance Audit Trail

Status: ready-for-dev

## Story

As an **admin**,
I want **to see an audit trail for attendance corrections**,
so that **I have traceability for any changes (FR14)**.

## Acceptance Criteria

1. **Given** I update an attendance record
   **When** the change is saved
   **Then** an audit log entry is created with:
   - Timestamp, Previous value, New value, User who made the change

2. **Given** I view an attendance record
   **When** I click "View History"
   **Then** an Audit Trail Panel appears showing all changes to that record

3. **And** the audit trail is read-only

4. **And** this fulfills FR14 for attendance

## Tasks / Subtasks

- [ ] Task 1: Create AuditLog entity and DbUp migration (AC: #1)
  - [ ] Create `AuditLog.cs` in `Domain/Entities/` with EntityType, EntityId, Field, OldValue, NewValue, ChangedBy, ChangedAt
  - [ ] Create DbUp migration script `Infrastructure/Migrations/Scripts/011_CreateAuditLog.sql`
  - [ ] No EF Core navigation properties -- plain POCO entity
- [ ] Task 2: Create AuditLog repository (AC: #1)
  - [ ] Create `IAuditLogRepository` interface in `Application/Interfaces/`
  - [ ] Create `AuditLogRepository` in `Infrastructure/Repositories/` using Dapper + `IDbConnectionFactory`
  - [ ] Methods: `CreateAsync`, `GetByEntityAsync(entityType, entityId)`
  - [ ] Register in `Infrastructure/DependencyInjection.cs`
- [ ] Task 3: Implement audit logging service (AC: #1)
  - [ ] Create `IAuditService` interface in `Application/Interfaces/`
  - [ ] Create `AuditService` in `Infrastructure/Audit/` using `IAuditLogRepository`
  - [ ] Log changes on attendance update by comparing old vs new values
  - [ ] Capture user from auth context
  - [ ] Register in `Infrastructure/DependencyInjection.cs`
- [ ] Task 4: Create audit retrieval endpoint (AC: #2)
  - [ ] GET `/api/audit-log?entityType=Attendance&entityId={id}`
  - [ ] Return list of audit entries via `IAuditLogRepository.GetByEntityAsync()`
- [ ] Task 5: Create AuditTrailPanel component (AC: #2, #3)
  - [ ] Display audit history in drawer or expandable panel
  - [ ] Show timestamp, old/new values, user
  - [ ] Read-only display
- [ ] Task 6: Add "View History" button to attendance row (AC: #2)
  - [ ] Open AuditTrailPanel on click

## Dev Notes

### Architecture Compliance

This story follows the project's **Dapper + DbUp** architecture (established in Story 0.1):
- **No EF Core** -- all data access uses Dapper via `IDbConnectionFactory`
- **Repository pattern**: `IAuditLogRepository` in `Application/Interfaces/`, `AuditLogRepository` in `Infrastructure/Repositories/`
- **DbUp migration**: SQL script in `Infrastructure/Migrations/Scripts/` with sequential numbering
- **Parameterized SQL only**, snake_case column names
- **No navigation properties** -- use explicit SQL JOINs where needed
- **DI registration** in `Infrastructure/DependencyInjection.cs`

### DbUp Migration Script (011_CreateAuditLog.sql)

```sql
-- Create AuditLog table for tracking entity changes
CREATE TABLE IF NOT EXISTS "audit_log" (
    "id" INTEGER NOT NULL CONSTRAINT "PK_audit_log" PRIMARY KEY AUTOINCREMENT,
    "entity_type" TEXT NOT NULL,
    "entity_id" INTEGER NOT NULL,
    "field" TEXT NOT NULL,
    "old_value" TEXT NULL,
    "new_value" TEXT NULL,
    "changed_by" TEXT NOT NULL,
    "changed_at" TEXT NOT NULL DEFAULT (datetime('now'))
);

CREATE INDEX IF NOT EXISTS "IX_audit_log_entity" ON "audit_log" ("entity_type", "entity_id");
CREATE INDEX IF NOT EXISTS "IX_audit_log_changed_at" ON "audit_log" ("changed_at");
```

### AuditLog Entity (Plain POCO -- no navigation properties)

```csharp
namespace Kcow.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}
```

### Repository Pattern (Dapper)

```csharp
// Application/Interfaces/IAuditLogRepository.cs
public interface IAuditLogRepository
{
    Task<int> CreateAsync(AuditLog entry, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default);
}

// Infrastructure/Repositories/AuditLogRepository.cs
public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AuditLogRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(AuditLog entry, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            INSERT INTO audit_log (entity_type, entity_id, field, old_value, new_value, changed_by, changed_at)
            VALUES (@EntityType, @EntityId, @Field, @OldValue, @NewValue, @ChangedBy, @ChangedAt)
            RETURNING id";
        return await connection.QuerySingleAsync<int>(sql, entry);
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, entity_type, entity_id, field, old_value, new_value, changed_by, changed_at
            FROM audit_log
            WHERE entity_type = @EntityType AND entity_id = @EntityId
            ORDER BY changed_at DESC";
        return await connection.QueryAsync<AuditLog>(sql, new { EntityType = entityType, EntityId = entityId });
    }
}
```

### Audit Trail Panel

```
+---------------------------------------------+
| Audit History - Attendance #123              |
+---------------------------------------------+
| 2026-01-02 14:30 by admin@kcow.co.za        |
| Status: Absent -> Present                    |
|                                              |
| 2026-01-02 10:15 by admin@kcow.co.za        |
| Notes: "" -> "Marked by teacher"             |
+---------------------------------------------+
```

### Previous Story Dependencies

- **Story 5.1** provides: Attendance API (Dapper + DbUp backend)
- **Story 5.2** provides: Attendance tab where audit will be shown

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 5.3]
- [Source: _bmad-output/planning-artifacts/prd.md#FR14]
- [Source: _bmad-output/planning-artifacts/ux-design-specification.md#Audit Trail Panel]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
