using Kcow.Application.Import;
using Kcow.Application.Interfaces;

namespace Kcow.Infrastructure.Import;

public sealed class LegacyClassGroupImportService
{
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ITruckRepository _truckRepository;
    private readonly LegacyClassGroupXmlParser _parser = new();
    private readonly LegacyImportAuditLog _auditLog = new();
    private readonly LegacyImportSummaryReport _summaryReport = new();

    public LegacyClassGroupImportService(
        IClassGroupRepository classGroupRepository,
        ISchoolRepository schoolRepository,
        ITruckRepository truckRepository)
    {
        _classGroupRepository = classGroupRepository;
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

        // Load valid school IDs for foreign key validation
        var validSchoolIds = new HashSet<int>((await _schoolRepository.GetAllAsync(cancellationToken))
            .Where(s => s.IsActive)
            .Select(s => s.Id));

        // Load valid truck IDs for foreign key validation
        var validTruckIds = new HashSet<int>((await _truckRepository.GetAllAsync(cancellationToken))
            .Where(t => t.IsActive)
            .Select(t => t.Id));

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
            // Since Dapper repository might not have a complex predicate check like EF Core, 
            // we might need to implement a specific check method or fetch potential duplicates.
            // For now, assuming ExistsAsync(id) isn't enough as the original check was composite.
            // However, based on typical repository patterns, we'll check by ID if available or just create.
            // Given the original code checked specific fields:
            // cg.Name == mapping.ClassGroup.Name && cg.SchoolId == mapping.ClassGroup.SchoolId && cg.DayOfWeek == mapping.ClassGroup.DayOfWeek
            
            // NOTE: A proper Dapper repository would need a method for this specific check to be efficient.
            // For this migration, we will use a naive approach or assume the repository has a matching method if we updated it.
            // But since I didn't add such a method to IClassGroupRepository in the prompt, I will assume we can skip this check 
            // or perform it in memory if the dataset is small, OR just rely on database constraints.
            // Let's assume for now we just try to create and catch exceptions or similar, OR fetch all for that school and check in memory.
            
            // Optimization: Load all class groups for the school to check duplicates in memory.
            var existingGroups = await _classGroupRepository.GetBySchoolIdAsync(mapping.ClassGroup.SchoolId, cancellationToken);
            var exists = existingGroups.Any(cg => 
                cg.Name == mapping.ClassGroup.Name && 
                cg.DayOfWeek == mapping.ClassGroup.DayOfWeek);

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

            await _classGroupRepository.CreateAsync(mapping.ClassGroup, cancellationToken);
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
