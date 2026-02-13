using Kcow.Application.Activities;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Activities;

/// <summary>
/// Implementation of activity management service using Dapper repositories.
/// </summary>
public class ActivityService : IActivityService
{
    private readonly IActivityRepository _activityRepository;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(IActivityRepository activityRepository, ILogger<ActivityService> logger)
    {
        _activityRepository = activityRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active activities.
    /// </summary>
    public async Task<List<ActivityDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var activities = (await _activityRepository.GetActiveAsync(cancellationToken))
            .Select(a => new ActivityDto
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                Description = a.Description,
                Folder = a.Folder,
                GradeLevel = a.GradeLevel,
                Icon = a.Icon,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            })
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Code)
            .ToList();

        _logger.LogInformation("Retrieved {Count} active activities", activities.Count);
        return activities;
    }

    /// <summary>
    /// Gets an activity by ID.
    /// </summary>
    public async Task<ActivityDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(id, cancellationToken);

        if (activity == null || !activity.IsActive)
        {
            _logger.LogWarning("Activity with ID {ActivityId} not found", id);
            return null;
        }

        _logger.LogInformation("Retrieved activity with ID {ActivityId}", id);

        return new ActivityDto
        {
            Id = activity.Id,
            Code = activity.Code,
            Name = activity.Name,
            Description = activity.Description,
            Folder = activity.Folder,
            GradeLevel = activity.GradeLevel,
            Icon = activity.Icon,
            IsActive = activity.IsActive,
            CreatedAt = activity.CreatedAt,
            UpdatedAt = activity.UpdatedAt
        };
    }

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    public async Task<ActivityDto> CreateAsync(CreateActivityRequest request, CancellationToken cancellationToken = default)
    {
        // Check for duplicate Code (if provided)
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var exists = await _activityRepository.ExistsByCodeAsync(request.Code, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Activity with code '{request.Code}' already exists");
            }
        }

        // Check for duplicate ID (if provided for legacy import)
        if (request.Id.HasValue)
        {
            var idExists = await _activityRepository.ExistsAsync(request.Id.Value, cancellationToken);
            if (idExists)
            {
                throw new InvalidOperationException($"Activity with ID '{request.Id.Value}' already exists");
            }
        }

        // Generate new ID if not provided (since entity uses ValueGeneratedNever)
        var activityId = request.Id ?? await GetNextIdAsync(cancellationToken);

        var activity = new Activity
        {
            Id = activityId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Folder = request.Folder,
            GradeLevel = request.GradeLevel,
            Icon = request.Icon,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _activityRepository.CreateAsync(activity, cancellationToken);
        activity.Id = id; // Set the ID returned by repository

        _logger.LogInformation("Created activity with ID {ActivityId} and code {Code}",
            activity.Id, activity.Code ?? "(no code)");

        return new ActivityDto
        {
            Id = activity.Id,
            Code = activity.Code,
            Name = activity.Name,
            Description = activity.Description,
            Folder = activity.Folder,
            GradeLevel = activity.GradeLevel,
            Icon = activity.Icon,
            IsActive = activity.IsActive,
            CreatedAt = activity.CreatedAt,
            UpdatedAt = activity.UpdatedAt
        };
    }

    /// <summary>
    /// Updates an existing activity.
    /// </summary>
    public async Task<ActivityDto?> UpdateAsync(int id, UpdateActivityRequest request, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(id, cancellationToken);

        if (activity == null || !activity.IsActive)
        {
            _logger.LogWarning("Cannot update: Activity with ID {ActivityId} not found", id);
            return null;
        }

        // Check for duplicate Code (if provided and changed)
        if (!string.IsNullOrWhiteSpace(request.Code) && request.Code != activity.Code)
        {
            var duplicateExists = await _activityRepository.ExistsByCodeAsync(request.Code, cancellationToken);
            if (duplicateExists)
            {
                throw new InvalidOperationException($"Activity with code '{request.Code}' already exists");
            }
        }

        activity.Code = request.Code;
        activity.Name = request.Name;
        activity.Description = request.Description;
        activity.Folder = request.Folder;
        activity.GradeLevel = request.GradeLevel;
        activity.Icon = request.Icon;
        activity.UpdatedAt = DateTime.UtcNow;

        await _activityRepository.UpdateAsync(activity, cancellationToken);

        _logger.LogInformation("Updated activity with ID {ActivityId}", id);

        return new ActivityDto
        {
            Id = activity.Id,
            Code = activity.Code,
            Name = activity.Name,
            Description = activity.Description,
            Folder = activity.Folder,
            GradeLevel = activity.GradeLevel,
            Icon = activity.Icon,
            IsActive = activity.IsActive,
            CreatedAt = activity.CreatedAt,
            UpdatedAt = activity.UpdatedAt
        };
    }

    /// <summary>
    /// Archives (soft-deletes) an activity.
    /// </summary>
    public async Task<bool> ArchiveAsync(int id, CancellationToken cancellationToken = default)
    {
        var activity = await _activityRepository.GetByIdAsync(id, cancellationToken);

        if (activity == null || !activity.IsActive)
        {
            _logger.LogWarning("Cannot archive: Activity with ID {ActivityId} not found", id);
            return false;
        }

        activity.IsActive = false;
        activity.UpdatedAt = DateTime.UtcNow;

        await _activityRepository.UpdateAsync(activity, cancellationToken);

        _logger.LogInformation("Archived activity with ID {ActivityId}", id);
        return true;
    }

    /// <summary>
    /// Gets the next available ID for a new activity.
    /// Required because Activity entity uses ValueGeneratedNever().
    /// </summary>
    private async Task<int> GetNextIdAsync(CancellationToken cancellationToken = default)
    {
        var allActivities = await _activityRepository.GetAllAsync(cancellationToken);
        var maxId = allActivities.Any() ? allActivities.Max(a => a.Id) : 0;
        return maxId + 1;
    }
}
