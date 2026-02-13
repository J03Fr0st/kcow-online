using System.Text.Json;
using Kcow.Application.Import;
using Kcow.Application.Import.Mappers;

namespace Kcow.Api.CliCommands;

/// <summary>
/// CLI command handler for running legacy data import with optional preview mode.
/// Usage:
///   dotnet run import run --preview --input docs/legacy   (preview only, no DB writes)
///   dotnet run import run --input docs/legacy             (full import)
/// </summary>
public static class ImportRunCommand
{
    private const string CommandName = "import";
    private const string SubCommandName = "run";

    public static bool IsImportRunCommand(string[] args)
    {
        return args.Length >= 2 &&
               args[0].Equals(CommandName, StringComparison.OrdinalIgnoreCase) &&
               args[1].Equals(SubCommandName, StringComparison.OrdinalIgnoreCase);
    }

    public static async Task<int> ExecuteAsync(
        string[] args, ILegacyParser parser, TextWriter? output = null,
        IImportExecutionService? importService = null)
    {
        output ??= Console.Out;

        var options = ParseOptions(args);
        if (options.Help)
        {
            PrintUsage(output);
            return 0;
        }

        if (string.IsNullOrEmpty(options.InputPath))
        {
            output.WriteLine("Error: Input path is required. Use --input <path>");
            PrintUsage(output);
            return 1;
        }

        if (!Directory.Exists(options.InputPath))
        {
            output.WriteLine($"Error: Input directory not found: {options.InputPath}");
            return 1;
        }

        // Parse all legacy files
        var parseResults = ParseAllEntities(parser, options.InputPath, output);

        // Map parsed records through data mappers
        var previewReport = BuildPreviewReport(parseResults, options.InputPath);

        if (options.Preview)
        {
            PrintPreviewReport(previewReport, output);

            if (!string.IsNullOrEmpty(options.OutputPath))
            {
                await WriteReportJsonAsync(previewReport, options.OutputPath);
                output.WriteLine($"Report written to: {options.OutputPath}");
            }

            return previewReport.TotalErrors > 0 ? 1 : 0;
        }
        else
        {
            // Full import mode — requires import execution service
            if (importService is null)
            {
                output.WriteLine("Error: Import execution service not available.");
                output.WriteLine("Full import requires a database connection. Use --preview for dry run.");
                return 1;
            }

            var importResult = await importService.ExecuteAsync(options.InputPath, options.ConflictMode);
            PrintImportResult(importResult, output);

            // Write exceptions file if there are any
            if (importResult.HasExceptions)
            {
                var exceptionsPath = !string.IsNullOrEmpty(options.OutputPath)
                    ? options.OutputPath
                    : ImportExceptionWriter.GetDefaultPath(options.InputPath);
                await ImportExceptionWriter.WriteAsync(importResult, exceptionsPath);
                output.WriteLine($"Exceptions saved to: {exceptionsPath}");
            }

            return importResult.TotalFailed > 0 ? 1 : 0;
        }
    }

    private static ParsedEntityResults ParseAllEntities(ILegacyParser parser, string inputPath, TextWriter output)
    {
        var results = new ParsedEntityResults();

        var entityConfigs = new[]
        {
            ("School", "1_School", "School.xml", "School.xsd"),
            ("ClassGroup", "2_Class_Group", "Class Group.xml", "Class Group.xsd"),
            ("Activity", "3_Activity", "Activity.xml", "Activity.xsd"),
            ("Children", "4_Children", "Children.xml", "Children.xsd")
        };

        foreach (var (entityName, folder, xmlFile, xsdFile) in entityConfigs)
        {
            var entityFolder = Path.Combine(inputPath, folder);
            var xmlPath = Path.Combine(entityFolder, xmlFile);
            var xsdPath = Path.Combine(entityFolder, xsdFile);

            if (!File.Exists(xmlPath) || !File.Exists(xsdPath))
            {
                results.SkippedEntities.Add(entityName);
                continue;
            }

            try
            {
                switch (entityName)
                {
                    case "School":
                        results.Schools = parser.ParseSchools(xmlPath, xsdPath);
                        break;
                    case "ClassGroup":
                        results.ClassGroups = parser.ParseClassGroups(xmlPath, xsdPath);
                        break;
                    case "Activity":
                        results.Activities = parser.ParseActivities(xmlPath, xsdPath);
                        break;
                    case "Children":
                        results.Children = parser.ParseChildren(xmlPath, xsdPath);
                        break;
                }
            }
            catch (Exception ex)
            {
                results.ParseErrors.Add($"{entityName}: {ex.Message}");
            }
        }

        return results;
    }

