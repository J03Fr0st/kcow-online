namespace Kcow.Application.Families;

public interface IFamilyService
{
    Task<List<FamilyDto>> GetAllAsync();
    Task<FamilyDto?> GetByIdAsync(int id);
    Task<FamilyDto> CreateAsync(CreateFamilyRequest request);
    Task<FamilyDto?> UpdateAsync(int id, UpdateFamilyRequest request);
    Task<bool> ArchiveAsync(int id);
    
    Task<List<FamilyDto>> GetByStudentIdAsync(int studentId);
    Task<bool> LinkToStudentAsync(int studentId, LinkFamilyRequest request);
    Task<bool> UnlinkFromStudentAsync(int studentId, int familyId);
}
