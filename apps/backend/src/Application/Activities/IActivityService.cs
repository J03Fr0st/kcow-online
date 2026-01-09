namespace Kcow.Application.Activities;

/// <summary>
/// Service interface for activity management operations.
/// </summary>
public interface IActivityService
{
    /// <summary>
    /// Gets all active activities.
    /// </summary>
    /// <returns>List of active activities</returns>
    Task<List<ActivityDto>> GetAllAsync();

    /// <summary>
    /// Gets an activity by ID.
    /// </summary>
    /// <param name="id">Activity ID</param>
    /// <returns>Activity DTO or null if not found</returns>
    Task<ActivityDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    /// <param name="request">Create activity request</param>
    /// <returns>Created activity DTO</returns>
    Task<ActivityDto> CreateAsync(CreateActivityRequest request);

    /// <summary>
    /// Updates an existing activity.
    /// </summary>
    /// <param name="id">Activity ID</param>
    /// <param name="request">Update activity request</param>
    /// <returns>Updated activity DTO or null if not found</returns>
    Task<ActivityDto?> UpdateAsync(int id, UpdateActivityRequest request);

    /// <summary>
    /// Archives (soft-deletes) an activity by setting IsActive to false.
    /// </summary>
    /// <param name="id">Activity ID</param>
    /// <returns>True if activity was archived, false if not found</returns>
    Task<bool> ArchiveAsync(int id);
}
