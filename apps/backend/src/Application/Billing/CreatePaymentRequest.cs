using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Billing;

public class CreatePaymentRequest
{
    public int? InvoiceId { get; set; }

    [Required(ErrorMessage = "PaymentDate is required")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "PaymentDate must be in ISO format (YYYY-MM-DD)")]
    public string PaymentDate { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Range(0, 3, ErrorMessage = "PaymentMethod must be 0 (Cash), 1 (Card), 2 (EFT), or 3 (Other)")]
    public int PaymentMethod { get; set; }

    public string? Notes { get; set; }
}
