using Kcow.Domain.Entities;

namespace Kcow.Application.Interfaces;

/// <summary>
/// Repository interface for School entity operations.
/// </summary>
public interface ISchoolRepository
{
    /// <summary>
    /// Gets all schools.
    /// </summary>
    Task<IEnumerable<School>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active schools.
    /// </summary>
    Task<IEnumerable<School>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a school by ID.
    /// </summary>
    Task<School?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new school.
    /// </summary>
    Task<int> CreateAsync(School school, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing school.
    /// </summary>
    Task<bool> UpdateAsync(School school, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a school by ID.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a school exists by ID.
    /// </summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}
