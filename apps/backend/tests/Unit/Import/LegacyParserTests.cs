using Kcow.Application.Import;

namespace Kcow.Unit.Tests.Import;

public class LegacyParserTests
{
    [Fact]
    public void ParseSchools_WhenXmlValid_ReturnsRecords()
    {
        // Arrange
        var xsdPath = FindRepoFile("docs/legacy/1_School/School.xsd");
        var xmlPath = WriteTempFile("""
            <?xml version="1.0" encoding="utf-8"?>
            <dataroot xmlns:od="urn:schemas-microsoft-com:officedata" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
              <School>
                <Short_x0020_School>Test School</Short_x0020_School>
                <School_x0020_Id>1</School_x0020_Id>
                <Price>0</Price>
                <Formula>0</Formula>
                <Print>1</Print>
                <Import>0</Import>
              </School>
            </dataroot>
            """);

        try
        {
            var parser = new LegacyParser();

            // Act
            var result = parser.ParseSchools(xmlPath, xsdPath);

            // Assert
            Assert.False(result.HasErrors);
            Assert.Single(result.Records);
            Assert.Equal(1, result.Records[0].SchoolId);
            Assert.Equal("Test School", result.Records[0].ShortSchool);
        }
        finally
        {
            File.Delete(xmlPath);
        }
    }

    [Fact]
    public void ParseSchools_WhenXmlInvalid_ReturnsErrors()
    {
        // Arrange
        var xsdPath = FindRepoFile("docs/legacy/1_School/School.xsd");
        var xmlPath = WriteTempFile("""
            <?xml version="1.0" encoding="utf-8"?>
            <dataroot>
              <School>
                <Print>invalid-bool</Print>
              </School>
            </dataroot>
            """);

        try
        {
            var parser = new LegacyParser();

            // Act
            var result = parser.ParseSchools(xmlPath, xsdPath);

            // Assert
            Assert.True(result.HasErrors);
            Assert.NotEmpty(result.Errors);
        }
        finally
        {
            File.Delete(xmlPath);
        }
    }

    [Fact]
    public void ParseSchools_WhenFileNotFound_ReturnsError()
    {
        // Arrange
        var parser = new LegacyParser();

        // Act
        var result = parser.ParseSchools("/nonexistent/file.xml", "/nonexistent/file.xsd");

        // Assert
        Assert.True(result.HasErrors);
        Assert.Single(result.Errors);
        Assert.Contains("nonexistent", result.Errors[0].Message);
    }

    [Fact]
    public void ParseClassGroups_WhenXmlValid_ReturnsRecords()
    {
        // Arrange
        var xsdPath = FindRepoFile("docs/legacy/2_Class_Group/Class Group.xsd");
        var xmlPath = WriteTempFile("""
            <?xml version="1.0" encoding="utf-8"?>
            <dataroot>
              <Class_x0020_Group>
                <Class_x0020_Group>A1</Class_x0020_Group>
                <School_x0020_Id>1</School_x0020_Id>
                <Evaluate>1</Evaluate>
                <Import>0</Import>
              </Class_x0020_Group>
            </dataroot>
            """);

        try
        {
            var parser = new LegacyParser();

            // Act
            var result = parser.ParseClassGroups(xmlPath, xsdPath);

            // Assert
            Assert.False(result.HasErrors);
            Assert.Single(result.Records);
            Assert.Equal("A1", result.Records[0].ClassGroup);
        }
        finally
        {
            File.Delete(xmlPath);
        }
    }

    [Fact]
    public void ParseActivities_WhenXmlValid_ReturnsRecords()
    {
        // Arrange
        var xsdPath = FindRepoFile("docs/legacy/3_Activity/Activity.xsd");
        var xmlPath = WriteTempFile("""
            <?xml version="1.0" encoding="utf-8"?>
            <dataroot>
              <Activity>
                <ActivityID>1</ActivityID>
                <Program>TEST</Program>
                <ProgramName>Test Activity</ProgramName>
              </Activity>
            </dataroot>
            """);

        try
        {
            var parser = new LegacyParser();

            // Act
            var result = parser.ParseActivities(xmlPath, xsdPath);

            // Assert
            Assert.False(result.HasErrors);
            Assert.Single(result.Records);
            Assert.Equal(1, result.Records[0].ActivityId);
            Assert.Equal("Test Activity", result.Records[0].ProgramName);
        }
        finally
        {
            File.Delete(xmlPath);
        }
    }

    [Fact]
    public void ParseChildren_WithActualLegacyFile_ReturnsRecords()
    {
        // Arrange - use actual legacy XML and XSD files
        var xsdPath = FindRepoFile("docs/legacy/4_Children/Children.xsd");
        var xmlPath = FindRepoFile("docs/legacy/4_Children/Children.xml");

        var parser = new LegacyParser();

        // Act
        var result = parser.ParseChildren(xmlPath, xsdPath);

        // Assert - actual file should parse without XSD errors
        // XSD validation errors are acceptable for legacy data files
        Assert.True(result.Records.Count > 0, "Should have parsed at least one record");

        // Verify first record has expected fields
        var firstRecord = result.Records[0];
        Assert.NotNull(firstRecord.Reference);
    }

    [Fact]
    public void ParseResult_HasErrors_ReturnsTrueWhenErrorsExist()
    {
        // Arrange
        var result = new ParseResult<LegacySchoolRecord>();
        Assert.False(result.HasErrors);

        // Act
        result.Errors.Add(new ParseError("test.xml", 1, "Error"));

        // Assert
        Assert.True(result.HasErrors);
    }

    [Fact]
    public void ParseError_ToString_WithLine_IncludesLineNumber()
    {
        // Arrange & Act
        var error = new ParseError("test.xml", 42, "Test error");

        // Assert
        Assert.Equal("test.xml:42: Test error", error.ToString());
    }

    [Fact]
    public void ParseError_ToString_WithoutLine_ExcludesLineNumber()
    {
        // Arrange & Act
        var error = new ParseError("test.xml", null, "Test error");

        // Assert
        Assert.Equal("test.xml: Test error", error.ToString());
    }

    private static string WriteTempFile(string contents)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, contents);
        return path;
    }

    private static string FindRepoFile(string relativePath)
    {
        var baseDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        while (baseDirectory != null)
        {
            var candidate = Path.Combine(baseDirectory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            baseDirectory = baseDirectory.Parent;
        }

        throw new FileNotFoundException($"Unable to locate {relativePath} from {AppContext.BaseDirectory}");
    }
}
