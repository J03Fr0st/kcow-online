namespace Kcow.Application.Schools;

/// <summary>
/// Service interface for school management operations.
/// </summary>
public interface ISchoolService
{
    /// <summary>
    /// Gets all active schools.
    /// </summary>
    /// <returns>List of active schools</returns>
    Task<List<SchoolDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a school by ID.
    /// </summary>
    /// <param name="id">School ID</param>
    /// <returns>School DTO or null if not found</returns>
    Task<SchoolDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new school.
    /// </summary>
    /// <param name="request">Create school request</param>
    /// <returns>Created school DTO</returns>
    Task<SchoolDto> CreateAsync(CreateSchoolRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing school.
    /// </summary>
    /// <param name="id">School ID</param>
    /// <param name="request">Update school request</param>
    /// <returns>Updated school DTO or null if not found</returns>
    Task<SchoolDto?> UpdateAsync(int id, UpdateSchoolRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives (soft-deletes) a school by setting IsActive to false.
    /// </summary>
    /// <param name="id">School ID</param>
    /// <returns>True if school was archived, false if not found</returns>
    Task<bool> ArchiveAsync(int id, CancellationToken cancellationToken = default);
}
