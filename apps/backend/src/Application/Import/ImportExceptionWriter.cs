using System.Text.Json;

namespace Kcow.Application.Import;

/// <summary>
/// Writes import exceptions to a JSON file for review.
/// </summary>
public static class ImportExceptionWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Writes the import result exceptions to a JSON file.
    /// </summary>
    public static async Task WriteAsync(ImportExecutionResult result, string outputPath)
    {
        var report = new
        {
            importRun = new
            {
                timestamp = result.ExecutedAt,
                inputPath = result.InputPath
            },
            summary = new
            {
                totalImported = result.TotalImported,
                totalFailed = result.TotalFailed,
                totalSkipped = result.TotalSkipped,
                successRate = result.SuccessRate
            },
            exceptions = result.Exceptions
        };

        var json = JsonSerializer.Serialize(report, JsonOptions);

        var dir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await File.WriteAllTextAsync(outputPath, json);
    }

    /// <summary>
    /// Generates a default exception file path based on current date.
    /// </summary>
    public static string GetDefaultPath(string basePath)
    {
        return Path.Combine(basePath, $"import-exceptions-{DateTime.UtcNow:yyyy-MM-dd}.json");
    }
}
