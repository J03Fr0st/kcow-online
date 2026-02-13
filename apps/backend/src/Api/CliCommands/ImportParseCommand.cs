using System.Text.Json;
using Kcow.Application.Import;

namespace Kcow.Api.CliCommands;

/// <summary>
/// CLI command handler for parsing legacy XML files.
/// Usage: dotnet run import parse --input docs/legacy
/// </summary>
public static class ImportParseCommand
{
    private const string CommandName = "import";
    private const string SubCommandName = "parse";
    private const string InputOption = "--input";
    private const string InputShortOption = "-i";
    private const string OutputOption = "--output";
    private const string OutputShortOption = "-o";
    private const string VerboseOption = "--verbose";
    private const string VerboseShortOption = "-v";

    /// <summary>
    /// Checks if the command line arguments indicate an import parse command.
    /// </summary>
    public static bool IsImportParseCommand(string[] args)
    {
        return args.Length >= 2 &&
               args[0].Equals(CommandName, StringComparison.OrdinalIgnoreCase) &&
               args[1].Equals(SubCommandName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Executes the import parse command.
    /// </summary>
    public static async Task<int> ExecuteAsync(string[] args, ILegacyParser parser, TextWriter? output = null)
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

        var report = await ParseAllFilesAsync(parser, options.InputPath, output, options.Verbose);

        if (!string.IsNullOrEmpty(options.OutputPath))
        {
            await WriteReportAsync(report, options.OutputPath);
            output.WriteLine($"Report written to: {options.OutputPath}");
        }

        output.WriteLine();
        PrintSummary(report, output);

        return report.HasErrors ? 1 : 0;
    }

    private static ImportParseOptions ParseOptions(string[] args)
    {
        var options = new ImportParseOptions();

        for (int i = 2; i < args.Length; i++) // Skip "import parse"
        {
            switch (args[i])
            {
                case InputOption:
                case InputShortOption:
                    if (i + 1 < args.Length)
                    {
                        options.InputPath = args[++i];
                    }
                    break;

                case OutputOption:
                case OutputShortOption:
                    if (i + 1 < args.Length)
                    {
                        options.OutputPath = args[++i];
                    }
                    break;

                case VerboseOption:
                case VerboseShortOption:
                    options.Verbose = true;
                    break;

                case "--help":
                case "-h":
                case "/?":
                    options.Help = true;
                    break;
            }
        }

        return options;
    }

    private static void PrintUsage(TextWriter output)
    {
        output.WriteLine();
        output.WriteLine("Usage: dotnet run import parse --input <path> [options]");
        output.WriteLine();
        output.WriteLine("Parses legacy XML/XSD files and validates against schemas.");
        output.WriteLine();
        output.WriteLine("Options:");
        output.WriteLine("  -i, --input <path>   Input directory containing legacy XML/XSD files (required)");
        output.WriteLine("  -o, --output <path>  Output file path for JSON report (optional)");
        output.WriteLine("  -v, --verbose        Show detailed parsing output");
        output.WriteLine("  -h, --help           Show this help message");
        output.WriteLine();
        output.WriteLine("Example:");
        output.WriteLine("  dotnet run --project apps/backend/src/Api import parse --input docs/legacy");
        output.WriteLine();
    }

    private static async Task<ImportParseReport> ParseAllFilesAsync(
        ILegacyParser parser,
        string inputPath,
        TextWriter output,
        bool verbose)
    {
        var report = new ImportParseReport
        {
            ExecutedAt = DateTime.UtcNow,
            InputPath = inputPath
        };

        // Define entity configurations with paths
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

            if (!File.Exists(xmlPath))
            {
                output.WriteLine($"[SKIP] {entityName}: XML file not found: {xmlPath}");
                report.SkippedEntities.Add(entityName);
                continue;
            }

            if (!File.Exists(xsdPath))
            {
                output.WriteLine($"[SKIP] {entityName}: XSD file not found: {xsdPath}");
                report.SkippedEntities.Add(entityName);
                continue;
            }

            if (verbose)
            {
                output.WriteLine($"[PARSING] {entityName}...");
            }

            var result = entityName switch
            {
                "School" => parser.ParseSchools(xmlPath, xsdPath) as object,
                "ClassGroup" => parser.ParseClassGroups(xmlPath, xsdPath),
                "Activity" => parser.ParseActivities(xmlPath, xsdPath),
                "Children" => parser.ParseChildren(xmlPath, xsdPath),
                _ => null
            };

            if (result != null)
            {
                var entityReport = CreateEntityReport(entityName, result);
                report.Entities.Add(entityReport);

                if (verbose)
                {
                    output.WriteLine($"  - Records: {entityReport.RecordCount}");
                    output.WriteLine($"  - Errors: {entityReport.ErrorCount}");
                }

                foreach (var error in entityReport.Errors)
                {
                    report.AllErrors.Add(error);
                }
            }
        }

        report.CompletedAt = DateTime.UtcNow;
        return report;
    }

