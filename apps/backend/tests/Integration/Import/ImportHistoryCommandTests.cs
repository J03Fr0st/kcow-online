using Kcow.Api.CliCommands;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using NSubstitute;

namespace Kcow.Integration.Tests.Import;

public class ImportHistoryCommandTests
{
    [Fact]
    public void IsImportHistoryCommand_WithHistory_ReturnsTrue()
    {
        Assert.True(ImportHistoryCommand.IsImportHistoryCommand(new[] { "import", "history" }));
    }

    [Fact]
    public void IsImportHistoryCommand_WithOther_ReturnsFalse()
    {
        Assert.False(ImportHistoryCommand.IsImportHistoryCommand(new[] { "import", "run" }));
    }

    [Fact]
    public async Task ExecuteAsync_NoHistory_ShowsEmptyMessage()
    {
        var repo = Substitute.For<IImportAuditLogRepository>();
        repo.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<ImportAuditLog>());

        var output = new StringWriter();
        var exitCode = await ImportHistoryCommand.ExecuteAsync(
            new[] { "import", "history" }, repo, output);

        Assert.Equal(0, exitCode);
        Assert.Contains("No import runs found", output.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WithHistory_ShowsTable()
    {
        var repo = Substitute.For<IImportAuditLogRepository>();
        repo.GetRecentAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<ImportAuditLog>
            {
                new()
                {
                    Id = 1,
                    StartedAt = new DateTime(2026, 1, 15),
                    Status = "Completed",
                    SchoolsCreated = 10,
                    StudentsCreated = 100,
                    TotalFailed = 0
                },
                new()
                {
                    Id = 2,
                    StartedAt = new DateTime(2026, 1, 16),
                    Status = "CompletedWithErrors",
                    SchoolsCreated = 10,
                    StudentsCreated = 95,
                    TotalFailed = 5
                }
            });

        var output = new StringWriter();
        var exitCode = await ImportHistoryCommand.ExecuteAsync(
            new[] { "import", "history" }, repo, output);

        Assert.Equal(0, exitCode);
        var text = output.ToString();
        Assert.Contains("IMPORT HISTORY", text);
        Assert.Contains("Completed", text);
        Assert.Contains("CompletedWithErrors", text);
    }

    [Fact]
    public async Task ExecuteAsync_WithHelp_ShowsUsage()
    {
        var repo = Substitute.For<IImportAuditLogRepository>();
        var output = new StringWriter();

        var exitCode = await ImportHistoryCommand.ExecuteAsync(
            new[] { "import", "history", "--help" }, repo, output);

        Assert.Equal(0, exitCode);
        Assert.Contains("--count", output.ToString());
    }
}
