namespace Kcow.Domain.Entities;

/// <summary>
/// Class Group entity aligned with legacy XSD schema (Class Group.xsd - 15 fields).
/// Represents a scheduled class session at a school with day/time/sequence and truck assignment.
/// </summary>
public class ClassGroup
{
    // Primary identifier (auto-generated)
    public int Id { get; set; }

    // XSD Field: "Class Group" (10 chars max) - Renamed to Name for clarity
    public string Name { get; set; } = string.Empty;

    // XSD Field: "DayTruck" (6 chars max) - Composite day+truck identifier for legacy compatibility
    public string? DayTruck { get; set; }

    // XSD Field: "Description" (35 chars max)
    public string? Description { get; set; }

    // XSD Field: "School_x0020_Id" (smallint)
    public int SchoolId { get; set; }

    // Optional truck assignment
    public int? TruckId { get; set; }

    // XSD Field: "DayId" (1 char) - Legacy day identifier, mapped to DayOfWeek enum
    public DayOfWeek DayOfWeek { get; set; }

    // XSD Field: "Start_x0020_Time" (5 chars) - HH:mm format
    public TimeOnly StartTime { get; set; }

    // XSD Field: "End_x0020_Time" (5 chars) - HH:mm format
    public TimeOnly EndTime { get; set; }

    // XSD Field: "Sequence" (50 chars in XSD but used as ordering integer)
    public int Sequence { get; set; } = 1;

    // XSD Field: "Evaluate" (boolean, required, default false)
    public bool Evaluate { get; set; } = false;

    // XSD Field: "Note" (255 chars max) - Renamed to Notes for .NET convention
    public string? Notes { get; set; }

    // XSD Field: "Import" (boolean, required, default false) - Renamed to ImportFlag to avoid keyword
    public bool ImportFlag { get; set; } = false;

    // XSD Field: "GroupMessage" (255 chars max)
    public string? GroupMessage { get; set; }

    // XSD Field: "Send_x0020_Certificates" (255 chars max)
    public string? SendCertificates { get; set; }

    // XSD Field: "Money_x0020_Message" (50 chars max)
    public string? MoneyMessage { get; set; }

    // XSD Field: "IXL" (3 chars max) - IXL integration identifier
    public string? Ixl { get; set; }

    // Soft delete flag (not in XSD, application-level)
    public bool IsActive { get; set; } = true;

    // Audit fields (not in XSD, application-level)
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public School School { get; set; } = null!;
    public Truck? Truck { get; set; }
    public ICollection<Student> Students { get; set; } = new List<Student>();
}
