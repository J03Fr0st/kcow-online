using Kcow.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Families;

public class LinkFamilyRequest
{
    [Required]
    public int FamilyId { get; set; }
    
    [Required]
    public RelationshipType RelationshipType { get; set; }
}
