namespace Kcow.Application.Trucks;

/// <summary>
/// Data transfer object for truck information.
/// </summary>
public class TruckDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string RegistrationNumber { get; set; }
    public required string Status { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
