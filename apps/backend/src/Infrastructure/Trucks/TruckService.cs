using Kcow.Application.Interfaces;
using Kcow.Application.Trucks;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Trucks;

/// <summary>
/// Implementation of truck management service using Dapper repositories.
/// </summary>
public class TruckService : ITruckService
{
    private readonly ITruckRepository _truckRepository;
    private readonly ILogger<TruckService> _logger;

    public TruckService(ITruckRepository truckRepository, ILogger<TruckService> logger)
    {
        _truckRepository = truckRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active trucks.
    /// </summary>
    public async Task<List<TruckDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var trucks = (await _truckRepository.GetActiveAsync(cancellationToken))
            .Select(t => new TruckDto
            {
                Id = t.Id,
                Name = t.Name,
                RegistrationNumber = t.RegistrationNumber,
                Status = t.Status,
                Notes = t.Notes,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .OrderBy(t => t.Name)
            .ToList();

        _logger.LogInformation("Retrieved {Count} active trucks", trucks.Count);
        return trucks;
    }

    /// <summary>
    /// Gets a truck by ID.
    /// </summary>
    public async Task<TruckDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var truck = await _truckRepository.GetByIdAsync(id, cancellationToken);

        if (truck == null || !truck.IsActive)
        {
            _logger.LogWarning("Truck with ID {TruckId} not found", id);
            return null;
        }

        _logger.LogInformation("Retrieved truck with ID {TruckId}", id);

        return new TruckDto
        {
            Id = truck.Id,
            Name = truck.Name,
            RegistrationNumber = truck.RegistrationNumber,
            Status = truck.Status,
            Notes = truck.Notes,
            IsActive = truck.IsActive,
            CreatedAt = truck.CreatedAt,
            UpdatedAt = truck.UpdatedAt
        };
    }

    /// <summary>
    /// Creates a new truck.
    /// </summary>
    public async Task<TruckDto> CreateAsync(CreateTruckRequest request, CancellationToken cancellationToken = default)
    {
        // Check for duplicate registration number
        var exists = await _truckRepository.ExistsByRegistrationNumberAsync(request.RegistrationNumber, cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"Truck with registration number '{request.RegistrationNumber}' already exists");
        }

        var truck = new Truck
        {
            Name = request.Name,
            RegistrationNumber = request.RegistrationNumber,
            Status = request.Status,
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _truckRepository.CreateAsync(truck, cancellationToken);
        truck.Id = id; // Set the ID returned by repository

        _logger.LogInformation("Created truck with ID {TruckId} and registration {RegistrationNumber}",
            truck.Id, truck.RegistrationNumber);

        return new TruckDto
        {
            Id = truck.Id,
            Name = truck.Name,
            RegistrationNumber = truck.RegistrationNumber,
            Status = truck.Status,
            Notes = truck.Notes,
            IsActive = truck.IsActive,
            CreatedAt = truck.CreatedAt,
            UpdatedAt = truck.UpdatedAt
        };
    }

    /// <summary>
    /// Updates an existing truck.
    /// </summary>
    public async Task<TruckDto?> UpdateAsync(int id, UpdateTruckRequest request, CancellationToken cancellationToken = default)
    {
        var truck = await _truckRepository.GetByIdAsync(id, cancellationToken);

        if (truck == null || !truck.IsActive)
        {
            _logger.LogWarning("Cannot update: Truck with ID {TruckId} not found", id);
            return null;
        }

        // Check for duplicate registration number (excluding current truck)
        var existingTruck = await _truckRepository.GetByRegistrationNumberAsync(request.RegistrationNumber, cancellationToken);
        if (existingTruck != null && existingTruck.Id != id)
        {
            throw new InvalidOperationException($"Truck with registration number '{request.RegistrationNumber}' already exists");
        }

        truck.Name = request.Name;
        truck.RegistrationNumber = request.RegistrationNumber;
        truck.Status = request.Status;
        truck.Notes = request.Notes;
        truck.UpdatedAt = DateTime.UtcNow;

        await _truckRepository.UpdateAsync(truck, cancellationToken);

        _logger.LogInformation("Updated truck with ID {TruckId}", id);

        return new TruckDto
        {
            Id = truck.Id,
            Name = truck.Name,
            RegistrationNumber = truck.RegistrationNumber,
            Status = truck.Status,
            Notes = truck.Notes,
            IsActive = truck.IsActive,
            CreatedAt = truck.CreatedAt,
            UpdatedAt = truck.UpdatedAt
        };
    }

    /// <summary>
    /// Archives (soft-deletes) a truck.
    /// </summary>
    public async Task<bool> ArchiveAsync(int id, CancellationToken cancellationToken = default)
    {
        var truck = await _truckRepository.GetByIdAsync(id, cancellationToken);

        if (truck == null || !truck.IsActive)
        {
            _logger.LogWarning("Cannot archive: Truck with ID {TruckId} not found", id);
            return false;
        }

        truck.IsActive = false;
        truck.UpdatedAt = DateTime.UtcNow;

        await _truckRepository.UpdateAsync(truck, cancellationToken);

        _logger.LogInformation("Archived truck with ID {TruckId}", id);
        return true;
    }
}
