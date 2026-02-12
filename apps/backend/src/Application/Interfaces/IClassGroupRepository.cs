using Kcow.Domain.Entities;

namespace Kcow.Application.Interfaces;

public interface IClassGroupRepository
{
    Task<IEnumerable<ClassGroup>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ClassGroup>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<ClassGroup?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClassGroup>> GetBySchoolIdAsync(int schoolId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClassGroup>> GetByDayAsync(DayOfWeek dayOfWeek, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(ClassGroup classGroup, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(ClassGroup classGroup, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a class group exists by ID.
    /// </summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
