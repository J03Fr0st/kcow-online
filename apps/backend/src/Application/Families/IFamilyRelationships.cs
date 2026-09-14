namespace Kcow.Application.Families;

public interface IFamilyRelationships
{
    Task<Dictionary<int, List<StudentFamilyDto>>> GetStudentsAsync(int[] familyIds, CancellationToken ct);
    Task<List<FamilyDto>> GetFamiliesAsync(int studentId, CancellationToken ct);
    Task LinkAsync(int studentId, LinkFamilyRequest request, CancellationToken ct);
    Task<bool> UnlinkAsync(int studentId, int familyId, CancellationToken ct);
}
