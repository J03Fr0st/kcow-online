# Story 6.8: E2E Tests - Billing Management & Financial Accuracy

Status: done

## Story

As a developer,
I want E2E tests covering billing workflows and financial data integrity,
So that invoicing, payments, and balance calculations are validated end-to-end.

## Acceptance Criteria

1. **Given** the application is running
   **When** I run E2E tests for Financial Tab in Student Profile
   **Then** all scenarios pass:
   - User can navigate to Financial tab
   - Billing summary shows: current balance, last payment date/amount, outstanding invoices
   - Invoices list displays with Date, Amount, Status
   - Payments list displays with Date, Amount, Receipt #
   - Summary and lists update correctly after changes

2. **Given** the application is running
   **When** I run E2E tests for Record Payment
   **Then** all scenarios pass:
   - User can click "Record Payment" to open inline form
   - Form displays: amount input, payment method dropdown, optional invoice, notes
   - User can submit valid payment
   - Payment appears in payments list
   - Balance is updated correctly
   - Receipt number is generated automatically
   - If applied to invoice, invoice status updates

3. **Given** the application is running
   **When** I run E2E tests for Create Invoice
   **Then** all scenarios pass:
   - User can click "Create Invoice" to open inline form
   - Form displays: amount input, due date picker, description/notes
   - User can submit valid invoice
   - Invoice appears in invoices list
   - Balance is updated
   - Status is set to "Pending"

4. **Given** the application is running
   **When** I run E2E tests for Billing Status in Profile Header
   **Then** all scenarios pass:
   - Profile header third column shows billing status
   - Current balance displays with color indicator (green if 0, red if outstanding)
   - Text shows "Up to date" or "Balance due"
   - Warning indicator displays for overdue invoices

5. **Given** the application is running (FR14 - Critical)
   **When** I run E2E tests for Billing Audit Trail
   **Then** all scenarios pass:
   - User updates a payment or invoice record
   - Audit log entry is created with timestamp, previous value, new value, user
   - User can click "View History" on billing record
   - Audit Trail Panel appears showing all changes
   - Audit trail is read-only

6. **Given** the application is running
   **When** I run E2E tests for Financial Calculations
   **Then** all scenarios pass:
   - Balance updates correctly when invoice is created
   - Balance updates correctly when payment is recorded
   - Applied payments reduce invoice balance
   - Calculations are accurate and consistent

7. **Given** E2E tests are organized
   **Then** tests are in `e2e/billing/`

8. **Given** tests execute
   **Then** tests use seeded test data (students, existing invoices/payments)

9. **Given** financial testing requirements
   **Then** financial calculations and audit trails are thoroughly tested

10. **Given** Epic 6 completion requirements
    **Then** all tests must pass for Epic 6 completion

## Tasks / Subtasks

