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
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} active schools", schools.Count);
        return schools.Select(MapToDto).ToList();
    }

    /// <summary>
    /// Gets a school by ID.
    /// </summary>
    public async Task<SchoolDto?> GetByIdAsync(int id)
    {
        var school = await _context.Schools
            .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

        if (school == null)
        {
            _logger.LogWarning("School with ID {SchoolId} not found", id);
            return null;
        }

        _logger.LogInformation("Retrieved school with ID {SchoolId}", id);
        return MapToDto(school);
    }

    /// <summary>
    /// Creates a new school.
    /// </summary>
    public async Task<SchoolDto> CreateAsync(CreateSchoolRequest request)
    {
        var school = new School
        {
            Name = request.Name,
            ShortName = request.ShortName,
            TruckId = request.TruckId,
            Price = request.Price,
            FeeDescription = request.FeeDescription,
            Formula = request.Formula,
            VisitDay = request.VisitDay,
            VisitSequence = request.VisitSequence,
            ContactPerson = request.ContactPerson,
            ContactCell = request.ContactCell,
            Phone = request.Phone,
            Telephone = request.Telephone,
            Fax = request.Fax,
            Email = request.Email,
            CircularsEmail = request.CircularsEmail,
            Address = request.Address,
            Address2 = request.Address2,
            Headmaster = request.Headmaster,
            HeadmasterCell = request.HeadmasterCell,
            IsActive = true,
            Language = request.Language,
            PrintInvoice = request.PrintInvoice,
            ImportFlag = request.ImportFlag,
            Afterschool1Name = request.Afterschool1Name,
            Afterschool1Contact = request.Afterschool1Contact,
            Afterschool2Name = request.Afterschool2Name,
            Afterschool2Contact = request.Afterschool2Contact,
            SchedulingNotes = request.SchedulingNotes,
            MoneyMessage = request.MoneyMessage,
            SafeNotes = request.SafeNotes,
            WebPage = request.WebPage,
            KcowWebPageLink = request.KcowWebPageLink,
            BillingSettings = request.BillingSettings == null ? null : new Domain.Entities.BillingSettings
            {
                DefaultSessionRate = request.BillingSettings.DefaultSessionRate,
                BillingCycle = request.BillingSettings.BillingCycle,
                BillingNotes = request.BillingSettings.BillingNotes
            },
            CreatedAt = DateTime.UtcNow
        };

        _context.Schools.Add(school);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created school with ID {SchoolId} and name '{SchoolName}'",
            school.Id, school.Name);

        return MapToDto(school);
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
        school.ShortName = request.ShortName;
        school.TruckId = request.TruckId;
        school.Price = request.Price;
        school.FeeDescription = request.FeeDescription;
        school.Formula = request.Formula;
        school.VisitDay = request.VisitDay;
        school.VisitSequence = request.VisitSequence;
        school.ContactPerson = request.ContactPerson;
        school.ContactCell = request.ContactCell;
        school.Phone = request.Phone;
        school.Telephone = request.Telephone;
        school.Fax = request.Fax;
        school.Email = request.Email;
        school.CircularsEmail = request.CircularsEmail;
        school.Address = request.Address;
        school.Address2 = request.Address2;
        school.Headmaster = request.Headmaster;
        school.HeadmasterCell = request.HeadmasterCell;
        school.Language = request.Language;
        school.PrintInvoice = request.PrintInvoice;
        school.ImportFlag = request.ImportFlag;
        school.Afterschool1Name = request.Afterschool1Name;
        school.Afterschool1Contact = request.Afterschool1Contact;
        school.Afterschool2Name = request.Afterschool2Name;
        school.Afterschool2Contact = request.Afterschool2Contact;
        school.SchedulingNotes = request.SchedulingNotes;
        school.MoneyMessage = request.MoneyMessage;
        school.SafeNotes = request.SafeNotes;
        school.WebPage = request.WebPage;
        school.KcowWebPageLink = request.KcowWebPageLink;
        school.BillingSettings = request.BillingSettings == null ? null : new Domain.Entities.BillingSettings
        {
            DefaultSessionRate = request.BillingSettings.DefaultSessionRate,
            BillingCycle = request.BillingSettings.BillingCycle,
            BillingNotes = request.BillingSettings.BillingNotes
        };
        school.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated school with ID {SchoolId}", id);

        return MapToDto(school);
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

    /// <summary>
    /// Maps a School entity to a SchoolDto.
    /// </summary>
    private static SchoolDto MapToDto(School school)
    {
        return new SchoolDto
        {
            Id = school.Id,
            Name = school.Name,
            ShortName = school.ShortName,
            TruckId = school.TruckId,
            Price = school.Price,
            FeeDescription = school.FeeDescription,
            Formula = school.Formula,
            VisitDay = school.VisitDay,
            VisitSequence = school.VisitSequence,
            ContactPerson = school.ContactPerson,
            ContactCell = school.ContactCell,
            Phone = school.Phone,
            Telephone = school.Telephone,
            Fax = school.Fax,
            Email = school.Email,
            CircularsEmail = school.CircularsEmail,
            Address = school.Address,
            Address2 = school.Address2,
            Headmaster = school.Headmaster,
            HeadmasterCell = school.HeadmasterCell,
            IsActive = school.IsActive,
            Language = school.Language,
            PrintInvoice = school.PrintInvoice,
            ImportFlag = school.ImportFlag,
            Afterschool1Name = school.Afterschool1Name,
            Afterschool1Contact = school.Afterschool1Contact,
            Afterschool2Name = school.Afterschool2Name,
            Afterschool2Contact = school.Afterschool2Contact,
            SchedulingNotes = school.SchedulingNotes,
            MoneyMessage = school.MoneyMessage,
            SafeNotes = school.SafeNotes,
            WebPage = school.WebPage,
            KcowWebPageLink = school.KcowWebPageLink,
            BillingSettings = school.BillingSettings == null ? null : new BillingSettingsDto
            {
                DefaultSessionRate = school.BillingSettings.DefaultSessionRate,
                BillingCycle = school.BillingSettings.BillingCycle,
                BillingNotes = school.BillingSettings.BillingNotes
            },
            CreatedAt = school.CreatedAt,
            UpdatedAt = school.UpdatedAt
        };
    }
}
