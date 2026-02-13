using Kcow.Application.Import;
using Kcow.Application.Import.Mappers;

namespace Kcow.Unit.Tests.Import;

public class ImportRerunTests
{
    // Task 1: LegacyId is set by mappers

    [Fact]
    public void SchoolDataMapper_SetsLegacyId_FromSchoolId()
    {
        var record = new LegacySchoolRecord(
            SchoolId: 42,
            ShortSchool: "TST",
            Trok: null, Price: null, FormulaDescription: null, Formula: null,
            Sequence: null, Day: null, SchoolDescription: "Test School",
            ContactPerson: null, EmailAddress: null, ContactCell: null,
            Telephone: null, Fax: null, Address1: null, Address2: null,
            Headmaster: null, HeadmasterCell: null, MoneyMessage: null,
            Print: false, Taal: null, Import: false, WebPage: null,
            Naskool1Name: null, Naskool1Contact: null,
            Naskool2Name: null, Naskool2Contact: null,
            Kluis: null, Omsendbriewe: null, KcowWebPageLink: null);

        var result = new SchoolDataMapper().Map(record);

        Assert.True(result.Success);
        Assert.Equal("42", result.Data!.LegacyId);
    }

    [Fact]
    public void ClassGroupDataMapper_SetsLegacyId_FromClassGroupCode()
    {
        var record = new LegacyClassGroupRecord(
            ClassGroup: "MON-A1",
            DayTruck: null, Description: "Monday Group A1",
            EndTime: "09:00", SchoolId: 1, DayId: "1",
            StartTime: "08:00", Evaluate: false, Note: null,
            Import: true, Sequence: "1",
            GroupMessage: null, SendCertificates: null,
            MoneyMessage: null, IXL: null);

        var result = new ClassGroupDataMapper().Map(record);

        Assert.True(result.Success);
        Assert.Equal("MON-A1", result.Data!.LegacyId);
    }

    [Fact]
    public void ActivityDataMapper_SetsLegacyId_FromActivityId()
    {
        var record = new LegacyActivityRecord(
            ActivityId: 99,
            Program: "MATH",
            ProgramName: "Mathematics",
            EducationalFocus: "Basic maths",
            Folder: null, Grade: null, Icon: null);

        var result = new ActivityDataMapper().Map(record);

        Assert.True(result.Success);
        Assert.Equal("99", result.Data!.LegacyId);
    }

    [Fact]
    public void StudentDataMapper_SetsLegacyId_FromReference()
    {
        var record = CreateChildRecord("REF001");

        var result = new StudentDataMapper().Map(record);

        Assert.True(result.Success);
        Assert.Equal("REF001", result.Data!.Student.LegacyId);
    }

    // Task 2: ConflictResolutionMode enum

    [Fact]
    public void ConflictResolutionMode_HasThreeValues()
    {
        var values = Enum.GetValues<ConflictResolutionMode>();
        Assert.Equal(3, values.Length);
        Assert.Contains(ConflictResolutionMode.FailOnConflict, values);
        Assert.Contains(ConflictResolutionMode.SkipExisting, values);
        Assert.Contains(ConflictResolutionMode.Update, values);
    }

    // Task 2: EntityImportResult has Updated field

    [Fact]
    public void EntityImportResult_HasUpdatedField()
    {
        var result = new EntityImportResult { Imported = 5, Updated = 3, Skipped = 2, Failed = 1 };
        Assert.Equal(3, result.Updated);
    }

    [Fact]
    public void ImportExecutionResult_TotalUpdated_SumsAllEntities()
    {
        var result = new ImportExecutionResult
        {
            Schools = new EntityImportResult { Updated = 2 },
            ClassGroups = new EntityImportResult { Updated = 3 },
            Activities = new EntityImportResult { Updated = 1 },
            Students = new EntityImportResult { Updated = 10 }
        };

        Assert.Equal(16, result.TotalUpdated);
    }

    [Fact]
    public void ImportExecutionResult_TotalProcessed_IncludesUpdated()
    {
        var result = new ImportExecutionResult
        {
            Schools = new EntityImportResult { Imported = 5, Updated = 3, Skipped = 2, Failed = 1 }
        };

        Assert.Equal(11, result.TotalProcessed);
    }

    [Fact]
    public void ImportExecutionResult_SuccessRate_IncludesUpdated()
    {
        var result = new ImportExecutionResult
        {
            Schools = new EntityImportResult { Imported = 5, Updated = 5, Failed = 0 }
        };

        Assert.Equal(100.0, result.SuccessRate);
    }

    [Fact]
    public void ImportExecutionResult_SuccessRate_UpdatesAndImports()
    {
        var result = new ImportExecutionResult
        {
            Schools = new EntityImportResult { Imported = 4, Updated = 4, Failed = 2 }
        };

        // (4 + 4) / 10 * 100 = 80.0
        Assert.Equal(80.0, result.SuccessRate);
    }

    [Fact]
    public void ImportExecutionResult_ConflictMode_IsStored()
    {
        var result = new ImportExecutionResult
        {
            ConflictMode = ConflictResolutionMode.Update
        };

        Assert.Equal(ConflictResolutionMode.Update, result.ConflictMode);
    }

    // Helper method to create a minimal LegacyChildRecord
    private static LegacyChildRecord CreateChildRecord(string reference)
    {
        return new LegacyChildRecord(
            Reference: reference,
            ChildName: "Test", ChildSurname: "Student",
            ChildBirthdate: null, Sex: null, Language: null,
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
}
