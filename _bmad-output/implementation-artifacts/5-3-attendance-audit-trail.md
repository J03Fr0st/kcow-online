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

- [ ] Task 1: Create AuditLog entity and table (AC: #1)
  - [ ] Create AuditLog entity with EntityType, EntityId, Field, OldValue, NewValue, ChangedBy, ChangedAt
  - [ ] Configure EF Core and apply migration
- [ ] Task 2: Implement audit logging service (AC: #1)
  - [ ] Create IAuditService interface
  - [ ] Log changes on attendance update
  - [ ] Capture user from auth context
- [ ] Task 3: Create audit retrieval endpoint (AC: #2)
  - [ ] GET `/api/audit-log?entityType=Attendance&entityId={id}`
  - [ ] Return list of audit entries
- [ ] Task 4: Create AuditTrailPanel component (AC: #2, #3)
  - [ ] Display audit history in drawer or expandable panel
  - [ ] Show timestamp, old/new values, user
  - [ ] Read-only display
- [ ] Task 5: Add "View History" button to attendance row (AC: #2)
  - [ ] Open AuditTrailPanel on click

## Dev Notes

### AuditLog Entity

```csharp
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

### Audit Trail Panel

```
┌─────────────────────────────────────────────┐
│ Audit History - Attendance #123             │
├─────────────────────────────────────────────┤
│ 2026-01-02 14:30 by admin@kcow.co.za        │
│ Status: Absent → Present                    │
│                                             │
│ 2026-01-02 10:15 by admin@kcow.co.za        │
│ Notes: "" → "Marked by teacher"             │
└─────────────────────────────────────────────┘
```

### Previous Story Dependencies

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
