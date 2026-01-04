using Kcow.Domain.Entities;

namespace Kcow.Application.Schools;

/// <summary>
/// Data transfer object for school information.
/// </summary>
public class SchoolDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Address { get; set; }
    public string? ContactName { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public BillingSettingsDto? BillingSettings { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Data transfer object for billing settings.
/// </summary>
public class BillingSettingsDto
{
    public decimal DefaultSessionRate { get; set; }
    public string BillingCycle { get; set; } = "Monthly";
    public string? BillingNotes { get; set; }
}
