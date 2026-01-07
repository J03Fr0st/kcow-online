using Kcow.Infrastructure.Data;
using Kcow.Infrastructure.Import;
using Microsoft.EntityFrameworkCore;

var countOnly = args.Any(arg => string.Equals(arg, "--count", StringComparison.OrdinalIgnoreCase));
var sampleArgIndex = Array.FindIndex(args, arg => string.Equals(arg, "--sample", StringComparison.OrdinalIgnoreCase));
var sampleCount = sampleArgIndex >= 0 && sampleArgIndex + 1 < args.Length && int.TryParse(args[sampleArgIndex + 1], out var parsedSample)
    ? parsedSample
    : 0;
var xmlPath = args.Length > 0 && !args[0].StartsWith("--") ? args[0] : Path.Combine("docs", "legacy", "2_Class_Group", "Class Group.xml");
var xsdPath = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : Path.Combine("docs", "legacy", "2_Class_Group", "Class Group.xsd");
var auditPath = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : Path.Combine("migration-output", "classgroup-import-audit.log");
var summaryPath = args.Length > 3 && !args[3].StartsWith("--") ? args[3] : Path.Combine("migration-output", "classgroup-import-summary.txt");

var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite("Data Source=kcow.db")
    .Options;

await using var context = new AppDbContext(options);
await context.Database.EnsureCreatedAsync();

if (countOnly)
{
    var count = await context.ClassGroups.CountAsync();
    Console.WriteLine($"Class Groups in kcow.db: {count}");
    return;
}

if (sampleCount > 0)
{
    var samples = await context.ClassGroups
        .Include(cg => cg.School)
        .OrderBy(cg => cg.Id)
        .Select(cg => new { cg.Id, cg.Name, SchoolName = cg.School!.Name, cg.DayOfWeek })
        .Take(sampleCount)
        .ToListAsync();

    Console.WriteLine($"Sample class groups (first {samples.Count}):");
    foreach (var sample in samples)
    {
        Console.WriteLine($"{sample.Id}: {sample.Name} at {sample.SchoolName} on {sample.DayOfWeek}");
    }

    return;
}

Directory.CreateDirectory(Path.GetDirectoryName(auditPath) ?? ".");
Directory.CreateDirectory(Path.GetDirectoryName(summaryPath) ?? ".");

var importer = new LegacyClassGroupImportService(context);
var summary = await importer.ImportAsync(xmlPath, xsdPath, auditPath, summaryPath);

Console.WriteLine("Legacy class group import complete.");
Console.WriteLine($"Imported: {summary.ImportedCount}");
Console.WriteLine($"Skipped: {summary.SkippedCount}");
Console.WriteLine($"Errors: {summary.ErrorCount}");
Console.WriteLine($"Audit log: {auditPath}");
Console.WriteLine($"Summary: {summaryPath}");
