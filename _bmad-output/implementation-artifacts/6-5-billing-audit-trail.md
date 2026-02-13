# Story 6.5: Billing Audit Trail

Status: review

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

- [x] Task 1: Extend audit logging for billing (AC: #1)
  - [x] Add audit logging to invoice updates using Dapper-based `IAuditLogRepository` from Story 5.3
  - [x] Add audit logging to payment updates using Dapper-based `IAuditLogRepository` from Story 5.3
  - [x] Reuse audit service and repository from Story 5.3
- [x] Task 2: Add "View History" to billing records (AC: #2)
  - [x] Add button to invoice rows
  - [x] Add button to payment rows
- [x] Task 3: Display billing audit (AC: #2, #3)
  - [x] Reuse AuditTrailPanel from Story 5.3
  - [x] Filter by Invoice or Payment entity type

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

Claude Opus 4.6

### Debug Log References

### Completion Notes List

- **Task 1 Complete**: Extended audit logging for billing by injecting `IAuditService` into `BillingService`
  - Added "Invoice" and "Payment" to valid entity types in `AuditService`
  - Added `createdBy` parameter to `CreateInvoiceAsync` and `CreatePaymentAsync` in `IBillingService`
  - Added audit logging for invoice creation (logs: "Invoice created - Amount, DueDate")
  - Added audit logging for payment creation (logs: "Payment created - Amount, Receipt")
  - Added audit logging for invoice status changes when marked as paid (logs: Status change from "Pending" to "Paid")
  - Added helper method `GetInvoiceStatusString` for converting invoice status codes to strings
  - Updated `BillingController` to pass current user from claims to service methods

- **Task 2 Complete**: Added "View History" buttons to billing records
  - Added clock icon button to invoice table rows
  - Added clock icon button to payment table rows
  - Both buttons use `$event.stopPropagation()` to prevent row selection when clicking

- **Task 3 Complete**: Display billing audit trail using existing AuditTrailPanel
  - Imported `AuditTrailPanelComponent` in FinancialTabComponent
  - Added state signals: `showAuditTrail`, `auditEntityType`, `auditEntityId`
  - Added `viewInvoiceHistory()` and `viewPaymentHistory()` methods
  - Added `closeAuditTrail()` method
  - Added billing-specific field labels to AuditTrailPanelComponent (Amount, Description, DueDate, etc.)
  - Fixed AuditTrailPanelComponent to use constructor-based effect() instead of ngOnInit-based

- **All tests passing**:
  - Backend: 12 unit tests, 11 integration tests
  - Frontend: 49 component tests

### File List

**Backend (Modified):**
- apps/backend/src/Application/Billing/IBillingService.cs
- apps/backend/src/Infrastructure/Audit/AuditService.cs
- apps/backend/src/Infrastructure/Billing/BillingService.cs
- apps/backend/src/Api/Controllers/BillingController.cs
- apps/backend/tests/Unit/BillingServiceTests.cs

**Frontend (Modified):**
- apps/frontend/src/app/features/students/student-profile/components/financial-tab/financial-tab.component.ts
- apps/frontend/src/app/features/students/student-profile/components/financial-tab/financial-tab.component.html
- apps/frontend/src/app/features/students/student-profile/components/audit-trail-panel/audit-trail-panel.component.ts
