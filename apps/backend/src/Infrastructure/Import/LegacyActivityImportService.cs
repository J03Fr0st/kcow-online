using Kcow.Application.Import;
using Kcow.Application.Interfaces;

namespace Kcow.Infrastructure.Import;

/// <summary>
/// Service for importing legacy Activity data from XML files.
/// Follows the same pattern as LegacyClassGroupImportService.
/// </summary>
public sealed class LegacyActivityImportService
{
    private readonly IActivityRepository _activityRepository;
    private readonly LegacyActivityXmlParser _parser = new();
    private readonly LegacyActivityMapper _mapper = new();
    private readonly LegacyImportAuditLog _auditLog = new();
    private readonly LegacyImportSummaryReport _summaryReport = new();

    public LegacyActivityImportService(IActivityRepository activityRepository)
    {
        _activityRepository = activityRepository;
    }

    public async Task<LegacyImportSummary> ImportAsync(
        string xmlPath,
        string xsdPath,
        string? auditLogPath,
        string? summaryOutputPath,
        bool preview = false,
        CancellationToken cancellationToken = default)
    {
        var result = _parser.Parse(xmlPath, xsdPath);
        _auditLog.AddValidationErrors(Path.GetFileName(xmlPath), result.ValidationErrors);

        var imported = 0;
        var skipped = 0;

        foreach (var record in result.Records)
        {
            try
            {
                var mapping = _mapper.Map(record);

                // Log any warnings from the mapper
                foreach (var warning in mapping.Warnings)
                {
                    _auditLog.AddValidationErrors(Path.GetFileName(xmlPath),
                        new[] { new LegacyXmlValidationError(warning, null, null) });
                }

                // Check for duplicates by ID
                var exists = await _activityRepository.ExistsAsync(record.ActivityId, cancellationToken);

                if (exists)
                {
                    _auditLog.AddValidationErrors(Path.GetFileName(xmlPath),
                        new[] { new LegacyXmlValidationError($"Skipped duplicate activity ID: {record.ActivityId}", null, null) });
                    skipped++;
                    continue;
                }

                if (!preview)
                {
                    await _activityRepository.CreateAsync(mapping.Activity, cancellationToken);
                }
                imported++;
            }
            catch (Exception ex)
            {
                _auditLog.AddValidationErrors(Path.GetFileName(xmlPath),
                    new[] { new LegacyXmlValidationError($"Error processing activity {record.ActivityId}: {ex.Message}", null, null) });
                skipped++;
            }
        }

        if (!string.IsNullOrWhiteSpace(auditLogPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(auditLogPath) ?? ".");
            using var writer = File.CreateText(auditLogPath);
            _auditLog.WriteTo(writer);
        }

        var summary = new LegacyImportSummary(imported, skipped, _auditLog.Entries.Count, DateTime.UtcNow);
        if (!string.IsNullOrWhiteSpace(summaryOutputPath))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(summaryOutputPath) ?? ".");
            _summaryReport.WriteToFile(summaryOutputPath, summary);
        }

        return summary;
    }
}
