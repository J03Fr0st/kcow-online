using Kcow.Application.Students;

namespace Kcow.Application.Families;

public class FamilyDto
{
    public int Id { get; set; }
    public required string FamilyName { get; set; }
    public required string PrimaryContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<StudentFamilyDto> Students { get; set; } = new List<StudentFamilyDto>();
}

public class StudentFamilyDto
{
    public int StudentId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Reference { get; set; }
    public string? RelationshipType { get; set; }
}
