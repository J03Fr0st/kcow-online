using Kcow.Application.Activities;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Activities;

/// <summary>
/// Implementation of activity management service.
/// </summary>
public class ActivityService : IActivityService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(AppDbContext context, ILogger<ActivityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active activities.
    /// </summary>
    public async Task<List<ActivityDto>> GetAllAsync()
    {
        var activities = await _context.Activities
            .Where(a => a.IsActive)
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Code)
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
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} active activities", activities.Count);
        return activities;
    }

    /// <summary>
    /// Gets an activity by ID.
    /// </summary>
    public async Task<ActivityDto?> GetByIdAsync(int id)
    {
        var activity = await _context.Activities
            .Where(a => a.Id == id && a.IsActive)
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
            .FirstOrDefaultAsync();

        if (activity == null)
        {
            _logger.LogWarning("Activity with ID {ActivityId} not found", id);
        }
        else
        {
            _logger.LogInformation("Retrieved activity with ID {ActivityId}", id);
        }

        return activity;
    }

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    public async Task<ActivityDto> CreateAsync(CreateActivityRequest request)
    {
        // Check for duplicate Code (if provided)
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var exists = await _context.Activities
                .AnyAsync(a => a.Code == request.Code && a.IsActive);

            if (exists)
            {
                throw new InvalidOperationException($"Activity with code '{request.Code}' already exists");
            }
        }

        // Check for duplicate ID (if provided for legacy import)
        if (request.Id.HasValue)
        {
            var idExists = await _context.Activities
                .IgnoreQueryFilters()
                .AnyAsync(a => a.Id == request.Id.Value);

            if (idExists)
            {
                throw new InvalidOperationException($"Activity with ID '{request.Id.Value}' already exists");
            }
        }

        // Generate new ID if not provided (since entity uses ValueGeneratedNever)
        var activityId = request.Id ?? await GetNextIdAsync();

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

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

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
    public async Task<ActivityDto?> UpdateAsync(int id, UpdateActivityRequest request)
    {
        var activity = await _context.Activities
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);

        if (activity == null)
        {
            _logger.LogWarning("Cannot update: Activity with ID {ActivityId} not found", id);
            return null;
        }

        // Check for duplicate Code (if provided and changed)
        if (!string.IsNullOrWhiteSpace(request.Code) && request.Code != activity.Code)
        {
            var duplicateExists = await _context.Activities
                .AnyAsync(a => a.Code == request.Code
                           && a.Id != id
                           && a.IsActive);

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

        await _context.SaveChangesAsync();

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
    public async Task<bool> ArchiveAsync(int id)
    {
        var activity = await _context.Activities
            .FirstOrDefaultAsync(a => a.Id == id && a.IsActive);

        if (activity == null)
        {
            _logger.LogWarning("Cannot archive: Activity with ID {ActivityId} not found", id);
            return false;
        }

        activity.IsActive = false;
        activity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Archived activity with ID {ActivityId}", id);
        return true;
    }

    /// <summary>
    /// Gets the next available ID for a new activity.
    /// Required because Activity entity uses ValueGeneratedNever().
    /// </summary>
    private async Task<int> GetNextIdAsync()
    {
        var maxId = await _context.Activities
            .IgnoreQueryFilters()
            .MaxAsync(a => (int?)a.Id) ?? 0;

        return maxId + 1;
    }
}