    private static ImportPreviewReport BuildPreviewReport(ParsedEntityResults parseResults, string inputPath)
    {
        var report = new ImportPreviewReport
        {
            ExecutedAt = DateTime.UtcNow,
            InputPath = inputPath,
            SkippedEntities = parseResults.SkippedEntities
        };

        // Map Schools
        if (parseResults.Schools is not null && !parseResults.Schools.HasErrors)
        {
            var schoolMapper = new SchoolDataMapper();
            var schoolResult = schoolMapper.MapMany(parseResults.Schools.Records);
            report.Schools = new EntityPreviewResult
            {
                TotalParsed = parseResults.Schools.Records.Count,
                TotalMapped = schoolResult.Data?.Count ?? 0,
                Warnings = schoolResult.Warnings.Select(w => $"{w.Field}: {w.Message}").ToList(),
                Errors = schoolResult.Errors.Select(e => $"{e.Field}: {e.Message}").ToList(),
                SampleRecords = schoolResult.Data?.Take(5)
                    .Select(s => $"{s.Id}. {s.Name} - {s.Address ?? "(no address)"}")
                    .ToList() ?? new()
            };
        }
        else if (parseResults.Schools?.HasErrors == true)
        {
            report.Schools = new EntityPreviewResult
            {
                ParseErrors = parseResults.Schools.Errors.Select(e => e.ToString()).ToList()
            };
        }

        // Map ClassGroups
        if (parseResults.ClassGroups is not null && !parseResults.ClassGroups.HasErrors)
        {
            var classGroupMapper = new ClassGroupDataMapper();
            var cgResult = classGroupMapper.MapMany(parseResults.ClassGroups.Records);
            report.ClassGroups = new EntityPreviewResult
            {
                TotalParsed = parseResults.ClassGroups.Records.Count,
                TotalMapped = cgResult.Data?.Count ?? 0,
                Warnings = cgResult.Warnings.Select(w => $"{w.Field}: {w.Message}").ToList(),
                Errors = cgResult.Errors.Select(e => $"{e.Field}: {e.Message}").ToList(),
                SampleRecords = cgResult.Data?.Take(5)
                    .Select(cg => $"{cg.Name} - {cg.DayOfWeek} {cg.StartTime}-{cg.EndTime}")
                    .ToList() ?? new()
            };
        }
        else if (parseResults.ClassGroups?.HasErrors == true)
        {
            report.ClassGroups = new EntityPreviewResult
            {
                ParseErrors = parseResults.ClassGroups.Errors.Select(e => e.ToString()).ToList()
            };
        }

        // Map Activities
        if (parseResults.Activities is not null && !parseResults.Activities.HasErrors)
        {
            var activityMapper = new ActivityDataMapper();
            var actResult = activityMapper.MapMany(parseResults.Activities.Records);
            report.Activities = new EntityPreviewResult
            {
                TotalParsed = parseResults.Activities.Records.Count,
                TotalMapped = actResult.Data?.Count ?? 0,
                Warnings = actResult.Warnings.Select(w => $"{w.Field}: {w.Message}").ToList(),
                Errors = actResult.Errors.Select(e => $"{e.Field}: {e.Message}").ToList(),
                SampleRecords = actResult.Data?.Take(5)
                    .Select(a => $"{a.Id}. {a.Name} ({a.Code})")
                    .ToList() ?? new()
            };
        }
        else if (parseResults.Activities?.HasErrors == true)
        {
            report.Activities = new EntityPreviewResult
            {
                ParseErrors = parseResults.Activities.Errors.Select(e => e.ToString()).ToList()
            };
        }

        // Map Students
        if (parseResults.Children is not null && !parseResults.Children.HasErrors)
        {
            var studentMapper = new StudentDataMapper();
            var studentResult = studentMapper.MapMany(parseResults.Children.Records);
            report.Students = new EntityPreviewResult
            {
                TotalParsed = parseResults.Children.Records.Count,
                TotalMapped = studentResult.Data?.Count ?? 0,
                Warnings = studentResult.Warnings.Select(w => $"{w.Field}: {w.Message}").ToList(),
                Errors = studentResult.Errors.Select(e => $"{e.Field}: {e.Message}").ToList(),
                SampleRecords = studentResult.Data?.Take(5)
                    .Select(s => $"{s.Student.Reference}: {s.Student.FirstName} {s.Student.LastName}")
                    .ToList() ?? new()
            };
        }
        else if (parseResults.Children?.HasErrors == true)
        {
            report.Students = new EntityPreviewResult
            {
                ParseErrors = parseResults.Children.Errors.Select(e => e.ToString()).ToList()
            };
        }

        report.ParseErrors = parseResults.ParseErrors;

        return report;
    }

