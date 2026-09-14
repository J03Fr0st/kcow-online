using Kcow.Application.Billing;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;

namespace Kcow.Application.Billing;

public class BillingService : IBillingService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IBillingWriter _writer;

    public BillingService(IInvoiceRepository invoices, IPaymentRepository payments, IStudentRepository students, IBillingWriter writer)
    {
        _invoiceRepository = invoices;
        _paymentRepository = payments;
        _studentRepository = students;
        _writer = writer;
    }

    public async Task<BillingSummaryDto> GetBillingSummaryAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await ValidateStudentExistsAsync(studentId, cancellationToken);

        var invoices = await _invoiceRepository.GetByStudentIdAsync(studentId, cancellationToken);
        var payments = await _paymentRepository.GetByStudentIdAsync(studentId, cancellationToken);

        var invoiceList = invoices.ToList();
        var paymentList = payments.ToList();

        var totalInvoiced = invoiceList.Sum(i => i.Amount);
        var totalPaid = paymentList.Sum(p => p.Amount);
        var overdueAmount = 0m;
        foreach (var invoice in invoiceList.Where(i => i.Status == 2)) // Overdue
        {
            var paymentsForInvoice = paymentList.Where(p => p.InvoiceId == invoice.Id).Sum(p => p.Amount);
            overdueAmount += Math.Max(0, invoice.Amount - paymentsForInvoice);
        }
        var outstandingCount = invoiceList.Count(i => i.Status == 0 || i.Status == 2); // Pending or Overdue

        var lastPayment = paymentList.FirstOrDefault(); // Already ordered DESC by date

        return new BillingSummaryDto
        {
            StudentId = studentId,
            CurrentBalance = totalInvoiced - totalPaid,
            TotalInvoiced = totalInvoiced,
            TotalPaid = totalPaid,
            OverdueAmount = overdueAmount,
            LastPaymentDate = lastPayment?.PaymentDate,
            LastPaymentAmount = lastPayment?.Amount,
            OutstandingInvoicesCount = outstandingCount
        };
    }

    public async Task<List<InvoiceDto>> GetInvoicesByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await ValidateStudentExistsAsync(studentId, cancellationToken);

        var invoices = await _invoiceRepository.GetByStudentIdAsync(studentId, cancellationToken);
        return invoices.Select(MapToInvoiceDto).ToList();
    }

    public Task<InvoiceDto> CreateInvoiceAsync(int studentId, CreateInvoiceRequest request, string createdBy, CancellationToken cancellationToken = default, string? idempotencyKey = null) =>
        _writer.RecordInvoiceAsync(studentId, request, createdBy, idempotencyKey, cancellationToken);

    public async Task<List<PaymentDto>> GetPaymentsByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await ValidateStudentExistsAsync(studentId, cancellationToken);

        var payments = await _paymentRepository.GetByStudentIdAsync(studentId, cancellationToken);
        return payments.Select(MapToPaymentDto).ToList();
    }

    public Task<PaymentDto> CreatePaymentAsync(int studentId, CreatePaymentRequest request, string createdBy, CancellationToken cancellationToken = default, string? idempotencyKey = null) =>
        _writer.RecordPaymentAsync(studentId, request, createdBy, idempotencyKey, cancellationToken);

    private async Task ValidateStudentExistsAsync(int studentId, CancellationToken cancellationToken)
    {
        var exists = await _studentRepository.ExistsAsync(studentId, cancellationToken);
        if (!exists)
            throw new InvalidOperationException($"Student with ID {studentId} does not exist");
    }

    private static InvoiceDto MapToInvoiceDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            StudentId = invoice.StudentId,
            InvoiceDate = invoice.InvoiceDate,
            Amount = invoice.Amount,
            DueDate = invoice.DueDate,
            Status = invoice.Status,
            Description = invoice.Description,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt
        };
    }

    private static PaymentDto MapToPaymentDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            StudentId = payment.StudentId,
            InvoiceId = payment.InvoiceId,
            PaymentDate = payment.PaymentDate,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod,
            ReceiptNumber = payment.ReceiptNumber,
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt
        };
    }
}
