using Kcow.Application.Import;
using Kcow.Application.Interfaces;

namespace Kcow.Infrastructure.Import;

public sealed class LegacySchoolImportService
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ITruckRepository _truckRepository;
    private readonly LegacySchoolXmlParser _parser = new();
    private readonly LegacySchoolMapper _mapper = new();
    private readonly LegacyImportAuditLog _auditLog = new();
    private readonly LegacyImportSummaryReport _summaryReport = new();

    public LegacySchoolImportService(ISchoolRepository schoolRepository, ITruckRepository truckRepository)
    {
        _schoolRepository = schoolRepository;
        _truckRepository = truckRepository;
    }

    public async Task<LegacyImportSummary> ImportAsync(
        string xmlPath,
        string xsdPath,
        string? auditLogPath,
        string? summaryOutputPath,
        CancellationToken cancellationToken = default)
    {
        var result = _parser.Parse(xmlPath, xsdPath);
        _auditLog.AddValidationErrors(Path.GetFileName(xmlPath), result.ValidationErrors);

        // Load valid truck IDs for foreign key validation
        var validTruckIds = new HashSet<int>((await _truckRepository.GetAllAsync(cancellationToken))
            .Where(t => t.IsActive)
            .Select(t => t.Id));

        // Create mapper with truck validation
        var mapper = new LegacySchoolMapper(validTruckIds);

        var imported = 0;
        var skipped = 0;

        foreach (var record in result.Records)
        {
            var mapping = mapper.Map(record);
            if (string.IsNullOrWhiteSpace(mapping.School.Name))
            {
                skipped++;
                continue;
            }

            var exists = await _schoolRepository.ExistsAsync(mapping.School.Id, cancellationToken);
            if (exists)
            {
                _auditLog.AddValidationErrors(Path.GetFileName(xmlPath),
                    new[] { new LegacyXmlValidationError($"Skipped duplicate school {mapping.School.Id}", null, null) });
                skipped++;
                continue;
            }

            await _schoolRepository.CreateAsync(mapping.School, cancellationToken);
            imported++;
        }

        if (!string.IsNullOrWhiteSpace(auditLogPath))
        {
            using var writer = File.CreateText(auditLogPath);
            _auditLog.WriteTo(writer);
        }

        var summary = new LegacyImportSummary(imported, skipped, _auditLog.Entries.Count, DateTime.UtcNow);
        if (!string.IsNullOrWhiteSpace(summaryOutputPath))
        {
            _summaryReport.WriteToFile(summaryOutputPath, summary);
        }

        return summary;
    }
}