    private static void PrintPreviewReport(ImportPreviewReport report, TextWriter output)
    {
        output.WriteLine();
        output.WriteLine("=== IMPORT PREVIEW ===");
        output.WriteLine("NOTE: NO data will be written to the database.");
        output.WriteLine();

        output.WriteLine("Record Counts:");
        PrintEntityCount(output, "Schools", report.Schools);
        PrintEntityCount(output, "Class Groups", report.ClassGroups);
        PrintEntityCount(output, "Activities", report.Activities);
        PrintEntityCount(output, "Students", report.Students);
        output.WriteLine();

        // Sample records
        PrintSampleRecords(output, "Schools", report.Schools);
        PrintSampleRecords(output, "Class Groups", report.ClassGroups);
        PrintSampleRecords(output, "Activities", report.Activities);
        PrintSampleRecords(output, "Students", report.Students);

        // Warnings
        var allWarnings = GetAllWarnings(report);
        if (allWarnings.Count > 0)
        {
            output.WriteLine($"Warnings ({allWarnings.Count} total):");
            foreach (var warning in allWarnings.Take(20))
            {
                output.WriteLine($"  - {warning}");
            }
            if (allWarnings.Count > 20)
                output.WriteLine($"  ... and {allWarnings.Count - 20} more warnings");
            output.WriteLine();
        }

        // Errors
        var allErrors = GetAllErrors(report);
        if (allErrors.Count > 0)
        {
            output.WriteLine($"Errors ({allErrors.Count} records will be skipped):");
            foreach (var error in allErrors.Take(20))
            {
                output.WriteLine($"  - {error}");
            }
            if (allErrors.Count > 20)
                output.WriteLine($"  ... and {allErrors.Count - 20} more errors");
            output.WriteLine();
        }

        // Skipped entities
        if (report.SkippedEntities.Count > 0)
        {
            output.WriteLine("Skipped (files not found):");
            foreach (var entity in report.SkippedEntities)
            {
                output.WriteLine($"  - {entity}");
            }
            output.WriteLine();
        }

        // Summary
        var totalValid = (report.Schools?.TotalMapped ?? 0) +
                         (report.ClassGroups?.TotalMapped ?? 0) +
                         (report.Activities?.TotalMapped ?? 0) +
                         (report.Students?.TotalMapped ?? 0);

        output.WriteLine("Summary:");
        output.WriteLine($"  - Valid: {totalValid} records");
        output.WriteLine($"  - Warnings: {allWarnings.Count} records");
        output.WriteLine($"  - Skipped: {allErrors.Count} records");
        output.WriteLine();
        output.WriteLine("Run without --preview to import data.");
    }

