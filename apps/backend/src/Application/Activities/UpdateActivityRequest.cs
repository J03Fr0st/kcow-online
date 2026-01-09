using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Activities;

/// <summary>
/// Request model for updating an existing activity.
/// </summary>
public class UpdateActivityRequest
{
    /// <summary>
    /// Activity code/program identifier (optional).
    /// </summary>
    [MaxLength(255, ErrorMessage = "Code cannot exceed 255 characters")]
    public string? Code { get; set; }

    /// <summary>
    /// Activity/program name.
    /// </summary>
    [MaxLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
    public string? Name { get; set; }

    /// <summary>
    /// Educational focus/description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Folder path (optional).
    /// </summary>
    [MaxLength(255, ErrorMessage = "Folder cannot exceed 255 characters")]
    public string? Folder { get; set; }

    /// <summary>
    /// Grade level (optional).
    /// </summary>
    [MaxLength(255, ErrorMessage = "Grade level cannot exceed 255 characters")]
    public string? GradeLevel { get; set; }

    /// <summary>
    /// Icon as base64 string (optional).
    /// </summary>
    public string? Icon { get; set; }
}
