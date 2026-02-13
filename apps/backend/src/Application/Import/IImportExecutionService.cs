using Kcow.Application.Import.Mappers;
using Kcow.Domain.Entities;

namespace Kcow.Application.Import;

/// <summary>
/// Service that executes the full import pipeline: parse, map, and persist legacy data.
/// </summary>
public interface IImportExecutionService
{
    /// <summary>
    /// Executes import for all entity types from the given input path.
    /// Records are inserted transactionally per entity type.
    /// Failed records are logged as exceptions and processing continues.
    /// </summary>
    Task<ImportExecutionResult> ExecuteAsync(string inputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes import with conflict resolution for re-run scenarios.
    /// When records with matching legacy_id exist, behavior depends on the conflict mode:
    /// - FailOnConflict: Throws on first conflict (default)
    /// - SkipExisting: Skips records that already exist
    /// - Update: Updates existing records with new values
    /// </summary>
    Task<ImportExecutionResult> ExecuteAsync(string inputPath, ConflictResolutionMode conflictMode, CancellationToken cancellationToken = default);
}
