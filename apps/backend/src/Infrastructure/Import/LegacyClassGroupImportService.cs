using Kcow.Application.Import;
using Kcow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Kcow.Infrastructure.Import;

public sealed class LegacyClassGroupImportService
{
    private readonly AppDbContext _context;
    private readonly LegacyClassGroupXmlParser _parser = new();
    private readonly LegacyImportAuditLog _auditLog = new();
    private readonly LegacyImportSummaryReport _summaryReport = new();

    public LegacyClassGroupImportService(AppDbContext context)
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

        // Load valid school IDs for foreign key validation
        var validSchoolIds = new HashSet<int>(await _context.Schools
            .Where(s => s.IsActive)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken));

        // Load valid truck IDs for foreign key validation
        var validTruckIds = new HashSet<int>(await _context.Trucks
            .Where(t => t.IsActive)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken));

        // Create mapper with validation
        var mapper = new LegacyClassGroupMapper(validSchoolIds, validTruckIds);

        var imported = 0;
        var skipped = 0;

        foreach (var record in result.Records)
        {
            var mapping = mapper.Map(record);

            // Skip if mapper returned null (e.g., Import=false or validation failed)
            if (mapping.ClassGroup == null)
            {
                skipped++;
                continue;
            }

            // Check for duplicates
            var exists = await _context.ClassGroups
                .AnyAsync(cg => cg.Name == mapping.ClassGroup.Name &&
                              cg.SchoolId == mapping.ClassGroup.SchoolId &&
                              cg.DayOfWeek == mapping.ClassGroup.DayOfWeek,
                              cancellationToken);

            if (exists)
            {
                _auditLog.AddValidationErrors(Path.GetFileName(xmlPath),
                    new[] { new LegacyXmlValidationError($"Skipped duplicate class group: {mapping.ClassGroup.Name} at school {mapping.ClassGroup.SchoolId}", null, null) });
                skipped++;
                continue;
            }

            // Log any warnings from the mapper
            foreach (var warning in mapping.Warnings)
            {
                _auditLog.AddValidationErrors(Path.GetFileName(xmlPath),
                    new[] { new LegacyXmlValidationError(warning, null, null) });
            }

            _context.ClassGroups.Add(mapping.ClassGroup);
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
