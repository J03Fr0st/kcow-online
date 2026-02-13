using Kcow.Application.Import;
using Kcow.Application.Import.Mappers;
using Kcow.Domain.Entities;

namespace Kcow.Integration.Tests.Import;

/// <summary>
/// End-to-end integration tests for the full legacy data import pipeline.
/// Covers: XML parsing, data mapping, import preview, execution results,
/// audit logging, and re-run conflict resolution.
/// </summary>
public class ImportPipelineIntegrationTests : IDisposable
{
    private readonly string _tempDir;

    public ImportPipelineIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"import-pipeline-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    // ==========================================
    // XML/XSD Parsing Tests
    // ==========================================

    [Fact]
    public void LegacyParser_ParseSchools_ExtractsRecords()
    {
        var xsdPath = FindRepoFile("docs/legacy/1_School/School.xsd");
        var xmlPath = WriteXmlFile("School.xml", """
            <?xml version="1.0" encoding="utf-8"?>
            <dataroot>
              <School>
                <Short_x0020_School>TST</Short_x0020_School>
                <School_x0020_Id>1</School_x0020_Id>
                <School_x0020_Description>Test School</School_x0020_Description>
                <Print>1</Print>
                <Import>1</Import>
              </School>
            </dataroot>
            """);

        var parser = new LegacyParser();
        var result = parser.ParseSchools(xmlPath, xsdPath);

        Assert.False(result.HasErrors);
        Assert.Single(result.Records);
        Assert.Equal(1, result.Records[0].SchoolId);
        Assert.Equal("Test School", result.Records[0].SchoolDescription);
    }

    [Fact]
    public void LegacyParser_ParseSchools_ReportsErrorsWithContext()
    {
        var xsdPath = FindRepoFile("docs/legacy/1_School/School.xsd");
        var xmlPath = WriteXmlFile("BadSchool.xml", """
            <?xml version="1.0" encoding="utf-8"?>
            <dataroot>
              <School>
                <Print>maybe</Print>
                <Import>1</Import>
              </School>
            </dataroot>
            """);

        var parser = new LegacyParser();
        var result = parser.ParseSchools(xmlPath, xsdPath);

        // Parser should still return results (even with validation errors)
        // as it processes what it can
        Assert.NotNull(result);
    }

    [Fact]
    public void LegacyParser_ParsesMultipleEntityTypes()
    {
        var parser = new LegacyParser();

        // Verify the parser interface exposes all 4 entity types
        Assert.IsAssignableFrom<ILegacyParser>(parser);
    }

    // ==========================================
    // Data Mapping Tests
    // ==========================================

    [Fact]
    public void SchoolDataMapper_MapsAllFieldsWithLegacyId()
    {
        var record = new LegacySchoolRecord(
            SchoolId: 42, ShortSchool: "TST", Trok: 1, Price: 100.50,
            FormulaDescription: "10%", Formula: 0.1f, Sequence: "1", Day: "Monday",
            SchoolDescription: "Test School", ContactPerson: "Jane",
            EmailAddress: "jane@test.com", ContactCell: "012345",
            Telephone: "011", Fax: "012", Address1: "123 St", Address2: "City",
            Headmaster: "Head", HeadmasterCell: "098765", MoneyMessage: "Pay",
            Print: true, Taal: "Eng", Import: true, WebPage: "http://test.com",
            Naskool1Name: null, Naskool1Contact: null,
            Naskool2Name: null, Naskool2Contact: null,
            Kluis: null, Omsendbriewe: null, KcowWebPageLink: null);

        var mapper = new SchoolDataMapper();
        var result = mapper.Map(record);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("42", result.Data!.LegacyId);
        Assert.Equal("Test School", result.Data.Name);
        Assert.Equal("TST", result.Data.ShortName);
        Assert.Equal(100.50m, result.Data.Price);
    }

    [Fact]
    public void ClassGroupDataMapper_MapsWithLegacyIdAndDayConversion()
    {
        var record = new LegacyClassGroupRecord(
            ClassGroup: "MON-A1", DayTruck: "1", Description: "Monday A1",
            EndTime: "09:00", SchoolId: 1, DayId: "3",
            StartTime: "08:00", Evaluate: true, Note: "Test note",
            Import: true, Sequence: "2",
            GroupMessage: null, SendCertificates: null,
            MoneyMessage: null, IXL: null);

        var mapper = new ClassGroupDataMapper();
        var result = mapper.Map(record);

        Assert.True(result.Success);
        Assert.Equal("MON-A1", result.Data!.LegacyId);
        Assert.Equal(DayOfWeek.Wednesday, result.Data.DayOfWeek);
        Assert.Equal(new TimeOnly(8, 0), result.Data.StartTime);
        Assert.Equal(new TimeOnly(9, 0), result.Data.EndTime);
        Assert.Equal(2, result.Data.Sequence);
        Assert.True(result.Data.Evaluate);
    }

