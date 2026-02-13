# Story 6.2: Financial Tab in Student Profile

Status: done

## Story

As an **admin**,
I want **to view and manage billing in the student profile**,
so that **I can track and update financial status without leaving the profile**.

## Acceptance Criteria

1. **Given** I am on the student profile
   **When** I select the "Financial" tab
   **Then** I see a billing summary showing:
   - Current balance
   - Last payment date and amount
   - Outstanding invoices count

2. **And** below the summary, I see two sections:
   - Invoices list (Date, Amount, Status)
   - Payments list (Date, Amount, Receipt #)

3. **Given** I click on an invoice
   **When** the detail appears
   **Then** I can view invoice details and status

## Tasks / Subtasks

- [x] Task 1: Create FinancialTab component (AC: #1)
  - [x] Replace placeholder in student profile
  - [x] Fetch billing summary from API using Angular service with Signals
  - [x] Display balance, last payment, outstanding count
- [x] Task 2: Create invoices list (AC: #2, #3)
  - [x] Display table with Date, Amount, Status
  - [x] Status chips (Pending/Paid/Overdue)
  - [x] Click to view details
- [x] Task 3: Create payments list (AC: #2)
  - [x] Display table with Date, Amount, Receipt #
- [x] Task 4: Style summary section
  - [x] Balance with color indicator
  - [x] Clear layout with DaisyUI

## Dev Notes

### Financial Tab Layout

```
+-------------------------------------------------------------+
| Financial Summary                                           |
+-------------------------------------------------------------+
| Current Balance: R 1,500.00  | Last Payment: R 500 on 01/01 |
| Outstanding Invoices: 2       |                              |
+-------------------------------------------------------------+
| Invoices                                    [Create Invoice] |
| Date       | Amount    | Status    |                        |
| 2026-01-01 | R 1,000   | [Pending] |                        |
| 2025-12-01 | R 2,000   | [Paid]    |                        |
+-------------------------------------------------------------+
| Payments                                   [Record Payment]  |
| Date       | Amount    | Receipt #  |                       |
| 2026-01-01 | R 500     | RCP-001234 |                       |
+-------------------------------------------------------------+
```

### Frontend Architecture

- Angular 21 with Signals + RxJS
- Billing service calls REST API endpoints from Story 6.1
- Components use DaisyUI for styling

### Previous Story Dependencies

- **Story 4.6** provides: Profile tabs
- **Story 6.1** provides: Billing API (Dapper-based backend)

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.2]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (via glm-5)

### Debug Log References

None - implementation completed without issues.

### Completion Notes List

- Created billing model types matching backend DTOs (BillingSummary, Invoice, Payment)
- Created BillingService with methods for getBillingSummary, getInvoices, getPayments
- Created FinancialTabComponent with Angular Signals for reactive state management
- Implemented financial summary card with balance, last payment, and outstanding invoices
- Implemented invoices list with status badges (Pending=warning, Paid=success, Overdue=error, Cancelled=ghost)
- Implemented payments list with receipt numbers
- Added invoice detail modal on click
- Added balance color indicator (green for <=0, red for >0)
- Integrated FinancialTabComponent into student-profile.page
- All 29 unit tests pass for FinancialTabComponent
- Frontend builds successfully

### File List

**New Files:**
- apps/frontend/src/app/features/billing/models/billing.model.ts
- apps/frontend/src/app/core/services/billing.service.ts
- apps/frontend/src/app/features/students/student-profile/components/financial-tab/financial-tab.component.ts
- apps/frontend/src/app/features/students/student-profile/components/financial-tab/financial-tab.component.html
- apps/frontend/src/app/features/students/student-profile/components/financial-tab/financial-tab.component.scss
- apps/frontend/src/app/features/students/student-profile/components/financial-tab/financial-tab.component.spec.ts

**Modified Files:**
- apps/frontend/src/app/features/students/student-profile/student-profile.page.ts
- apps/frontend/src/app/features/students/student-profile/student-profile.page.html

### Senior Developer Review (AI)

**Reviewer:** Joe on 2026-02-13
**Outcome:** Approved with fixes applied

**Issues Found:** 1 High, 1 Medium, 0 Low -- all HIGH and MEDIUM fixed automatically

| # | Severity | Issue | Fix Applied |
|---|----------|-------|-------------|
| H1 | HIGH | Stale unit tests in student-profile.page.spec.ts expected placeholder text (Epic 5, Epic 6, Financial Information) replaced by real components | Fixed: Updated 4 tests to check for component elements instead of placeholder text |
| M2 | MEDIUM | isLoading race condition - loadBillingData fires 3 parallel HTTP calls but only clears isLoading in payments subscriber | Fixed: Replaced 3 separate subscribes with forkJoin for atomic loading state |

## Change Log

- 2026-02-13: Implemented Financial Tab component with billing summary, invoices list, and payments list (Story 6.2)
- 2026-02-13: Code review fixes - stale test assertions (H1), isLoading race condition (M2)
