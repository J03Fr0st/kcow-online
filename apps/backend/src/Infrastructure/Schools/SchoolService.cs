using Kcow.Application.Schools;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Schools;

/// <summary>
/// Implementation of school management service.
/// </summary>
public class SchoolService : ISchoolService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SchoolService> _logger;

    public SchoolService(AppDbContext context, ILogger<SchoolService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active schools.
    /// </summary>
    public async Task<List<SchoolDto>> GetAllAsync()
    {
        var schools = await _context.Schools
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new SchoolDto
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                ContactName = s.ContactName,
                ContactPhone = s.ContactPhone,
                ContactEmail = s.ContactEmail,
                BillingSettings = s.BillingSettings == null ? null : new BillingSettingsDto
                {
                    DefaultSessionRate = s.BillingSettings.DefaultSessionRate,
                    BillingCycle = s.BillingSettings.BillingCycle,
                    BillingNotes = s.BillingSettings.BillingNotes
                },
                Notes = s.Notes,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} active schools", schools.Count);
        return schools;
    }

    /// <summary>
    /// Gets a school by ID.
    /// </summary>
    public async Task<SchoolDto?> GetByIdAsync(int id)
    {
        var school = await _context.Schools
            .Where(s => s.Id == id && s.IsActive)
            .Select(s => new SchoolDto
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                ContactName = s.ContactName,
                ContactPhone = s.ContactPhone,
                ContactEmail = s.ContactEmail,
                BillingSettings = s.BillingSettings == null ? null : new BillingSettingsDto
                {
                    DefaultSessionRate = s.BillingSettings.DefaultSessionRate,
                    BillingCycle = s.BillingSettings.BillingCycle,
                    BillingNotes = s.BillingSettings.BillingNotes
                },
                Notes = s.Notes,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (school == null)
        {
            _logger.LogWarning("School with ID {SchoolId} not found", id);
        }
        else
        {
            _logger.LogInformation("Retrieved school with ID {SchoolId}", id);
        }

        return school;
    }

    /// <summary>
    /// Creates a new school.
    /// </summary>
    public async Task<SchoolDto> CreateAsync(CreateSchoolRequest request)
    {
        var school = new School
        {
            Name = request.Name,
            Address = request.Address,
            ContactName = request.ContactName,
            ContactPhone = request.ContactPhone,
            ContactEmail = request.ContactEmail,
            BillingSettings = request.BillingSettings == null ? null : new Domain.Entities.BillingSettings
            {
                DefaultSessionRate = request.BillingSettings.DefaultSessionRate,
                BillingCycle = request.BillingSettings.BillingCycle,
                BillingNotes = request.BillingSettings.BillingNotes
            },
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Schools.Add(school);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created school with ID {SchoolId} and name '{SchoolName}'",
            school.Id, school.Name);

        return new SchoolDto
        {
            Id = school.Id,
            Name = school.Name,
            Address = school.Address,
            ContactName = school.ContactName,
            ContactPhone = school.ContactPhone,
            ContactEmail = school.ContactEmail,
            BillingSettings = school.BillingSettings == null ? null : new BillingSettingsDto
            {
                DefaultSessionRate = school.BillingSettings.DefaultSessionRate,
                BillingCycle = school.BillingSettings.BillingCycle,
                BillingNotes = school.BillingSettings.BillingNotes
            },
            Notes = school.Notes,
            IsActive = school.IsActive,
            CreatedAt = school.CreatedAt,
            UpdatedAt = school.UpdatedAt
        };
    }

    /// <summary>
    /// Updates an existing school.
    /// </summary>
    public async Task<SchoolDto?> UpdateAsync(int id, UpdateSchoolRequest request)
    {
        var school = await _context.Schools
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (school == null)
        {
            _logger.LogWarning("Cannot update: School with ID {SchoolId} not found", id);
            return null;
        }

        school.Name = request.Name;
        school.Address = request.Address;
        school.ContactName = request.ContactName;
        school.ContactPhone = request.ContactPhone;
        school.ContactEmail = request.ContactEmail;
        school.BillingSettings = request.BillingSettings == null ? null : new Domain.Entities.BillingSettings
        {
            DefaultSessionRate = request.BillingSettings.DefaultSessionRate,
            BillingCycle = request.BillingSettings.BillingCycle,
            BillingNotes = request.BillingSettings.BillingNotes
        };
        school.Notes = request.Notes;
        school.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated school with ID {SchoolId}", id);

        return new SchoolDto
        {
            Id = school.Id,
            Name = school.Name,
            Address = school.Address,
            ContactName = school.ContactName,
            ContactPhone = school.ContactPhone,
            ContactEmail = school.ContactEmail,
            BillingSettings = school.BillingSettings == null ? null : new BillingSettingsDto
            {
                DefaultSessionRate = school.BillingSettings.DefaultSessionRate,
                BillingCycle = school.BillingSettings.BillingCycle,
                BillingNotes = school.BillingSettings.BillingNotes
            },
            Notes = school.Notes,
            IsActive = school.IsActive,
            CreatedAt = school.CreatedAt,
            UpdatedAt = school.UpdatedAt
        };
    }

    /// <summary>
    /// Archives (soft-deletes) a school.
    /// </summary>
    public async Task<bool> ArchiveAsync(int id)
    {
        var school = await _context.Schools
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (school == null)
        {
            _logger.LogWarning("Cannot archive: School with ID {SchoolId} not found", id);
            return false;
        }

        school.IsActive = false;
        school.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Archived school with ID {SchoolId}", id);
        return true;
    }
}
