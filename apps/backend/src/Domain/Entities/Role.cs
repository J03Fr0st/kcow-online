namespace Kcow.Domain.Entities;

/// <summary>
/// Represents a user role in the system.
/// </summary>
public class Role
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
}