- [x] Task 1: Set Up E2E Test Infrastructure (AC: #7, #8)
  - [x] Create `e2e/billing/` test directory
  - [x] Create test fixtures with students, invoices, payments
  - [x] Include records with various balance states

- [x] Task 2: Implement Financial Tab Tests (AC: #1)
  - [x] Test navigation to Financial tab
  - [x] Test billing summary display
  - [x] Test invoices list display
  - [x] Test payments list display
  - [x] Test updates after changes

- [x] Task 3: Implement Record Payment Tests (AC: #2)
  - [x] Test "Record Payment" button
  - [x] Test form display and fields
  - [x] Test payment submission
  - [x] Test payments list update
  - [x] Test balance update
  - [x] Test receipt generation
  - [x] Test invoice status update

- [x] Task 4: Implement Create Invoice Tests (AC: #3)
  - [x] Test "Create Invoice" button
  - [x] Test form display and fields
  - [x] Test invoice submission
  - [x] Test invoices list update
  - [x] Test balance update
  - [x] Test initial "Pending" status

- [x] Task 5: Implement Profile Header Billing Tests (AC: #4)
  - [x] Test billing status in header
  - [x] Test balance color indicators
  - [x] Test status text
  - [x] Test overdue warning

- [x] Task 6: Implement Billing Audit Trail Tests (AC: #5, #9)
  - [x] Test audit log creation on update
  - [x] Test audit entry content
  - [x] Test "View History" button
  - [x] Test Audit Trail Panel display
  - [x] Test read-only nature

- [x] Task 7: Implement Financial Calculation Tests (AC: #6, #9)
  - [x] Test balance calculation on invoice creation
  - [x] Test balance calculation on payment
  - [x] Test applied payment calculations
  - [x] Test accuracy across scenarios

- [x] Task 8: Validate Epic 6 Completion (AC: #10)
  - [x] Run full test suite
  - [x] Document test coverage
  - [x] Fix any failing tests

## Dev Notes

### Architecture Requirements
- **E2E Test Location**: `apps/frontend/e2e/billing/`
- **Test Framework**: Playwright
- **Test Configuration**: `apps/frontend/playwright.config.ts`
- **Backend**: Dapper + DbUp architecture (no EF Core)

### Test Organization Pattern
```
e2e/billing/
  invoicing.spec.ts           # Invoice creation tests
  payments.spec.ts            # Payment recording tests
  financial-calculations.spec.ts  # Balance and accuracy tests
```

### Critical Test Scenarios
**FR14 - Billing Audit Trail (Critical):**
- Audit log must be created for every billing update
- Audit entries must capture: timestamp, old value, new value, user

**Financial Calculations (Critical):**
- Balance = Sum(Invoices) - Sum(Payments)
- Test with multiple invoices and payments
- Test edge cases: exact balance = 0, negative balance, etc.

### Balance Color Indicators
Test these exact mappings:
- Balance = 0 -> green, "Up to date"
- Balance > 0 -> red, "Balance due"
- Overdue invoices -> warning indicator

### Test Data Strategy
- Seed students with various billing states
- Include students with outstanding balances
- Include students with overdue invoices
- Include students with up-to-date billing

### Previous Story Context
- Stories 6-1 to 6-6 implemented full Billing functionality using Dapper + DbUp
- Story 6-7 added data migration for seed data
- Financial tab and profile header billing are functional

### Testing Standards
- Financial calculations must be accurate
- FR14 audit trail tests are critical
- All balance scenarios must pass

### References
- [Source: _bmad-output/planning-artifacts/architecture.md#E2E-Testing-Strategy]
- [Source: _bmad-output/planning-artifacts/epics.md#Story-6.8]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6

### Debug Log References

### Completion Notes List

- Created 6 E2E test files in `apps/frontend/e2e/billing/` directory
- Total of ~52 test cases covering all acceptance criteria
- Tests use API for test data setup (via `page.request.post`)
- Tests clean up by deleting student (cascades to invoices/payments)
- Adjusted tests to use actual API endpoints: `/api/students/{studentId}/invoices` and `/api/students/{studentId}/payments`
- Audit trail tests use `/api/audit-log` endpoint

### File List

- `apps/frontend/e2e/billing/financial-tab.spec.ts` - Financial tab E2E tests (5 tests)
- `apps/frontend/e2e/billing/payments.spec.ts` - Record payment E2E tests (10 tests)
- `apps/frontend/e2e/billing/invoicing.spec.ts` - Create invoice E2E tests (10 tests)
- `apps/frontend/e2e/billing/profile-header-billing.spec.ts` - Profile header billing E2E tests (8 tests)
- `apps/frontend/e2e/billing/billing-audit-trail.spec.ts` - Billing audit trail E2E tests (11 tests)
- `apps/frontend/e2e/billing/financial-calculations.spec.ts` - Financial calculations E2E tests (9 tests)

### Senior Developer Review (AI)

**Reviewer:** Joe on 2026-02-13
**Outcome:** Approved with fixes applied

**Issues Found:** 0 High, 1 Medium, 0 Low -- all HIGH and MEDIUM fixed automatically

| # | Severity | Issue | Fix Applied |
|---|----------|-------|-------------|
| M7 | MEDIUM | Per-file E2E test counts wrong in 3 of 6 files | Fixed: Corrected per-file test counts in story file |

## Change Log

| Date | Change |
|------|--------|
| 2026-01-06 | Story file created from backlog |
| 2026-02-06 | Updated to reference Dapper + DbUp architecture (no EF Core) |
| 2026-02-13 | Implemented all E2E tests for billing functionality |
| 2026-02-13 | Code review - corrected per-file test counts |
