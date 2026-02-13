using Kcow.Application.Import;
using Kcow.Application.Import.Mappers;

namespace Kcow.Unit.Tests.Import;

public class StudentDataMapperTests
{
    private readonly StudentDataMapper _mapper = new();

    [Fact]
    public void Map_ValidRecord_ReturnsSuccessWithStudent()
    {
        var record = CreateRecord(reference: "REF001", childName: "Alice", childSurname: "Smith");

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("REF001", result.Data!.Student.Reference);
        Assert.Equal("Alice", result.Data.Student.FirstName);
        Assert.Equal("Smith", result.Data.Student.LastName);
    }

    [Fact]
    public void Map_MissingReference_ReturnsError()
    {
        var record = CreateRecord(reference: "");

        var result = _mapper.Map(record);

        Assert.False(result.Success);
        Assert.Null(result.Data);
        Assert.Contains(result.Errors, e => e.Field == "Reference");
    }

    [Fact]
    public void Map_ParsesDateOfBirth()
    {
        var record = CreateRecord(birthdate: "2015-03-21");

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.Equal(new DateTime(2015, 3, 21), result.Data!.Student.DateOfBirth);
    }

    [Fact]
    public void Map_InvalidDate_ReturnsNullWithWarning()
    {
        var record = CreateRecord(birthdate: "not-a-date");

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.Null(result.Data!.Student.DateOfBirth);
        Assert.Contains(result.Warnings, w => w.Field == "DateOfBirth");
    }

    [Fact]
    public void Map_MapsSchoolIdFromLookup()
    {
        var schools = new Dictionary<string, int> { { "Springfield Elementary", 42 } };
        var mapper = new StudentDataMapper(schools);
        var record = CreateRecord(schoolName: "Springfield Elementary");

        var result = mapper.Map(record);

        Assert.True(result.Success);
        Assert.Equal(42, result.Data!.Student.SchoolId);
    }

    [Fact]
    public void Map_UnknownSchool_WarnsAndNullsSchoolId()
    {
        var schools = new Dictionary<string, int> { { "Known School", 1 } };
        var mapper = new StudentDataMapper(schools);
        var record = CreateRecord(schoolName: "Unknown School");

        var result = mapper.Map(record);

        Assert.True(result.Success);
        Assert.Null(result.Data!.Student.SchoolId);
        Assert.Contains(result.Warnings, w => w.Field == "SchoolId");
    }

    [Fact]
    public void Map_MapsClassGroupIdFromLookup()
    {
        var classGroups = new Dictionary<string, int> { { "MON1", 10 } };
        var mapper = new StudentDataMapper(classGroupIdsByCode: classGroups);
        var record = CreateRecord(classGroup: "MON1");

        var result = mapper.Map(record);

        Assert.True(result.Success);
        Assert.Equal(10, result.Data!.Student.ClassGroupId);
    }

    [Fact]
    public void Map_ExtractsFamilyInfo_WhenFamilyPresent()
    {
        var record = CreateRecord(
            family: "SMITH/JONES",
            accountPersonName: "John",
            accountPersonCellphone: "0821234567",
            accountPersonEmail: "john@test.com",
            address1: "123 Main St",
            address2: "Unit 4",
            code: "2000");

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.NotNull(result.Data!.FamilyInfo);
        Assert.Equal("SMITH/JONES", result.Data.FamilyInfo!.FamilyName);
        Assert.Equal("John", result.Data.FamilyInfo.PrimaryContactName);
        Assert.Equal("0821234567", result.Data.FamilyInfo.Phone);
        Assert.Equal("john@test.com", result.Data.FamilyInfo.Email);
        Assert.Contains("123 Main St", result.Data.FamilyInfo.Address!);
    }

    [Fact]
    public void Map_NoFamily_FamilyInfoIsNull()
    {
        var record = CreateRecord(family: null);

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.Null(result.Data!.FamilyInfo);
    }

    [Fact]
    public void Map_ParsesChargeAsDecimal()
    {
        var record = CreateRecord(charge: "199.50");

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.Equal(199.50m, result.Data!.Student.Charge);
    }

    [Fact]
    public void MapMany_MapsAllRecords_SkipsInvalid()
    {
        var records = new[]
        {
            CreateRecord(reference: "REF1", childName: "Alice"),
            CreateRecord(reference: ""),  // will fail
            CreateRecord(reference: "REF3", childName: "Charlie")
        };

        var result = _mapper.MapMany(records);

        Assert.True(result.Success);
        Assert.Equal(2, result.Data!.Count);
        Assert.True(result.HasErrors); // from invalid record
    }

    private static LegacyChildRecord CreateRecord(
        string reference = "REF001",
        string? childName = "Test",
        string? childSurname = "Child",
        string? birthdate = null,
        string? sex = null,
        string? schoolName = null,
        string? classGroup = null,
        string? family = null,
        string? accountPersonName = null,
        string? accountPersonCellphone = null,
        string? accountPersonEmail = null,
        string? address1 = null,
        string? address2 = null,
        string? code = null,
        string? charge = null)
    {
        return new LegacyChildRecord(
            Reference: reference,
            ChildName: childName,
            ChildSurname: childSurname,
            ChildBirthdate: birthdate,
            Sex: sex,
            Language: null,
            AccountPersonName: accountPersonName,
            AccountPersonSurname: null,
            AccountPersonIdnumber: null,
            AccountPersonCellphone: accountPersonCellphone,
            AccountPersonOffice: null,
            AccountPersonHome: null,
            AccountPersonEmail: accountPersonEmail,
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
            Address1: address1,
            Address2: address2,
            Code: code,
            SchoolName: schoolName,
            ClassGroup: classGroup,
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
            Family: family,
            Sequence: null,
            FinancialCode: null,
            Charge: charge,
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
