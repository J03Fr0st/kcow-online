namespace Kcow.Application.Billing;

public class InvoiceDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string InvoiceDate { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string DueDate { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string CreatedAt { get; set; } = string.Empty;
}
