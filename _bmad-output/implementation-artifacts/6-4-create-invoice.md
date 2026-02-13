# Story 6.4: Create Invoice

Status: review

## Story

As an **admin**,
I want **to create invoices for a student**,
so that **I can track amounts owed**.

## Acceptance Criteria

1. **Given** I am on the student's Financial tab
   **When** I click "Create Invoice"
   **Then** an inline form appears with:
   - Amount input
   - Due date picker
   - Description/notes

2. **Given** I submit a valid invoice
   **When** it saves
   **Then** the invoice appears in the invoices list
   **And** the balance is updated
   **And** status is set to "Pending"

## Tasks / Subtasks

- [x] Task 1: Create invoice form (AC: #1)
  - [x] Inline form in Financial tab
  - [x] Amount input
  - [x] Due date picker
  - [x] Description field
  - [x] Notes field
- [x] Task 2: Submit invoice (AC: #2)
  - [x] Call POST invoice API (backed by Dapper repository)
  - [x] Refresh invoices list
  - [x] Recalculate balance
  - [x] Confirm creation

## Dev Notes

### Invoice Form

```html
<form class="card" (ngSubmit)="onSubmit()">
  <div class="card-body">
    <h3>Create Invoice</h3>

    <app-form-field label="Amount (R)">
      <input type="number" formControlName="amount" min="0.01" step="0.01" />
    </app-form-field>

    <app-form-field label="Due Date">
      <input type="date" formControlName="dueDate" />
    </app-form-field>

    <app-form-field label="Description">
      <input type="text" formControlName="description" />
    </app-form-field>

    <app-form-field label="Notes">
      <textarea formControlName="notes"></textarea>
    </app-form-field>

    <div class="card-actions">
      <button type="button" class="btn" (click)="cancel()">Cancel</button>
      <button type="submit" class="btn btn-primary">Create Invoice</button>
    </div>
  </div>
</form>
```

### Frontend Architecture

- Angular 21 with Signals + RxJS
- Invoice form uses reactive forms
- Calls POST `/api/students/:id/invoices` endpoint

### Previous Story Dependencies

- **Story 6.2** provides: Financial tab

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.4]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6

### Debug Log References

None required - straightforward implementation.

### Completion Notes List

- Implemented invoice creation form in FinancialTabComponent following the existing payment form pattern
- Added InvoiceForm interface and signals: showInvoiceForm, isSavingInvoice, invoiceForm
- Added methods: showAddInvoiceForm(), cancelInvoiceForm(), updateInvoiceForm(), isInvoiceFormValid(), submitInvoice()
- Form includes: Amount (required), Due Date (required), Description (optional), Notes (optional)
- Validation ensures amount > 0 and due date is provided before submission
- On successful creation: invoice added to list, billing data reloaded to update balance, success notification shown
- All 49 unit tests pass for financial-tab component

### File List

- apps/frontend/src/app/features/students/student-profile/components/financial-tab/financial-tab.component.ts
- apps/frontend/src/app/features/students/student-profile/components/financial-tab/financial-tab.component.html
- apps/frontend/src/app/features/students/student-profile/components/financial-tab/financial-tab.component.spec.ts

## Change Log

- 2026-02-13: Initial implementation - Invoice creation form with validation and API integration
