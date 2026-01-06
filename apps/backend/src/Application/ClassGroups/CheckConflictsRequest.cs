namespace Kcow.Application.ClassGroups;

/// <summary>
/// Request to check for scheduling conflicts.
/// </summary>
public class CheckConflictsRequest
{
    /// <summary>
    /// The truck ID to check for conflicts (required).
    /// </summary>
    public int TruckId { get; set; }

    /// <summary>
    /// The day of week to check (0=Sunday, 1=Monday, etc.).
    /// </summary>
    public int DayOfWeek { get; set; }

    /// <summary>
    /// The start time to check.
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// The end time to check.
    /// </summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Optional: Exclude this class group ID from conflict check (useful when editing).
    /// </summary>
    public int? ExcludeId { get; set; }
}
