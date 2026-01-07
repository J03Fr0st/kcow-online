namespace Kcow.Domain.Entities;

/// <summary>
/// Activity entity aligned with legacy XSD schema (Activity.xsd).
/// Represents a KCOW activity/program entry.
/// </summary>
public class Activity
{
    // XSD Field: "ActivityID" (int, required)
    public int Id { get; set; }

    // XSD Field: "Program" (255 chars max)
    public string? Program { get; set; }

    // XSD Field: "ProgramName" (255 chars max)
    public string? ProgramName { get; set; }

    // XSD Field: "Educational Focus" (memo)
    public string? EducationalFocus { get; set; }

    // XSD Field: "Folder" (255 chars max)
    public string? Folder { get; set; }

    // XSD Field: "Grade" (255 chars max)
    public string? Grade { get; set; }

    // XSD Field: "Icon" (image/oleobject)
    public byte[]? Icon { get; set; }
}


