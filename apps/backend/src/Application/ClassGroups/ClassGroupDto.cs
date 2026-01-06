namespace Kcow.Application.ClassGroups;

/// <summary>
/// Data transfer object for class group information.
/// Includes all XSD schema fields for legacy compatibility.
/// </summary>
public class ClassGroupDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? DayTruck { get; set; }
    public string? Description { get; set; }
    public int SchoolId { get; set; }
    public int? TruckId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int Sequence { get; set; }
    public bool Evaluate { get; set; }
    public string? Notes { get; set; }
    public bool ImportFlag { get; set; }
    public string? GroupMessage { get; set; }
    public string? SendCertificates { get; set; }
    public string? MoneyMessage { get; set; }
    public string? Ixl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Nested school and truck information
    public SchoolDto? School { get; set; }
    public TruckDto? Truck { get; set; }
}

/// <summary>
/// Lightweight school information for nested display.
/// </summary>
public class SchoolDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? ShortName { get; set; }
}

/// <summary>
/// Lightweight truck information for nested display.
/// </summary>
public class TruckDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string RegistrationNumber { get; set; }
}
