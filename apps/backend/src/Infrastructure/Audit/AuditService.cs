using Kcow.Application.Interfaces;

namespace Kcow.Infrastructure.Audit;

/// <summary>
/// Service for creating and managing audit logs.
/// </summary>
public class AuditService : Application.Audit.IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;

    // Whitelist of valid entity types for audit logging
    private static readonly HashSet<string> ValidEntityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Attendance",
        "Student",
        "Family",
        "School",
        "ClassGroup",
        "Activity",
        "Billing",
        "Evaluation"
    };

    public AuditService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task LogChangeAsync(
        string entityType,
        int entityId,
        string field,
        string? oldValue,
        string? newValue,
        string changedBy,
        CancellationToken cancellationToken = default)
    {
        // Validate entityType against whitelist to prevent data integrity issues
        if (!ValidEntityTypes.Contains(entityType))
        {
            throw new ArgumentException(
                $"Invalid entity type '{entityType}'. Valid types are: {string.Join(", ", ValidEntityTypes)}",
                nameof(entityType));
        }

        var entry = new Domain.Entities.AuditLog
        {
            EntityType = entityType,
            EntityId = entityId,
            Field = field,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedBy = changedBy,
            ChangedAt = DateTime.UtcNow
        };

        await _auditLogRepository.CreateAsync(entry, cancellationToken);
    }

    public async Task LogChangesAsync(
        string entityType,
        int entityId,
        Dictionary<string, (string? oldVal, string? newVal)> changes,
        string changedBy,
        CancellationToken cancellationToken = default)
    {
        foreach (var change in changes)
        {
            await LogChangeAsync(
                entityType,
                entityId,
                change.Key,
                change.Value.oldVal,
                change.Value.newVal,
                changedBy,
                cancellationToken);
        }
    }
}
