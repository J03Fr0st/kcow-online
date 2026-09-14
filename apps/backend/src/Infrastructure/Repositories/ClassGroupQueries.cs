using Dapper;
using Kcow.Application.ClassGroups;
using Kcow.Infrastructure.Database;

namespace Kcow.Infrastructure.Repositories;

public sealed class ClassGroupQueries(IDbConnectionFactory connections) : IClassGroupQueries
{
    public async Task<List<ClassGroupDto>> ListAsync(int? schoolId, int? truckId, CancellationToken ct)
    {
        using var db = await connections.CreateAsync(ct);
        var command = new CommandDefinition("""
            SELECT cg.*, s.id AS Id, s.name AS Name, s.short_name AS ShortName,
                t.id AS Id, t.name AS Name, t.registration_number AS RegistrationNumber
            FROM class_groups cg LEFT JOIN schools s ON s.id=cg.school_id LEFT JOIN trucks t ON t.id=cg.truck_id
            WHERE cg.is_active=1 AND (@SchoolId IS NULL OR cg.school_id=@SchoolId)
                AND (@TruckId IS NULL OR cg.truck_id=@TruckId)
            ORDER BY cg.school_id,cg.day_of_week,cg.start_time,cg.id
            """, new { SchoolId = schoolId, TruckId = truckId }, cancellationToken: ct);
        var rows = await db.QueryAsync<ClassGroupDto, SchoolDto, TruckDto, ClassGroupDto>(command,
            (group, school, truck) => { group.School = school; group.Truck = truck; return group; }, splitOn: "Id,Id");
        return rows.ToList();
    }
}
