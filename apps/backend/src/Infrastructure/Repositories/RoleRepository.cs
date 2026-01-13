using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using System.Data;

namespace Kcow.Infrastructure.Repositories;

/// <summary>
/// Dapper implementation of IRoleRepository.
/// </summary>
public class RoleRepository : IRoleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT Id, Name, CreatedAt
            FROM Roles";
        return await connection.QueryAsync<Role>(sql);
    }

    public async Task<Role?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT Id, Name, CreatedAt
            FROM Roles
            WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { Id = id });
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT Id, Name, CreatedAt
            FROM Roles
            WHERE Name = @Name";
        return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { Name = name });
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "SELECT COUNT(1) FROM Roles WHERE Name = @Name";
        var count = await connection.QuerySingleAsync<int>(sql, new { Name = name });
        return count > 0;
    }

    public async Task<int> CreateAsync(Role role, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            INSERT INTO Roles (Name, CreatedAt)
            VALUES (@Name, @CreatedAt)
            RETURNING Id";
        return await connection.QuerySingleAsync<int>(sql, role);
    }

    public async Task<bool> UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            UPDATE Roles
            SET Name = @Name
            WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, role);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "DELETE FROM Roles WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "SELECT COUNT(1) FROM Roles WHERE Id = @Id";
        var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}
