using Kcow.Application.Import.Mappers;

namespace Kcow.Unit.Tests.Import;

public class DataMapperInterfaceTests
{
    [Fact]
    public void MappingResult_Success_HasDataAndNoErrors()
    {
        var result = MappingResult<string>.Ok("test");

        Assert.True(result.Success);
        Assert.Equal("test", result.Data);
        Assert.Empty(result.Warnings);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void MappingResult_WithWarning_StillSucceeds()
    {
        var result = MappingResult<string>.Ok("test");
        result.Warnings.Add(new MappingWarning("Field", "Message"));

        Assert.True(result.Success);
        Assert.Single(result.Warnings);
        Assert.Equal("Field", result.Warnings[0].Field);
        Assert.Equal("Message", result.Warnings[0].Message);
    }

    [Fact]
    public void MappingResult_WithError_IsNotSuccess()
    {
        var result = MappingResult<string>.Fail("Field", "Error message");

        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Single(result.Errors);
        Assert.Equal("Field", result.Errors[0].Field);
        Assert.Equal("Error message", result.Errors[0].Message);
    }

    [Fact]
    public void MappingResult_Skipped_HasNoDataAndNoErrors()
    {
        var result = MappingResult<string>.Skipped("Reason");

        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Empty(result.Errors);
        Assert.Single(result.Warnings);
        Assert.Contains("Reason", result.Warnings[0].Message);
    }

    [Fact]
    public void MappingWarning_RecordsOriginalAndMappedValues()
    {
        var warning = new MappingWarning("Gender", "Unknown gender value", "X", null);

        Assert.Equal("Gender", warning.Field);
        Assert.Equal("Unknown gender value", warning.Message);
        Assert.Equal("X", warning.OriginalValue);
        Assert.Null(warning.MappedValue);
    }

    [Fact]
    public void MappingError_RecordsFieldAndMessage()
    {
        var error = new MappingError("Reference", "Required field is missing");

        Assert.Equal("Reference", error.Field);
        Assert.Equal("Required field is missing", error.Message);
    }

    [Fact]
    public void MappingResult_HasWarnings_ReturnsTrueWhenWarningsExist()
    {
        var result = MappingResult<string>.Ok("test");
        Assert.False(result.HasWarnings);

        result.Warnings.Add(new MappingWarning("Field", "Warning"));
        Assert.True(result.HasWarnings);
    }

    [Fact]
    public void MappingResult_HasErrors_ReturnsTrueWhenErrorsExist()
    {
        var result = MappingResult<string>.Ok("test");
        Assert.False(result.HasErrors);

        var failed = MappingResult<string>.Fail("Field", "Error");
        Assert.True(failed.HasErrors);
    }
}
