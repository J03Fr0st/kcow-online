namespace Kcow.Application.Trucks;

/// <summary>
/// Service interface for truck management operations.
/// </summary>
public interface ITruckService
{
    /// <summary>
    /// Gets all active trucks.
    /// </summary>
    /// <returns>List of active trucks</returns>
    Task<List<TruckDto>> GetAllAsync();

    /// <summary>
    /// Gets a truck by ID.
    /// </summary>
    /// <param name="id">Truck ID</param>
    /// <returns>Truck DTO or null if not found</returns>
    Task<TruckDto?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new truck.
    /// </summary>
    /// <param name="request">Create truck request</param>
    /// <returns>Created truck DTO</returns>
    Task<TruckDto> CreateAsync(CreateTruckRequest request);

    /// <summary>
    /// Updates an existing truck.
    /// </summary>
    /// <param name="id">Truck ID</param>
    /// <param name="request">Update truck request</param>
    /// <returns>Updated truck DTO or null if not found</returns>
    Task<TruckDto?> UpdateAsync(int id, UpdateTruckRequest request);

    /// <summary>
    /// Archives (soft-deletes) a truck by setting IsActive to false.
    /// </summary>
    /// <param name="id">Truck ID</param>
    /// <returns>True if truck was archived, false if not found</returns>
    Task<bool> ArchiveAsync(int id);
}
