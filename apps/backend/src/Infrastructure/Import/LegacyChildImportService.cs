using Kcow.Application.Import;
using Kcow.Infrastructure.Data;
using Kcow.Infrastructure.Import;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Import;

/// <summary>
/// Service for importing legacy Children (Students) and Families data from XML.
/// This is a thin wrapper around LegacyChildImportRunner for consistency with other import services.
/// </summary>
public sealed class LegacyChildImportService
{
    private readonly AppDbContext _context;
    private readonly ILogger<LegacyChildImportService> _logger;
    private readonly ILoggerFactory _loggerFactory;

    public LegacyChildImportService(AppDbContext context, ILogger<LegacyChildImportService> logger, ILoggerFactory loggerFactory)
    {
        _context = context;
        _logger = logger;
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
        var runner = new LegacyChildImportRunner(_context, runnerLogger);
        var summary = await runner.RunAsync(xmlPath, xsdPath);

        return summary;
    }
}
