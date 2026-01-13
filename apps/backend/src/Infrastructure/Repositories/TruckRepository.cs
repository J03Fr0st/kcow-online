using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using System.Data;

namespace Kcow.Infrastructure.Repositories;

/// <summary>
/// Dapper implementation of ITruckRepository.
/// </summary>
public class TruckRepository : ITruckRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TruckRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Truck>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, name, registration_number, status, notes, is_active, created_at, updated_at
            FROM trucks";
        return await connection.QueryAsync<Truck>(sql);
    }

    public async Task<IEnumerable<Truck>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, name, registration_number, status, notes, is_active, created_at, updated_at
            FROM trucks
            WHERE is_active = 1";
        return await connection.QueryAsync<Truck>(sql);
    }

    public async Task<Truck?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, name, registration_number, status, notes, is_active, created_at, updated_at
            FROM trucks
            WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Truck>(sql, new { Id = id });
    }

    public async Task<Truck?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, name, registration_number, status, notes, is_active, created_at, updated_at
            FROM trucks
            WHERE registration_number = @RegistrationNumber";
        return await connection.QueryFirstOrDefaultAsync<Truck>(sql, new { RegistrationNumber = registrationNumber });
    }

    public async Task<int> CreateAsync(Truck truck, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            INSERT INTO trucks (name, registration_number, status, notes, is_active, created_at, updated_at)
            VALUES (@Name, @RegistrationNumber, @Status, @Notes, @IsActive, @CreatedAt, @UpdatedAt)
            RETURNING id";
        return await connection.QuerySingleAsync<int>(sql, truck);
    }

    public async Task<bool> UpdateAsync(Truck truck, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            UPDATE trucks
            SET name = @Name,
                registration_number = @RegistrationNumber,
                status = @Status,
                notes = @Notes,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, truck);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "DELETE FROM trucks WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "SELECT COUNT(1) FROM trucks WHERE id = @Id";
        var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task<bool> ExistsByRegistrationNumberAsync(string registrationNumber, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "SELECT COUNT(1) FROM trucks WHERE registration_number = @RegistrationNumber";
        var count = await connection.QuerySingleAsync<int>(sql, new { RegistrationNumber = registrationNumber });
        return count > 0;
    }
}
