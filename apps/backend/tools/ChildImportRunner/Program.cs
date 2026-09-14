using Kcow.Infrastructure.Database;
using Kcow.Infrastructure.Repositories;
using Kcow.Infrastructure.Import;
using Microsoft.Extensions.Logging;

if (args.Contains("--help"))
{
    Console.WriteLine("Legacy import: [xmlPath] [xsdPath] [auditPath] [summaryPath] [--count | --sample N]. Activity runner also supports --preview.");
    Console.WriteLine("Set ConnectionStrings__DefaultConnection to an absolute SQLite path. Count/sample/preview use read-only access to existing schema.");
    return;
}

var countOnly = args.Any(arg => string.Equals(arg, "--count", StringComparison.OrdinalIgnoreCase));
var sampleArgIndex = Array.FindIndex(args, arg => string.Equals(arg, "--sample", StringComparison.OrdinalIgnoreCase));
var sampleCount = sampleArgIndex >= 0 && sampleArgIndex + 1 < args.Length && int.TryParse(args[sampleArgIndex + 1], out var parsedSample)
    ? parsedSample
    : 0;
var xmlPath = args.Length > 0 ? args[0] : Path.Combine("docs", "legacy", "4_Children", "Children.xml");
var xsdPath = args.Length > 1 ? args[1] : Path.Combine("docs", "legacy", "4_Children", "Children.xsd");

if (!countOnly && sampleCount == 0 && (!File.Exists(xmlPath) || !File.Exists(xsdPath)))
{
    Console.Error.WriteLine("XML and XSD input files must exist.");
    Environment.ExitCode = 1;
    return;
}
var connectionFactory = LegacyToolDatabase.Open(!countOnly && sampleCount == 0);


var students = new StudentRepository(connectionFactory);
var schools = new SchoolRepository(connectionFactory);
var groups = new ClassGroupRepository(connectionFactory);
var families = new FamilyRepository(connectionFactory);

if (countOnly)
{
    var studentCount = (await students.GetAllAsync()).Count();
    var familyCount = (await families.GetAllAsync()).Count();
    Console.WriteLine($"Students in kcow.db: {studentCount}");
    Console.WriteLine($"Families in kcow.db: {familyCount}");
    return;
}

if (sampleCount > 0)
{
    var samples = (await students.GetAllAsync())
        .OrderBy(s => s.Id)
        .Select(s => new { s.Id, s.Reference, s.FirstName, s.LastName, s.Family })
        .Take(sampleCount)
        .ToList();

    Console.WriteLine($"Sample students (first {samples.Count}):");
    foreach (var sample in samples)
    {
        Console.WriteLine($"{sample.Id}: [{sample.Reference}] {sample.FirstName} {sample.LastName} (Family: {sample.Family ?? "n/a"})");
    }

    return;
}

// Create a logger factory for the importer
using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

var logger = loggerFactory.CreateLogger<Program>();
logger.LogInformation("Starting Child/Student import from {XmlPath}", xmlPath);

var importer = new LegacyChildImportRunner(students, schools, groups, families, loggerFactory.CreateLogger<LegacyChildImportRunner>());

try
{
    var summary = await importer.RunAsync(xmlPath, xsdPath);

    Console.WriteLine();
    Console.WriteLine("===========================================");
    Console.WriteLine("Legacy Child/Student Import Complete");
    Console.WriteLine("===========================================");
    Console.WriteLine($"Imported: {summary.ImportedCount}");
    Console.WriteLine($"Skipped:   {summary.SkippedCount}");
    Console.WriteLine($"Errors:    {summary.ErrorCount}");
    Console.WriteLine($"Completed: {summary.CompletedAt:O}");
    Console.WriteLine("===========================================");
}
catch (Exception ex)
{
    logger.LogError(ex, "Import failed with error");
    Console.WriteLine($"Error: {ex.Message}");
    Environment.Exit(1);
}
