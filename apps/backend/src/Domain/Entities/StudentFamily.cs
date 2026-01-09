using Kcow.Domain.Enums;

namespace Kcow.Domain.Entities;

/// <summary>
/// Join entity for the many-to-many relationship between Students and Families.
/// Includes relationship type (e.g. Parent, Guardian).
/// </summary>
public class StudentFamily
{
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int FamilyId { get; set; }
    public Family Family { get; set; } = null!;

    public RelationshipType RelationshipType { get; set; }
}
