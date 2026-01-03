using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Trucks;

/// <summary>
/// Request model for updating an existing truck.
/// </summary>
public class UpdateTruckRequest
{
    /// <summary>
    /// Truck name or identifier.
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public required string Name { get; set; }

    /// <summary>
    /// Unique registration number for the truck.
    /// </summary>
    [Required(ErrorMessage = "Registration number is required")]
    [MaxLength(50, ErrorMessage = "Registration number cannot exceed 50 characters")]
    public required string RegistrationNumber { get; set; }

    /// <summary>
    /// Truck status (e.g., Active, Maintenance, Retired).
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public required string Status { get; set; }

    /// <summary>
    /// Optional notes about the truck.
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }
}
