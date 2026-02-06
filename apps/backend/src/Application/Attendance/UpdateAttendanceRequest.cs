using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Attendance;

/// <summary>
/// Request model for updating an existing attendance record.
/// </summary>
public class UpdateAttendanceRequest
{
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
