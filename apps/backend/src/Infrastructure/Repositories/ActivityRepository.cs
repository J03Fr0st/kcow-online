using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using System.Data;

namespace Kcow.Infrastructure.Repositories;

/// <summary>
/// Dapper implementation of IActivityRepository.
/// </summary>
public class ActivityRepository : IActivityRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ActivityRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Activity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, code, name, description, folder, grade_level, icon, is_active, created_at, updated_at
            FROM activities";
        return await connection.QueryAsync<Activity>(sql);
    }

    public async Task<IEnumerable<Activity>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, code, name, description, folder, grade_level, icon, is_active, created_at, updated_at
            FROM activities
            WHERE is_active = 1";
        return await connection.QueryAsync<Activity>(sql);
    }

    public async Task<Activity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, code, name, description, folder, grade_level, icon, is_active, created_at, updated_at
            FROM activities
            WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Activity>(sql, new { Id = id });
    }

    public async Task<Activity?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, code, name, description, folder, grade_level, icon, is_active, created_at, updated_at
            FROM activities
            WHERE code = @Code";
        return await connection.QueryFirstOrDefaultAsync<Activity>(sql, new { Code = code });
    }

    public async Task<int> CreateAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            INSERT INTO activities (id, code, name, description, folder, grade_level, icon, is_active, created_at, updated_at)
            VALUES (@Id, @Code, @Name, @Description, @Folder, @GradeLevel, @Icon, @IsActive, @CreatedAt, @UpdatedAt)
            RETURNING id";
        return await connection.QuerySingleAsync<int>(sql, activity);
    }

    public async Task<bool> UpdateAsync(Activity activity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            UPDATE activities
            SET code = @Code,
                name = @Name,
                description = @Description,
                folder = @Folder,
                grade_level = @GradeLevel,
                icon = @Icon,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, activity);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "DELETE FROM activities WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "SELECT COUNT(1) FROM activities WHERE id = @Id";
        var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "SELECT COUNT(1) FROM activities WHERE code = @Code";
        var count = await connection.QuerySingleAsync<int>(sql, new { Code = code });
        return count > 0;
    }
}
