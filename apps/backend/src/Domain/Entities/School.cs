namespace Kcow.Domain.Entities;

public class School
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public BillingSettings? BillingSettings { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

public class BillingSettings
{
    public decimal DefaultSessionRate { get; set; }
    public string BillingCycle { get; set; } = "Monthly"; // Monthly, Termly
    public string? BillingNotes { get; set; }
}
