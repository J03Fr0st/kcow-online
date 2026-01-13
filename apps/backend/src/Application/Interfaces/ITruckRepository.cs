using Kcow.Domain.Entities;

namespace Kcow.Application.Interfaces;

/// <summary>
/// Repository interface for Truck entity operations.
/// </summary>
public interface ITruckRepository
{
    /// <summary>
    /// Gets all trucks.
    /// </summary>
    Task<IEnumerable<Truck>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active trucks.
    /// </summary>
    Task<IEnumerable<Truck>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a truck by ID.
    /// </summary>
    Task<Truck?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a truck by registration number.
    /// </summary>
    Task<Truck?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new truck.
    /// </summary>
    Task<int> CreateAsync(Truck truck, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing truck.
    /// </summary>
    Task<bool> UpdateAsync(Truck truck, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a truck by ID.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a truck exists by ID.
    /// </summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a truck exists by registration number.
    /// </summary>
    Task<bool> ExistsByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default);
}
