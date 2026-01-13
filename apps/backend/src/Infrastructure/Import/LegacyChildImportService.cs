using Kcow.Application.Import;
using Kcow.Application.Interfaces;
using Kcow.Infrastructure.Import;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Import;

/// <summary>
/// Service for importing legacy Children (Students) and Families data from XML.
/// This is a thin wrapper around LegacyChildImportRunner for consistency with other import services.
/// </summary>
public sealed class LegacyChildImportService
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly ILoggerFactory _loggerFactory;

    public LegacyChildImportService(
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        IClassGroupRepository classGroupRepository,
        IFamilyRepository familyRepository,
        ILoggerFactory loggerFactory)
    {
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _classGroupRepository = classGroupRepository;
        _familyRepository = familyRepository;
        _loggerFactory = loggerFactory;
    }

    public async Task<LegacyImportSummary> ImportAsync(
        string xmlPath,
        string xsdPath,
        string? auditLogPath,
        string? summaryOutputPath,
        CancellationToken cancellationToken = default)
    {
        var runnerLogger = _loggerFactory.CreateLogger<LegacyChildImportRunner>();
        var runner = new LegacyChildImportRunner(
            _studentRepository,
            _schoolRepository,
            _classGroupRepository,
            _familyRepository,
            runnerLogger);

        var summary = await runner.RunAsync(xmlPath, xsdPath);

        return summary;
    }
}
