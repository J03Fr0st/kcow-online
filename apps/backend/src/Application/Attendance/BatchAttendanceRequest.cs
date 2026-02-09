using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Attendance;

/// <summary>
/// Request model for batch attendance operations.
/// Allows creating/updating multiple attendance records for a class group in a single transaction.
/// </summary>
public class BatchAttendanceRequest
{
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
    /// Attendance entries for each student.
    /// </summary>
    [Required(ErrorMessage = "Entries are required")]
    [MinLength(1, ErrorMessage = "At least one attendance entry is required")]
    public List<BatchAttendanceEntry> Entries { get; set; } = new();
}

/// <summary>
/// Single student attendance entry in batch operation.
/// </summary>
public class BatchAttendanceEntry
{
    /// <summary>
    /// The student ID (required).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive integer")]
    public int StudentId { get; set; }

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

/// <summary>
/// Response model for batch attendance operations.
/// </summary>
public class BatchAttendanceResponse
{
    /// <summary>
    /// Number of new attendance records created.
    /// </summary>
    public int Created { get; set; }

    /// <summary>
    /// Number of existing attendance records updated.
    /// </summary>
    public int Updated { get; set; }

    /// <summary>
    /// Number of records that failed to save.
    /// </summary>
    public int Failed { get; set; }

    /// <summary>
    /// Error messages for any failed records.
    /// </summary>
    public List<string>? Errors { get; set; }
}
