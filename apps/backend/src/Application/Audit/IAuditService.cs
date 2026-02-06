namespace Kcow.Application.Audit;

/// <summary>
/// Service interface for audit logging operations.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Logs a change to an entity field.
    /// </summary>
    Task LogChangeAsync(
        string entityType,
        int entityId,
        string field,
        string? oldValue,
        string? newValue,
        string changedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs multiple changes to an entity in a single operation.
    /// </summary>
    Task LogChangesAsync(
        string entityType,
        int entityId,
        Dictionary<string, (string? oldVal, string? newVal)> changes,
        string changedBy,
        CancellationToken cancellationToken = default);
}
