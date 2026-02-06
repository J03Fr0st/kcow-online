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
    [Required(ErrorMessage = "StudentId is required")]
    public int StudentId { get; set; }

    /// <summary>
    /// The class group ID (required).
    /// </summary>
    [Required(ErrorMessage = "ClassGroupId is required")]
    public int ClassGroupId { get; set; }

    /// <summary>
    /// The session date in ISO format (YYYY-MM-DD) (required).
    /// </summary>
    [Required(ErrorMessage = "SessionDate is required")]
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
