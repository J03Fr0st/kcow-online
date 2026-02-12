using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Billing;

public class CreateInvoiceRequest
{
    [Required(ErrorMessage = "InvoiceDate is required")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "InvoiceDate must be in ISO format (YYYY-MM-DD)")]
    public string InvoiceDate { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "DueDate is required")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "DueDate must be in ISO format (YYYY-MM-DD)")]
    public string DueDate { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Notes { get; set; }
}
