using Kcow.Application.ClassGroups;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.ClassGroups;

/// <summary>
/// Implementation of class group management service.
/// </summary>
public class ClassGroupService : IClassGroupService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClassGroupService> _logger;

    public ClassGroupService(AppDbContext context, ILogger<ClassGroupService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active class groups with optional filtering.
    /// </summary>
    public async Task<List<ClassGroupDto>> GetAllAsync(int? schoolId = null, int? truckId = null)
    {
        var query = _context.ClassGroups
            .Include(cg => cg.School)
            .Include(cg => cg.Truck)
            .Where(cg => cg.IsActive);

        if (schoolId.HasValue)
        {
            query = query.Where(cg => cg.SchoolId == schoolId.Value);
        }

        if (truckId.HasValue)
        {
            query = query.Where(cg => cg.TruckId == truckId.Value);
        }

        // Load entities first, then map to DTOs in memory
        // This avoids EF Core translation issues with conditional expressions in Select
        var entities = await query
            .OrderBy(cg => cg.SchoolId)
            .ThenBy(cg => cg.DayOfWeek)
            .ThenBy(cg => cg.StartTime)
            .ToListAsync();

        var classGroups = entities.Select(cg => new ClassGroupDto
        {
            Id = cg.Id,
            Name = cg.Name,
            DayTruck = cg.DayTruck,
            Description = cg.Description,
            SchoolId = cg.SchoolId,
            TruckId = cg.TruckId,
            DayOfWeek = cg.DayOfWeek,
            StartTime = cg.StartTime,
            EndTime = cg.EndTime,
            Sequence = cg.Sequence,
            Evaluate = cg.Evaluate,
            Notes = cg.Notes,
            ImportFlag = cg.ImportFlag,
            GroupMessage = cg.GroupMessage,
            SendCertificates = cg.SendCertificates,
            MoneyMessage = cg.MoneyMessage,
            Ixl = cg.Ixl,
            IsActive = cg.IsActive,
            CreatedAt = cg.CreatedAt,
            UpdatedAt = cg.UpdatedAt,
            School = cg.School != null ? new SchoolDto
            {
                Id = cg.School.Id,
                Name = cg.School.Name,
                ShortName = cg.School.ShortName
            } : null,
            Truck = cg.Truck != null ? new TruckDto
            {
                Id = cg.Truck.Id,
                Name = cg.Truck.Name,
                RegistrationNumber = cg.Truck.RegistrationNumber
            } : null
        }).ToList();

        _logger.LogInformation("Retrieved {Count} active class groups (SchoolId: {SchoolId}, TruckId: {TruckId})",
            classGroups.Count, schoolId, truckId);
        return classGroups;
    }

    /// <summary>
    /// Gets a class group by ID with school and truck details.
    /// </summary>
    public async Task<ClassGroupDto?> GetByIdAsync(int id)
    {
        var entity = await _context.ClassGroups
            .Include(cg => cg.School)
            .Include(cg => cg.Truck)
            .FirstOrDefaultAsync(cg => cg.Id == id && cg.IsActive);

        if (entity == null)
        {
            return null;
        }

        // Map entity to DTO in memory to avoid EF Core translation issues
        var classGroup = new ClassGroupDto
        {
            Id = entity.Id,
            Name = entity.Name,
            DayTruck = entity.DayTruck,
            Description = entity.Description,
            SchoolId = entity.SchoolId,
            TruckId = entity.TruckId,
            DayOfWeek = entity.DayOfWeek,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            Sequence = entity.Sequence,
            Evaluate = entity.Evaluate,
            Notes = entity.Notes,
            ImportFlag = entity.ImportFlag,
            GroupMessage = entity.GroupMessage,
            SendCertificates = entity.SendCertificates,
            MoneyMessage = entity.MoneyMessage,
            Ixl = entity.Ixl,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            School = entity.School != null ? new SchoolDto
            {
                Id = entity.School.Id,
                Name = entity.School.Name,
                ShortName = entity.School.ShortName
            } : null,
            Truck = entity.Truck != null ? new TruckDto
            {
                Id = entity.Truck.Id,
                Name = entity.Truck.Name,
                RegistrationNumber = entity.Truck.RegistrationNumber
            } : null
        };

        if (classGroup == null)
        {
            _logger.LogWarning("Class group with ID {ClassGroupId} not found", id);
        }
        else
        {
            _logger.LogInformation("Retrieved class group with ID {ClassGroupId}", id);
        }

        return classGroup;
    }

    /// <summary>
    /// Creates a new class group.
    /// </summary>
    public async Task<ClassGroupDto> CreateAsync(CreateClassGroupRequest request)
    {
        // Validate that the school exists
        var schoolExists = await _context.Schools.AnyAsync(s => s.Id == request.SchoolId && s.IsActive);
        if (!schoolExists)
        {
            throw new InvalidOperationException($"School with ID {request.SchoolId} does not exist or is not active");
        }

        // Validate that the truck exists (if provided)
        if (request.TruckId.HasValue)
        {
            var truckExists = await _context.Trucks.AnyAsync(t => t.Id == request.TruckId.Value && t.IsActive);
            if (!truckExists)
            {
                throw new InvalidOperationException($"Truck with ID {request.TruckId.Value} does not exist or is not active");
            }
        }

        // Validate time range
        if (request.EndTime <= request.StartTime)
        {
            throw new InvalidOperationException("End time must be after start time");
        }

        var classGroup = new ClassGroup
        {
            Name = request.Name,
            DayTruck = request.DayTruck,
            Description = request.Description,
            SchoolId = request.SchoolId,
            TruckId = request.TruckId,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Sequence = request.Sequence,
            Evaluate = request.Evaluate,
            Notes = request.Notes,
            ImportFlag = request.ImportFlag,
            GroupMessage = request.GroupMessage,
            SendCertificates = request.SendCertificates,
            MoneyMessage = request.MoneyMessage,
            Ixl = request.Ixl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ClassGroups.Add(classGroup);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created class group with ID {ClassGroupId}: {Name} at School {SchoolId}",
            classGroup.Id, classGroup.Name, classGroup.SchoolId);

        // Reload with navigation properties
        var created = await GetByIdAsync(classGroup.Id);
        return created!;
    }

    /// <summary>
    /// Updates an existing class group.
    /// </summary>
    public async Task<ClassGroupDto?> UpdateAsync(int id, UpdateClassGroupRequest request)
    {
        var classGroup = await _context.ClassGroups
            .Include(cg => cg.School)
            .Include(cg => cg.Truck)
            .FirstOrDefaultAsync(cg => cg.Id == id && cg.IsActive);

        if (classGroup == null)
        {
            _logger.LogWarning("Cannot update: Class group with ID {ClassGroupId} not found", id);
            return null;
        }

        // Validate that the school exists
        var schoolExists = await _context.Schools.AnyAsync(s => s.Id == request.SchoolId && s.IsActive);
        if (!schoolExists)
        {
            throw new InvalidOperationException($"School with ID {request.SchoolId} does not exist or is not active");
        }

        // Validate that the truck exists (if provided)
        if (request.TruckId.HasValue)
        {
            var truckExists = await _context.Trucks.AnyAsync(t => t.Id == request.TruckId.Value && t.IsActive);
            if (!truckExists)
            {
                throw new InvalidOperationException($"Truck with ID {request.TruckId.Value} does not exist or is not active");
            }
        }

        // Validate time range
        if (request.EndTime <= request.StartTime)
        {
            throw new InvalidOperationException("End time must be after start time");
        }

        classGroup.Name = request.Name;
        classGroup.DayTruck = request.DayTruck;
        classGroup.Description = request.Description;
        classGroup.SchoolId = request.SchoolId;
        classGroup.TruckId = request.TruckId;
        classGroup.DayOfWeek = request.DayOfWeek;
        classGroup.StartTime = request.StartTime;
        classGroup.EndTime = request.EndTime;
        classGroup.Sequence = request.Sequence;
        classGroup.Evaluate = request.Evaluate;
        classGroup.Notes = request.Notes;
        classGroup.ImportFlag = request.ImportFlag;
        classGroup.GroupMessage = request.GroupMessage;
        classGroup.SendCertificates = request.SendCertificates;
        classGroup.MoneyMessage = request.MoneyMessage;
        classGroup.Ixl = request.Ixl;
        classGroup.IsActive = request.IsActive;
        classGroup.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated class group with ID {ClassGroupId}", id);

        // Reload with navigation properties
        var updated = await GetByIdAsync(id);
        return updated;
    }

    /// <summary>
    /// Archives (soft-deletes) a class group.
    /// </summary>
    public async Task<bool> ArchiveAsync(int id)
    {
        var classGroup = await _context.ClassGroups
            .FirstOrDefaultAsync(cg => cg.Id == id && cg.IsActive);

        if (classGroup == null)
        {
            _logger.LogWarning("Cannot archive: Class group with ID {ClassGroupId} not found", id);
            return false;
        }

        classGroup.IsActive = false;
        classGroup.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Archived class group with ID {ClassGroupId}", id);
        return true;
    }

    /// <summary>
    /// Checks for scheduling conflicts with existing class groups.
    /// </summary>
    public async Task<CheckConflictsResponse> CheckConflictsAsync(CheckConflictsRequest request)
    {
        _logger.LogInformation(
            "Checking conflicts for TruckId: {TruckId}, DayOfWeek: {DayOfWeek}, StartTime: {StartTime}, EndTime: {EndTime}, ExcludeId: {ExcludeId}",
            request.TruckId, request.DayOfWeek, request.StartTime, request.EndTime, request.ExcludeId);

        // Find class groups that match the criteria
        var query = _context.ClassGroups
            .Include(cg => cg.School)
            .Where(cg => cg.IsActive);
        // Filter by truck
        query = query.Where(cg => cg.TruckId == request.TruckId);
        // Filter by day of week (convert int to DayOfWeek enum)
        query = query.Where(cg => cg.DayOfWeek == (DayOfWeek)request.DayOfWeek);
        // Exclude current class group when editing
        if (request.ExcludeId.HasValue)
        {
            query = query.Where(cg => cg.Id != request.ExcludeId.Value);
        }

        var allClassGroups = await query.ToListAsync();

        // Filter for time overlaps in memory
        // Two time ranges overlap if: startA < endB AND startB < endA
        var conflicts = allClassGroups
            .Where(cg =>
                cg.StartTime < request.EndTime && request.StartTime < cg.EndTime)
            .Select(cg => new ScheduleConflictDto
            {
                Id = cg.Id,
                Name = cg.Name,
                SchoolName = cg.School?.Name ?? $"School {cg.SchoolId}",
                StartTime = cg.StartTime,
                EndTime = cg.EndTime
            })
            .ToList();

        var response = new CheckConflictsResponse
        {
            HasConflicts = conflicts.Count > 0,
            Conflicts = conflicts
        };

        _logger.LogInformation("Found {ConflictCount} conflicts for TruckId: {TruckId}",
            conflicts.Count, request.TruckId);

        return response;
    }
}
