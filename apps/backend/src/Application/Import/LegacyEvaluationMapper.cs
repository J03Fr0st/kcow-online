namespace Kcow.Application.Import;

/// <summary>
/// Input record for evaluation import mapping.
/// </summary>
public sealed record LegacyEvaluationImportRecord(
    int StudentId,
    int ActivityId,
    string EvaluationDate,
    int? Score,
    decimal? SpeedMetric,
    decimal? AccuracyMetric,
    string? Notes,
    DateTime? OriginalCreatedAt,
    DateTime? OriginalModifiedAt);

/// <summary>
/// Result of mapping a legacy evaluation record to the Evaluation entity.
/// </summary>
public sealed record LegacyEvaluationMappingResult(Kcow.Domain.Entities.Evaluation? Evaluation, IReadOnlyList<string> Warnings);

/// <summary>
/// Maps legacy evaluation/assessment data to Evaluation entities.
/// Validates student and activity references and maps score metrics.
/// </summary>
public sealed class LegacyEvaluationMapper
{
    private readonly HashSet<int> _validStudentIds;
    private readonly HashSet<int> _validActivityIds;

    public LegacyEvaluationMapper(IEnumerable<int> validStudentIds, IEnumerable<int> validActivityIds)
    {
        _validStudentIds = new HashSet<int>(validStudentIds);
        _validActivityIds = new HashSet<int>(validActivityIds);
    }

    public LegacyEvaluationMappingResult Map(LegacyEvaluationImportRecord record)
    {
        var warnings = new List<string>();

        // Validate student reference
        if (!_validStudentIds.Contains(record.StudentId))
        {
            warnings.Add($"Student ID {record.StudentId} not found in database. Skipping evaluation record.");
            return new LegacyEvaluationMappingResult(null, warnings);
        }

        // Validate activity reference
        if (!_validActivityIds.Contains(record.ActivityId))
        {
            warnings.Add($"Activity ID {record.ActivityId} not found in database. Skipping evaluation record.");
            return new LegacyEvaluationMappingResult(null, warnings);
        }

        // Validate evaluation date
        var isoDate = LegacyAttendanceEvaluationXmlParser.ParseDateToIso(record.EvaluationDate);
        if (isoDate == null)
        {
            warnings.Add($"Invalid evaluation date '{record.EvaluationDate}' for Student {record.StudentId}. Skipping.");
            return new LegacyEvaluationMappingResult(null, warnings);
        }

        // Validate score range
        if (record.Score.HasValue && (record.Score.Value < 0 || record.Score.Value > 100))
        {
            warnings.Add($"Score {record.Score} out of range (0-100) for Student {record.StudentId}, Activity {record.ActivityId}. Clamping.");
        }
        var score = record.Score.HasValue ? Math.Clamp(record.Score.Value, 0, 100) : (int?)null;

        // Validate speed metric
        if (record.SpeedMetric.HasValue && record.SpeedMetric.Value < 0)
        {
            warnings.Add($"Negative speed metric {record.SpeedMetric} for Student {record.StudentId}. Setting to 0.");
        }
        var speedMetric = record.SpeedMetric.HasValue ? Math.Max(0, record.SpeedMetric.Value) : (decimal?)null;

        // Validate accuracy metric
        if (record.AccuracyMetric.HasValue && (record.AccuracyMetric.Value < 0 || record.AccuracyMetric.Value > 100))
        {
            warnings.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Accuracy metric {0} out of range (0-100) for Student {1}. Clamping.",
                record.AccuracyMetric, record.StudentId));
        }
        var accuracyMetric = record.AccuracyMetric.HasValue ? Math.Clamp(record.AccuracyMetric.Value, 0, 100) : (decimal?)null;

        // Preserve historical timestamps
        var createdAt = record.OriginalCreatedAt ?? DateTime.UtcNow;
        var modifiedAt = record.OriginalModifiedAt;

        var evaluation = new Kcow.Domain.Entities.Evaluation
        {
            StudentId = record.StudentId,
            ActivityId = record.ActivityId,
            EvaluationDate = isoDate,
            Score = score,
            SpeedMetric = speedMetric,
            AccuracyMetric = accuracyMetric,
            Notes = record.Notes,
            CreatedAt = createdAt,
            ModifiedAt = modifiedAt
        };

        return new LegacyEvaluationMappingResult(evaluation, warnings);
    }
}
