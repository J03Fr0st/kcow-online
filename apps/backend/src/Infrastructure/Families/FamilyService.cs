using Kcow.Application.Families;
using Kcow.Domain.Entities;
using Kcow.Domain.Enums;
using Kcow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Families;

public class FamilyService : IFamilyService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FamilyService> _logger;

    public FamilyService(AppDbContext context, ILogger<FamilyService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<FamilyDto>> GetAllAsync()
    {
        // Use projection to avoid N+1 query problem
        var families = await _context.Families
            .Where(f => f.IsActive)
            .OrderBy(f => f.FamilyName)
            .Select(f => new FamilyDto
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
                Students = f.StudentFamilies
                    .Select(sf => new StudentFamilyDto
                    {
                        StudentId = sf.StudentId,
                        FirstName = sf.Student.FirstName,
                        LastName = sf.Student.LastName,
                        Reference = sf.Student.Reference,
                        RelationshipType = sf.RelationshipType.ToString()
                    })
                    .ToList()
            })
            .AsNoTracking()
            .ToListAsync();

        return families;
    }

    public async Task<FamilyDto?> GetByIdAsync(int id)
    {
        // Use projection to avoid N+1 query problem and filter by IsActive
        var family = await _context.Families
            .Where(f => f.Id == id && f.IsActive)
            .Select(f => new FamilyDto
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
                Students = f.StudentFamilies
                    .Select(sf => new StudentFamilyDto
                    {
                        StudentId = sf.StudentId,
                        FirstName = sf.Student.FirstName,
                        LastName = sf.Student.LastName,
                        Reference = sf.Student.Reference,
                        RelationshipType = sf.RelationshipType.ToString()
                    })
                    .ToList()
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return family;
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

        _context.Families.Add(family);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created family with ID {FamilyId}", family.Id);

        return MapToDto(family);
    }

    public async Task<FamilyDto?> UpdateAsync(int id, UpdateFamilyRequest request)
    {
        var family = await _context.Families.FirstOrDefaultAsync(f => f.Id == id);

        if (family == null) return null;

        family.FamilyName = request.FamilyName;
        family.PrimaryContactName = request.PrimaryContactName;
        family.Phone = request.Phone;
        family.Email = request.Email;
        family.Address = request.Address;
        family.Notes = request.Notes;
        family.IsActive = request.IsActive;
        family.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated family with ID {FamilyId}", id);

        return await GetByIdAsync(id);
    }

    public async Task<bool> ArchiveAsync(int id)
    {
        var family = await _context.Families.FirstOrDefaultAsync(f => f.Id == id && f.IsActive);

        if (family == null) return false;

        family.IsActive = false;
        family.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Archived family with ID {FamilyId}", id);
        return true;
    }

    public async Task<List<FamilyDto>> GetByStudentIdAsync(int studentId)
    {
        // Use projection to avoid N+1 query problem
        var families = await _context.StudentFamilies
            .Where(sf => sf.StudentId == studentId)
            .Select(sf => new
            {
                sf.Family.Id,
                sf.Family.FamilyName,
                sf.Family.PrimaryContactName,
                sf.Family.Phone,
                sf.Family.Email,
                sf.Family.Address,
                sf.Family.Notes,
                sf.Family.IsActive,
                sf.Family.CreatedAt,
                sf.Family.UpdatedAt,
                RelationshipType = sf.RelationshipType.ToString()
            })
            .ToListAsync();

        // Project to FamilyDto with empty Students list to avoid N+1
        return families.Select(f => new FamilyDto
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
            Students = new List<StudentFamilyDto>() // Empty list to avoid N+1 queries
        }).ToList();
    }

    public async Task<bool> LinkToStudentAsync(int studentId, LinkFamilyRequest request)
    {
        var studentExists = await _context.Students.AnyAsync(s => s.Id == studentId);
        if (!studentExists) return false;

        var familyExists = await _context.Families.AnyAsync(f => f.Id == request.FamilyId);
        if (!familyExists) return false;

        var alreadyLinked = await _context.StudentFamilies.AnyAsync(sf => sf.StudentId == studentId && sf.FamilyId == request.FamilyId);
        if (alreadyLinked) return true;

        var studentFamily = new StudentFamily
        {
            StudentId = studentId,
            FamilyId = request.FamilyId,
            RelationshipType = request.RelationshipType
        };

        _context.StudentFamilies.Add(studentFamily);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Linked student {StudentId} to family {FamilyId} as {RelationshipType}", 
            studentId, request.FamilyId, request.RelationshipType);
            
        return true;
    }

    public async Task<bool> UnlinkFromStudentAsync(int studentId, int familyId)
    {
        var studentFamily = await _context.StudentFamilies
            .FirstOrDefaultAsync(sf => sf.StudentId == studentId && sf.FamilyId == familyId);

        if (studentFamily == null) return false;

        _context.StudentFamilies.Remove(studentFamily);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Unlinked student {StudentId} from family {FamilyId}", studentId, familyId);
        
        return true;
    }

    private static FamilyDto MapToDto(Family f)
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
            Students = f.StudentFamilies.Select(sf => new StudentFamilyDto
            {
                StudentId = sf.StudentId,
                FirstName = sf.Student?.FirstName,
                LastName = sf.Student?.LastName,
                Reference = sf.Student?.Reference,
                RelationshipType = sf.RelationshipType.ToString()
            }).ToList()
        };
    }
}
