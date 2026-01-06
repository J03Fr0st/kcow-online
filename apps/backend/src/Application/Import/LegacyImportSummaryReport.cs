namespace Kcow.Application.Import;

public sealed record LegacyImportSummary(int ImportedCount, int SkippedCount, int ErrorCount, DateTime CompletedAt);

public sealed class LegacyImportSummaryReport
{
    public string Render(LegacyImportSummary summary)
    {
        return $"""
            Legacy Import Summary
            Completed: {summary.CompletedAt:O}
            Imported: {summary.ImportedCount}
            Skipped: {summary.SkippedCount}
            Errors: {summary.ErrorCount}
            """;
    }

    public void WriteToFile(string outputPath, LegacyImportSummary summary)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Output path is required.", nameof(outputPath));
        }

        var contents = Render(summary);
        File.WriteAllText(outputPath, contents);
    }
}
