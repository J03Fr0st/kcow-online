using Kcow.Application.Trucks;
using Kcow.Domain;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Trucks;

/// <summary>
/// Implementation of truck management service.
/// </summary>
public class TruckService : ITruckService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TruckService> _logger;

    public TruckService(AppDbContext context, ILogger<TruckService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active trucks.
    /// </summary>
    public async Task<List<TruckDto>> GetAllAsync()
    {
        var trucks = await _context.Trucks
            .Where(t => t.IsActive)
            .OrderBy(t => t.Name)
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
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} active trucks", trucks.Count);
        return trucks;
    }

    /// <summary>
    /// Gets a truck by ID.
    /// </summary>
    public async Task<TruckDto?> GetByIdAsync(int id)
    {
        var truck = await _context.Trucks
            .Where(t => t.Id == id && t.IsActive)
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
            .FirstOrDefaultAsync();

        if (truck == null)
        {
            _logger.LogWarning("Truck with ID {TruckId} not found", id);
        }
        else
        {
            _logger.LogInformation("Retrieved truck with ID {TruckId}", id);
        }

        return truck;
    }

    /// <summary>
    /// Creates a new truck.
    /// </summary>
    public async Task<TruckDto> CreateAsync(CreateTruckRequest request)
    {
        // Check for duplicate registration number
        var exists = await _context.Trucks
            .AnyAsync(t => t.RegistrationNumber == request.RegistrationNumber && t.IsActive);

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

        _context.Trucks.Add(truck);
        await _context.SaveChangesAsync();

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
    public async Task<TruckDto?> UpdateAsync(int id, UpdateTruckRequest request)
    {
        var truck = await _context.Trucks
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

        if (truck == null)
        {
            _logger.LogWarning("Cannot update: Truck with ID {TruckId} not found", id);
            return null;
        }

        // Check for duplicate registration number (excluding current truck)
        var duplicateExists = await _context.Trucks
            .AnyAsync(t => t.RegistrationNumber == request.RegistrationNumber
                       && t.Id != id
                       && t.IsActive);

        if (duplicateExists)
        {
            throw new InvalidOperationException($"Truck with registration number '{request.RegistrationNumber}' already exists");
        }

        truck.Name = request.Name;
        truck.RegistrationNumber = request.RegistrationNumber;
        truck.Status = request.Status;
        truck.Notes = request.Notes;
        truck.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

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
    public async Task<bool> ArchiveAsync(int id)
    {
        var truck = await _context.Trucks
            .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

        if (truck == null)
        {
            _logger.LogWarning("Cannot archive: Truck with ID {TruckId} not found", id);
            return false;
        }

        truck.IsActive = false;
        truck.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Archived truck with ID {TruckId}", id);
        return true;
    }
}
