using Kcow.Application.Import;
using Kcow.Application.Import.Mappers;

namespace Kcow.Unit.Tests.Import;

/// <summary>
/// Tests for cross-mapper validation and flagging behavior (AC #3).
/// Verifies that missing or invalid values are flagged for review.
/// </summary>
public class MappingValidationTests
{
    [Fact]
    public void SchoolMapper_MissingRequiredName_FlagsWarning()
    {
        var mapper = new SchoolDataMapper();
        var record = new LegacySchoolRecord(
            SchoolId: 1, ShortSchool: null, Trok: null, Price: null,
            FormulaDescription: null, Formula: null, Sequence: null, Day: null,
            SchoolDescription: null, ContactPerson: null, EmailAddress: null,
            ContactCell: null, Telephone: null, Fax: null, Address1: null,
            Address2: null, Headmaster: null, HeadmasterCell: null, MoneyMessage: null,
            Print: false, Taal: null, Import: false, WebPage: null,
            Naskool1Name: null, Naskool1Contact: null, Naskool2Name: null,
            Naskool2Contact: null, Kluis: null, Omsendbriewe: null, KcowWebPageLink: null);

        var result = mapper.Map(record);

        Assert.True(result.Success); // still maps, just warns
        Assert.Contains(result.Warnings, w => w.Field == "Name");
    }

    [Fact]
    public void SchoolMapper_InvalidForeignKey_FlagsWarningWithOriginalValue()
    {
        var mapper = new SchoolDataMapper(new HashSet<int> { 1 });
        var record = new LegacySchoolRecord(
            SchoolId: 1, ShortSchool: "S", Trok: 99, Price: null,
            FormulaDescription: null, Formula: null, Sequence: null, Day: null,
            SchoolDescription: "Name", ContactPerson: null, EmailAddress: null,
            ContactCell: null, Telephone: null, Fax: null, Address1: null,
            Address2: null, Headmaster: null, HeadmasterCell: null, MoneyMessage: null,
            Print: false, Taal: null, Import: false, WebPage: null,
            Naskool1Name: null, Naskool1Contact: null, Naskool2Name: null,
            Naskool2Contact: null, Kluis: null, Omsendbriewe: null, KcowWebPageLink: null);

        var result = mapper.Map(record);

        var truckWarning = Assert.Single(result.Warnings, w => w.Field == "TruckId");
        Assert.Equal("99", truckWarning.OriginalValue);
        Assert.Null(truckWarning.MappedValue);
    }

