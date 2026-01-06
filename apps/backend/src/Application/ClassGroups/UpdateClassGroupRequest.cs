using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.ClassGroups;

/// <summary>
/// Request model for updating an existing class group.
/// Aligned with legacy XSD schema constraints.
/// </summary>
public class UpdateClassGroupRequest
{
    /// <summary>
    /// Class group name (XSD: 10 chars max).
    /// </summary>
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(10, ErrorMessage = "Name cannot exceed 10 characters")]
    public required string Name { get; set; }

    /// <summary>
    /// DayTruck composite identifier for legacy compatibility (XSD: 6 chars max).
    /// </summary>
    [MaxLength(6, ErrorMessage = "DayTruck cannot exceed 6 characters")]
    public string? DayTruck { get; set; }

    /// <summary>
    /// Class group description (XSD: 35 chars max).
    /// </summary>
    [MaxLength(35, ErrorMessage = "Description cannot exceed 35 characters")]
    public string? Description { get; set; }

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
    /// Evaluation flag (XSD: boolean, default false).
    /// </summary>
    public bool Evaluate { get; set; }

    /// <summary>
    /// Optional notes about this class group (XSD: 255 chars max).
    /// </summary>
    [MaxLength(255, ErrorMessage = "Notes cannot exceed 255 characters")]
    public string? Notes { get; set; }

    /// <summary>
    /// Import flag (XSD: boolean, default false).
    /// </summary>
    public bool ImportFlag { get; set; }

    /// <summary>
    /// Group message (XSD: 255 chars max).
    /// </summary>
    [MaxLength(255, ErrorMessage = "GroupMessage cannot exceed 255 characters")]
    public string? GroupMessage { get; set; }

    /// <summary>
    /// Send certificates flag/message (XSD: 255 chars max).
    /// </summary>
    [MaxLength(255, ErrorMessage = "SendCertificates cannot exceed 255 characters")]
    public string? SendCertificates { get; set; }

    /// <summary>
    /// Money message (XSD: 50 chars max).
    /// </summary>
    [MaxLength(50, ErrorMessage = "MoneyMessage cannot exceed 50 characters")]
    public string? MoneyMessage { get; set; }

    /// <summary>
    /// IXL integration identifier (XSD: 3 chars max).
    /// </summary>
    [MaxLength(3, ErrorMessage = "IXL cannot exceed 3 characters")]
    public string? Ixl { get; set; }

    /// <summary>
    /// Whether this class group is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
