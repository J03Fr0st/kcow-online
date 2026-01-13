using Kcow.Domain.Entities;
using Kcow.Domain.Enums;

namespace Kcow.Application.Interfaces;

public interface IFamilyRepository
{
    Task<IEnumerable<Family>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Family>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<Family?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(Family family, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Family family, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
    Task AddStudentToFamilyAsync(int studentId, int familyId, RelationshipType relationshipType, CancellationToken cancellationToken = default);
}
