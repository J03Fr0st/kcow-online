namespace Kcow.Application.Families;

public interface IFamilyService
{
    Task<List<FamilyDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<FamilyDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<FamilyDto> CreateAsync(CreateFamilyRequest request, CancellationToken cancellationToken = default);
    Task<FamilyDto?> UpdateAsync(int id, UpdateFamilyRequest request, CancellationToken cancellationToken = default);
    Task<bool> ArchiveAsync(int id, CancellationToken cancellationToken = default);

    Task<List<FamilyDto>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);
    Task<bool> LinkToStudentAsync(int studentId, LinkFamilyRequest request, CancellationToken cancellationToken = default);
    Task<bool> UnlinkFromStudentAsync(int studentId, int familyId, CancellationToken cancellationToken = default);
}
