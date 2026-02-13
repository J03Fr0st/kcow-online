using Kcow.Application.Interfaces;
using Kcow.Application.Schools;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Schools;

/// <summary>
/// Implementation of school management service using Dapper repositories.
/// </summary>
public class SchoolService : ISchoolService
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger<SchoolService> _logger;

    public SchoolService(ISchoolRepository schoolRepository, ILogger<SchoolService> logger)
    {
        _schoolRepository = schoolRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active schools.
    /// </summary>
    public async Task<List<SchoolDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var schools = (await _schoolRepository.GetActiveAsync(cancellationToken))
                .OrderBy(s => s.Name)
                .ToList();

            _logger.LogInformation("Retrieved {Count} active schools", schools.Count);

            return schools.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all schools");
            throw;
        }
    }

    /// <summary>
    /// Gets a school by ID.
    /// </summary>
    public async Task<SchoolDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var school = await _schoolRepository.GetByIdAsync(id, cancellationToken);

        if (school == null || !school.IsActive)
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
    public async Task<SchoolDto> CreateAsync(CreateSchoolRequest request, CancellationToken cancellationToken = default)
    {
        var school = new School
        {
            Name = request.Name,
            ShortName = request.ShortName,
            SchoolDescription = request.SchoolDescription,
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
            Omsendbriewe = request.Omsendbriewe,
            KcowWebPageLink = request.KcowWebPageLink,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _schoolRepository.CreateAsync(school, cancellationToken);
        school.Id = id; // Set the ID returned by repository

        _logger.LogInformation("Created school with ID {SchoolId} and name '{SchoolName}'",
            school.Id, school.Name);

        return MapToDto(school);
    }

    /// <summary>
    /// Updates an existing school.
    /// </summary>
    public async Task<SchoolDto?> UpdateAsync(int id, UpdateSchoolRequest request, CancellationToken cancellationToken = default)
    {
        var school = await _schoolRepository.GetByIdAsync(id, cancellationToken);

        if (school == null || !school.IsActive)
        {
            _logger.LogWarning("Cannot update: School with ID {SchoolId} not found", id);
            return null;
        }

        school.Name = request.Name;
        school.ShortName = request.ShortName;
        school.SchoolDescription = request.SchoolDescription;
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
        school.Omsendbriewe = request.Omsendbriewe;
        school.KcowWebPageLink = request.KcowWebPageLink;
        school.UpdatedAt = DateTime.UtcNow;

        await _schoolRepository.UpdateAsync(school, cancellationToken);

        _logger.LogInformation("Updated school with ID {SchoolId}", id);

        return MapToDto(school);
    }

    /// <summary>
    /// Archives (soft-deletes) a school.
    /// </summary>
    public async Task<bool> ArchiveAsync(int id, CancellationToken cancellationToken = default)
    {
        var school = await _schoolRepository.GetByIdAsync(id, cancellationToken);

        if (school == null || !school.IsActive)
        {
            _logger.LogWarning("Cannot archive: School with ID {SchoolId} not found", id);
            return false;
        }

        school.IsActive = false;
        school.UpdatedAt = DateTime.UtcNow;

        await _schoolRepository.UpdateAsync(school, cancellationToken);

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
            SchoolDescription = school.SchoolDescription,
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
            Omsendbriewe = school.Omsendbriewe,
            KcowWebPageLink = school.KcowWebPageLink,
            LegacyId = school.LegacyId,
            CreatedAt = school.CreatedAt,
            UpdatedAt = school.UpdatedAt
        };
    }
}
