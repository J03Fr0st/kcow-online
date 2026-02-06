namespace Kcow.Application.Interfaces;

/// <summary>
/// Repository interface for AuditLog entity operations.
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    Task<int> CreateAsync(Domain.Entities.AuditLog entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit log entries for a specific entity.
    /// </summary>
    Task<IEnumerable<Domain.Entities.AuditLog>> GetByEntityAsync(string entityType, int entityId, CancellationToken cancellationToken = default);
}
