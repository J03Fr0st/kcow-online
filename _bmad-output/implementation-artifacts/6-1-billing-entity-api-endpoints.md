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
   - `Invoice`: Id, StudentId (FK), InvoiceDate, Amount, DueDate, Status (0=Pending, 1=Paid, 2=Overdue), Description, Notes, CreatedAt
   - `Payment`: Id, StudentId (FK), InvoiceId (FK, optional), PaymentDate, Amount, PaymentMethod (0=Cash, 1=Card, 2=EFT, 3=Other), ReceiptNumber, Notes, CreatedAt

2. **And** DbUp migration scripts create the billing tables (`invoices`, `payments`, `receipts`)

3. **And** repository interfaces (`IInvoiceRepository`, `IPaymentRepository`) are created in `Application/Interfaces/`

4. **And** repository implementations using Dapper are created in `Infrastructure/Repositories/`

5. **And** `/api/billing` endpoints support:
   - GET `/api/students/:id/billing` (billing summary with balance)
   - GET `/api/students/:id/invoices` (list invoices)
   - POST `/api/students/:id/invoices` (create invoice)
   - GET `/api/students/:id/payments` (list payments)
   - POST `/api/students/:id/payments` (record payment)

6. **And** endpoints require authentication

## Architecture Compliance

- **Repository Interfaces**: `Application/Interfaces/IInvoiceRepository.cs`, `Application/Interfaces/IPaymentRepository.cs`
- **Repository Implementations**: `Infrastructure/Repositories/InvoiceRepository.cs`, `Infrastructure/Repositories/PaymentRepository.cs` using `IDbConnectionFactory` and Dapper
- **Service Layer**: `Infrastructure/Billing/BillingService.cs`
- **Migration Scripts**: `Infrastructure/Migrations/Scripts/NNN_CreateInvoices.sql`, `NNN_CreatePayments.sql`, `NNN_CreateReceipts.sql`
- **DI Registration**: `Infrastructure/DependencyInjection.cs`
- **No EF Core** -- all data access via Dapper with parameterized SQL and snake_case column names
- **No navigation properties** -- use explicit SQL JOINs where needed

## Tasks / Subtasks

- [ ] Task 1: Create Invoice entity (AC: #1)
  - [ ] Create `Domain/Entities/Invoice.cs` with flat properties (no navigation properties)
  - [ ] Use integer status codes (0=Pending, 1=Paid, 2=Overdue)
- [ ] Task 2: Create Payment entity (AC: #1)
  - [ ] Create `Domain/Entities/Payment.cs` with flat properties (no navigation properties)
  - [ ] Use integer payment method codes (0=Cash, 1=Card, 2=EFT, 3=Other)
- [ ] Task 3: Create DbUp migration scripts (AC: #2)
  - [ ] Create `Infrastructure/Migrations/Scripts/NNN_CreateInvoices.sql` with snake_case columns
  - [ ] Create `Infrastructure/Migrations/Scripts/NNN_CreatePayments.sql` with snake_case columns
  - [ ] Create `Infrastructure/Migrations/Scripts/NNN_CreateReceipts.sql` with snake_case columns
  - [ ] Include foreign key constraints to `students` table
- [ ] Task 4: Create repository interfaces (AC: #3)
  - [ ] Create `Application/Interfaces/IInvoiceRepository.cs`
  - [ ] Create `Application/Interfaces/IPaymentRepository.cs`
  - [ ] Define CRUD + query methods (GetByStudentIdAsync, GetByIdAsync, CreateAsync, UpdateAsync)
- [ ] Task 5: Create repository implementations (AC: #4)
  - [ ] Create `Infrastructure/Repositories/InvoiceRepository.cs` using `IDbConnectionFactory` + Dapper
  - [ ] Create `Infrastructure/Repositories/PaymentRepository.cs` using `IDbConnectionFactory` + Dapper
  - [ ] Use `QueryAsync<T>`, `QueryFirstOrDefaultAsync<T>`, `ExecuteAsync`
  - [ ] Parameterized SQL only, snake_case column names
- [ ] Task 6: Create billing service (AC: #5)
  - [ ] Create `Infrastructure/Billing/BillingService.cs`
  - [ ] Calculate balance (Sum invoices - Sum payments)
  - [ ] Generate receipt numbers (RCP-{YYYYMMDD}-{sequence})
  - [ ] Invoice status management
- [ ] Task 7: Register DI services (AC: #4)
  - [ ] Register `IInvoiceRepository` / `InvoiceRepository` in `Infrastructure/DependencyInjection.cs`
  - [ ] Register `IPaymentRepository` / `PaymentRepository` in `Infrastructure/DependencyInjection.cs`
  - [ ] Register `IBillingService` / `BillingService`
- [ ] Task 8: Create endpoints (AC: #5, #6)
  - [ ] Add `BillingController` or extend `StudentsController`
  - [ ] Implement all billing endpoints
  - [ ] Add authentication requirement

## Dev Notes

### Billing Entities

```csharp
public class Invoice
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string InvoiceDate { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string DueDate { get; set; } = string.Empty;
    public int Status { get; set; } // 0=Pending, 1=Paid, 2=Overdue
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}

public class Payment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int? InvoiceId { get; set; }
    public string PaymentDate { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int PaymentMethod { get; set; } // 0=Cash, 1=Card, 2=EFT, 3=Other
    public string ReceiptNumber { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
```

### Repository Interface Example

```csharp
public interface IInvoiceRepository
{
    Task<IEnumerable<Invoice>> GetByStudentIdAsync(int studentId);
    Task<Invoice?> GetByIdAsync(int id);
    Task<int> CreateAsync(Invoice invoice);
    Task<bool> UpdateAsync(Invoice invoice);
}

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetByStudentIdAsync(int studentId);
    Task<Payment?> GetByIdAsync(int id);
    Task<int> CreateAsync(Payment payment);
}
```

### Repository Implementation Example

```csharp
public class InvoiceRepository : IInvoiceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public InvoiceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Invoice>> GetByStudentIdAsync(int studentId)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<Invoice>(
            @"SELECT id, student_id AS StudentId, invoice_date AS InvoiceDate,
                     amount, due_date AS DueDate, status, description, notes,
                     created_at AS CreatedAt
              FROM invoices
              WHERE student_id = @StudentId
              ORDER BY invoice_date DESC",
            new { StudentId = studentId });
    }

    public async Task<int> CreateAsync(Invoice invoice)
    {
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(
            @"INSERT INTO invoices (student_id, invoice_date, amount, due_date, status, description, notes, created_at)
              VALUES (@StudentId, @InvoiceDate, @Amount, @DueDate, @Status, @Description, @Notes, @CreatedAt)",
            invoice);
    }
}
```

### DbUp Migration Script Example

```sql
-- NNN_CreateInvoices.sql
CREATE TABLE IF NOT EXISTS invoices (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    student_id INTEGER NOT NULL,
    invoice_date TEXT NOT NULL,
    amount REAL NOT NULL,
    due_date TEXT NOT NULL,
    status INTEGER NOT NULL DEFAULT 0,
    description TEXT,
    notes TEXT,
    created_at TEXT NOT NULL DEFAULT (datetime('now')),
    FOREIGN KEY (student_id) REFERENCES students(id)
);
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
