namespace Kcow.Domain.Entities;

/// <summary>
/// Family entity representing a household or primary contact group for students.
/// </summary>
public class Family
{
    public int Id { get; set; }
    
    public string FamilyName { get; set; } = string.Empty;
    
    public string PrimaryContactName { get; set; } = string.Empty;
    
    public string? Phone { get; set; }
    
    public string? Email { get; set; }
    
    public string? Address { get; set; }
    
    public string? Notes { get; set; }
    
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<StudentFamily> StudentFamilies { get; set; } = new List<StudentFamily>();
}
