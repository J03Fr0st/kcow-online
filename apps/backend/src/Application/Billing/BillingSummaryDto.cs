namespace Kcow.Application.Billing;

public class BillingSummaryDto
{
    public int StudentId { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal OverdueAmount { get; set; }
    public string? LastPaymentDate { get; set; }
    public decimal? LastPaymentAmount { get; set; }
    public int OutstandingInvoicesCount { get; set; }
}
