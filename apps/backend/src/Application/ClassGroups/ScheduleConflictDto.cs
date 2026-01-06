namespace Kcow.Application.ClassGroups;

/// <summary>
/// Represents a scheduling conflict with another class group.
/// </summary>
public class ScheduleConflictDto
{
    /// <summary>
    /// The ID of the conflicting class group.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The name of the conflicting class group.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The name of the school.
    /// </summary>
    public string SchoolName { get; set; } = string.Empty;

    /// <summary>
    /// The start time of the conflicting class group.
    /// </summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// The end time of the conflicting class group.
    /// </summary>
    public TimeOnly EndTime { get; set; }
}

/// <summary>
/// Response from a conflict check request.
/// </summary>
public class CheckConflictsResponse
{
    /// <summary>
    /// Whether any conflicts were detected.
    /// </summary>
    public bool HasConflicts { get; set; }

    /// <summary>
    /// List of conflicting class groups.
    /// </summary>
    public List<ScheduleConflictDto> Conflicts { get; set; } = new();
}
