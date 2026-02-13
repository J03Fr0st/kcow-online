using Kcow.Api.CliCommands;
using Kcow.Application.Import;

namespace Kcow.Integration.Tests.Import;

public class ImportParseCommandTests
{
    [Fact]
    public void IsImportParseCommand_WithValidArgs_ReturnsTrue()
    {
        // Arrange
        var args = new[] { "import", "parse", "--input", "docs/legacy" };

        // Act
        var result = ImportParseCommand.IsImportParseCommand(args);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsImportParseCommand_WithUpperCaseArgs_ReturnsTrue()
    {
        // Arrange
        var args = new[] { "IMPORT", "PARSE", "--input", "docs/legacy" };

        // Act
        var result = ImportParseCommand.IsImportParseCommand(args);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsImportParseCommand_WithWrongCommand_ReturnsFalse()
    {
        // Arrange
        var args = new[] { "run", "parse", "--input", "docs/legacy" };

        // Act
        var result = ImportParseCommand.IsImportParseCommand(args);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsImportParseCommand_WithWrongSubCommand_ReturnsFalse()
    {
        // Arrange
        var args = new[] { "import", "execute", "--input", "docs/legacy" };

        // Act
        var result = ImportParseCommand.IsImportParseCommand(args);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsImportParseCommand_WithTooFewArgs_ReturnsFalse()
    {
        // Arrange
        var args = new[] { "import" };

        // Act
        var result = ImportParseCommand.IsImportParseCommand(args);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithHelp_ReturnsZero()
    {
        // Arrange
        var args = new[] { "import", "parse", "--help" };
        var parser = new MockLegacyParser();
        var output = new StringWriter();

        // Act
        var result = await ImportParseCommand.ExecuteAsync(args, parser, output);

        // Assert
        Assert.Equal(0, result);
        Assert.Contains("Usage:", output.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutInput_ReturnsOne()
    {
        // Arrange
        var args = new[] { "import", "parse" };
        var parser = new MockLegacyParser();
        var output = new StringWriter();

        // Act
        var result = await ImportParseCommand.ExecuteAsync(args, parser, output);

        // Assert
        Assert.Equal(1, result);
        Assert.Contains("Input path is required", output.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WithNonexistentInput_ReturnsOne()
    {
        // Arrange
        var args = new[] { "import", "parse", "--input", "/nonexistent/path" };
        var parser = new MockLegacyParser();
        var output = new StringWriter();

        // Act
        var result = await ImportParseCommand.ExecuteAsync(args, parser, output);

        // Assert
        Assert.Equal(1, result);
        Assert.Contains("Input directory not found", output.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WithValidInput_ReturnsZeroAndPrintsSummary()
    {
        // Arrange
        var tempDir = CreateTempLegacyDirectory();
        var args = new[] { "import", "parse", "--input", tempDir, "-v" };
        var parser = new MockLegacyParser();
        var output = new StringWriter();

        try
        {
            // Act
            var result = await ImportParseCommand.ExecuteAsync(args, parser, output);

            // Assert
            Assert.Equal(0, result);
            var outputStr = output.ToString();
            Assert.Contains("Parse Summary", outputStr);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithShortInputOption_ReturnsZero()
    {
        // Arrange
        var tempDir = CreateTempLegacyDirectory();
        var args = new[] { "import", "parse", "-i", tempDir };
        var parser = new MockLegacyParser();
        var output = new StringWriter();

        try
        {
            // Act
            var result = await ImportParseCommand.ExecuteAsync(args, parser, output);

            // Assert
            Assert.Equal(0, result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithOutputOption_WritesReport()
    {
        // Arrange
        var tempDir = CreateTempLegacyDirectory();
        var tempOutput = Path.GetTempFileName();
        File.Delete(tempOutput);
        var args = new[] { "import", "parse", "--input", tempDir, "--output", tempOutput };
        var parser = new MockLegacyParser();
        var output = new StringWriter();

        try
        {
            // Act
            var result = await ImportParseCommand.ExecuteAsync(args, parser, output);

            // Assert
            Assert.Equal(0, result);
            Assert.True(File.Exists(tempOutput));
            var content = await File.ReadAllTextAsync(tempOutput);
            Assert.Contains("executedAt", content);
        }
        finally
        {
            Directory.Delete(tempDir, true);
            if (File.Exists(tempOutput))
            {
                File.Delete(tempOutput);
            }
        }
    }

    private static string CreateTempLegacyDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"legacy_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        // Create minimal folder structure
        var folders = new[] { "1_School", "2_Class_Group", "3_Activity", "4_Children" };
        foreach (var folder in folders)
        {
            var folderPath = Path.Combine(tempDir, folder);
            Directory.CreateDirectory(folderPath);

            // Create minimal XML files
            var xmlFileName = folder switch
            {
                "1_School" => "School.xml",
                "2_Class_Group" => "Class Group.xml",
                "3_Activity" => "Activity.xml",
                "4_Children" => "Children.xml",
                _ => "data.xml"
            };
            File.WriteAllText(Path.Combine(folderPath, xmlFileName), """<?xml version="1.0"?><dataroot></dataroot>""");

            // Create minimal XSD files
            var xsdFileName = folder switch
            {
                "1_School" => "School.xsd",
                "2_Class_Group" => "Class Group.xsd",
                "3_Activity" => "Activity.xsd",
                "4_Children" => "Children.xsd",
                _ => "schema.xsd"
            };
            File.WriteAllText(Path.Combine(folderPath, xsdFileName), """<?xml version="1.0"?><xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema"></xsd:schema>""");
        }

        return tempDir;
    }

    /// <summary>
    /// Mock parser for CLI command tests.
    /// </summary>
    private sealed class MockLegacyParser : ILegacyParser
    {
        public ParseResult<LegacySchoolRecord> ParseSchools(string xmlPath, string xsdPath)
            => new() { Records = { CreateMockSchool() } };

        public ParseResult<LegacyClassGroupRecord> ParseClassGroups(string xmlPath, string xsdPath)
            => new() { Records = { CreateMockClassGroup() } };

        public ParseResult<LegacyActivityRecord> ParseActivities(string xmlPath, string xsdPath)
            => new() { Records = { CreateMockActivity() } };

        public ParseResult<LegacyChildRecord> ParseChildren(string xmlPath, string xsdPath)
            => new() { Records = { CreateMockChild() } };

        private static LegacySchoolRecord CreateMockSchool()
            => new(
                SchoolId: 1,
                ShortSchool: "Test",
                Trok: null,
                Price: null,
                FormulaDescription: null,
                Formula: null,
                Sequence: null,
                Day: null,
                SchoolDescription: null,
                ContactPerson: null,
                EmailAddress: null,
                ContactCell: null,
                Telephone: null,
                Fax: null,
                Address1: null,
                Address2: null,
                Headmaster: null,
                HeadmasterCell: null,
                MoneyMessage: null,
                Print: false,
                Taal: null,
                Import: false,
                WebPage: null,
                Naskool1Name: null,
                Naskool1Contact: null,
                Naskool2Name: null,
                Naskool2Contact: null,
                Kluis: null,
                Omsendbriewe: null,
                KcowWebPageLink: null);

        private static LegacyClassGroupRecord CreateMockClassGroup()
            => new(
                ClassGroup: "A1",
                DayTruck: null,
                Description: null,
                EndTime: null,
                SchoolId: 1,
                DayId: null,
                StartTime: null,
                Evaluate: false,
                Note: null,
                Import: false,
                Sequence: null,
                GroupMessage: null,
                SendCertificates: null,
                MoneyMessage: null,
                IXL: null);

        private static LegacyActivityRecord CreateMockActivity()
            => new(
                ActivityId: 1,
                Program: "TEST",
                ProgramName: "Test Activity",
                EducationalFocus: null,
                Folder: null,
                Grade: null,
                Icon: null);

        private static LegacyChildRecord CreateMockChild()
            => new(
                Reference: "REF001",
                ChildName: "John",
                ChildSurname: "Doe",
                ChildBirthdate: null,
                Sex: null,
                Language: null,
                AccountPersonName: null,
                AccountPersonSurname: null,
                AccountPersonIdnumber: null,
                AccountPersonCellphone: null,
                AccountPersonOffice: null,
                AccountPersonHome: null,
                AccountPersonEmail: null,
                Relation: null,
                MotherName: null,
                MotherSurname: null,
                MotherOffice: null,
                MotherCell: null,
                MotherHome: null,
                MotherEmail: null,
                FatherName: null,
                FatherSurname: null,
                FatherOffice: null,
                FatherCell: null,
                FatherHome: null,
                FatherEmail: null,
                Address1: null,
                Address2: null,
                Code: null,
                SchoolName: null,
                ClassGroup: null,
                Grade: null,
                Teacher: null,
                AttendingKcowAt: null,
                Aftercare: null,
                Extra: null,
                HomeTime: null,
                StartClasses: null,
                Terms: null,
                Seat: null,
                Truck: null,
                Family: null,
                Sequence: null,
                FinancialCode: null,
                Charge: null,
                Deposit: null,
                PayDate: null,
                TshirtCode: null,
                TshirtMoney1: null,
                TshirtMoneyDate1: null,
                TshirtReceived1: null,
                TshirtRecDate1: null,
                ReceiveNote1: null,
                TshirtSize1: null,
                TshirtColor1: null,
                TshirtDesign1: null,
                TshirtSize2: null,
                TshirtMoney2: null,
                TshirtMoneyDate2: null,
                TshirtReceived2: null,
                TshirtRecDate2: null,
                ReceiveNote2: null,
                TshirtColor2: null,
                TshirtDesign2: null,
                Indicator1: null,
                Indicator2: null,
                GeneralNote: null,
                PrintIdCard: null,
                AcceptTermsCond: null,
                Status: null,
                SmsOrEmail: null,
                SchoolClose: null,
                Cnt: null,
                OnlineEntry: null,
                Created: null,
                Submitted: null,
                Updated: null,
                BookEmail: null,
                Report1GivenOut: null,
                AccountGivenOut: null,
                CertificatePrinted: null,
                Report2GivenOut: null,
                Social: null,
                ActivityReportGivenOut: null,
                Photo: null,
                PhotoUpdated: null);
    }
}
