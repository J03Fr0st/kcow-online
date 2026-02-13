namespace Kcow.Domain.Entities;

/// <summary>
/// Activity entity aligned with legacy XSD schema (Activity.xsd).
/// Represents a KCOW activity/program entry.
/// </summary>
public class Activity
{
    // XSD Field: "ActivityID" (int, required)
    public int Id { get; set; }

    // XSD Field: "Program" (255 chars max) - Mapped to Code for English naming
    public string? Code { get; set; }

    // XSD Field: "ProgramName" (255 chars max) - Mapped to Name for English naming
    public string? Name { get; set; }

    // XSD Field: "Educational Focus" (memo) - Mapped to Description for English naming
    public string? Description { get; set; }

    // XSD Field: "Folder" (255 chars max)
    public string? Folder { get; set; }

    // XSD Field: "Grade" (255 chars max) - Mapped to GradeLevel for English naming
    public string? GradeLevel { get; set; }

    // XSD Field: "Icon" (image/oleobject) - Stored as TEXT for base64 string
    public string? Icon { get; set; }

    // Soft-delete support
    public bool IsActive { get; set; } = true;

    public string? LegacyId { get; set; }

    // Audit timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}


