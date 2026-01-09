namespace Kcow.Application.Activities;

/// <summary>
/// Data transfer object for activity information.
/// </summary>
public class ActivityDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Folder { get; set; }
    public string? GradeLevel { get; set; }
    public string? Icon { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
