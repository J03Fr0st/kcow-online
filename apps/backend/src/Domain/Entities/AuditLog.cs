namespace Kcow.Domain.Entities;

/// <summary>
/// AuditLog entity for tracking changes to domain entities.
/// Provides traceability for all data modifications (FR14).
/// </summary>
public class AuditLog
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
}
