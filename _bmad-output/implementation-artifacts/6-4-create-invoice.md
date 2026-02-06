# Story 6.4: Create Invoice

Status: ready-for-dev

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

- [ ] Task 1: Create invoice form (AC: #1)
  - [ ] Inline form in Financial tab
  - [ ] Amount input
  - [ ] Due date picker
  - [ ] Description field
  - [ ] Notes field
- [ ] Task 2: Submit invoice (AC: #2)
  - [ ] Call POST invoice API (backed by Dapper repository)
  - [ ] Refresh invoices list
  - [ ] Recalculate balance
  - [ ] Confirm creation

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

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
