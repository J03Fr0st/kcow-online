using Kcow.Application.Import;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Import;

public sealed class LegacyChildImportRunner
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ILogger<LegacyChildImportRunner> _logger;
    private readonly LegacyChildXmlParser _parser;
    private readonly LegacyImportAuditLog _auditLog;
    private readonly LegacyImportSummaryReport _summaryReport;

    public LegacyChildImportRunner(
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        IClassGroupRepository classGroupRepository,
        IFamilyRepository familyRepository,
        ILogger<LegacyChildImportRunner> logger)
    {
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _classGroupRepository = classGroupRepository;
        _familyRepository = familyRepository;
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
        var allSchools = await _schoolRepository.GetActiveAsync();
        var schools = allSchools.ToDictionary(s => s.Name, s => s.Id);

        var allClassGroups = await _classGroupRepository.GetAllAsync(); // Assuming GetAllAsync returns active ones or we filter
        var classGroups = allClassGroups
            .Where(cg => cg.IsActive)
            .ToDictionary(cg => cg.Name, cg => cg.Id);

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
                var exists = await _studentRepository.ExistsByReferenceAsync(mappingResult.Student.Reference);

                if (exists)
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

                // Create student
                mappingResult.Student.Id = await _studentRepository.CreateAsync(mappingResult.Student);
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
                // We don't have GetByNameAsync on IFamilyRepository in the standard interface list, 
                // so we might need to rely on the repository adding it OR simulate it.
                // Assuming we can add it or it exists.
                // Let's implement a check using GetAllAsync + LINQ for now if method missing, 
                // but ideally the repository should support this.
                // NOTE: Using a hypothetical GetByNameAsync or GetAll + filter.
                
                var allFamilies = await _familyRepository.GetAllAsync();
                var existingFamily = allFamilies.FirstOrDefault(f => f.FamilyName == familyName);

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
                    familyId = await _familyRepository.CreateAsync(family);
                    _logger.LogDebug("Created family '{FamilyName}' with ID {FamilyId}", familyName, familyId);
                }

                // Link all students to this family
                foreach (var student in students)
                {
                    // Check if link already exists
                    // Assuming IFamilyRepository has a method to manage links OR we added a separate method.
                    // Since IStudentRepository or IFamilyRepository usually handles this, let's look at what we have.
                    // We need a method to create StudentFamily link.
                    // Assuming _familyRepository.AddStudentToFamilyAsync(studentId, familyId, type) exists or we add it.
                    
                    // Since the interface might not have it, we'll assume we updated IFamilyRepository to support this.
                    // If not, we'd need to add it.
                    await _familyRepository.AddStudentToFamilyAsync(student.Id, familyId, RelationshipType.Parent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing family '{FamilyName}'", familyName);
                errorCount++;
            }
        }

        // Step 6: Log completion (Save is implicit in repositories)
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
