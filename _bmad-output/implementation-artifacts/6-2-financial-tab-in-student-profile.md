# Story 6.2: Financial Tab in Student Profile

Status: ready-for-dev

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

- [ ] Task 1: Create FinancialTab component (AC: #1)
  - [ ] Replace placeholder in student profile
  - [ ] Fetch billing summary from API using Angular service with Signals
  - [ ] Display balance, last payment, outstanding count
- [ ] Task 2: Create invoices list (AC: #2, #3)
  - [ ] Display table with Date, Amount, Status
  - [ ] Status chips (Pending/Paid/Overdue)
  - [ ] Click to view details
- [ ] Task 3: Create payments list (AC: #2)
  - [ ] Display table with Date, Amount, Receipt #
- [ ] Task 4: Style summary section
  - [ ] Balance with color indicator
  - [ ] Clear layout with DaisyUI

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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
