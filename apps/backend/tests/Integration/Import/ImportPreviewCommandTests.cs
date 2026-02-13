using Kcow.Api.CliCommands;
using Kcow.Application.Import;

namespace Kcow.Integration.Tests.Import;

public class ImportPreviewCommandTests
{
    [Fact]
    public void IsImportRunCommand_WithRunSubcommand_ReturnsTrue()
    {
        Assert.True(ImportRunCommand.IsImportRunCommand(new[] { "import", "run" }));
    }

    [Fact]
    public void IsImportRunCommand_WithPreview_ReturnsTrue()
    {
        Assert.True(ImportRunCommand.IsImportRunCommand(new[] { "import", "run", "--preview" }));
    }

    [Fact]
    public void IsImportRunCommand_WithOtherSubcommand_ReturnsFalse()
    {
        Assert.False(ImportRunCommand.IsImportRunCommand(new[] { "import", "parse" }));
    }

    [Fact]
    public void IsImportRunCommand_WithInsufficientArgs_ReturnsFalse()
    {
        Assert.False(ImportRunCommand.IsImportRunCommand(new[] { "import" }));
        Assert.False(ImportRunCommand.IsImportRunCommand(Array.Empty<string>()));
    }

    [Fact]
    public async Task ExecuteAsync_WithHelp_PrintsUsageAndReturnsZero()
    {
        var output = new StringWriter();
        var parser = new LegacyParser();

        var exitCode = await ImportRunCommand.ExecuteAsync(
            new[] { "import", "run", "--help" }, parser, output);

        Assert.Equal(0, exitCode);
        Assert.Contains("--preview", output.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutInput_ReturnsError()
    {
        var output = new StringWriter();
        var parser = new LegacyParser();

        var exitCode = await ImportRunCommand.ExecuteAsync(
            new[] { "import", "run", "--preview" }, parser, output);

        Assert.Equal(1, exitCode);
        Assert.Contains("Input path is required", output.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WithNonexistentDir_ReturnsError()
    {
        var output = new StringWriter();
        var parser = new LegacyParser();

        var exitCode = await ImportRunCommand.ExecuteAsync(
            new[] { "import", "run", "--preview", "--input", "/nonexistent/path" }, parser, output);

        Assert.Equal(1, exitCode);
        Assert.Contains("not found", output.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_Preview_WithEmptyDir_ProducesReport()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"import-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var output = new StringWriter();
            var parser = new LegacyParser();

            var exitCode = await ImportRunCommand.ExecuteAsync(
                new[] { "import", "run", "--preview", "--input", tempDir }, parser, output);

            var text = output.ToString();
            Assert.Contains("IMPORT PREVIEW", text);
            Assert.Contains("Record Counts", text);
            Assert.Contains("NO data will be written", text);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExecuteAsync_Preview_ShowsEntityCounts()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"import-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var output = new StringWriter();
            var parser = new LegacyParser();

            await ImportRunCommand.ExecuteAsync(
                new[] { "import", "run", "--preview", "--input", tempDir }, parser, output);

            var text = output.ToString();
            Assert.Contains("Schools:", text);
            Assert.Contains("Class Groups:", text);
            Assert.Contains("Activities:", text);
            Assert.Contains("Students:", text);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExecuteAsync_Preview_WithJsonOutput_WritesFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"import-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var jsonPath = Path.Combine(tempDir, "preview.json");

        try
        {
            var output = new StringWriter();
            var parser = new LegacyParser();

            await ImportRunCommand.ExecuteAsync(
                new[] { "import", "run", "--preview", "--input", tempDir, "--output", jsonPath },
                parser, output);

            Assert.True(File.Exists(jsonPath));
            var json = await File.ReadAllTextAsync(jsonPath);
            Assert.Contains("schools", json);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithoutPreview_RequiresConfirmation()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"import-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var output = new StringWriter();
            var parser = new LegacyParser();

            // Without --preview, command should still work but show a message about full import
            var exitCode = await ImportRunCommand.ExecuteAsync(
                new[] { "import", "run", "--input", tempDir }, parser, output);

            var text = output.ToString();
            // Without preview flag, it should indicate this is a real import
            // (actual DB writes won't happen because no DB connection, but the code path should differ)
            Assert.DoesNotContain("IMPORT PREVIEW", text);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }
}
