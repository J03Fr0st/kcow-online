using Dapper;
using Kcow.Application.Families;
using Kcow.Infrastructure.Database;

namespace Kcow.Infrastructure.Repositories;

public sealed class FamilyRelationships(IDbConnectionFactory connections) : IFamilyRelationships
{
    public async Task<Dictionary<int, List<StudentFamilyDto>>> GetStudentsAsync(int[] familyIds, CancellationToken ct)
    {
        if (familyIds.Length == 0) return [];
        using var db = await connections.CreateAsync(ct);
        // json_each uses one parameter even for large family lists (no SQLite variable limit).
        var rows = await db.QueryAsync<StudentRow>(new CommandDefinition("""
            SELECT sf.family_id AS FamilyId, s.id AS StudentId, s.first_name AS FirstName,
                   s.last_name AS LastName, s.reference AS Reference, sf.relationship_type AS RelationshipType
            FROM student_families sf JOIN students s ON s.id=sf.student_id
            WHERE sf.family_id IN (SELECT value FROM json_each(@Ids)) AND s.is_active=1
            ORDER BY s.id
            """, new { Ids = System.Text.Json.JsonSerializer.Serialize(familyIds) }, cancellationToken: ct));
        return rows.GroupBy(r => r.FamilyId).ToDictionary(g => g.Key, g => g.Select(r => new StudentFamilyDto
        { StudentId = r.StudentId, FirstName = r.FirstName, LastName = r.LastName, Reference = r.Reference, RelationshipType = r.RelationshipType }).ToList());
    }

    public async Task<List<FamilyDto>> GetFamiliesAsync(int studentId, CancellationToken ct)
    {
        using var db = await connections.CreateAsync(ct);
        return (await db.QueryAsync<FamilyDto>(new CommandDefinition("""
            SELECT f.* FROM families f JOIN student_families sf ON f.id=sf.family_id
            WHERE sf.student_id=@StudentId ORDER BY f.family_name, f.id
            """, new { StudentId = studentId }, cancellationToken: ct))).ToList();
    }

    public async Task LinkAsync(int studentId, LinkFamilyRequest request, CancellationToken ct)
    {
        using var db = await connections.CreateAsync(ct);
        await db.ExecuteAsync(new CommandDefinition("""
            INSERT INTO student_families(student_id,family_id,relationship_type)
            VALUES(@StudentId,@FamilyId,@RelationshipType) ON CONFLICT(student_id,family_id) DO NOTHING
            """, new { StudentId = studentId, request.FamilyId, RelationshipType = request.RelationshipType.ToString() }, cancellationToken: ct));
    }

    public async Task<bool> UnlinkAsync(int studentId, int familyId, CancellationToken ct)
    {
        using var db = await connections.CreateAsync(ct);
        return await db.ExecuteAsync(new CommandDefinition("DELETE FROM student_families WHERE student_id=@StudentId AND family_id=@FamilyId",
            new { StudentId = studentId, FamilyId = familyId }, cancellationToken: ct)) > 0;
    }

    private sealed class StudentRow : StudentFamilyDto { public int FamilyId { get; set; } }
}
