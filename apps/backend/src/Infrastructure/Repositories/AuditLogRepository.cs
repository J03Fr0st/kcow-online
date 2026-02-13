using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;

namespace Kcow.Infrastructure.Repositories;

/// <summary>
/// Dapper implementation of IAuditLogRepository.
/// </summary>
public class AuditLogRepository : IAuditLogRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AuditLogRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(AuditLog entry, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            INSERT INTO audit_log (entity_type, entity_id, field, old_value, new_value, changed_by, changed_at)
            VALUES (@EntityType, @EntityId, @Field, @OldValue, @NewValue, @ChangedBy, @ChangedAt)
            RETURNING id";
        return await connection.QuerySingleAsync<int>(new CommandDefinition(sql, entry, cancellationToken: cancellationToken));
    }

    public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateAsync(cancellationToken);
        const string sql = @"
            SELECT id, entity_type, entity_id, field, old_value, new_value, changed_by, changed_at
            FROM audit_log
            WHERE entity_type = @EntityType AND entity_id = @EntityId
            ORDER BY changed_at DESC";
        return await connection.QueryAsync<AuditLog>(new CommandDefinition(sql, new { EntityType = entityType, EntityId = entityId }, cancellationToken: cancellationToken));
    }
}
