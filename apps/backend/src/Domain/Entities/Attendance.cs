namespace Kcow.Domain.Entities;

/// <summary>
/// Attendance entity for tracking student attendance per session.
/// Represents a single attendance record for a student in a class group on a specific date.
/// </summary>
public class Attendance
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ClassGroupId { get; set; }
    public string SessionDate { get; set; } = string.Empty; // Stored as ISO date string (YYYY-MM-DD)
    public AttendanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Navigation properties
    public Student Student { get; set; } = null!;
    public ClassGroup ClassGroup { get; set; } = null!;
}

/// <summary>
/// Attendance status enum matching story specification.
/// </summary>
public enum AttendanceStatus
{
    Present,
    Absent,
    Late
}
