# Story 6.3: Record Payment

Status: ready-for-dev

## Story

As an **admin**,
I want **to record a payment for a student**,
so that **I can track payments against invoices**.

## Acceptance Criteria

1. **Given** I am on the student's Financial tab
   **When** I click "Record Payment"
   **Then** an inline form appears with:
   - Amount input
   - Payment method dropdown
   - Optional invoice to apply against
   - Notes field

2. **Given** I submit a valid payment
   **When** it saves
   **Then** the payment appears in the payments list
   **And** the balance is updated
   **And** a receipt number is generated

3. **And** if applied to an invoice, the invoice status updates

## Tasks / Subtasks

- [ ] Task 1: Create payment form (AC: #1)
  - [ ] Inline form in Financial tab
  - [ ] Amount input with validation
  - [ ] Payment method dropdown (Cash, Card, EFT, Other)
  - [ ] Invoice dropdown (optional)
  - [ ] Notes field
- [ ] Task 2: Submit payment (AC: #2)
  - [ ] Call POST payment API
  - [ ] Refresh payments list
  - [ ] Recalculate balance
  - [ ] Show generated receipt number
- [ ] Task 3: Update invoice (AC: #3)
  - [ ] Backend logic to update invoice status when paid
  - [ ] Refresh invoices list

## Dev Notes

### Payment Form

```html
<form class="card" (ngSubmit)="onSubmit()">
  <div class="card-body">
    <h3>Record Payment</h3>
    
    <app-form-field label="Amount (R)">
      <input type="number" formControlName="amount" min="0.01" step="0.01" />
    </app-form-field>
    
    <app-form-field label="Payment Method">
      <select formControlName="paymentMethod">
        <option value="Cash">Cash</option>
        <option value="Card">Card</option>
        <option value="EFT">EFT</option>
        <option value="Other">Other</option>
      </select>
    </app-form-field>
    
    <app-form-field label="Apply to Invoice (optional)">
      <select formControlName="invoiceId">
        <option value="">-- No specific invoice --</option>
        @for (invoice of pendingInvoices(); track invoice.id) {
          <option [value]="invoice.id">{{ invoice.invoiceDate }} - R {{ invoice.amount }}</option>
        }
      </select>
    </app-form-field>
    
    <app-form-field label="Notes">
      <textarea formControlName="notes"></textarea>
    </app-form-field>
    
    <div class="card-actions">
      <button type="button" class="btn" (click)="cancel()">Cancel</button>
      <button type="submit" class="btn btn-primary">Save Payment</button>
    </div>
  </div>
</form>
```

### Receipt Number Generation

Backend generates unique receipt number: `RCP-{YYYYMMDD}-{sequence}`

### Previous Story Dependencies

- **Story 6.2** provides: Financial tab

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.3]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
