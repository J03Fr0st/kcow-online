using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using System.Data;

namespace Kcow.Infrastructure.Repositories;

/// <summary>
/// Dapper implementation of IClassGroupRepository.
/// </summary>
public class ClassGroupRepository : IClassGroupRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    private const string SelectColumns = @"
        id, name, day_truck, description, school_id, truck_id, day_of_week, start_time, end_time,
        sequence, evaluate, notes, import_flag, group_message, send_certificates, money_message,
        ixl, is_active, created_at, updated_at";

    public ClassGroupRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ClassGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        var sql = $"SELECT {SelectColumns} FROM class_groups";
        return await connection.QueryAsync<ClassGroup>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<ClassGroup>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        var sql = $"SELECT {SelectColumns} FROM class_groups WHERE is_active = 1";
        return await connection.QueryAsync<ClassGroup>(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    public async Task<ClassGroup?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        var sql = $"SELECT {SelectColumns} FROM class_groups WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<ClassGroup>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<ClassGroup>> GetBySchoolIdAsync(int schoolId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        var sql = $"SELECT {SelectColumns} FROM class_groups WHERE school_id = @SchoolId";
        return await connection.QueryAsync<ClassGroup>(new CommandDefinition(sql, new { SchoolId = schoolId }, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<ClassGroup>> GetByDayAsync(DayOfWeek dayOfWeek, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        var sql = $"SELECT {SelectColumns} FROM class_groups WHERE day_of_week = @DayOfWeek";
        return await connection.QueryAsync<ClassGroup>(new CommandDefinition(sql, new { DayOfWeek = (int)dayOfWeek }, cancellationToken: cancellationToken));
    }

    public async Task<int> CreateAsync(ClassGroup classGroup, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            INSERT INTO class_groups (name, day_truck, description, school_id, truck_id, day_of_week, start_time, end_time,
                   sequence, evaluate, notes, import_flag, group_message, send_certificates, money_message,
                   ixl, is_active, created_at, updated_at)
            VALUES (@Name, @DayTruck, @Description, @SchoolId, @TruckId, @DayOfWeek, @StartTime, @EndTime,
                   @Sequence, @Evaluate, @Notes, @ImportFlag, @GroupMessage, @SendCertificates, @MoneyMessage,
                   @Ixl, @IsActive, @CreatedAt, @UpdatedAt)
            RETURNING id";
        return await connection.QuerySingleAsync<int>(new CommandDefinition(sql, classGroup, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(ClassGroup classGroup, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            UPDATE class_groups
            SET name = @Name,
                day_truck = @DayTruck,
                description = @Description,
                school_id = @SchoolId,
                truck_id = @TruckId,
                day_of_week = @DayOfWeek,
                start_time = @StartTime,
                end_time = @EndTime,
                sequence = @Sequence,
                evaluate = @Evaluate,
                notes = @Notes,
                import_flag = @ImportFlag,
                group_message = @GroupMessage,
                send_certificates = @SendCertificates,
                money_message = @MoneyMessage,
                ixl = @Ixl,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, classGroup, cancellationToken: cancellationToken));
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = "DELETE FROM class_groups WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = "SELECT COUNT(1) FROM class_groups WHERE id = @Id";
        var count = await connection.QuerySingleAsync<int>(new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));
        return count > 0;
    }
}
