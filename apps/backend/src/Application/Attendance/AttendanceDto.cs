namespace Kcow.Application.Attendance;

/// <summary>
/// Data transfer object for attendance information.
/// Includes related student and class group names for display.
/// </summary>
public class AttendanceDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
    public int ClassGroupId { get; set; }
    public string? ClassGroupName { get; set; }
    public string SessionDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
