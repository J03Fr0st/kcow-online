namespace Kcow.Application.Import;

/// <summary>
/// Determines how to handle records that already exist (by legacy_id match).
/// </summary>
public enum ConflictResolutionMode
{
    /// <summary>Fail and stop if a matching record exists (default).</summary>
    FailOnConflict,

    /// <summary>Skip records that already exist.</summary>
    SkipExisting,

    /// <summary>Update existing records with new values.</summary>
    Update
}

/// <summary>
/// Result of executing the import pipeline.
/// </summary>
public sealed class ImportExecutionResult
{
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string InputPath { get; set; } = string.Empty;
    public ConflictResolutionMode ConflictMode { get; set; }
    public EntityImportResult Schools { get; set; } = new();
    public EntityImportResult ClassGroups { get; set; } = new();
    public EntityImportResult Activities { get; set; } = new();
    public EntityImportResult Students { get; set; } = new();
    public List<ImportException> Exceptions { get; set; } = new();
    public bool HasExceptions => Exceptions.Count > 0;

    public int TotalImported => Schools.Imported + ClassGroups.Imported +
                                Activities.Imported + Students.Imported;
    public int TotalUpdated => Schools.Updated + ClassGroups.Updated +
                               Activities.Updated + Students.Updated;
    public int TotalFailed => Schools.Failed + ClassGroups.Failed +
                              Activities.Failed + Students.Failed;
    public int TotalSkipped => Schools.Skipped + ClassGroups.Skipped +
                               Activities.Skipped + Students.Skipped;
    public int TotalProcessed => TotalImported + TotalUpdated + TotalFailed + TotalSkipped;

    public double SuccessRate => TotalProcessed > 0
        ? Math.Round((double)(TotalImported + TotalUpdated) / TotalProcessed * 100, 1)
        : 0;
}

/// <summary>
/// Import counts for a single entity type.
/// </summary>
public sealed class EntityImportResult
{
    public int Imported { get; set; }
    public int Updated { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
}

/// <summary>
/// Exception record for a failed import record.
/// </summary>
public sealed class ImportException
{
    public string EntityType { get; set; } = string.Empty;
    public string LegacyId { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? OriginalValue { get; set; }

    public ImportException() { }

    public ImportException(string entityType, string legacyId, string field, string reason, string? originalValue = null)
    {
        EntityType = entityType;
        LegacyId = legacyId;
        Field = field;
        Reason = reason;
        OriginalValue = originalValue;
    }
}
