# Story 6.8: E2E Tests - Billing Management & Financial Accuracy

Status: ready-for-dev

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

- [ ] Task 1: Set Up E2E Test Infrastructure (AC: #7, #8)
  - [ ] Create `e2e/billing/` test directory
  - [ ] Create test fixtures with students, invoices, payments
  - [ ] Include records with various balance states

- [ ] Task 2: Implement Financial Tab Tests (AC: #1)
  - [ ] Test navigation to Financial tab
  - [ ] Test billing summary display
  - [ ] Test invoices list display
  - [ ] Test payments list display
  - [ ] Test updates after changes

- [ ] Task 3: Implement Record Payment Tests (AC: #2)
  - [ ] Test "Record Payment" button
  - [ ] Test form display and fields
  - [ ] Test payment submission
  - [ ] Test payments list update
  - [ ] Test balance update
  - [ ] Test receipt generation
  - [ ] Test invoice status update

- [ ] Task 4: Implement Create Invoice Tests (AC: #3)
  - [ ] Test "Create Invoice" button
  - [ ] Test form display and fields
  - [ ] Test invoice submission
  - [ ] Test invoices list update
  - [ ] Test balance update
  - [ ] Test initial "Pending" status

- [ ] Task 5: Implement Profile Header Billing Tests (AC: #4)
  - [ ] Test billing status in header
  - [ ] Test balance color indicators
  - [ ] Test status text
  - [ ] Test overdue warning

- [ ] Task 6: Implement Billing Audit Trail Tests (AC: #5, #9)
  - [ ] Test audit log creation on update
  - [ ] Test audit entry content
  - [ ] Test "View History" button
  - [ ] Test Audit Trail Panel display
  - [ ] Test read-only nature

- [ ] Task 7: Implement Financial Calculation Tests (AC: #6, #9)
  - [ ] Test balance calculation on invoice creation
  - [ ] Test balance calculation on payment
  - [ ] Test applied payment calculations
  - [ ] Test accuracy across scenarios

- [ ] Task 8: Validate Epic 6 Completion (AC: #10)
  - [ ] Run full test suite
  - [ ] Document test coverage
  - [ ] Fix any failing tests

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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|------|--------|
| 2026-01-06 | Story file created from backlog |
| 2026-02-06 | Updated to reference Dapper + DbUp architecture (no EF Core) |
