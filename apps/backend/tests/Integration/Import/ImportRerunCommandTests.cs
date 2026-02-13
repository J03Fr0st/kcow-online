using Kcow.Api.CliCommands;
using Kcow.Application.Import;

namespace Kcow.Integration.Tests.Import;

public class ImportRerunCommandTests
{
    [Fact]
    public async Task ExecuteAsync_WithHelp_ShowsConflictFlags()
    {
        var output = new StringWriter();
        var parser = new LegacyParser();

        var exitCode = await ImportRunCommand.ExecuteAsync(
            new[] { "import", "run", "--help" }, parser, output);

        Assert.Equal(0, exitCode);
        var text = output.ToString();
        Assert.Contains("--skip-existing", text);
        Assert.Contains("--update", text);
        Assert.Contains("--fail-on-conflict", text);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutImportService_FullImport_ReturnsError()
    {
        var tempDir = CreateEmptyTempDir();
        try
        {
            var output = new StringWriter();
            var parser = new LegacyParser();

            var exitCode = await ImportRunCommand.ExecuteAsync(
                new[] { "import", "run", "--input", tempDir }, parser, output);

            Assert.Equal(1, exitCode);
            Assert.Contains("Import execution service not available", output.ToString());
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithSkipExisting_PassesConflictMode()
    {
        var tempDir = CreateEmptyTempDir();
        try
        {
            var output = new StringWriter();
            var parser = new LegacyParser();
            var mockService = new MockImportExecutionService();

            var exitCode = await ImportRunCommand.ExecuteAsync(
                new[] { "import", "run", "--input", tempDir, "--skip-existing" },
                parser, output, mockService);

            Assert.Equal(0, exitCode);
            Assert.Equal(ConflictResolutionMode.SkipExisting, mockService.LastConflictMode);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithUpdate_PassesConflictMode()
    {
        var tempDir = CreateEmptyTempDir();
        try
        {
            var output = new StringWriter();
            var parser = new LegacyParser();
            var mockService = new MockImportExecutionService();

            var exitCode = await ImportRunCommand.ExecuteAsync(
                new[] { "import", "run", "--input", tempDir, "--update" },
                parser, output, mockService);

            Assert.Equal(0, exitCode);
            Assert.Equal(ConflictResolutionMode.Update, mockService.LastConflictMode);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DefaultConflictMode_IsFailOnConflict()
    {
        var tempDir = CreateEmptyTempDir();
        try
        {
            var output = new StringWriter();
            var parser = new LegacyParser();
            var mockService = new MockImportExecutionService();

            var exitCode = await ImportRunCommand.ExecuteAsync(
                new[] { "import", "run", "--input", tempDir },
                parser, output, mockService);

            Assert.Equal(0, exitCode);
            Assert.Equal(ConflictResolutionMode.FailOnConflict, mockService.LastConflictMode);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithUpdatedRecords_PrintsReimportSummary()
    {
        var tempDir = CreateEmptyTempDir();
        try
        {
            var output = new StringWriter();
            var parser = new LegacyParser();
            var mockService = new MockImportExecutionService
            {
                ResultOverride = new ImportExecutionResult
                {
                    Schools = new EntityImportResult { Imported = 2, Updated = 3, Skipped = 1 },
                    ConflictMode = ConflictResolutionMode.Update
                }
            };

            var exitCode = await ImportRunCommand.ExecuteAsync(
                new[] { "import", "run", "--input", tempDir, "--update" },
                parser, output, mockService);

            Assert.Equal(0, exitCode);
            var text = output.ToString();
            Assert.Contains("RE-IMPORT COMPLETE", text);
            Assert.Contains("New records: 2", text);
            Assert.Contains("Updated: 3", text);
            Assert.Contains("Skipped: 1", text);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    private static string CreateEmptyTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"import-rerun-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(dir);
        return dir;
    }

    private sealed class MockImportExecutionService : IImportExecutionService
    {
        public ConflictResolutionMode? LastConflictMode { get; private set; }
        public ImportExecutionResult? ResultOverride { get; set; }

        public Task<ImportExecutionResult> ExecuteAsync(string inputPath, CancellationToken cancellationToken = default)
        {
            return ExecuteAsync(inputPath, ConflictResolutionMode.FailOnConflict, cancellationToken);
        }

        public Task<ImportExecutionResult> ExecuteAsync(string inputPath, ConflictResolutionMode conflictMode, CancellationToken cancellationToken = default)
        {
            LastConflictMode = conflictMode;
            var result = ResultOverride ?? new ImportExecutionResult { InputPath = inputPath, ConflictMode = conflictMode };
            return Task.FromResult(result);
        }
    }
}
