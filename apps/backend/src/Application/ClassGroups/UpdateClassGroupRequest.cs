using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.ClassGroups;

/// <summary>
/// Request model for updating an existing class group.
/// </summary>
public class UpdateClassGroupRequest
{
    /// <summary>
    /// Class group name.
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public required string Name { get; set; }

    /// <summary>
    /// School ID this class group belongs to.
    /// </summary>
    [Required(ErrorMessage = "School ID is required")]
    public int SchoolId { get; set; }

    /// <summary>
    /// Optional Truck ID assigned to this class group.
    /// </summary>
    public int? TruckId { get; set; }

    /// <summary>
    /// Day of the week when this class meets.
    /// </summary>
    [Required(ErrorMessage = "Day of week is required")]
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Start time of the class.
    /// </summary>
    [Required(ErrorMessage = "Start time is required")]
    public TimeOnly StartTime { get; set; }

    /// <summary>
    /// End time of the class.
    /// </summary>
    [Required(ErrorMessage = "End time is required")]
    public TimeOnly EndTime { get; set; }

    /// <summary>
    /// Sequence/order for scheduling purposes.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Sequence must be at least 1")]
    public int Sequence { get; set; }

    /// <summary>
    /// Optional notes about this class group.
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    /// <summary>
    /// Whether this class group is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
