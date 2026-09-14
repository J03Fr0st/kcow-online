using Kcow.Infrastructure.Database;
using Kcow.Infrastructure.Repositories;
using Kcow.Infrastructure.Import;

// Parse command line arguments
if (args.Contains("--help"))
{
    Console.WriteLine("Legacy import: [xmlPath] [xsdPath] [auditPath] [summaryPath] [--count | --sample N]. Activity runner also supports --preview.");
    Console.WriteLine("Set ConnectionStrings__DefaultConnection to an absolute SQLite path. Count/sample/preview use read-only access to existing schema.");
    return;
}

var countOnly = args.Any(arg => string.Equals(arg, "--count", StringComparison.OrdinalIgnoreCase));
var preview = args.Any(arg => string.Equals(arg, "--preview", StringComparison.OrdinalIgnoreCase));
var sampleArgIndex = Array.FindIndex(args, arg => string.Equals(arg, "--sample", StringComparison.OrdinalIgnoreCase));
var sampleCount = sampleArgIndex >= 0 && sampleArgIndex + 1 < args.Length && int.TryParse(args[sampleArgIndex + 1], out var parsedSample)
    ? parsedSample
    : 0;

// Default file paths
var xmlPath = args.Length > 0 && !args[0].StartsWith("--") ? args[0] : Path.Combine("docs", "legacy", "3_Activity", "Activity.xml");
var xsdPath = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : Path.Combine("docs", "legacy", "3_Activity", "Activity.xsd");
var auditPath = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : Path.Combine("migration-output", "activity-import-audit.log");
var summaryPath = args.Length > 3 && !args[3].StartsWith("--") ? args[3] : Path.Combine("migration-output", "activity-import-summary.txt");

// Build database context
if (!countOnly && sampleCount == 0 && (!File.Exists(xmlPath) || !File.Exists(xsdPath)))
{
    Console.Error.WriteLine("XML and XSD input files must exist.");
    Environment.ExitCode = 1;
    return;
}
var connectionFactory = LegacyToolDatabase.Open(!countOnly && sampleCount == 0 && !preview);


var activityRepository = new ActivityRepository(connectionFactory);

// Handle --count flag
if (countOnly)
{
    var count = (await activityRepository.GetAllAsync()).Count();
    Console.WriteLine($"Activities in kcow.db: {count}");
    return;
}

// Handle --sample flag
if (sampleCount > 0)
{
    var samples = (await activityRepository.GetAllAsync())
        .OrderBy(a => a.Id)
        .Select(a => new { a.Id, a.Code, a.Name, a.GradeLevel })
        .Take(sampleCount)
        .ToList();

    Console.WriteLine($"Sample activities (first {samples.Count}):");
    foreach (var sample in samples)
    {
        Console.WriteLine($"{sample.Id}: [{sample.Code ?? "-"}] {sample.Name ?? "-"} ({sample.GradeLevel ?? "-"})");
    }

    return;
}

// Validate input files exist
if (!File.Exists(xmlPath))
{
    Console.Error.WriteLine($"Error: XML file not found: {xmlPath}");
    Environment.Exit(1);
    return;
}

if (!File.Exists(xsdPath))
{
    Console.Error.WriteLine($"Error: XSD file not found: {xsdPath}");
    Environment.Exit(1);
    return;
}

// Preview must not create report directories.
if (!preview)
{
    Directory.CreateDirectory(Path.GetDirectoryName(auditPath) ?? ".");
    Directory.CreateDirectory(Path.GetDirectoryName(summaryPath) ?? ".");
}

Console.WriteLine($"Importing activities from: {xmlPath}");
Console.WriteLine($"Using XSD schema: {xsdPath}");

if (preview)
{
    Console.WriteLine("PREVIEW MODE - No data will be written to database");
}

// Run import
var importer = new LegacyActivityImportService(activityRepository);
var summary = await importer.ImportAsync(xmlPath, xsdPath, preview ? null : auditPath, preview ? null : summaryPath, preview);

Console.WriteLine();
Console.WriteLine("Legacy activity import complete.");
Console.WriteLine($"Imported: {summary.ImportedCount}");
Console.WriteLine($"Skipped: {summary.SkippedCount}");
Console.WriteLine($"Errors: {summary.ErrorCount}");

if (!preview)
{
    Console.WriteLine($"Audit log: {auditPath}");
    Console.WriteLine($"Summary: {summaryPath}");
}

Console.WriteLine();
Console.WriteLine($"Total activities in database: {(await activityRepository.GetAllAsync()).Count()}");
