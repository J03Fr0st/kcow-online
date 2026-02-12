using Dapper;
using Kcow.Application.Import;
using Kcow.Application.Interfaces;
using Kcow.Infrastructure.Database;
using AttendanceEntity = Kcow.Domain.Entities.Attendance;
using EvaluationEntity = Kcow.Domain.Entities.Evaluation;

namespace Kcow.Infrastructure.Import;

/// <summary>
/// Extended import summary that separates attendance and evaluation counts.
/// </summary>
public sealed record LegacyAttendanceEvaluationImportSummary(
    int AttendanceImported,
    int AttendanceSkipped,
    int EvaluationImported,
    int EvaluationSkipped,
    int ErrorCount,
    DateTime CompletedAt)
{
    public int TotalImported => AttendanceImported + EvaluationImported;
    public int TotalSkipped => AttendanceSkipped + EvaluationSkipped;
}

/// <summary>
/// Service for importing legacy attendance and evaluation data.
/// Parses Activity XML for context, then imports attendance and evaluation records
/// using the existing student-class group-activity relationships in the database.
/// </summary>
public sealed class LegacyAttendanceEvaluationImportService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly LegacyAttendanceEvaluationXmlParser _parser = new();
    private readonly LegacyImportAuditLog _auditLog = new();
    private readonly LegacyImportSummaryReport _summaryReport = new();

    public LegacyAttendanceEvaluationImportService(
        IAttendanceRepository attendanceRepository,
        IEvaluationRepository evaluationRepository,
        IDbConnectionFactory connectionFactory)
    {
        _attendanceRepository = attendanceRepository;
        _evaluationRepository = evaluationRepository;
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Imports attendance records from provided data.
    /// Validates foreign keys against existing database records.
    /// </summary>
    public async Task<LegacyAttendanceEvaluationImportSummary> ImportAttendanceAsync(
        IReadOnlyList<LegacyAttendanceImportRecord> records,
        string? auditLogPath,
        string? summaryOutputPath,
        bool preview = false,
        CancellationToken cancellationToken = default)
    {
        // Load valid foreign keys
        var validStudentIds = await LoadStudentIdsAsync(cancellationToken);
        var validClassGroupIds = await LoadClassGroupIdsAsync(cancellationToken);

        var mapper = new LegacyAttendanceMapper(validStudentIds, validClassGroupIds);
        var attendanceRecords = new List<AttendanceEntity>();
        var skipped = 0;

        foreach (var record in records)
        {
            var mapping = mapper.Map(record);

            foreach (var warning in mapping.Warnings)
            {
                _auditLog.AddValidationErrors("attendance-import",
                    new[] { new LegacyXmlValidationError(warning, null, null) });
            }

            if (mapping.Attendance == null)
            {
                skipped++;
                continue;
            }

            attendanceRecords.Add(mapping.Attendance);
        }

        int created = 0;
        int updated = 0;

        if (!preview && attendanceRecords.Count > 0)
        {
            try
            {
                (created, updated) = await _attendanceRepository.BatchSaveAsync(attendanceRecords, cancellationToken);
            }
            catch (Exception ex)
            {
                _auditLog.AddValidationErrors("attendance-import",
                    new[] { new LegacyXmlValidationError($"Batch save failed: {ex.Message}", null, null) });
            }
        }
        else if (preview)
        {
            created = attendanceRecords.Count;
        }

        var summary = new LegacyAttendanceEvaluationImportSummary(
            AttendanceImported: created,
            AttendanceSkipped: skipped + updated,
            EvaluationImported: 0,
            EvaluationSkipped: 0,
            ErrorCount: _auditLog.Entries.Count,
            CompletedAt: DateTime.UtcNow);

        WriteOutputFiles(auditLogPath, summaryOutputPath, summary);
        return summary;
    }

    /// <summary>
    /// Imports evaluation records from provided data.
    /// Validates foreign keys against existing database records.
    /// </summary>
    public async Task<LegacyAttendanceEvaluationImportSummary> ImportEvaluationsAsync(
        IReadOnlyList<LegacyEvaluationImportRecord> records,
        string? auditLogPath,
        string? summaryOutputPath,
        bool preview = false,
        CancellationToken cancellationToken = default)
    {
        // Load valid foreign keys
        var validStudentIds = await LoadStudentIdsAsync(cancellationToken);
        var validActivityIds = await LoadActivityIdsAsync(cancellationToken);

        var mapper = new LegacyEvaluationMapper(validStudentIds, validActivityIds);
        var evaluationRecords = new List<EvaluationEntity>();
        var skipped = 0;

        foreach (var record in records)
        {
            var mapping = mapper.Map(record);

            foreach (var warning in mapping.Warnings)
            {
                _auditLog.AddValidationErrors("evaluation-import",
                    new[] { new LegacyXmlValidationError(warning, null, null) });
            }

            if (mapping.Evaluation == null)
            {
                skipped++;
                continue;
            }

            evaluationRecords.Add(mapping.Evaluation);
        }

        int created = 0;
        int batchSkipped = 0;

        if (!preview && evaluationRecords.Count > 0)
        {
            try
            {
                (created, batchSkipped) = await _evaluationRepository.BatchCreateAsync(evaluationRecords, cancellationToken);
            }
            catch (Exception ex)
            {
                _auditLog.AddValidationErrors("evaluation-import",
                    new[] { new LegacyXmlValidationError($"Batch create failed: {ex.Message}", null, null) });
            }
        }
        else if (preview)
        {
            created = evaluationRecords.Count;
        }

        var summary = new LegacyAttendanceEvaluationImportSummary(
            AttendanceImported: 0,
            AttendanceSkipped: 0,
            EvaluationImported: created,
            EvaluationSkipped: skipped + batchSkipped,
            ErrorCount: _auditLog.Entries.Count,
            CompletedAt: DateTime.UtcNow);

        WriteOutputFiles(auditLogPath, summaryOutputPath, summary);
        return summary;
    }

    /// <summary>
    /// Full import: parses Activity XML for context, then imports both attendance and evaluation records.
    /// </summary>
    public async Task<LegacyAttendanceEvaluationImportSummary> ImportAllAsync(
        string activityXmlPath,
        string activityXsdPath,
        IReadOnlyList<LegacyAttendanceImportRecord> attendanceRecords,
        IReadOnlyList<LegacyEvaluationImportRecord> evaluationRecords,
        string? auditLogPath,
        string? summaryOutputPath,
        bool preview = false,
        CancellationToken cancellationToken = default)
    {
        // Parse Activity XML for validation context
        var parseResult = _parser.Parse(activityXmlPath, activityXsdPath);
        _auditLog.AddValidationErrors(Path.GetFileName(activityXmlPath), parseResult.ValidationErrors);

        // Load valid foreign keys
        var validStudentIds = await LoadStudentIdsAsync(cancellationToken);
        var validClassGroupIds = await LoadClassGroupIdsAsync(cancellationToken);
        var validActivityIds = await LoadActivityIdsAsync(cancellationToken);

        // Import attendance
        var attendanceMapper = new LegacyAttendanceMapper(validStudentIds, validClassGroupIds);
        var mappedAttendance = new List<AttendanceEntity>();
        var attendanceSkipped = 0;

        foreach (var record in attendanceRecords)
        {
            var mapping = attendanceMapper.Map(record);
            foreach (var warning in mapping.Warnings)
            {
                _auditLog.AddValidationErrors("attendance-import",
                    new[] { new LegacyXmlValidationError(warning, null, null) });
            }

            if (mapping.Attendance == null)
            {
                attendanceSkipped++;
                continue;
            }
            mappedAttendance.Add(mapping.Attendance);
        }

        // Import evaluations
        var evaluationMapper = new LegacyEvaluationMapper(validStudentIds, validActivityIds);
        var mappedEvaluations = new List<EvaluationEntity>();
        var evaluationSkipped = 0;

        foreach (var record in evaluationRecords)
        {
            var mapping = evaluationMapper.Map(record);
            foreach (var warning in mapping.Warnings)
            {
                _auditLog.AddValidationErrors("evaluation-import",
                    new[] { new LegacyXmlValidationError(warning, null, null) });
            }

            if (mapping.Evaluation == null)
            {
                evaluationSkipped++;
                continue;
            }
            mappedEvaluations.Add(mapping.Evaluation);
        }

        int attCreated = 0, attUpdated = 0, evalCreated = 0, evalBatchSkipped = 0;

        if (!preview)
        {
            if (mappedAttendance.Count > 0)
            {
                try
                {
                    (attCreated, attUpdated) = await _attendanceRepository.BatchSaveAsync(mappedAttendance, cancellationToken);
                }
                catch (Exception ex)
                {
                    _auditLog.AddValidationErrors("attendance-import",
                        new[] { new LegacyXmlValidationError($"Batch save failed: {ex.Message}", null, null) });
                }
            }

            if (mappedEvaluations.Count > 0)
            {
                try
                {
                    (evalCreated, evalBatchSkipped) = await _evaluationRepository.BatchCreateAsync(mappedEvaluations, cancellationToken);
                }
                catch (Exception ex)
                {
                    _auditLog.AddValidationErrors("evaluation-import",
                        new[] { new LegacyXmlValidationError($"Batch create failed: {ex.Message}", null, null) });
                }
            }
        }
        else
        {
            attCreated = mappedAttendance.Count;
            evalCreated = mappedEvaluations.Count;
        }

        var summary = new LegacyAttendanceEvaluationImportSummary(
            AttendanceImported: attCreated,
            AttendanceSkipped: attendanceSkipped + attUpdated,
            EvaluationImported: evalCreated,
            EvaluationSkipped: evaluationSkipped + evalBatchSkipped,
            ErrorCount: _auditLog.Entries.Count,
            CompletedAt: DateTime.UtcNow);

        WriteOutputFiles(auditLogPath, summaryOutputPath, summary);
        return summary;
    }

    private void WriteOutputFiles(string? auditLogPath, string? summaryOutputPath,
        LegacyAttendanceEvaluationImportSummary summary)
    {
        if (!string.IsNullOrWhiteSpace(auditLogPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(auditLogPath) ?? ".");
            using var writer = File.CreateText(auditLogPath);
            _auditLog.WriteTo(writer);
        }

        if (!string.IsNullOrWhiteSpace(summaryOutputPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(summaryOutputPath) ?? ".");
            var legacySummary = new LegacyImportSummary(
                summary.TotalImported, summary.TotalSkipped, summary.ErrorCount, summary.CompletedAt);
            _summaryReport.WriteToFile(summaryOutputPath, legacySummary);
        }
    }

    private async Task<HashSet<int>> LoadStudentIdsAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.Create();
        var ids = await connection.QueryAsync<int>("SELECT id FROM students");
        return new HashSet<int>(ids);
    }

    private async Task<HashSet<int>> LoadClassGroupIdsAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.Create();
        var ids = await connection.QueryAsync<int>("SELECT id FROM class_groups");
        return new HashSet<int>(ids);
    }

    private async Task<HashSet<int>> LoadActivityIdsAsync(CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.Create();
        var ids = await connection.QueryAsync<int>("SELECT id FROM activities");
        return new HashSet<int>(ids);
    }
}