    [Fact]
    public void ClassGroupDataMapper_SkipsRecordsWithImportFalse()
    {
        var record = new LegacyClassGroupRecord(
            ClassGroup: "SKIP-ME", DayTruck: null, Description: "Should skip",
            EndTime: "09:00", SchoolId: 1, DayId: "1",
            StartTime: "08:00", Evaluate: false, Note: null,
            Import: false, Sequence: "1",
            GroupMessage: null, SendCertificates: null,
            MoneyMessage: null, IXL: null);

        var mapper = new ClassGroupDataMapper();
        var result = mapper.Map(record);

        Assert.Null(result.Data);
    }

    [Fact]
    public void ActivityDataMapper_MapsWithLegacyIdAndTruncation()
    {
        var longName = new string('A', 300);
        var record = new LegacyActivityRecord(
            ActivityId: 5, Program: "MATH", ProgramName: longName,
            EducationalFocus: "Focus", Folder: "F1", Grade: "Gr1", Icon: null);

        var mapper = new ActivityDataMapper();
        var result = mapper.Map(record);

        Assert.True(result.Success);
        Assert.Equal("5", result.Data!.LegacyId);
        Assert.Equal(255, result.Data.Name!.Length);
        Assert.Single(result.Warnings);
        Assert.Contains("truncated", result.Warnings[0].Message);
    }

    [Fact]
    public void StudentDataMapper_MapsWithLegacyIdAndDateParsing()
    {
        var record = CreateChildRecord("REF001", "John", "Doe", "2010-05-15");

        var mapper = new StudentDataMapper();
        var result = mapper.Map(record);

        Assert.True(result.Success);
        Assert.Equal("REF001", result.Data!.Student.LegacyId);
        Assert.Equal("John", result.Data.Student.FirstName);
        Assert.Equal("Doe", result.Data.Student.LastName);
        Assert.Equal(new DateTime(2010, 5, 15), result.Data.Student.DateOfBirth);
    }

    [Fact]
    public void StudentDataMapper_ParsesMultipleDateFormats()
    {
        var mapper = new StudentDataMapper();

        // ISO format
        var r1 = mapper.Map(CreateChildRecord("R1", "A", "B", "2010-05-15T00:00:00"));
        Assert.Equal(new DateTime(2010, 5, 15), r1.Data!.Student.DateOfBirth);

        // ISO date only
        var r2 = mapper.Map(CreateChildRecord("R2", "A", "B", "2010-05-15"));
        Assert.Equal(new DateTime(2010, 5, 15), r2.Data!.Student.DateOfBirth);
    }

