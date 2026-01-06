using Kcow.Application.Import;

namespace Kcow.Integration.Tests.Import;

public class LegacySchoolMapperTests
{
    [Fact]
    public void Map_WhenDescriptionPresent_MapsAllFields()
    {
        var mapper = new LegacySchoolMapper();
        var record = new LegacySchoolRecord(
            SchoolId: 5,
            ShortSchool: "Short",
            Trok: 1,
            Price: 200.5,
            FormulaDescription: "Desc",
            Formula: 0.2f,
            Sequence: "10",
            Day: "Friday",
            SchoolDescription: "School Name",
            ContactPerson: "Contact",
            EmailAddress: "contact@example.com",
            ContactCell: "0123",
            Telephone: "011 555 0000",
            Fax: "011 555 0001",
            Address1: "Line 1",
            Address2: "Line 2",
            Headmaster: "Head",
            HeadmasterCell: "0999",
            MoneyMessage: "Money",
            Print: true,
            Taal: "Afr",
            Import: false,
            WebPage: "example.com",
            Naskool1Name: "After1",
            Naskool1Contact: "AfterContact1",
            Naskool2Name: "After2",
            Naskool2Contact: "AfterContact2",
            Kluis: "Safe",
            Omsendbriewe: "circulars@example.com",
            KcowWebPageLink: "kcow-link");

        var result = mapper.Map(record);

        Assert.Empty(result.Warnings);
        var school = result.School;
        Assert.Equal(5, school.Id);
        Assert.Equal("School Name", school.Name);
        Assert.Equal("Short", school.ShortName);
        Assert.Equal(1, school.TruckId);
        Assert.Equal(200.5m, school.Price);
        Assert.Equal("Desc", school.FeeDescription);
        Assert.Equal(0.2m, school.Formula);
        Assert.Equal("Friday", school.VisitDay);
        Assert.Equal("10", school.VisitSequence);
        Assert.Equal("Contact", school.ContactPerson);
        Assert.Equal("0123", school.ContactCell);
        Assert.Equal("011 555 0000", school.Telephone);
        Assert.Equal("011 555 0001", school.Fax);
        Assert.Equal("contact@example.com", school.Email);
        Assert.Equal("circulars@example.com", school.CircularsEmail);
        Assert.Equal("Line 1", school.Address);
        Assert.Equal("Line 2", school.Address2);
        Assert.Equal("Head", school.Headmaster);
        Assert.Equal("0999", school.HeadmasterCell);
        Assert.Equal("Money", school.MoneyMessage);
        Assert.True(school.PrintInvoice);
        Assert.Equal("Afr", school.Language);
        Assert.False(school.ImportFlag);
        Assert.Equal("After1", school.Afterschool1Name);
        Assert.Equal("AfterContact1", school.Afterschool1Contact);
        Assert.Equal("After2", school.Afterschool2Name);
        Assert.Equal("AfterContact2", school.Afterschool2Contact);
        Assert.Equal("Safe", school.SafeNotes);
        Assert.Equal("example.com", school.WebPage);
        Assert.Equal("kcow-link", school.KcowWebPageLink);
    }

    [Fact]
    public void Map_WhenNameMissing_EmitsWarning()
    {
        var mapper = new LegacySchoolMapper();
        var record = new LegacySchoolRecord(
            SchoolId: 6,
            ShortSchool: null,
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

        var result = mapper.Map(record);

        Assert.Single(result.Warnings);
        Assert.Equal(6, result.School.Id);
        Assert.Equal(string.Empty, result.School.Name);
    }
}
