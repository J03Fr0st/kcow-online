namespace Kcow.Application.Billing;

public interface IBillingService
{
    Task<BillingSummaryDto> GetBillingSummaryAsync(int studentId, CancellationToken cancellationToken = default);
    Task<List<InvoiceDto>> GetInvoicesByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<InvoiceDto> CreateInvoiceAsync(int studentId, CreateInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<List<PaymentDto>> GetPaymentsByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<PaymentDto> CreatePaymentAsync(int studentId, CreatePaymentRequest request, CancellationToken cancellationToken = default);
}
