using Kcow.Application.Import;
using Kcow.Infrastructure.Data;
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

    public LegacyChildImportService(AppDbContext context, ILogger<LegacyChildImportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<LegacyImportSummary> ImportAsync(
        string xmlPath,
        string xsdPath,
        string? auditLogPath,
        string? summaryOutputPath,
        CancellationToken cancellationToken = default)
    {
        var runner = new LegacyChildImportRunner(_context, _logger);
        var summary = await runner.RunAsync(xmlPath, xsdPath);

        return summary;
    }
}
