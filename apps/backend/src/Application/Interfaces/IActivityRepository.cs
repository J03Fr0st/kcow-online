using Kcow.Domain.Entities;

namespace Kcow.Application.Interfaces;

/// <summary>
/// Repository interface for Activity entity operations.
/// </summary>
public interface IActivityRepository
{
    /// <summary>
    /// Gets all activities.
    /// </summary>
    Task<IEnumerable<Activity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active activities.
    /// </summary>
    Task<IEnumerable<Activity>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an activity by ID.
    /// </summary>
    Task<Activity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an activity by code.
    /// </summary>
    Task<Activity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    Task<int> CreateAsync(Activity activity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing activity.
    /// </summary>
    Task<bool> UpdateAsync(Activity activity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an activity by ID.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an activity exists by ID.
    /// </summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an activity exists by code.
    /// </summary>
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default);
}
