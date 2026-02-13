using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using System.Data;

namespace Kcow.Infrastructure.Repositories;

/// <summary>
/// Dapper implementation of IUserRepository.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            SELECT Id, Email, PasswordHash, Name, RoleId, CreatedAt, UpdatedAt
            FROM Users";
        return await connection.QueryAsync<User>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            SELECT Id, Email, PasswordHash, Name, RoleId, CreatedAt, UpdatedAt
            FROM Users
            WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<User>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            SELECT Id, Email, PasswordHash, Name, RoleId, CreatedAt, UpdatedAt
            FROM Users
            WHERE Email = @Email";
        return await connection.QueryFirstOrDefaultAsync<User>(new CommandDefinition(sql, new { Email = email }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            INSERT INTO Users (Email, PasswordHash, Name, RoleId, CreatedAt, UpdatedAt)
            VALUES (@Email, @PasswordHash, @Name, @RoleId, @CreatedAt, @UpdatedAt)
            RETURNING Id";
        return await connection.QuerySingleAsync<int>(new CommandDefinition(sql, user, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            UPDATE Users
            SET Email = @Email,
                PasswordHash = @PasswordHash,
                Name = @Name,
                RoleId = @RoleId,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, user, cancellationToken: cancellationToken));
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = "DELETE FROM Users WHERE Id = @Id";
        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = "SELECT COUNT(1) FROM Users WHERE Id = @Id";
        var count = await connection.QuerySingleAsync<int>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return count > 0;
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
        var count = await connection.QuerySingleAsync<int>(new CommandDefinition(sql, new { Email = email }, cancellationToken: cancellationToken));
        return count > 0;
    }
}
