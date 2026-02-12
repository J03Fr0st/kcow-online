using Kcow.Application.Billing;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Billing;

public class BillingService : IBillingService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<BillingService> _logger;

    public BillingService(
        IInvoiceRepository invoiceRepository,
        IPaymentRepository paymentRepository,
        IStudentRepository studentRepository,
        ILogger<BillingService> logger)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
        _studentRepository = studentRepository;
        _logger = logger;
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

    public async Task<InvoiceDto> CreateInvoiceAsync(int studentId, CreateInvoiceRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateStudentExistsAsync(studentId, cancellationToken);

        var invoice = new Invoice
        {
            StudentId = studentId,
            InvoiceDate = request.InvoiceDate,
            Amount = request.Amount,
            DueDate = request.DueDate,
            Status = 0, // Pending
            Description = request.Description,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow.ToString("o")
        };

        var id = await _invoiceRepository.CreateAsync(invoice, cancellationToken);
        invoice.Id = id;

        _logger.LogInformation("Created invoice {InvoiceId} for student {StudentId}", id, studentId);
        return MapToInvoiceDto(invoice);
    }

    public async Task<List<PaymentDto>> GetPaymentsByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        await ValidateStudentExistsAsync(studentId, cancellationToken);

        var payments = await _paymentRepository.GetByStudentIdAsync(studentId, cancellationToken);
        return payments.Select(MapToPaymentDto).ToList();
    }

    public async Task<PaymentDto> CreatePaymentAsync(int studentId, CreatePaymentRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateStudentExistsAsync(studentId, cancellationToken);

        if (request.InvoiceId.HasValue)
        {
            var invoice = await _invoiceRepository.GetByIdAsync(request.InvoiceId.Value, cancellationToken);
            if (invoice == null)
                throw new InvalidOperationException($"Invoice with ID {request.InvoiceId.Value} does not exist");
        }

        var payment = new Payment
        {
            StudentId = studentId,
            InvoiceId = request.InvoiceId,
            PaymentDate = request.PaymentDate,
            Amount = request.Amount,
            PaymentMethod = request.PaymentMethod,
            ReceiptNumber = "PENDING",
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow.ToString("o")
        };

        var id = await _paymentRepository.CreateAsync(payment, cancellationToken);
        payment.Id = id;

        // Generate receipt number using the unique payment ID
        payment.ReceiptNumber = GenerateReceiptNumber(request.PaymentDate, id);
        await _paymentRepository.UpdateReceiptNumberAsync(id, payment.ReceiptNumber, cancellationToken);

        // If payment is linked to an invoice, check if invoice should be marked as Paid
        if (request.InvoiceId.HasValue)
        {
            await TryMarkInvoicePaidAsync(request.InvoiceId.Value, cancellationToken);
        }

        _logger.LogInformation("Created payment {PaymentId} for student {StudentId} with receipt {ReceiptNumber}", id, studentId, payment.ReceiptNumber);
        return MapToPaymentDto(payment);
    }

    private async Task TryMarkInvoicePaidAsync(int invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId, cancellationToken);
        if (invoice == null) return;

        var payments = await _paymentRepository.GetByStudentIdAsync(invoice.StudentId, cancellationToken);
        var invoicePayments = payments.Where(p => p.InvoiceId == invoiceId).Sum(p => p.Amount);

        if (invoicePayments >= invoice.Amount)
        {
            invoice.Status = 1; // Paid
            await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
            _logger.LogInformation("Invoice {InvoiceId} marked as Paid (total payments: {TotalPayments})", invoiceId, invoicePayments);
        }
    }

    private static string GenerateReceiptNumber(string paymentDate, int paymentId)
    {
        var datePart = paymentDate.Replace("-", "");
        return $"RCP-{datePart}-{paymentId:D5}";
    }

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
