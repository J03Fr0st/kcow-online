using Kcow.Application.Import;

namespace Kcow.Integration.Tests.Import;

public class LegacyAttendanceEvaluationXmlParserTests
{
    private readonly LegacyAttendanceEvaluationXmlParser _parser = new();

    [Fact]
    public void Parse_ValidActivityXml_ReturnsActivityRecords()
    {
        var xmlPath = WriteTempFile("""
            <?xml version="1.0" encoding="utf-8"?>
            <dataroot>
              <Activity>
                <ActivityID>1</ActivityID>
                <Program>test-prog</Program>
                <ProgramName>Test Program</ProgramName>
                <Folder>W03</Folder>
                <Grade>Grade R</Grade>
              </Activity>
              <Activity>
                <ActivityID>2</ActivityID>
                <Program>prog2</Program>
                <ProgramName>Program 2</ProgramName>
              </Activity>
            </dataroot>
            """);
        var xsdPath = FindRepoFile("docs/legacy/3_Activity/Activity.xsd");

        try
        {
            var result = _parser.Parse(xmlPath, xsdPath);

            Assert.Equal(2, result.ActivityRecords.Count);
            Assert.Equal(1, result.ActivityRecords[0].ActivityId);
            Assert.Equal("test-prog", result.ActivityRecords[0].Program);
            Assert.Equal("Test Program", result.ActivityRecords[0].ProgramName);
            Assert.Equal("W03", result.ActivityRecords[0].Folder);
            Assert.Equal("Grade R", result.ActivityRecords[0].Grade);
            Assert.Equal(2, result.ActivityRecords[1].ActivityId);
        }
        finally
        {
            File.Delete(xmlPath);
        }
    }

    [Fact]
    public void Parse_EmptyXml_ReturnsEmptyRecords()
    {
        var xmlPath = WriteTempFile("""
            <?xml version="1.0" encoding="utf-8"?>
            <dataroot>
            </dataroot>
            """);
        var xsdPath = FindRepoFile("docs/legacy/3_Activity/Activity.xsd");

        try
        {
            var result = _parser.Parse(xmlPath, xsdPath);

            Assert.Empty(result.ActivityRecords);
        }
        finally
        {
            File.Delete(xmlPath);
        }
    }

    [Fact]
    public void Parse_InvalidXml_ReturnsValidationErrors()
    {
        var xmlPath = WriteTempFile("not valid xml");
        var xsdPath = FindRepoFile("docs/legacy/3_Activity/Activity.xsd");

        try
        {
            var result = _parser.Parse(xmlPath, xsdPath);

            Assert.Empty(result.ActivityRecords);
            Assert.NotEmpty(result.ValidationErrors);
        }
        finally
        {
            File.Delete(xmlPath);
        }
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("2024-01-15T00:00:00", "2024-01-15")]
    [InlineData("2024-01-15", "2024-01-15")]
    [InlineData("01/15/2024", "2024-01-15")]
    [InlineData("2024-01-15 10:30:00", "2024-01-15")]
    public void ParseDateToIso_HandlesVariousFormats(string? input, string? expected)
    {
        var result = LegacyAttendanceEvaluationXmlParser.ParseDateToIso(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ParseDateToIso_InvalidDate_ReturnsNull()
    {
        var result = LegacyAttendanceEvaluationXmlParser.ParseDateToIso("not-a-date");
        Assert.Null(result);
    }

    [Fact]
    public void Parse_NullXmlPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _parser.Parse(null!, "some.xsd"));
    }

    [Fact]
    public void Parse_NullXsdPath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _parser.Parse("some.xml", null!));
    }

    [Fact]
    public void Parse_AllSevenFields_ExtractsCorrectly()
    {
        var xmlPath = WriteTempFile("""
            <?xml version="1.0" encoding="utf-8"?>
            <dataroot>
              <Activity>
                <ActivityID>42</ActivityID>
                <Program>play&amp;learn35</Program>
                <ProgramName>DK Play and learn 3-5</ProgramName>
                <Educational_x0020_Focus>Test educational focus text</Educational_x0020_Focus>
                <Folder>W03</Folder>
                <Grade>Grade R</Grade>
                <Icon>dGVzdA==</Icon>
              </Activity>
            </dataroot>
            """);
        var xsdPath = FindRepoFile("docs/legacy/3_Activity/Activity.xsd");

        try
        {
            var result = _parser.Parse(xmlPath, xsdPath);

            Assert.Single(result.ActivityRecords);
            var record = result.ActivityRecords[0];
            Assert.Equal(42, record.ActivityId);
            Assert.Equal("play&learn35", record.Program);
            Assert.Equal("DK Play and learn 3-5", record.ProgramName);
            Assert.Equal("Test educational focus text", record.EducationalFocus);
            Assert.Equal("W03", record.Folder);
            Assert.Equal("Grade R", record.Grade);
            Assert.Equal("dGVzdA==", record.Icon);
        }
        finally
        {
            File.Delete(xmlPath);
        }
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