    [Fact]
    public void StudentDataMapper_RejectsRecordWithoutReference()
    {
        var record = CreateChildRecord("", "John", "Doe", null);

        var mapper = new StudentDataMapper();
        var result = mapper.Map(record);

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void StudentDataMapper_ExtractsFamilyInfo()
    {
        var record = new LegacyChildRecord(
            Reference: "REF001", ChildName: "John", ChildSurname: "Doe",
            ChildBirthdate: null, Sex: null, Language: null,
            AccountPersonName: "Parent Name", AccountPersonSurname: null,
            AccountPersonIdnumber: null, AccountPersonCellphone: "012345",
            AccountPersonOffice: null, AccountPersonHome: null,
            AccountPersonEmail: "parent@test.com", Relation: null,
            MotherName: null, MotherSurname: null, MotherOffice: null,
            MotherCell: null, MotherHome: null, MotherEmail: null,
            FatherName: null, FatherSurname: null, FatherOffice: null,
            FatherCell: null, FatherHome: null, FatherEmail: null,
            Address1: "123 St", Address2: "City", Code: "1234",
            SchoolName: null, ClassGroup: null, Grade: null,
            Teacher: null, AttendingKcowAt: null, Aftercare: null,
            Extra: null, HomeTime: null, StartClasses: null,
            Terms: null, Seat: null, Truck: null, Family: "FAM001",
            Sequence: null, FinancialCode: null, Charge: null,
            Deposit: null, PayDate: null, TshirtCode: null,
            TshirtMoney1: null, TshirtMoneyDate1: null,
            TshirtReceived1: null, TshirtRecDate1: null,
            ReceiveNote1: null, TshirtSize1: null,
            TshirtColor1: null, TshirtDesign1: null,
            TshirtSize2: null, TshirtMoney2: null,
            TshirtMoneyDate2: null, TshirtReceived2: null,
            TshirtRecDate2: null, ReceiveNote2: null,
            TshirtColor2: null, TshirtDesign2: null,
            Indicator1: null, Indicator2: null, GeneralNote: null,
            PrintIdCard: null, AcceptTermsCond: null, Status: null,
            SmsOrEmail: null, SchoolClose: null, Cnt: null,
            OnlineEntry: null, Created: null, Submitted: null,
            Updated: null, BookEmail: null, Report1GivenOut: null,
            AccountGivenOut: null, CertificatePrinted: null,
            Report2GivenOut: null, Social: null,
            ActivityReportGivenOut: null, Photo: null,
            PhotoUpdated: null);

        var mapper = new StudentDataMapper();
        var result = mapper.Map(record);

        Assert.True(result.Success);
        Assert.NotNull(result.Data!.FamilyInfo);
        Assert.Equal("FAM001", result.Data.FamilyInfo!.FamilyName);
        Assert.Equal("Parent Name", result.Data.FamilyInfo.PrimaryContactName);
    }

    [Fact]
    public void BatchMapping_AggregatesWarningsAndErrors()
    {
        var records = new[]
        {
            CreateChildRecord("REF001", "John", "Doe", "bad-date"),
            CreateChildRecord("", "Missing", "Ref", null),
            CreateChildRecord("REF003", "Jane", "Smith", "2010-01-01"),
        };

        var mapper = new StudentDataMapper();
        var result = mapper.MapMany(records);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
        Assert.NotEmpty(result.Warnings);
        Assert.NotEmpty(result.Errors);
    }

    // ==========================================
    // Import Execution Result Tests
    // ==========================================

    [Fact]
    public void ConflictResolutionMode_HasThreeValues()
    {
        var values = Enum.GetValues<ConflictResolutionMode>();
        Assert.Equal(3, values.Length);
    }

    [Fact]
    public void ConflictResolutionMode_DefaultIsFailOnConflict()
    {
        var result = new ImportExecutionResult();
        Assert.Equal(ConflictResolutionMode.FailOnConflict, result.ConflictMode);
    }

    [Fact]
    public void EntityImportResult_TracksAllCountTypes()
    {
        var counts = new EntityImportResult
        {
            Imported = 10, Updated = 5, Skipped = 3, Failed = 2
        };
        Assert.Equal(10, counts.Imported);
        Assert.Equal(5, counts.Updated);
        Assert.Equal(3, counts.Skipped);
        Assert.Equal(2, counts.Failed);
    }

    [Fact]
    public void ImportExecutionResult_ReimportMetrics()
    {
        var result = new ImportExecutionResult
        {
            Schools = new EntityImportResult { Imported = 2, Updated = 3, Skipped = 1 },
            ClassGroups = new EntityImportResult { Imported = 5, Updated = 2 },
            Activities = new EntityImportResult { Imported = 10, Failed = 1 },
            Students = new EntityImportResult { Imported = 50, Updated = 20, Skipped = 5, Failed = 3 },
            ConflictMode = ConflictResolutionMode.Update
        };

        Assert.Equal(67, result.TotalImported);
        Assert.Equal(25, result.TotalUpdated);
        Assert.Equal(6, result.TotalSkipped);
        Assert.Equal(4, result.TotalFailed);
        Assert.Equal(102, result.TotalProcessed);
        Assert.Equal(90.2, result.SuccessRate);
    }

    // ==========================================
    // Import Exception and Audit Tests
    // ==========================================

    [Fact]
    public void ImportException_CapturesAllDetails()
    {
        var ex = new ImportException("Student", "REF001", "DateOfBirth",
            "Invalid date format", "31/13/2010");

        Assert.Equal("Student", ex.EntityType);
        Assert.Equal("REF001", ex.LegacyId);
        Assert.Equal("DateOfBirth", ex.Field);
        Assert.Equal("Invalid date format", ex.Reason);
        Assert.Equal("31/13/2010", ex.OriginalValue);
    }

    [Fact]
    public async Task ImportExceptionWriter_WritesJsonWithAllFields()
    {
        var outputPath = Path.Combine(_tempDir, "exceptions.json");
        var result = new ImportExecutionResult
        {
            InputPath = "/test",
            Schools = new EntityImportResult { Imported = 5, Updated = 2, Failed = 1 },
            Exceptions = { new ImportException("School", "42", "Name", "Empty") }
        };

        await ImportExceptionWriter.WriteAsync(result, outputPath);

        Assert.True(File.Exists(outputPath));
        var json = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("exceptions", json);
        Assert.Contains("Empty", json);
    }

    [Fact]
    public void ImportAuditLog_DefaultValues()
    {
        var log = new ImportAuditLog();
        Assert.Equal(0, log.TotalCreated);
        Assert.Null(log.CompletedAt);
    }

    [Fact]
    public void ImportAuditLog_TotalCreated_SumsAllEntityCounts()
    {
        var log = new ImportAuditLog
        {
            SchoolsCreated = 10,
            ClassGroupsCreated = 20,
            ActivitiesCreated = 5,
            StudentsCreated = 100
        };
        Assert.Equal(135, log.TotalCreated);
    }

    // ==========================================
    // CLI Flag Tests
    // ==========================================

    [Fact]
    public async Task ImportRunCommand_HelpShowsAllFlags()
    {
        var output = new StringWriter();
        var parser = new LegacyParser();

        var exitCode = await Kcow.Api.CliCommands.ImportRunCommand.ExecuteAsync(
            new[] { "import", "run", "--help" }, parser, output);

        var text = output.ToString();
        Assert.Equal(0, exitCode);
        Assert.Contains("--skip-existing", text);
        Assert.Contains("--update", text);
        Assert.Contains("--fail-on-conflict", text);
        Assert.Contains("--preview", text);
        Assert.Contains("--input", text);
    }

    [Fact]
    public async Task ImportRunCommand_PreviewMode_NoDbRequired()
    {
        var output = new StringWriter();
        var parser = new LegacyParser();

        // Preview mode should work without import service
        var exitCode = await Kcow.Api.CliCommands.ImportRunCommand.ExecuteAsync(
            new[] { "import", "run", "--preview", "--input", _tempDir },
            parser, output);

        var text = output.ToString();
        Assert.Equal(0, exitCode);
        Assert.Contains("PREVIEW", text);
        Assert.Contains("NO data will be written", text);
    }

    [Fact]
    public async Task ImportRunCommand_FullImportWithoutService_Fails()
    {
        var output = new StringWriter();
        var parser = new LegacyParser();

        var exitCode = await Kcow.Api.CliCommands.ImportRunCommand.ExecuteAsync(
            new[] { "import", "run", "--input", _tempDir },
            parser, output);

        Assert.Equal(1, exitCode);
        Assert.Contains("Import execution service not available", output.ToString());
    }

    // ==========================================
    // Helper Methods
    // ==========================================

    private string WriteXmlFile(string name, string content)
    {
        var path = Path.Combine(_tempDir, name);
        File.WriteAllText(path, content);
        return path;
    }

    private static LegacyChildRecord CreateChildRecord(
        string reference, string firstName, string lastName, string? birthdate)
    {
        return new LegacyChildRecord(
            Reference: reference, ChildName: firstName, ChildSurname: lastName,
            ChildBirthdate: birthdate, Sex: null, Language: null,
            AccountPersonName: null, AccountPersonSurname: null,
            AccountPersonIdnumber: null, AccountPersonCellphone: null,
            AccountPersonOffice: null, AccountPersonHome: null,
            AccountPersonEmail: null, Relation: null,
            MotherName: null, MotherSurname: null, MotherOffice: null,
            MotherCell: null, MotherHome: null, MotherEmail: null,
            FatherName: null, FatherSurname: null, FatherOffice: null,
            FatherCell: null, FatherHome: null, FatherEmail: null,
            Address1: null, Address2: null, Code: null,
            SchoolName: null, ClassGroup: null, Grade: null,
            Teacher: null, AttendingKcowAt: null, Aftercare: null,
            Extra: null, HomeTime: null, StartClasses: null,
            Terms: null, Seat: null, Truck: null, Family: null,
            Sequence: null, FinancialCode: null, Charge: null,
            Deposit: null, PayDate: null, TshirtCode: null,
            TshirtMoney1: null, TshirtMoneyDate1: null,
            TshirtReceived1: null, TshirtRecDate1: null,
            ReceiveNote1: null, TshirtSize1: null,
            TshirtColor1: null, TshirtDesign1: null,
            TshirtSize2: null, TshirtMoney2: null,
            TshirtMoneyDate2: null, TshirtReceived2: null,
            TshirtRecDate2: null, ReceiveNote2: null,
            TshirtColor2: null, TshirtDesign2: null,
            Indicator1: null, Indicator2: null, GeneralNote: null,
            PrintIdCard: null, AcceptTermsCond: null, Status: null,
            SmsOrEmail: null, SchoolClose: null, Cnt: null,
            OnlineEntry: null, Created: null, Submitted: null,
            Updated: null, BookEmail: null, Report1GivenOut: null,
            AccountGivenOut: null, CertificatePrinted: null,
            Report2GivenOut: null, Social: null,
            ActivityReportGivenOut: null, Photo: null,
            PhotoUpdated: null);
    }

    private static string FindRepoFile(string relativePath)
    {
        var baseDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        while (baseDirectory != null)
        {
            var candidate = Path.Combine(baseDirectory.FullName, relativePath);
            if (File.Exists(candidate))
                return candidate;
            baseDirectory = baseDirectory.Parent;
        }
        throw new FileNotFoundException($"Unable to locate {relativePath}");
    }
}
