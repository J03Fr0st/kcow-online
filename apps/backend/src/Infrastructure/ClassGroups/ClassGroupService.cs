using Kcow.Application.ClassGroups;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.ClassGroups;

/// <summary>
/// Implementation of class group management service using Dapper repositories.
/// </summary>
public class ClassGroupService : IClassGroupService
{
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ITruckRepository _truckRepository;
    private readonly ILogger<ClassGroupService> _logger;

    public ClassGroupService(
        IClassGroupRepository classGroupRepository,
        ISchoolRepository schoolRepository,
        ITruckRepository truckRepository,
        ILogger<ClassGroupService> logger)
    {
        _classGroupRepository = classGroupRepository;
        _schoolRepository = schoolRepository;
        _truckRepository = truckRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active class groups with optional filtering.
    /// </summary>
    public async Task<List<ClassGroupDto>> GetAllAsync(int? schoolId = null, int? truckId = null)
    {
        var classGroups = await _classGroupRepository.GetActiveAsync();
        var entities = classGroups.ToList();

        // Apply filters
        if (schoolId.HasValue)
        {
            entities = entities.Where(cg => cg.SchoolId == schoolId.Value).ToList();
        }

        if (truckId.HasValue)
        {
            entities = entities.Where(cg => cg.TruckId == truckId.Value).ToList();
        }

        // Order by SchoolId, DayOfWeek, StartTime
        entities = entities.OrderBy(cg => cg.SchoolId)
            .ThenBy(cg => cg.DayOfWeek)
            .ThenBy(cg => cg.StartTime)
            .ToList();

        // Load related entities and map to DTOs
        var result = new List<ClassGroupDto>();
        foreach (var cg in entities)
        {
            result.Add(await MapToDtoAsync(cg));
        }

        _logger.LogInformation("Retrieved {Count} active class groups (SchoolId: {SchoolId}, TruckId: {TruckId})",
            result.Count, schoolId, truckId);
        return result;
    }

    /// <summary>
    /// Gets a class group by ID with school and truck details.
    /// </summary>
    public async Task<ClassGroupDto?> GetByIdAsync(int id)
    {
        var entity = await _classGroupRepository.GetByIdAsync(id);

        if (entity == null || !entity.IsActive)
        {
            _logger.LogWarning("Class group with ID {ClassGroupId} not found", id);
            return null;
        }

        _logger.LogInformation("Retrieved class group with ID {ClassGroupId}", id);
        return await MapToDtoAsync(entity);
    }

    /// <summary>
    /// Creates a new class group.
    /// </summary>
    public async Task<ClassGroupDto> CreateAsync(CreateClassGroupRequest request)
    {
        // Validate that the school exists
        var schoolExists = await _schoolRepository.ExistsAsync(request.SchoolId);
        if (!schoolExists)
        {
            throw new InvalidOperationException($"School with ID {request.SchoolId} does not exist");
        }

        // Validate that the truck exists (if provided)
        if (request.TruckId.HasValue)
        {
            var truckExists = await _truckRepository.ExistsAsync(request.TruckId.Value);
            if (!truckExists)
            {
                throw new InvalidOperationException($"Truck with ID {request.TruckId.Value} does not exist");
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
            DayOfWeek = (DayOfWeek)request.DayOfWeek,
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

        var id = await _classGroupRepository.CreateAsync(classGroup);
        classGroup.Id = id;

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
        var classGroup = await _classGroupRepository.GetByIdAsync(id);

        if (classGroup == null || !classGroup.IsActive)
        {
            _logger.LogWarning("Cannot update: Class group with ID {ClassGroupId} not found", id);
            return null;
        }

        // Validate that the school exists
        var schoolExists = await _schoolRepository.ExistsAsync(request.SchoolId);
        if (!schoolExists)
        {
            throw new InvalidOperationException($"School with ID {request.SchoolId} does not exist");
        }

        // Validate that the truck exists (if provided)
        if (request.TruckId.HasValue)
        {
            var truckExists = await _truckRepository.ExistsAsync(request.TruckId.Value);
            if (!truckExists)
            {
                throw new InvalidOperationException($"Truck with ID {request.TruckId.Value} does not exist");
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
        classGroup.DayOfWeek = (DayOfWeek)request.DayOfWeek;
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

        await _classGroupRepository.UpdateAsync(classGroup);

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
        var classGroup = await _classGroupRepository.GetByIdAsync(id);

        if (classGroup == null || !classGroup.IsActive)
        {
            _logger.LogWarning("Cannot archive: Class group with ID {ClassGroupId} not found", id);
            return false;
        }

        classGroup.IsActive = false;
        classGroup.UpdatedAt = DateTime.UtcNow;

        await _classGroupRepository.UpdateAsync(classGroup);

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

        // Get all active class groups
        var allClassGroups = (await _classGroupRepository.GetActiveAsync()).ToList();

        // Filter by truck
        var matchingClassGroups = allClassGroups
            .Where(cg => cg.TruckId == request.TruckId)
            .ToList();

        // Filter by day of week - CAST TO INT
        matchingClassGroups = matchingClassGroups
            .Where(cg => (int)cg.DayOfWeek == request.DayOfWeek)
            .ToList();

        // Exclude current class group when editing
        if (request.ExcludeId.HasValue)
        {
            matchingClassGroups = matchingClassGroups
                .Where(cg => cg.Id != request.ExcludeId.Value)
                .ToList();
        }

        // Filter for time overlaps in memory
        // Two time ranges overlap if: startA < endB AND startB < endA
        var conflicts = new List<ScheduleConflictDto>();
        foreach (var cg in matchingClassGroups)
        {
            if (cg.StartTime < request.EndTime && request.StartTime < cg.EndTime)
            {
                // Load school name for conflict display
                var school = await _schoolRepository.GetByIdAsync(cg.SchoolId);
                conflicts.Add(new ScheduleConflictDto
                {
                    Id = cg.Id,
                    Name = cg.Name,
                    SchoolName = school?.Name ?? $"School {cg.SchoolId}",
                    StartTime = cg.StartTime,
                    EndTime = cg.EndTime
                });
            }
        }

        var response = new CheckConflictsResponse
        {
            HasConflicts = conflicts.Count > 0,
            Conflicts = conflicts
        };

        _logger.LogInformation("Found {ConflictCount} conflicts for TruckId: {TruckId}",
            conflicts.Count, request.TruckId);

        return response;
    }

    public async Task<IEnumerable<ClassGroupDto>> GetBySchoolIdAsync(int schoolId, bool includeDetails = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var classGroups = await _classGroupRepository.GetBySchoolIdAsync(schoolId, cancellationToken);
            
            var dtos = new List<ClassGroupDto>();
            foreach (var group in classGroups)
            {
                // Ensure SchoolDto and TruckDto are correctly instantiated
                SchoolDto? schoolDto = null;
                TruckDto? truckDto = null;

                if (includeDetails)
                {
                    var school = await _schoolRepository.GetByIdAsync(group.SchoolId, cancellationToken);
                    if (school != null)
                    {
                        schoolDto = new SchoolDto { Id = school.Id, Name = school.Name, ShortName = school.ShortName };
                    }

                    if (group.TruckId.HasValue)
                    {
                        var truck = await _truckRepository.GetByIdAsync(group.TruckId.Value, cancellationToken);
                        if (truck != null)
                        {
                            truckDto = new TruckDto { Id = truck.Id, Name = truck.Name, RegistrationNumber = truck.RegistrationNumber };
                        }
                    }
                }

                dtos.Add(new ClassGroupDto
                {
                    Id = group.Id,
                    Name = group.Name,
                    SchoolId = group.SchoolId,
                    TruckId = group.TruckId,
                    DayOfWeek = group.DayOfWeek,
                    StartTime = group.StartTime,
                    EndTime = group.EndTime,
                    Sequence = group.Sequence,
                    Evaluate = group.Evaluate,
                    Notes = group.Notes,
                    ImportFlag = group.ImportFlag,
                    GroupMessage = group.GroupMessage,
                    SendCertificates = group.SendCertificates,
                    MoneyMessage = group.MoneyMessage,
                    Ixl = group.Ixl,
                    IsActive = group.IsActive,
                    CreatedAt = group.CreatedAt,
                    UpdatedAt = group.UpdatedAt,
                    School = schoolDto,
                    Truck = truckDto
                });
            }

            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting class groups for school {SchoolId}", schoolId);
            throw;
        }
    }

    public async Task<IEnumerable<ClassGroupDto>> GetByDayAsync(int dayOfWeek, CancellationToken cancellationToken = default)
    {
        try
        {
            var classGroups = await _classGroupRepository.GetByDayAsync((DayOfWeek)dayOfWeek, cancellationToken);
            
            var dtos = new List<ClassGroupDto>();
            foreach (var group in classGroups)
            {
                dtos.Add(new ClassGroupDto
                {
                    Id = group.Id,
                    Name = group.Name,
                    SchoolId = group.SchoolId,
                    TruckId = group.TruckId,
                    DayOfWeek = group.DayOfWeek,
                    StartTime = group.StartTime,
                    EndTime = group.EndTime,
                    Sequence = group.Sequence,
                    Evaluate = group.Evaluate,
                    Notes = group.Notes,
                    ImportFlag = group.ImportFlag,
                    GroupMessage = group.GroupMessage,
                    SendCertificates = group.SendCertificates,
                    MoneyMessage = group.MoneyMessage,
                    Ixl = group.Ixl,
                    IsActive = group.IsActive,
                    CreatedAt = group.CreatedAt,
                    UpdatedAt = group.UpdatedAt
                });
            }
            return dtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting class groups for day {DayOfWeek}", dayOfWeek);
            throw;
        }
    }

    /// <summary>
    /// Maps a ClassGroup entity to a ClassGroupDto, loading related entities.
    /// </summary>
    private async Task<ClassGroupDto> MapToDtoAsync(ClassGroup cg)
    {
        SchoolDto? schoolDto = null;
        if (cg.SchoolId > 0)
        {
            var school = await _schoolRepository.GetByIdAsync(cg.SchoolId);
            if (school != null)
            {
                schoolDto = new SchoolDto
                {
                    Id = school.Id,
                    Name = school.Name,
                    ShortName = school.ShortName
                };
            }
        }

        TruckDto? truckDto = null;
        if (cg.TruckId.HasValue)
        {
            var truck = await _truckRepository.GetByIdAsync(cg.TruckId.Value);
            if (truck != null)
            {
                truckDto = new TruckDto
                {
                    Id = truck.Id,
                    Name = truck.Name,
                    RegistrationNumber = truck.RegistrationNumber
                };
            }
        }

        return new ClassGroupDto
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
            School = schoolDto,
            Truck = truckDto
        };
    }
}