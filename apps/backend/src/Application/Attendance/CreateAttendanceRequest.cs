using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Attendance;

/// <summary>
/// Request model for creating a new attendance record.
/// </summary>
public class CreateAttendanceRequest
{
    /// <summary>
    /// The student ID (required).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive integer")]
    public int StudentId { get; set; }

    /// <summary>
    /// The class group ID (required).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "ClassGroupId must be a positive integer")]
    public int ClassGroupId { get; set; }

    /// <summary>
    /// The session date in ISO format (YYYY-MM-DD) (required).
    /// </summary>
    [Required(ErrorMessage = "SessionDate is required")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "SessionDate must be in ISO format (YYYY-MM-DD)")]
    public string SessionDate { get; set; } = string.Empty;

    /// <summary>
    /// Attendance status: Present, Absent, or Late (required).
    /// </summary>
    [Required(ErrorMessage = "Status is required")]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Optional notes for the attendance record.
    /// </summary>
    public string? Notes { get; set; }
}
