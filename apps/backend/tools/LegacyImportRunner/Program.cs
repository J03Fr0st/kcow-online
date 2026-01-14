using Kcow.Infrastructure.Database;
using Kcow.Infrastructure.Database.Seeders;
using Kcow.Infrastructure.Import;
using Kcow.Infrastructure.Repositories;
using Microsoft.Extensions.Logging;

var countOnly = args.Any(arg => string.Equals(arg, "--count", StringComparison.OrdinalIgnoreCase));
var sampleArgIndex = Array.FindIndex(args, arg => string.Equals(arg, "--sample", StringComparison.OrdinalIgnoreCase));
var sampleCount = sampleArgIndex >= 0 && sampleArgIndex + 1 < args.Length && int.TryParse(args[sampleArgIndex + 1], out var parsedSample)
    ? parsedSample
    : 0;
// Find project root first (needed for all paths)
var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
var projectRoot = currentDir.FullName;

// Navigate up to find project root (contains docs directory or README.md)
while (currentDir != null && 
       !Directory.Exists(Path.Combine(currentDir.FullName, "docs")) && 
       !File.Exists(Path.Combine(currentDir.FullName, "README.md")))
{
    currentDir = currentDir.Parent;
}

if (currentDir != null)
{
    projectRoot = currentDir.FullName;
}

var xmlPath = args.Length > 0 && !args[0].StartsWith("--") ? args[0] : Path.Combine(projectRoot, "docs", "legacy", "1_School", "School.xml");
var xsdPath = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : Path.Combine(projectRoot, "docs", "legacy", "1_School", "School.xsd");
var auditPath = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : Path.Combine(projectRoot, "migration-output", "school-import-audit.log");
var summaryPath = args.Length > 3 && !args[3].StartsWith("--") ? args[3] : Path.Combine(projectRoot, "migration-output", "school-import-summary.txt");

// Ensure database exists and migrations are run
// Use the same database location as the API (in the Api directory)
// The API runs from apps/backend/src/Api, so it uses "kcow.db" relative to that directory
var apiDbPath = Path.Combine(projectRoot, "apps", "backend", "src", "Api", "kcow.db");
var connectionString = $"Data Source={apiDbPath}";
Console.WriteLine($"Using database: {apiDbPath}");
var connectionFactory = new SqliteConnectionFactory(connectionString);

// Run migrations
var scriptsPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "src", "Infrastructure", "Migrations", "Scripts");
if (!Directory.Exists(scriptsPath))
{
    scriptsPath = Path.Combine(AppContext.BaseDirectory, "Migrations", "Scripts");
}

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Warning);
});

var logger = loggerFactory.CreateLogger<DbUpBootstrapper>();
var dbUpBootstrapper = new DbUpBootstrapper(connectionString, logger, scriptsPath);
dbUpBootstrapper.RunMigrations();

// Create repositories
var schoolRepository = new SchoolRepository(connectionFactory);
var truckRepository = new TruckRepository(connectionFactory);

// Seed trucks if database is empty (needed for foreign key constraints)
var existingTrucks = await truckRepository.GetAllAsync();
if (!existingTrucks.Any())
{
    Console.WriteLine("No trucks found. Seeding trucks...");
    await TruckSeeder.SeedAsync(truckRepository, logger);
    Console.WriteLine("Trucks seeded successfully.");
}

if (countOnly)
{
    var schools = await schoolRepository.GetAllAsync();
    var count = schools.Count();
    Console.WriteLine($"Schools in kcow.db: {count}");
    return;
}

if (sampleCount > 0)
{
    var schools = await schoolRepository.GetAllAsync();
    var samples = schools
        .OrderBy(s => s.Id)
        .Take(sampleCount)
        .ToList();

    Console.WriteLine($"Sample schools (first {samples.Count}):");
    foreach (var sample in samples)
    {
        Console.WriteLine($"{sample.Id}: {sample.Name} (ShortName: {sample.ShortName ?? "n/a"}, TruckId: {sample.TruckId?.ToString() ?? "n/a"})");
    }

    return;
}

Directory.CreateDirectory(Path.GetDirectoryName(auditPath) ?? ".");
Directory.CreateDirectory(Path.GetDirectoryName(summaryPath) ?? ".");

var importer = new LegacySchoolImportService(schoolRepository, truckRepository);
var summary = await importer.ImportAsync(xmlPath, xsdPath, auditPath, summaryPath);

Console.WriteLine("Legacy school import complete.");
Console.WriteLine($"Imported: {summary.ImportedCount}");
Console.WriteLine($"Skipped: {summary.SkippedCount}");
Console.WriteLine($"Errors: {summary.ErrorCount}");
Console.WriteLine($"Audit log: {auditPath}");
Console.WriteLine($"Summary: {summaryPath}");