    private static void PrintImportResult(ImportExecutionResult result, TextWriter output)
    {
        output.WriteLine();
        var isReimport = result.TotalUpdated > 0 || result.TotalSkipped > 0;
        output.WriteLine(isReimport ? "=== RE-IMPORT COMPLETE ===" : "=== IMPORT COMPLETE ===");
        output.WriteLine();

        output.WriteLine("Results:");
        PrintEntityImportCount(output, "Schools", result.Schools);
        PrintEntityImportCount(output, "Class Groups", result.ClassGroups);
        PrintEntityImportCount(output, "Activities", result.Activities);
        PrintEntityImportCount(output, "Students", result.Students);
        output.WriteLine();

        if (result.TotalFailed > 0)
        {
            output.WriteLine("Failed:");
            if (result.Schools.Failed > 0) output.WriteLine($"  - Schools: {result.Schools.Failed}");
            if (result.ClassGroups.Failed > 0) output.WriteLine($"  - Class Groups: {result.ClassGroups.Failed}");
            if (result.Activities.Failed > 0) output.WriteLine($"  - Activities: {result.Activities.Failed}");
            if (result.Students.Failed > 0) output.WriteLine($"  - Students: {result.Students.Failed}");
            output.WriteLine();
        }

        output.WriteLine($"Total: {result.TotalProcessed} processed, {result.TotalFailed} failed ({result.SuccessRate}% success rate)");

        if (isReimport)
        {
            output.WriteLine();
            output.WriteLine($"  - New records: {result.TotalImported}");
            output.WriteLine($"  - Updated: {result.TotalUpdated}");
            output.WriteLine($"  - Skipped: {result.TotalSkipped}");
            if (result.TotalUpdated > 0)
                output.WriteLine("Updates logged to audit trail.");
        }
    }

    private static void PrintEntityImportCount(TextWriter output, string name, EntityImportResult result)
    {
        var parts = new List<string>();
        if (result.Imported > 0) parts.Add($"{result.Imported} new");
        if (result.Updated > 0) parts.Add($"{result.Updated} updated");
        if (result.Skipped > 0) parts.Add($"{result.Skipped} skipped");
        if (result.Failed > 0) parts.Add($"{result.Failed} failed");

        var detail = parts.Count > 0 ? string.Join(", ", parts) : "0";
        output.WriteLine($"  - {name}: {detail}");
    }

    private static void PrintEntityCount(TextWriter output, string name, EntityPreviewResult? result)
    {
        if (result is null)
        {
            output.WriteLine($"  - {name}: 0 (no data files found)");
            return;
        }

        var skipped = result.TotalParsed - result.TotalMapped;
        output.WriteLine(skipped > 0
            ? $"  - {name}: {result.TotalMapped} (parsed: {result.TotalParsed}, skipped: {skipped})"
            : $"  - {name}: {result.TotalMapped}");
    }

    private static void PrintSampleRecords(TextWriter output, string name, EntityPreviewResult? result)
    {
        if (result?.SampleRecords is null || result.SampleRecords.Count == 0) return;

        output.WriteLine($"Sample Records ({name}):");
        foreach (var sample in result.SampleRecords)
        {
            output.WriteLine($"  {sample}");
        }
        output.WriteLine();
    }

    private static List<string> GetAllWarnings(ImportPreviewReport report)
    {
        var warnings = new List<string>();
        if (report.Schools?.Warnings is not null) warnings.AddRange(report.Schools.Warnings);
        if (report.ClassGroups?.Warnings is not null) warnings.AddRange(report.ClassGroups.Warnings);
        if (report.Activities?.Warnings is not null) warnings.AddRange(report.Activities.Warnings);
        if (report.Students?.Warnings is not null) warnings.AddRange(report.Students.Warnings);
        return warnings;
    }

    private static List<string> GetAllErrors(ImportPreviewReport report)
    {
        var errors = new List<string>();
        if (report.Schools?.Errors is not null) errors.AddRange(report.Schools.Errors);
        if (report.ClassGroups?.Errors is not null) errors.AddRange(report.ClassGroups.Errors);
        if (report.Activities?.Errors is not null) errors.AddRange(report.Activities.Errors);
        if (report.Students?.Errors is not null) errors.AddRange(report.Students.Errors);
        return errors;
    }

