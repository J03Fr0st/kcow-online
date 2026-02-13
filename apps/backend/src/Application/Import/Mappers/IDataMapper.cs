namespace Kcow.Application.Import.Mappers;

/// <summary>
/// Generic interface for mapping legacy data records to domain entities.
/// Provides a consistent contract for all legacy-to-domain transformations.
/// </summary>
/// <typeparam name="TSource">Legacy record type (e.g., LegacySchoolRecord)</typeparam>
/// <typeparam name="TTarget">Domain entity type (e.g., School)</typeparam>
public interface IDataMapper<TSource, TTarget>
{
    /// <summary>
    /// Maps a single legacy record to a domain entity.
    /// Returns a result containing the mapped entity, warnings, and errors.
    /// </summary>
    MappingResult<TTarget> Map(TSource source);

    /// <summary>
    /// Maps a collection of legacy records to domain entities.
    /// Each record is mapped independently; individual failures don't stop the batch.
    /// </summary>
    MappingResult<List<TTarget>> MapMany(IEnumerable<TSource> sources);
}
