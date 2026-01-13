using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Domain.Enums;
using Kcow.Infrastructure.Database;
using System.Data;

namespace Kcow.Infrastructure.Repositories;

/// <summary>
/// Dapper implementation of IFamilyRepository.
/// </summary>
public class FamilyRepository : IFamilyRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FamilyRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Family>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, family_name, primary_contact_name, phone, email, address, notes, is_active, created_at, updated_at
            FROM families";
        return await connection.QueryAsync<Family>(sql);
    }

    public async Task<IEnumerable<Family>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, family_name, primary_contact_name, phone, email, address, notes, is_active, created_at, updated_at
            FROM families
            WHERE is_active = 1";
        return await connection.QueryAsync<Family>(sql);
    }

    public async Task<Family?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, family_name, primary_contact_name, phone, email, address, notes, is_active, created_at, updated_at
            FROM families
            WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Family>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Family family, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            INSERT INTO families (family_name, primary_contact_name, phone, email, address, notes, is_active, created_at, updated_at)
            VALUES (@FamilyName, @PrimaryContactName, @Phone, @Email, @Address, @Notes, @IsActive, @CreatedAt, @UpdatedAt)
            RETURNING id";
        return await connection.QuerySingleAsync<int>(sql, family);
    }

    public async Task<bool> UpdateAsync(Family family, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            UPDATE families
            SET family_name = @FamilyName,
                primary_contact_name = @PrimaryContactName,
                phone = @Phone,
                email = @Email,
                address = @Address,
                notes = @Notes,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, family);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "DELETE FROM families WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "SELECT COUNT(1) FROM families WHERE id = @Id";
        var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task AddStudentToFamilyAsync(int studentId, int familyId, RelationshipType relationshipType, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            INSERT INTO student_families (student_id, family_id, relationship_type)
            VALUES (@StudentId, @FamilyId, @RelationshipType)";
        
        await connection.ExecuteAsync(sql, new { 
            StudentId = studentId, 
            FamilyId = familyId, 
            RelationshipType = relationshipType.ToString() 
        });
    }
}
