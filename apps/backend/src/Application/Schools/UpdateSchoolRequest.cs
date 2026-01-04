using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Schools;

/// <summary>
/// Request model for updating an existing school.
/// </summary>
public class UpdateSchoolRequest
{
    /// <summary>
    /// School name.
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public required string Name { get; set; }

    /// <summary>
    /// School physical address.
    /// </summary>
    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    /// <summary>
    /// Primary contact person name.
    /// </summary>
    [MaxLength(200, ErrorMessage = "Contact name cannot exceed 200 characters")]
    public string? ContactName { get; set; }

    /// <summary>
    /// Primary contact phone number.
    /// </summary>
    [Phone(ErrorMessage = "Contact phone must be a valid phone number")]
    [MaxLength(50, ErrorMessage = "Contact phone cannot exceed 50 characters")]
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Primary contact email address.
    /// </summary>
    [EmailAddress(ErrorMessage = "Contact email must be a valid email address")]
    [MaxLength(255, ErrorMessage = "Contact email cannot exceed 255 characters")]
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Billing settings for the school.
    /// </summary>
    public BillingSettingsDto? BillingSettings { get; set; }

    /// <summary>
    /// Optional notes about the school.
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; set; }
}
