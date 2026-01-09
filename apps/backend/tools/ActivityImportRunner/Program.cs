using Kcow.Infrastructure.Data;
using Kcow.Infrastructure.Import;
using Microsoft.EntityFrameworkCore;

// Parse command line arguments
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
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite("Data Source=kcow.db")
    .Options;

await using var context = new AppDbContext(options);
await context.Database.EnsureCreatedAsync();

// Handle --count flag
if (countOnly)
{
    var count = await context.Activities.CountAsync();
    Console.WriteLine($"Activities in kcow.db: {count}");
    return;
}

// Handle --sample flag
if (sampleCount > 0)
{
    var samples = await context.Activities
        .OrderBy(a => a.Id)
        .Select(a => new { a.Id, a.Code, a.Name, a.GradeLevel })
        .Take(sampleCount)
        .ToListAsync();

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

// Ensure output directories exist
Directory.CreateDirectory(Path.GetDirectoryName(auditPath) ?? ".");
Directory.CreateDirectory(Path.GetDirectoryName(summaryPath) ?? ".");

Console.WriteLine($"Importing activities from: {xmlPath}");
Console.WriteLine($"Using XSD schema: {xsdPath}");

if (preview)
{
    Console.WriteLine("PREVIEW MODE - No data will be written to database");
}

// Run import
var importer = new LegacyActivityImportService(context);
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
Console.WriteLine($"Total activities in database: {await context.Activities.CountAsync()}");
