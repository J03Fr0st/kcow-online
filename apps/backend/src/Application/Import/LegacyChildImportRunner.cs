using Kcow.Domain.Entities;
using Kcow.Domain.Enums;
using Kcow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kcow.Application.Import;

public sealed class LegacyChildImportRunner
{
    private readonly AppDbContext _context;
    private readonly ILogger<LegacyChildImportRunner> _logger;
    private readonly LegacyChildXmlParser _parser;
    private readonly LegacyImportAuditLog _auditLog;
    private readonly LegacyImportSummaryReport _summaryReport;

    public LegacyChildImportRunner(
        AppDbContext context,
        ILogger<LegacyChildImportRunner> logger)
    {
        _context = context;
        _logger = logger;
        _parser = new LegacyChildXmlParser();
        _auditLog = new LegacyImportAuditLog();
        _summaryReport = new LegacyImportSummaryReport();
    }

    public async Task<LegacyImportSummary> RunAsync(string xmlPath, string xsdPath)
    {
        if (string.IsNullOrWhiteSpace(xmlPath))
        {
            throw new ArgumentException("XML path is required.", nameof(xmlPath));
        }

        if (string.IsNullOrWhiteSpace(xsdPath))
        {
            throw new ArgumentException("XSD path is required.", nameof(xsdPath));
        }

        _logger.LogInformation("Starting Children import from {XmlPath}", xmlPath);

        // Step 1: Parse XML
        _logger.LogInformation("Parsing XML file...");
        var parseResult = _parser.Parse(xmlPath, xsdPath);

        if (parseResult.ValidationErrors.Count > 0)
        {
            _auditLog.AddValidationErrors(xmlPath, parseResult.ValidationErrors);
            _logger.LogWarning("Found {Count} validation errors in XML.", parseResult.ValidationErrors.Count);
        }

        // Step 2: Load existing schools and class groups for mapping
        _logger.LogInformation("Loading existing schools and class groups...");
        var schools = await _context.Schools
            .Where(s => s.IsActive)
            .ToDictionaryAsync(s => s.Name, s => s.Id);

        var classGroups = await _context.ClassGroups
            .Where(cg => cg.IsActive)
            .ToDictionaryAsync(cg => cg.Name, cg => cg.Id);

        _logger.LogInformation("Loaded {SchoolCount} schools and {ClassGroupCount} class groups.",
            schools.Count, classGroups.Count);

        // Step 3: Create mapper
        var mapper = new LegacyChildMapper(schools, classGroups);

        // Step 4: Process records and group by family
        _logger.LogInformation("Processing {RecordCount} child records...", parseResult.Records.Count);
        var importedCount = 0;
        var skippedCount = 0;
        var errorCount = 0;
        var familyGroups = new Dictionary<string, (List<Student> students, FamilyInfo info)>();

        foreach (var record in parseResult.Records)
        {
            try
            {
                var mappingResult = mapper.Map(record);

                // Log warnings
                foreach (var warning in mappingResult.Warnings)
                {
                    _logger.LogWarning("Reference {Reference}: {Warning}",
                        record.Reference, warning);
                }

                // Skip if no student was mapped
                if (mappingResult.Student == null)
                {
                    skippedCount++;
                    continue;
                }

                // Check if student already exists
                var existingStudent = await _context.Students
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Reference == mappingResult.Student.Reference);

                if (existingStudent != null)
                {
                    _logger.LogDebug("Student with Reference {Reference} already exists. Skipping.",
                        mappingResult.Student.Reference);
                    skippedCount++;
                    continue;
                }

                // Add to family group if family info exists
                if (mappingResult.FamilyInfo != null)
                {
                    var familyKey = mappingResult.FamilyInfo.FamilyName;
                    if (!familyGroups.ContainsKey(familyKey))
                    {
                        familyGroups[familyKey] = (new List<Student>(), mappingResult.FamilyInfo);
                    }
                    familyGroups[familyKey].students.Add(mappingResult.Student);
                }

                _context.Students.Add(mappingResult.Student);
                importedCount++;

                if (importedCount % 100 == 0)
                {
                    _logger.LogInformation("Processed {Count} records...", importedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing record with Reference {Reference}", record.Reference);
                errorCount++;
            }
        }

        // Step 5: Create families and link students
        _logger.LogInformation("Creating {FamilyCount} families...", familyGroups.Count);
        foreach (var (familyName, (students, familyInfo)) in familyGroups)
        {
            try
            {
                // Check if family already exists
                var existingFamily = await _context.Families
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.FamilyName == familyName);

                int familyId;
                if (existingFamily != null)
                {
                    familyId = existingFamily.Id;
                    _logger.LogDebug("Family '{FamilyName}' already exists. Reusing.", familyName);
                }
                else
                {
                    var family = new Family
                    {
                        FamilyName = familyInfo.FamilyName,
                        PrimaryContactName = familyInfo.PrimaryContactName,
                        Phone = familyInfo.Phone,
                        Email = familyInfo.Email,
                        Address = familyInfo.Address,
                        Notes = $"Imported from legacy data on {DateTime.UtcNow:yyyy-MM-dd}",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Families.Add(family);
                    await _context.SaveChangesAsync();
                    familyId = family.Id;
                    _logger.LogDebug("Created family '{FamilyName}' with ID {FamilyId}", familyName, familyId);
                }

                // Link all students to this family
                foreach (var student in students)
                {
                    // Check if link already exists
                    var existingLink = await _context.StudentFamilies
                        .AnyAsync(sf => sf.StudentId == student.Id && sf.FamilyId == familyId);

                    if (!existingLink)
                    {
                        var studentFamily = new StudentFamily
                        {
                            StudentId = student.Id,
                            FamilyId = familyId,
                            RelationshipType = RelationshipType.Parent // Default to Parent
                        };
                        _context.StudentFamilies.Add(studentFamily);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing family '{FamilyName}'", familyName);
                errorCount++;
            }
        }

        // Step 6: Save all changes
        _logger.LogInformation("Saving changes to database...");
        await _context.SaveChangesAsync();
        _logger.LogInformation("Import complete.");

        var summary = new LegacyImportSummary(importedCount, skippedCount, errorCount, DateTime.UtcNow);

        // Write audit log
        var auditLogPath = Path.Combine(
            Path.GetDirectoryName(xmlPath) ?? ".",
            $"child-import-audit-{DateTime.UtcNow:yyyyMMdd-HHmmss}.log");
        using (var writer = new StreamWriter(auditLogPath))
        {
            _auditLog.WriteTo(writer);
        }
        _logger.LogInformation("Audit log written to {AuditLogPath}", auditLogPath);

        // Write summary report
        var summaryPath = Path.Combine(
            Path.GetDirectoryName(xmlPath) ?? ".",
            $"child-import-summary-{DateTime.UtcNow:yyyyMMdd-HHmmss}.txt");
        _summaryReport.WriteToFile(summaryPath, summary);
        _logger.LogInformation("Summary report written to {SummaryPath}", summaryPath);

        return summary;
    }
}
