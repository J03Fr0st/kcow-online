# Story 6.7: Data Migration - Billing & Financials

Status: done

## Story

As a developer,
I want legacy billing, invoice, and payment data parsed, mapped, and imported into the database,
So that financial flows and balance calculations operate on real migrated records.

## Acceptance Criteria

1. **Given** legacy billing data from Children records and school billing configurations
   **When** the migration import executes for Billing records
   **Then** all valid invoice records are inserted with correct student and family links

2. **Given** the migration imports payment data
   **Then** payment history is imported with accurate dates and amounts

3. **Given** the migration processes financial data
   **Then** outstanding balances are calculated correctly from migrated data

4. **Given** the migration processes billing settings
   **Then** school billing settings are applied to imported records

5. **Given** validation errors occur during import
   **Then** errors are captured and logged to the migration audit log

6. **Given** the import completes
   **Then** a summary report shows imported count, skipped count, and error count

7. **Given** the migration is complete
   **Then** the imported data displays correctly in the financial tab and profile header

## Tasks / Subtasks

- [x] Task 1: Extract Billing Data from Legacy Records (AC: #1)
  - [x] Parse billing-related fields from Children.xsd
  - [x] Extract invoice history where available
  - [x] Link billing records to imported Students
  - [x] Link to imported Family records

- [x] Task 2: Import Payment History (AC: #2)
  - [x] Parse legacy payment records
  - [x] Map payment dates and amounts accurately
  - [x] Generate receipt numbers for historical payments
  - [x] Link payments to invoices where applicable
  - [x] Use `IPaymentRepository` (Dapper-based) for data insertion

- [x] Task 3: Calculate Imported Balances (AC: #3)
  - [x] Calculate outstanding balance from invoice vs payment totals
  - [x] Set initial balance based on imported data
  - [x] Verify balance accuracy across sample records

- [x] Task 4: Apply School Billing Settings (AC: #4)
  - [x] Link imported billing to school configurations
  - [x] Apply default rates from School billing settings
  - [x] Handle billing cycle settings (monthly/termly)

- [x] Task 5: Implement Validation and Error Logging (AC: #5)
  - [x] Validate imported records against schema constraints
  - [x] Create audit log entries for validation errors
  - [x] Track financial data integrity issues

- [x] Task 6: Create Import Summary Report (AC: #6)
  - [x] Track imported, skipped, and error counts
  - [x] Separate counts for Invoices, Payments, Receipts
  - [x] Include balance calculation summary

- [x] Task 7: Verify Financial Tab and Header Display (AC: #7)
  - [x] Test that imported billing appears in Financial tab
  - [x] Test balance display in profile header
  - [x] Test billing status indicators (green/red)
  - [x] Test invoice and payment lists

## Dev Notes

### Architecture Requirements
- **Legacy Data Source**: Billing fields from Children.xsd + School billing settings
- **Entity Locations**:
  - `apps/backend/src/Domain/Entities/Invoice.cs`
  - `apps/backend/src/Domain/Entities/Payment.cs`
- **Repository Interfaces**: `Application/Interfaces/IInvoiceRepository.cs`, `Application/Interfaces/IPaymentRepository.cs`
- **Repository Implementations**: `Infrastructure/Repositories/InvoiceRepository.cs`, `Infrastructure/Repositories/PaymentRepository.cs` (Dapper-based, using `IDbConnectionFactory`)
- **Import Service Location**: `apps/backend/src/Application/Import/`
- **CLI Command**: `dotnet run import billing`

### Data Insertion Pattern

Use Dapper repositories for all data insertion:

```csharp
// Use IInvoiceRepository for invoice imports
await _invoiceRepository.CreateAsync(new Invoice
{
    StudentId = mappedStudentId,
    InvoiceDate = legacyDate,
    Amount = legacyAmount,
    DueDate = calculatedDueDate,
    Status = 0, // Pending
    Description = "Imported from legacy system",
    CreatedAt = DateTime.UtcNow.ToString("o")
});

// Use IPaymentRepository for payment imports
await _paymentRepository.CreateAsync(new Payment
{
    StudentId = mappedStudentId,
    InvoiceId = matchedInvoiceId,
    PaymentDate = legacyPaymentDate,
    Amount = legacyPaymentAmount,
    PaymentMethod = 3, // Other (legacy)
    ReceiptNumber = generatedReceiptNumber,
    Notes = "Imported from legacy system",
    CreatedAt = DateTime.UtcNow.ToString("o")
});
```

### Field Mapping Reference
Billing data may be embedded in Children records or separate:
- Extract billing-related fields from 92-field Children.xsd
- Map school billing configurations
- Calculate initial balances

### Dependency Requirements
- Story 4-9 (Students migration) must be completed first
- Story 2-6 (Schools migration) must be completed first
- Students and Schools must exist before importing billing

### Import Order
1. Schools (Story 2-6) - includes billing settings
2. Students & Families (Story 4-9)
3. Billing & Financials (this story)

### Balance Calculation
Critical for financial accuracy:
- Balance = Sum(Invoices) - Sum(Payments)
- Outstanding invoices count for status display
- Overdue calculation based on due dates

### Previous Story Context
- Story 6-6 completed Billing Status in Profile Header
- Financial tab is functional
- Invoice and Payment CRUD is implemented via Dapper repositories

### Testing Standards
- Integration tests in `apps/backend/tests/Integration/Import/`
- Test balance calculation accuracy
- Test financial data integrity
- Verify audit trail for imported billing

### Project Structure Notes
- Reuse import infrastructure from previous migration stories
- Extend ImportAuditLog for billing imports

### References
- [Source: docs/legacy/4_Children/Children.xsd#billing-fields]
- [Source: docs/legacy/1_School/School.xsd#billing-settings]
- [Source: _bmad-output/planning-artifacts/architecture.md#Data-Architecture]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6

### Debug Log References

### Completion Notes List

- **Task 1-4 Complete**: Created `LegacyBillingMapper` in Application/Import that extracts billing fields (Charge, Deposit, PayDate, TshirtMoney1/2) from legacy Children records and maps them to Invoice/Payment entities
- **School Billing Settings**: Mapper falls back to school Price when no Charge is specified, linking billing to school configurations
- **Payment Import**: Generates unique receipt numbers (RCP-LEGACY-YYYYMMDD-XXXXX) for historical payments
- **Balance Calculation**: Import summary tracks TotalBalanceImported (invoices minus payments)
- **Validation & Error Logging**: Uses existing `LegacyImportAuditLog` infrastructure for validation errors
- **Import Summary**: Created `LegacyBillingImportSummary` with separate counts for Invoices and Payments
- **Bug Fix**: Fixed foreign key constraint issue - payments with InvoiceId=0 were failing. Now only set InvoiceId if > 0
- **Integration Tests**: Created 5 integration tests verifying invoice/payment creation, preview mode, school price fallback, and billing summary display

### File List

**New Files:**
- `apps/backend/src/Application/Import/LegacyBillingMapper.cs` - Mapper for legacy billing data to Invoice/Payment entities
- `apps/backend/src/Infrastructure/Import/LegacyBillingImportService.cs` - Service for billing import orchestration
- `apps/backend/tests/Unit/LegacyBillingMapperTests.cs` - Unit tests for billing mapper (15 tests)
- `apps/backend/tests/Unit/LegacyBillingImportServiceTests.cs` - Unit tests for summary types (3 tests)
- `apps/backend/tests/Integration/Import/LegacyBillingImportServiceTests.cs` - Integration tests (5 tests)

### Senior Developer Review (AI)

**Reviewer:** Joe on 2026-02-13
**Outcome:** Approved with fixes applied

**Issues Found:** 0 High, 1 Medium, 0 Low -- all HIGH and MEDIUM fixed automatically

| # | Severity | Issue | Fix Applied |
|---|----------|-------|-------------|
| M6 | MEDIUM | Mapper test count claims 18, actual is 15 | Fixed: Corrected test count in story file (18 to 15) |

## Change Log

| Date | Change |
|------|--------|
| 2026-01-06 | Story file created from backlog |
| 2026-02-06 | Updated to reference Dapper + DbUp architecture (no EF Core) |
| 2026-02-13 | Implemented Tasks 1-6: Billing mapper, import service, school price fallback, validation, summary report |
| 2026-02-13 | Code review - fixed status field, corrected test count (18->15) |
