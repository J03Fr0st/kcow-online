using Kcow.Application.Import;

namespace Kcow.Unit.Tests.Import;

public class ImportExecutionResultTests
{
    [Fact]
    public void TotalImported_SumsAllEntities()
    {
        var result = new ImportExecutionResult
        {
            Schools = new EntityImportResult { Imported = 10 },
            ClassGroups = new EntityImportResult { Imported = 20 },
            Activities = new EntityImportResult { Imported = 5 },
            Students = new EntityImportResult { Imported = 100 }
        };

        Assert.Equal(135, result.TotalImported);
    }

    [Fact]
    public void TotalFailed_SumsAllEntities()
    {
        var result = new ImportExecutionResult
        {
            Schools = new EntityImportResult { Failed = 1 },
            ClassGroups = new EntityImportResult { Failed = 2 },
            Activities = new EntityImportResult { Failed = 0 },
            Students = new EntityImportResult { Failed = 3 }
        };

        Assert.Equal(6, result.TotalFailed);
    }

    [Fact]
    public void SuccessRate_CalculatesCorrectly()
    {
        var result = new ImportExecutionResult
        {
            Schools = new EntityImportResult { Imported = 90 },
            Students = new EntityImportResult { Imported = 7, Failed = 3 }
        };

        Assert.Equal(97.0, result.SuccessRate);
    }

    [Fact]
    public void SuccessRate_ZeroProcessed_ReturnsZero()
    {
        var result = new ImportExecutionResult();
        Assert.Equal(0, result.SuccessRate);
    }

    [Fact]
    public void HasExceptions_TrueWhenExceptionsExist()
    {
        var result = new ImportExecutionResult();
        Assert.False(result.HasExceptions);

        result.Exceptions.Add(new ImportException("Student", "123", "Name", "Missing"));
        Assert.True(result.HasExceptions);
    }

    [Fact]
    public void ImportException_StoresAllFields()
    {
        var ex = new ImportException("Student", "123", "FirstName", "Required field is empty", "");

        Assert.Equal("Student", ex.EntityType);
        Assert.Equal("123", ex.LegacyId);
        Assert.Equal("FirstName", ex.Field);
        Assert.Equal("Required field is empty", ex.Reason);
        Assert.Equal("", ex.OriginalValue);
    }

    [Fact]
    public async Task ImportExceptionWriter_WritesValidJson()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"import-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var filePath = Path.Combine(tempDir, "exceptions.json");

        try
        {
            var result = new ImportExecutionResult
            {
                InputPath = "/test/path",
                Schools = new EntityImportResult { Imported = 10, Failed = 1 },
                Exceptions = { new ImportException("School", "5", "Name", "Empty name") }
            };

            await ImportExceptionWriter.WriteAsync(result, filePath);

            Assert.True(File.Exists(filePath));
            var json = await File.ReadAllTextAsync(filePath);
            Assert.Contains("importRun", json);
            Assert.Contains("exceptions", json);
            Assert.Contains("Empty name", json);
            Assert.Contains("totalImported", json);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void ImportExceptionWriter_GetDefaultPath_IncludesDate()
    {
        var path = ImportExceptionWriter.GetDefaultPath("/output");
        Assert.Contains("import-exceptions-", path);
        Assert.EndsWith(".json", path);
    }
}
