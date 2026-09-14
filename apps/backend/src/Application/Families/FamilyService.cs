using Kcow.Application.Families;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Kcow.Application.Families;

public class FamilyService : IFamilyService
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IFamilyRelationships _relationships;
    private readonly ILogger<FamilyService> _logger;

    public FamilyService(
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository,
        IFamilyRelationships relationships,
        ILogger<FamilyService> logger)
    {
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
        _relationships = relationships;
        _logger = logger;
    }

    public async Task<List<FamilyDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var families = (await _familyRepository.GetActiveAsync(cancellationToken))
            .OrderBy(f => f.FamilyName)
            .ToList();

        var related = await _relationships.GetStudentsAsync(families.Select(f => f.Id).ToArray(), cancellationToken);
        var result = new List<FamilyDto>();
        foreach (var f in families)
        {
            var students = related.GetValueOrDefault(f.Id) ?? [];
            result.Add(MapToDto(f, students));
        }

        return result;
    }

    public async Task<FamilyDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var family = await _familyRepository.GetByIdAsync(id, cancellationToken);
        if (family == null || !family.IsActive)
        {
            return null;
        }

        var students = (await _relationships.GetStudentsAsync([id], cancellationToken)).GetValueOrDefault(id) ?? [];
        return MapToDto(family, students);
    }

    public async Task<FamilyDto> CreateAsync(CreateFamilyRequest request, CancellationToken cancellationToken = default)
    {
        var family = new Family
        {
            FamilyName = request.FamilyName,
            PrimaryContactName = request.PrimaryContactName,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _familyRepository.CreateAsync(family, cancellationToken);
        family.Id = id;

        _logger.LogInformation("Created family with ID {FamilyId}", family.Id);
        return MapToDto(family, new List<StudentFamilyDto>());
    }

    public async Task<FamilyDto?> UpdateAsync(int id, UpdateFamilyRequest request, CancellationToken cancellationToken = default)
    {
        var family = await _familyRepository.GetByIdAsync(id, cancellationToken);
        if (family == null)
        {
            return null;
        }

        family.FamilyName = request.FamilyName;
        family.PrimaryContactName = request.PrimaryContactName;
        family.Phone = request.Phone;
        family.Email = request.Email;
        family.Address = request.Address;
        family.Notes = request.Notes;
        family.IsActive = request.IsActive;
        family.UpdatedAt = DateTime.UtcNow;

        await _familyRepository.UpdateAsync(family, cancellationToken);

        _logger.LogInformation("Updated family with ID {FamilyId}", id);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> ArchiveAsync(int id, CancellationToken cancellationToken = default)
    {
        var family = await _familyRepository.GetByIdAsync(id, cancellationToken);
        if (family == null || !family.IsActive)
        {
            return false;
        }

        family.IsActive = false;
        family.UpdatedAt = DateTime.UtcNow;

        await _familyRepository.UpdateAsync(family, cancellationToken);

        _logger.LogInformation("Archived family with ID {FamilyId}", id);
        return true;
    }

    public Task<List<FamilyDto>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default) =>
        _relationships.GetFamiliesAsync(studentId, cancellationToken);

    public async Task<bool> LinkToStudentAsync(int studentId, LinkFamilyRequest request, CancellationToken cancellationToken = default)
    {
        if (!await _studentRepository.ExistsAsync(studentId, cancellationToken) ||
            !await _familyRepository.ExistsAsync(request.FamilyId, cancellationToken)) return false;
        await _relationships.LinkAsync(studentId, request, cancellationToken);
        return true;
    }

    public Task<bool> UnlinkFromStudentAsync(int studentId, int familyId, CancellationToken cancellationToken = default) =>
        _relationships.UnlinkAsync(studentId, familyId, cancellationToken);

    private static FamilyDto MapToDto(Family f, List<StudentFamilyDto> students)
    {
        return new FamilyDto
        {
            Id = f.Id,
            FamilyName = f.FamilyName,
            PrimaryContactName = f.PrimaryContactName,
            Phone = f.Phone,
            Email = f.Email,
            Address = f.Address,
            Notes = f.Notes,
            IsActive = f.IsActive,
            CreatedAt = f.CreatedAt,
            UpdatedAt = f.UpdatedAt,
            Students = students
        };
    }
}
