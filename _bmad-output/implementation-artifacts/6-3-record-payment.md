# Story 6.3: Record Payment

Status: review

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

- [x] Task 1: Create payment form (AC: #1)
  - [x] Inline form in Financial tab
  - [x] Amount input with validation
  - [x] Payment method dropdown (Cash, Card, EFT, Other)
  - [x] Invoice dropdown (optional)
  - [x] Notes field
- [x] Task 2: Submit payment (AC: #2)
  - [x] Call POST payment API (backed by Dapper repository)
  - [x] Refresh payments list
  - [x] Recalculate balance
  - [x] Show generated receipt number
- [x] Task 3: Update invoice (AC: #3)
  - [x] Backend logic to update invoice status when paid (via IInvoiceRepository)
  - [x] Refresh invoices list

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

Backend generates unique receipt number via `BillingService`: `RCP-{YYYYMMDD}-{sequence}`

### Frontend Architecture

- Angular 21 with Signals + RxJS
- Payment form uses reactive forms
- Calls POST `/api/students/:id/payments` endpoint

### Previous Story Dependencies

- **Story 6.2** provides: Financial tab

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.3]

## Dev Agent Record

### Agent Model Used

Claude Opus 4.6 (via glm-5)

### Debug Log References

None - implementation completed without issues.

### Completion Notes List

- Added payment form signals (showPaymentForm, isSavingPayment, paymentForm)
- Added payment method options (Cash, Card, Transfer, Other)
- Added pendingInvoices computed signal for invoice dropdown
- Implemented showAddPaymentForm, cancelPaymentForm, updatePaymentForm methods
- Implemented isPaymentFormValid validation
- Implemented submitPayment method that calls BillingService.createPayment
- Updated HTML template with payment form card
- Updated payments table to show payment method
- Added 10 new unit tests for payment functionality
- All 39 tests pass
- Frontend builds successfully

### File List

**Modified Files:**
- apps/frontend/src/app/features/students/student-profile/components/financial-tab/financial-tab.component.ts
- apps/frontend/src/app/features/students/student-profile/components/financial-tab/financial-tab.component.html
- apps/frontend/src/app/features/students/student-profile/components/financial-tab/financial-tab.component.spec.ts

## Change Log

- 2026-02-13: Implemented payment recording functionality with inline form, validation, and API integration (Story 6.3)