    private static EntityParseReport CreateEntityReport(string entityName, object result)
    {
        return result switch
        {
            ParseResult<LegacySchoolRecord> schoolResult => new EntityParseReport
            {
                EntityName = entityName,
                RecordCount = schoolResult.Records.Count,
                Errors = schoolResult.Errors.ToList()
            },
            ParseResult<LegacyClassGroupRecord> classGroupResult => new EntityParseReport
            {
                EntityName = entityName,
                RecordCount = classGroupResult.Records.Count,
                Errors = classGroupResult.Errors.ToList()
            },
            ParseResult<LegacyActivityRecord> activityResult => new EntityParseReport
            {
                EntityName = entityName,
                RecordCount = activityResult.Records.Count,
                Errors = activityResult.Errors.ToList()
            },
            ParseResult<LegacyChildRecord> childResult => new EntityParseReport
            {
                EntityName = entityName,
                RecordCount = childResult.Records.Count,
                Errors = childResult.Errors.ToList()
            },
            _ => new EntityParseReport { EntityName = entityName }
        };
    }

    private static async Task WriteReportAsync(ImportParseReport report, string outputPath)
    {
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await File.WriteAllTextAsync(outputPath, json);
    }

    private static void PrintSummary(ImportParseReport report, TextWriter output)
    {
        output.WriteLine("=== Parse Summary ===");
        output.WriteLine($"Input Path: {report.InputPath}");
        output.WriteLine($"Executed At: {report.ExecutedAt:yyyy-MM-dd HH:mm:ss} UTC");
        output.WriteLine();

        foreach (var entity in report.Entities)
        {
            var status = entity.ErrorCount == 0 ? "[OK]" : "[ERRORS]";
            output.WriteLine($"{status} {entity.EntityName}: {entity.RecordCount} records, {entity.ErrorCount} errors");
        }

        foreach (var skipped in report.SkippedEntities)
        {
            output.WriteLine($"[SKIP] {skipped}: Files not found");
        }

        output.WriteLine();

        if (report.AllErrors.Any())
        {
            output.WriteLine($"Total Errors: {report.AllErrors.Count}");
            output.WriteLine();

            foreach (var error in report.AllErrors.Take(20))
            {
                output.WriteLine($"  {error}");
            }

            if (report.AllErrors.Count > 20)
            {
                output.WriteLine($"  ... and {report.AllErrors.Count - 20} more errors");
            }
        }
        else
        {
            output.WriteLine("All files parsed successfully with no errors.");
        }
    }

    private sealed class ImportParseOptions
    {
        public string? InputPath { get; set; }
        public string? OutputPath { get; set; }
        public bool Verbose { get; set; }
        public bool Help { get; set; }
    }
}

/// <summary>
/// Report containing results of parsing all legacy entity files.
/// </summary>
public sealed class ImportParseReport
{
    public DateTime ExecutedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string InputPath { get; set; } = string.Empty;
    public List<EntityParseReport> Entities { get; set; } = new();
    public List<string> SkippedEntities { get; set; } = new();
    public List<ParseError> AllErrors { get; set; } = new();
    public bool HasErrors => AllErrors.Any();
}

/// <summary>
/// Report for a single entity type's parse results.
/// </summary>
public sealed class EntityParseReport
{
    public string EntityName { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public int ErrorCount => Errors.Count;
    public List<ParseError> Errors { get; set; } = new();
}
