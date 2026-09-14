namespace Kcow.Application.ClassGroups;

public interface IClassGroupQueries
{
    Task<List<ClassGroupDto>> ListAsync(int? schoolId, int? truckId, CancellationToken ct);
}
