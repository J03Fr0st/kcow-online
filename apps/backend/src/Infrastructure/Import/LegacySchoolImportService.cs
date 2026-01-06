using Kcow.Application.Import;
using Kcow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kcow.Infrastructure.Import;

public sealed class LegacySchoolImportService
{
    private readonly AppDbContext _context;
    private readonly LegacySchoolXmlParser _parser = new();
    private readonly LegacySchoolMapper _mapper = new();
    private readonly LegacyImportAuditLog _auditLog = new();
    private readonly LegacyImportSummaryReport _summaryReport = new();

    public LegacySchoolImportService(AppDbContext context)
    {
        _context = context;
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
        var validTruckIds = new HashSet<int>(await _context.Trucks
            .Where(t => t.IsActive)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken));

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

            var exists = await _context.Schools.AnyAsync(s => s.Id == mapping.School.Id, cancellationToken);
            if (exists)
            {
                _auditLog.AddValidationErrors(Path.GetFileName(xmlPath),
                    new[] { new LegacyXmlValidationError($"Skipped duplicate school {mapping.School.Id}", null, null) });
                skipped++;
                continue;
            }

            _context.Schools.Add(mapping.School);
            imported++;
        }

        await _context.SaveChangesAsync(cancellationToken);

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
