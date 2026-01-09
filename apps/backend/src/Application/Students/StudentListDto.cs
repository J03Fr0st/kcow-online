namespace Kcow.Application.Students;

/// <summary>
/// Lightweight DTO for student list view.
/// </summary>
public class StudentListDto
{
    public int Id { get; set; }
    public required string Reference { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Grade { get; set; }
    public bool IsActive { get; set; }
    public string? Status { get; set; }

    // Nested summary information
    public SchoolDto? School { get; set; }
    public ClassGroupDto? ClassGroup { get; set; }
}
