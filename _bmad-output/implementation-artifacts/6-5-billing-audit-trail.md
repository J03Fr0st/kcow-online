# Story 6.5: Billing Audit Trail

Status: ready-for-dev

## Story

As an **admin**,
I want **to see an audit trail for billing changes**,
so that **I have traceability for financial corrections (FR14)**.

## Acceptance Criteria

1. **Given** I update a payment or invoice record
   **When** the change is saved
   **Then** an audit log entry is created with:
   - Timestamp, Previous value, New value, User who made the change

2. **Given** I view a billing record
   **When** I click "View History"
   **Then** an Audit Trail Panel appears showing all changes

3. **And** the audit trail is read-only

4. **And** this fulfills FR14 for billing

## Tasks / Subtasks

- [ ] Task 1: Extend audit logging for billing (AC: #1)
  - [ ] Add audit logging to invoice updates using Dapper-based `IAuditLogRepository` from Story 5.3
  - [ ] Add audit logging to payment updates using Dapper-based `IAuditLogRepository` from Story 5.3
  - [ ] Reuse audit service and repository from Story 5.3
- [ ] Task 2: Add "View History" to billing records (AC: #2)
  - [ ] Add button to invoice rows
  - [ ] Add button to payment rows
- [ ] Task 3: Display billing audit (AC: #2, #3)
  - [ ] Reuse AuditTrailPanel from Story 5.3
  - [ ] Filter by Invoice or Payment entity type

## Dev Notes

### Audit Logging for Billing

Reuse the Dapper-based audit infrastructure from Story 5.3:
- `IAuditLogRepository` (in `Application/Interfaces/`) for data access
- `AuditLogRepository` (in `Infrastructure/Repositories/`) using `IDbConnectionFactory` + Dapper
- EntityType: "Invoice" or "Payment"
- Log changes to: Amount, Status, Notes

### Backend Pattern

```csharp
// Reuse IAuditLogRepository from Story 5.3
await _auditLogRepository.CreateAsync(new AuditLog
{
    EntityType = "Invoice",
    EntityId = invoice.Id,
    FieldName = "Status",
    OldValue = oldStatus.ToString(),
    NewValue = newStatus.ToString(),
    ChangedBy = currentUser,
    ChangedAt = DateTime.UtcNow.ToString("o")
});
```

### Previous Story Dependencies

- **Story 5.3** provides: `IAuditLogRepository`, `AuditLogRepository` (Dapper-based), and AuditTrailPanel component
- **Story 6.2** provides: Financial tab

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.5]
- [Source: _bmad-output/planning-artifacts/prd.md#FR14]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