    private static ImportRunOptions ParseOptions(string[] args)
    {
        var options = new ImportRunOptions();

        for (int i = 2; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--input":
                case "-i":
                    if (i + 1 < args.Length) options.InputPath = args[++i];
                    break;
                case "--output":
                case "-o":
                    if (i + 1 < args.Length) options.OutputPath = args[++i];
                    break;
                case "--preview":
                case "-p":
                    options.Preview = true;
                    break;
                case "--help":
                case "-h":
                case "/?":
                    options.Help = true;
                    break;
                case "--skip-existing":
                    options.ConflictMode = ConflictResolutionMode.SkipExisting;
                    break;
                case "--update":
                    options.ConflictMode = ConflictResolutionMode.Update;
                    break;
                case "--fail-on-conflict":
                    options.ConflictMode = ConflictResolutionMode.FailOnConflict;
                    break;
            }
        }

        return options;
    }

    private static void PrintUsage(TextWriter output)
    {
        output.WriteLine();
        output.WriteLine("Usage: dotnet run import run [options]");
        output.WriteLine();
        output.WriteLine("Runs the legacy data import pipeline.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -i, --input <path>       Input directory containing legacy XML/XSD files (required)");
        output.WriteLine("  -o, --output <path>      Output file path for JSON report (optional)");
        output.WriteLine("  -p, --preview            Preview mode - show what would be imported without writing to database");
        output.WriteLine("  --skip-existing          Skip records that already exist (match by legacy_id)");
        output.WriteLine("  --update                 Update existing records with new values (match by legacy_id)");
        output.WriteLine("  --fail-on-conflict       Fail if matching records exist (default)");
        output.WriteLine("  -h, --help               Show this help message");
        output.WriteLine();
        output.WriteLine("Examples:");
        output.WriteLine("  dotnet run --project apps/backend/src/Api import run --preview --input docs/legacy");
        output.WriteLine("  dotnet run --project apps/backend/src/Api import run --input docs/legacy");
        output.WriteLine("  dotnet run --project apps/backend/src/Api import run --input docs/legacy --skip-existing");
        output.WriteLine("  dotnet run --project apps/backend/src/Api import run --input docs/legacy --update");
        output.WriteLine();
    }

    private static async Task WriteReportJsonAsync(ImportPreviewReport report, string outputPath)
    {
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await File.WriteAllTextAsync(outputPath, json);
    }

    private sealed class ImportRunOptions
    {
        public string? InputPath { get; set; }
        public string? OutputPath { get; set; }
        public bool Preview { get; set; }
        public bool Help { get; set; }
        public ConflictResolutionMode ConflictMode { get; set; } = ConflictResolutionMode.FailOnConflict;
    }
}

/// <summary>
/// Results from parsing all entity types.
/// </summary>
public sealed class ParsedEntityResults
{
    public ParseResult<LegacySchoolRecord>? Schools { get; set; }
    public ParseResult<LegacyClassGroupRecord>? ClassGroups { get; set; }
    public ParseResult<LegacyActivityRecord>? Activities { get; set; }
    public ParseResult<LegacyChildRecord>? Children { get; set; }
    public List<string> SkippedEntities { get; set; } = new();
    public List<string> ParseErrors { get; set; } = new();
}

/// <summary>
/// Preview report for the import pipeline.
/// </summary>
public sealed class ImportPreviewReport
{
    public DateTime ExecutedAt { get; set; }
    public string InputPath { get; set; } = string.Empty;
    public EntityPreviewResult? Schools { get; set; }
    public EntityPreviewResult? ClassGroups { get; set; }
    public EntityPreviewResult? Activities { get; set; }
    public EntityPreviewResult? Students { get; set; }
    public List<string> SkippedEntities { get; set; } = new();
    public List<string> ParseErrors { get; set; } = new();
    public int TotalErrors =>
        (Schools?.Errors.Count ?? 0) +
        (ClassGroups?.Errors.Count ?? 0) +
        (Activities?.Errors.Count ?? 0) +
        (Students?.Errors.Count ?? 0);
}

/// <summary>
/// Preview result for a single entity type showing mapped counts, warnings, and samples.
/// </summary>
public sealed class EntityPreviewResult
{
    public int TotalParsed { get; set; }
    public int TotalMapped { get; set; }
    public List<string> SampleRecords { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> ParseErrors { get; set; } = new();
}
