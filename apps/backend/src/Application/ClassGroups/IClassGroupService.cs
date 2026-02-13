namespace Kcow.Application.ClassGroups;

/// <summary>
/// Service interface for class group management operations.
/// </summary>
public interface IClassGroupService
{
    /// <summary>
    /// Gets all active class groups with optional filtering.
    /// </summary>
    /// <param name="schoolId">Optional school ID filter</param>
    /// <param name="truckId">Optional truck ID filter</param>
    /// <returns>List of active class groups</returns>
    Task<List<ClassGroupDto>> GetAllAsync(int? schoolId = null, int? truckId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a class group by ID with school and truck details.
    /// </summary>
    /// <param name="id">Class group ID</param>
    /// <returns>Class group DTO or null if not found</returns>
    Task<ClassGroupDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new class group.
    /// </summary>
    /// <param name="request">Create class group request</param>
    /// <returns>Created class group DTO</returns>
    Task<ClassGroupDto> CreateAsync(CreateClassGroupRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing class group.
    /// </summary>
    /// <param name="id">Class group ID</param>
    /// <param name="request">Update class group request</param>
    /// <returns>Updated class group DTO or null if not found</returns>
    Task<ClassGroupDto?> UpdateAsync(int id, UpdateClassGroupRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives (soft-deletes) a class group by setting IsActive to false.
    /// </summary>
    /// <param name="id">Class group ID</param>
    /// <returns>True if class group was archived, false if not found</returns>
    Task<bool> ArchiveAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks for scheduling conflicts with existing class groups.
    /// </summary>
    /// <param name="request">Conflict check request</param>
    /// <returns>Conflict check response with list of conflicts</returns>
    Task<CheckConflictsResponse> CheckConflictsAsync(CheckConflictsRequest request, CancellationToken cancellationToken = default);
}
