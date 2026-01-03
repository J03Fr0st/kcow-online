# Story 6.1: Billing Entity & API Endpoints

Status: ready-for-dev

## Story

As a **developer**,
I want **the Billing domain entities and REST API endpoints**,
so that **financial data can be tracked per student**.

## Acceptance Criteria

1. **Given** the backend project with Student entity
   **When** the billing entities are created
   **Then** the following entities exist:
   - `Invoice`: Id, StudentId (FK), InvoiceDate, Amount, DueDate, Status (Pending/Paid/Overdue), Notes
   - `Payment`: Id, StudentId (FK), InvoiceId (FK, optional), PaymentDate, Amount, PaymentMethod, ReceiptNumber, Notes
   - `Receipt`: Id, PaymentId (FK), ReceiptDate, ReceiptNumber

2. **And** EF Core configuration and migrations create the billing tables

3. **And** `/api/billing` endpoints support:
   - GET `/api/students/:id/billing` (billing summary with balance)
   - GET `/api/students/:id/invoices` (list invoices)
   - POST `/api/students/:id/invoices` (create invoice)
   - GET `/api/students/:id/payments` (list payments)
   - POST `/api/students/:id/payments` (record payment)

4. **And** endpoints require authentication

## Tasks / Subtasks

- [ ] Task 1: Create Invoice entity (AC: #1)
  - [ ] Create Invoice.cs with all properties
  - [ ] Add InvoiceStatus enum
- [ ] Task 2: Create Payment entity (AC: #1)
  - [ ] Create Payment.cs
  - [ ] Add PaymentMethod enum
- [ ] Task 3: Create Receipt entity (AC: #1)
  - [ ] Create Receipt.cs linked to Payment
- [ ] Task 4: Configure EF Core (AC: #2)
  - [ ] Configure all tables with relationships
  - [ ] Create and apply migration
- [ ] Task 5: Create billing service (AC: #3)
  - [ ] Calculate balance
  - [ ] CRUD for invoices and payments
  - [ ] Generate receipt numbers
- [ ] Task 6: Create endpoints (AC: #3, #4)
  - [ ] Add billing endpoints to StudentsController or BillingController

## Dev Notes

### Billing Entities

```csharp
public class Invoice
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public decimal Amount { get; set; }
    public DateOnly DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Student Student { get; set; } = null!;
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public enum InvoiceStatus { Pending, Paid, Overdue }

public class Payment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int? InvoiceId { get; set; }
    public DateOnly PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Student Student { get; set; } = null!;
    public Invoice? Invoice { get; set; }
}

public enum PaymentMethod { Cash, Card, EFT, Other }
```

### Billing Summary Response

```json
{
  "studentId": 123,
  "currentBalance": 1500.00,
  "totalInvoiced": 3000.00,
  "totalPaid": 1500.00,
  "overdueAmount": 500.00,
  "lastPaymentDate": "2026-01-01",
  "lastPaymentAmount": 500.00,
  "outstandingInvoicesCount": 2
}
```

### Previous Story Dependencies

- **Story 4.1** provides: Student entity

### References

- [Source: _bmad-output/planning-artifacts/epics.md#Story 6.1]

## Dev Agent Record

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List
