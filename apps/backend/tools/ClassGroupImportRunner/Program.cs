using Kcow.Infrastructure.Database;
using Kcow.Infrastructure.Repositories;
using Kcow.Infrastructure.Import;

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
var xmlPath = args.Length > 0 && !args[0].StartsWith("--") ? args[0] : Path.Combine("docs", "legacy", "2_Class_Group", "Class Group.xml");
var xsdPath = args.Length > 1 && !args[1].StartsWith("--") ? args[1] : Path.Combine("docs", "legacy", "2_Class_Group", "Class Group.xsd");
var auditPath = args.Length > 2 && !args[2].StartsWith("--") ? args[2] : Path.Combine("migration-output", "classgroup-import-audit.log");
var summaryPath = args.Length > 3 && !args[3].StartsWith("--") ? args[3] : Path.Combine("migration-output", "classgroup-import-summary.txt");

if (!countOnly && sampleCount == 0 && (!File.Exists(xmlPath) || !File.Exists(xsdPath)))
{
    Console.Error.WriteLine("XML and XSD input files must exist.");
    Environment.ExitCode = 1;
    return;
}
var connectionFactory = LegacyToolDatabase.Open(!countOnly && sampleCount == 0);


var groups = new ClassGroupRepository(connectionFactory);
var schools = new SchoolRepository(connectionFactory);
var trucks = new TruckRepository(connectionFactory);

if (countOnly)
{
    var count = (await groups.GetAllAsync()).Count();
    Console.WriteLine($"Class Groups in kcow.db: {count}");
    return;
}

if (sampleCount > 0)
{
    var schoolNames = (await schools.GetAllAsync()).ToDictionary(school => school.Id, school => school.Name);
    var samples = (await groups.GetAllAsync())
        .OrderBy(cg => cg.Id)
        .Select(cg => new { cg.Id, cg.Name, SchoolName = cg.SchoolId.HasValue ? schoolNames.GetValueOrDefault(cg.SchoolId.Value) : null, cg.DayOfWeek })
        .Take(sampleCount)
        .ToList();

    Console.WriteLine($"Sample class groups (first {samples.Count}):");
    foreach (var sample in samples)
    {
        Console.WriteLine($"{sample.Id}: {sample.Name} at {sample.SchoolName} on {sample.DayOfWeek}");
    }

    return;
}

Directory.CreateDirectory(Path.GetDirectoryName(auditPath) ?? ".");
Directory.CreateDirectory(Path.GetDirectoryName(summaryPath) ?? ".");

var importer = new LegacyClassGroupImportService(groups, schools, trucks);
var summary = await importer.ImportAsync(xmlPath, xsdPath, auditPath, summaryPath);

Console.WriteLine("Legacy class group import complete.");
Console.WriteLine($"Imported: {summary.ImportedCount}");
Console.WriteLine($"Skipped: {summary.SkippedCount}");
Console.WriteLine($"Errors: {summary.ErrorCount}");
Console.WriteLine($"Audit log: {auditPath}");
Console.WriteLine($"Summary: {summaryPath}");
