using Dapper;
using Kcow.Application.Families;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Kcow.Infrastructure.Families;

public class FamilyService : IFamilyService
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<FamilyService> _logger;

    public FamilyService(
        IFamilyRepository familyRepository,
        IStudentRepository studentRepository,
        IDbConnectionFactory connectionFactory,
        ILogger<FamilyService> logger)
    {
        _familyRepository = familyRepository;
        _studentRepository = studentRepository;
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<List<FamilyDto>> GetAllAsync()
    {
        var families = (await _familyRepository.GetActiveAsync())
            .OrderBy(f => f.FamilyName)
            .ToList();

        var result = new List<FamilyDto>();
        foreach (var f in families)
        {
            var students = await GetStudentsForFamilyAsync(f.Id);
            result.Add(MapToDto(f, students));
        }

        return result;
    }

    public async Task<FamilyDto?> GetByIdAsync(int id)
    {
        var family = await _familyRepository.GetByIdAsync(id);
        if (family == null || !family.IsActive)
        {
            return null;
        }

        var students = await GetStudentsForFamilyAsync(id);
        return MapToDto(family, students);
    }

    public async Task<FamilyDto> CreateAsync(CreateFamilyRequest request)
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

        var id = await _familyRepository.CreateAsync(family);
        family.Id = id;

        _logger.LogInformation("Created family with ID {FamilyId}", family.Id);
        return MapToDto(family, new List<StudentFamilyDto>());
    }

    public async Task<FamilyDto?> UpdateAsync(int id, UpdateFamilyRequest request)
    {
        var family = await _familyRepository.GetByIdAsync(id);
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

        await _familyRepository.UpdateAsync(family);

        _logger.LogInformation("Updated family with ID {FamilyId}", id);
        return await GetByIdAsync(id);
    }

    public async Task<bool> ArchiveAsync(int id)
    {
        var family = await _familyRepository.GetByIdAsync(id);
        if (family == null || !family.IsActive)
        {
            return false;
        }

        family.IsActive = false;
        family.UpdatedAt = DateTime.UtcNow;

        await _familyRepository.UpdateAsync(family);

        _logger.LogInformation("Archived family with ID {FamilyId}", id);
        return true;
    }

    public async Task<List<FamilyDto>> GetByStudentIdAsync(int studentId)
    {
        const string sql = @"
            SELECT f.id, f.family_name, f.primary_contact_name, f.phone, f.email,
                   f.address, f.notes, f.is_active, f.created_at, f.updated_at,
                   sf.relationship_type
            FROM families f
            INNER JOIN student_families sf ON f.id = sf.family_id
            WHERE sf.student_id = @StudentId";

        using var connection = _connectionFactory.Create();
        var records = await connection.QueryAsync(sql, new { StudentId = studentId });

        var result = new List<FamilyDto>();
        foreach (var record in records)
        {
            result.Add(new FamilyDto
            {
                Id = record.id,
                FamilyName = record.family_name,
                PrimaryContactName = record.primary_contact_name,
                Phone = record.phone,
                Email = record.email,
                Address = record.address,
                Notes = record.notes,
                IsActive = record.is_active,
                CreatedAt = record.created_at,
                UpdatedAt = record.updated_at,
                Students = new List<StudentFamilyDto>() // Empty list to avoid N+1 queries
            });
        }

        return result;
    }

    public async Task<bool> LinkToStudentAsync(int studentId, LinkFamilyRequest request)
    {
        var studentExists = await _studentRepository.ExistsAsync(studentId);
        if (!studentExists) return false;

        var familyExists = await _familyRepository.ExistsAsync(request.FamilyId);
        if (!familyExists) return false;

        // Check if already linked
        const string checkSql = @"
            SELECT COUNT(1) FROM student_families
            WHERE student_id = @StudentId AND family_id = @FamilyId";

        using var connection = _connectionFactory.Create();
        var alreadyLinked = await connection.QuerySingleAsync<int>(checkSql, new { StudentId = studentId, FamilyId = request.FamilyId });
        if (alreadyLinked > 0) return true;

        // Create the link
        const string insertSql = @"
            INSERT INTO student_families (student_id, family_id, relationship_type)
            VALUES (@StudentId, @FamilyId, @RelationshipType)";

        await connection.ExecuteAsync(insertSql, new
        {
            StudentId = studentId,
            FamilyId = request.FamilyId,
            RelationshipType = request.RelationshipType.ToString()
        });

        _logger.LogInformation("Linked student {StudentId} to family {FamilyId} as {RelationshipType}",
            studentId, request.FamilyId, request.RelationshipType);

        return true;
    }

    public async Task<bool> UnlinkFromStudentAsync(int studentId, int familyId)
    {
        const string sql = @"
            DELETE FROM student_families
            WHERE student_id = @StudentId AND family_id = @FamilyId";

        using var connection = _connectionFactory.Create();
        var rowsAffected = await connection.ExecuteAsync(sql, new { StudentId = studentId, FamilyId = familyId });

        if (rowsAffected > 0)
        {
            _logger.LogInformation("Unlinked student {StudentId} from family {FamilyId}", studentId, familyId);
        }

        return rowsAffected > 0;
    }

    private async Task<List<StudentFamilyDto>> GetStudentsForFamilyAsync(int familyId)
    {
        const string sql = @"
            SELECT s.id as StudentId, s.first_name as FirstName, s.last_name as LastName,
                   s.reference as Reference, sf.relationship_type as RelationshipType
            FROM students s
            INNER JOIN student_families sf ON s.id = sf.student_id
            WHERE sf.family_id = @FamilyId AND s.is_active = 1";

        using var connection = _connectionFactory.Create();
        return (await connection.QueryAsync<StudentFamilyDto>(sql, new { FamilyId = familyId })).ToList();
    }

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
