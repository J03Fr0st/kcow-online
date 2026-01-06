using Kcow.Infrastructure.Data;
using Kcow.Infrastructure.Import;
using Microsoft.EntityFrameworkCore;

var countOnly = args.Any(arg => string.Equals(arg, "--count", StringComparison.OrdinalIgnoreCase));
var sampleArgIndex = Array.FindIndex(args, arg => string.Equals(arg, "--sample", StringComparison.OrdinalIgnoreCase));
var sampleCount = sampleArgIndex >= 0 && sampleArgIndex + 1 < args.Length && int.TryParse(args[sampleArgIndex + 1], out var parsedSample)
    ? parsedSample
    : 0;
var xmlPath = args.Length > 0 ? args[0] : Path.Combine("docs", "legacy", "1_School", "School.xml");
var xsdPath = args.Length > 1 ? args[1] : Path.Combine("docs", "legacy", "1_School", "School.xsd");
var auditPath = args.Length > 2 ? args[2] : Path.Combine("migration-output", "school-import-audit.log");
var summaryPath = args.Length > 3 ? args[3] : Path.Combine("migration-output", "school-import-summary.txt");

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite("Data Source=kcow.db")
    .Options;

await using var context = new AppDbContext(options);
await context.Database.EnsureCreatedAsync();

if (countOnly)
{
    var count = await context.Schools.CountAsync();
    Console.WriteLine($"Schools in kcow.db: {count}");
    return;
}

if (sampleCount > 0)
{
    var samples = await context.Schools
        .OrderBy(s => s.Id)
        .Select(s => new { s.Id, s.Name, s.ShortName, s.TruckId })
        .Take(sampleCount)
        .ToListAsync();

    Console.WriteLine($"Sample schools (first {samples.Count}):");
    foreach (var sample in samples)
    {
        Console.WriteLine($"{sample.Id}: {sample.Name} (ShortName: {sample.ShortName ?? "n/a"}, TruckId: {sample.TruckId?.ToString() ?? "n/a"})");
    }

    return;
}

Directory.CreateDirectory(Path.GetDirectoryName(auditPath) ?? ".");
Directory.CreateDirectory(Path.GetDirectoryName(summaryPath) ?? ".");

var importer = new LegacySchoolImportService(context);
var summary = await importer.ImportAsync(xmlPath, xsdPath, auditPath, summaryPath);

Console.WriteLine("Legacy school import complete.");
Console.WriteLine($"Imported: {summary.ImportedCount}");
Console.WriteLine($"Skipped: {summary.SkippedCount}");
Console.WriteLine($"Errors: {summary.ErrorCount}");
Console.WriteLine($"Audit log: {auditPath}");
Console.WriteLine($"Summary: {summaryPath}");
