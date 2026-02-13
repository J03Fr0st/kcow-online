using Kcow.Application.Import;
using Kcow.Application.Import.Mappers;

namespace Kcow.Unit.Tests.Import;

public class SchoolDataMapperTests
{
    private readonly SchoolDataMapper _mapper = new();

    [Fact]
    public void Map_ValidRecord_ReturnsSuccessWithSchool()
    {
        var record = CreateRecord(schoolId: 1, description: "Test School", shortSchool: "TS");

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data!.Id);
        Assert.Equal("Test School", result.Data.Name);
        Assert.Equal("TS", result.Data.ShortName);
    }

    [Fact]
    public void Map_MissingName_WarnsButSucceeds()
    {
        var record = CreateRecord(schoolId: 2, description: null, shortSchool: null);

        var result = _mapper.Map(record);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.HasWarnings);
        Assert.Contains(result.Warnings, w => w.Field == "Name");
    }

    [Fact]
    public void Map_InvalidTruckId_WarnsAndNullsTruck()
    {
        var validTruckIds = new HashSet<int> { 1, 2, 3 };
        var mapper = new SchoolDataMapper(validTruckIds);
        var record = CreateRecord(schoolId: 3, truckId: 99);

        var result = mapper.Map(record);

        Assert.True(result.Success);
        Assert.Null(result.Data!.TruckId);
        Assert.Contains(result.Warnings, w => w.Field == "TruckId");
    }

    [Fact]
    public void Map_TrimsWhitespace_FromStringFields()
    {
        var record = CreateRecord(schoolId: 4, description: "  Padded  ", shortSchool: "  S  ");

        var result = _mapper.Map(record);

        Assert.Equal("Padded", result.Data!.Name);
        Assert.Equal("S", result.Data.ShortName);
    }

    [Fact]
    public void Map_MapsAllOptionalFields()
    {
        var record = CreateRecord(
            schoolId: 5,
            description: "Full School",
            contactPerson: "John",
            email: "john@test.com",
            telephone: "555-1234",
            fax: "555-5678",
            address1: "123 Main St",
            address2: "Suite A",
            headmaster: "Dr Smith",
            language: "English",
            webPage: "https://school.com");

        var result = _mapper.Map(record);

        Assert.Equal("John", result.Data!.ContactPerson);
        Assert.Equal("john@test.com", result.Data.Email);
        Assert.Equal("555-1234", result.Data.Telephone);
        Assert.Equal("555-5678", result.Data.Fax);
        Assert.Equal("123 Main St", result.Data.Address);
        Assert.Equal("Suite A", result.Data.Address2);
        Assert.Equal("Dr Smith", result.Data.Headmaster);
        Assert.Equal("English", result.Data.Language);
        Assert.Equal("https://school.com", result.Data.WebPage);
    }

    [Fact]
    public void MapMany_MapsAllRecords_AggregatesWarnings()
    {
        var records = new[]
        {
            CreateRecord(schoolId: 1, description: "School A"),
            CreateRecord(schoolId: 2, description: null, shortSchool: null), // will warn
            CreateRecord(schoolId: 3, description: "School C")
        };

        var result = _mapper.MapMany(records);

        Assert.True(result.Success);
        Assert.Equal(3, result.Data!.Count);
        Assert.True(result.HasWarnings); // from record 2
    }

    [Fact]
    public void Map_PriceConversion_ConvertsDoubleToDecimal()
    {
        var record = CreateRecord(schoolId: 6, description: "Priced", price: 199.99);

        var result = _mapper.Map(record);

        Assert.Equal(199.99m, result.Data!.Price);
    }

    private static LegacySchoolRecord CreateRecord(
        int schoolId = 1,
        string? description = "Default School",
        string? shortSchool = "DS",
        byte? truckId = null,
        double? price = null,
        string? contactPerson = null,
        string? email = null,
        string? telephone = null,
        string? fax = null,
        string? address1 = null,
        string? address2 = null,
        string? headmaster = null,
        string? language = null,
        string? webPage = null)
    {
        return new LegacySchoolRecord(
            SchoolId: schoolId,
            ShortSchool: shortSchool,
            Trok: truckId,
            Price: price,
            FormulaDescription: null,
            Formula: null,
            Sequence: null,
            Day: null,
            SchoolDescription: description,
            ContactPerson: contactPerson,
            EmailAddress: email,
            ContactCell: null,
            Telephone: telephone,
            Fax: fax,
            Address1: address1,
            Address2: address2,
            Headmaster: headmaster,
            HeadmasterCell: null,
            MoneyMessage: null,
            Print: false,
            Taal: language,
            Import: false,
            WebPage: webPage,
            Naskool1Name: null,
            Naskool1Contact: null,
            Naskool2Name: null,
            Naskool2Contact: null,
            Kluis: null,
            Omsendbriewe: null,
            KcowWebPageLink: null);
    }
}
