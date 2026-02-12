namespace Kcow.Domain.Entities;

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