    [Fact]
    public void ClassGroupMapper_MissingName_FlagsError()
    {
        var mapper = new ClassGroupDataMapper();
        var record = new LegacyClassGroupRecord(
            ClassGroup: "", DayTruck: null, Description: null, EndTime: "09:00",
            SchoolId: 1, DayId: "1", StartTime: "08:00", Evaluate: false,
            Note: null, Import: true, Sequence: null, GroupMessage: null,
            SendCertificates: null, MoneyMessage: null, IXL: null);

        var result = mapper.Map(record);

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Field == "Name");
    }

    [Fact]
    public void StudentMapper_MissingReference_FlagsError()
    {
        var mapper = new StudentDataMapper();
        var record = CreateMinimalChildRecord(reference: " ");

        var result = mapper.Map(record);

        Assert.False(result.Success);
        Assert.Contains(result.Errors, e => e.Field == "Reference");
    }

    [Fact]
    public void StudentMapper_UnresolvableSchool_FlagsWarningWithSchoolName()
    {
        var mapper = new StudentDataMapper(new Dictionary<string, int> { { "Known", 1 } });
        var record = CreateMinimalChildRecord(schoolName: "Unknown School XYZ");

        var result = mapper.Map(record);

        Assert.True(result.Success);
        var warning = Assert.Single(result.Warnings, w => w.Field == "SchoolId");
        Assert.Equal("Unknown School XYZ", warning.OriginalValue);
    }

    [Fact]
    public void StudentMapper_InvalidDateFormat_FlagsWarningWithOriginalValue()
    {
        var mapper = new StudentDataMapper();
        var record = CreateMinimalChildRecord(birthdate: "32/13/2020");

        var result = mapper.Map(record);

        Assert.True(result.Success);
        var warning = Assert.Single(result.Warnings, w => w.Field == "DateOfBirth");
        Assert.Equal("32/13/2020", warning.OriginalValue);
    }

    [Fact]
    public void ActivityMapper_FieldTruncation_FlagsWarningWithLengths()
    {
        var mapper = new ActivityDataMapper();
        var longName = new string('Z', 300);
        var record = new LegacyActivityRecord(1, null, longName, null, null, null, null);

        var result = mapper.Map(record);

        Assert.True(result.Success);
        var warning = Assert.Single(result.Warnings, w => w.Field == "Name");
        Assert.Contains("300", warning.Message);
        Assert.Contains("255", warning.Message);
    }

    [Fact]
    public void MapMany_AggregatesWarningsAndErrors_AcrossRecords()
    {
        var mapper = new StudentDataMapper();
        var records = new[]
        {
            CreateMinimalChildRecord(reference: ""), // error
            CreateMinimalChildRecord(reference: "R1", birthdate: "bad-date"), // warning
            CreateMinimalChildRecord(reference: "R2") // clean
        };

        var result = mapper.MapMany(records);

        Assert.Equal(2, result.Data!.Count); // 2 successful
        Assert.True(result.HasErrors); // 1 error from first record
        Assert.True(result.HasWarnings); // 1 warning from second record
    }

    private static LegacyChildRecord CreateMinimalChildRecord(
        string reference = "REF",
        string? schoolName = null,
        string? birthdate = null)
    {
        return new LegacyChildRecord(
            Reference: reference, ChildName: "Test", ChildSurname: "Child",
            ChildBirthdate: birthdate, Sex: null, Language: null,
            AccountPersonName: null, AccountPersonSurname: null, AccountPersonIdnumber: null,
            AccountPersonCellphone: null, AccountPersonOffice: null, AccountPersonHome: null,
            AccountPersonEmail: null, Relation: null, MotherName: null, MotherSurname: null,
            MotherOffice: null, MotherCell: null, MotherHome: null, MotherEmail: null,
            FatherName: null, FatherSurname: null, FatherOffice: null, FatherCell: null,
            FatherHome: null, FatherEmail: null, Address1: null, Address2: null, Code: null,
            SchoolName: schoolName, ClassGroup: null, Grade: null, Teacher: null,
            AttendingKcowAt: null, Aftercare: null, Extra: null, HomeTime: null,
            StartClasses: null, Terms: null, Seat: null, Truck: null, Family: null,
            Sequence: null, FinancialCode: null, Charge: null, Deposit: null, PayDate: null,
            TshirtCode: null, TshirtMoney1: null, TshirtMoneyDate1: null, TshirtReceived1: null,
            TshirtRecDate1: null, ReceiveNote1: null, TshirtSize1: null, TshirtColor1: null,
            TshirtDesign1: null, TshirtSize2: null, TshirtMoney2: null, TshirtMoneyDate2: null,
            TshirtReceived2: null, TshirtRecDate2: null, ReceiveNote2: null, TshirtColor2: null,
            TshirtDesign2: null, Indicator1: null, Indicator2: null, GeneralNote: null,
            PrintIdCard: null, AcceptTermsCond: null, Status: null, SmsOrEmail: null,
            SchoolClose: null, Cnt: null, OnlineEntry: null, Created: null, Submitted: null,
            Updated: null, BookEmail: null, Report1GivenOut: null, AccountGivenOut: null,
            CertificatePrinted: null, Report2GivenOut: null, Social: null,
            ActivityReportGivenOut: null, Photo: null, PhotoUpdated: null);
    }
}
