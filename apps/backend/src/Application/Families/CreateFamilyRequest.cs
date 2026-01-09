using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Families;

public class CreateFamilyRequest
{
    [Required(ErrorMessage = "Family name is required")]
    [MaxLength(200, ErrorMessage = "Family name cannot exceed 200 characters")]
    public required string FamilyName { get; set; }

    [Required(ErrorMessage = "Primary contact name is required")]
    [MaxLength(200, ErrorMessage = "Primary contact name cannot exceed 200 characters")]
    public required string PrimaryContactName { get; set; }

    [MaxLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string? Email { get; set; }

    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
