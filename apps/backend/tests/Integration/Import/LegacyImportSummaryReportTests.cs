using Kcow.Application.Import;

namespace Kcow.Integration.Tests.Import;

public class LegacyImportSummaryReportTests
{
    [Fact]
    public void Render_IncludesCounts()
    {
        var report = new LegacyImportSummaryReport();
        var summary = new LegacyImportSummary(10, 2, 1, new DateTime(2026, 1, 6, 12, 0, 0, DateTimeKind.Utc));

        var output = report.Render(summary);

        Assert.Contains("Imported: 10", output);
        Assert.Contains("Skipped: 2", output);
        Assert.Contains("Errors: 1", output);
    }

    [Fact]
    public void WriteToFile_WritesSummaryToPath()
    {
        var report = new LegacyImportSummaryReport();
        var summary = new LegacyImportSummary(1, 0, 0, DateTime.UtcNow);
        var path = Path.GetTempFileName();

        try
        {
            report.WriteToFile(path, summary);

            var contents = File.ReadAllText(path);
            Assert.Contains("Legacy Import Summary", contents);
            Assert.Contains("Imported: 1", contents);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
