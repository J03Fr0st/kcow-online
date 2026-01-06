using Kcow.Application.Import;

namespace Kcow.Integration.Tests.Import;

public class LegacySchoolXmlParserTests
{
    [Fact]
    public void Parse_WhenXmlValid_ExtractsAllFields()
    {
        var xsdPath = FindRepoFile("docs/legacy/1_School/School.xsd");
        var xmlPath = WriteTempFile($"""
            <?xml version="1.0" encoding="utf-8"?>
            <dataroot>
              <School>
                <Short_x0020_School>Test</Short_x0020_School>
                <School_x0020_Id>42</School_x0020_Id>
                <Trok>2</Trok>
                <Price>123.45</Price>
                <F_x0020_Descr>10%</F_x0020_Descr>
                <Formula>0.1</Formula>
                <Sequence>SEQ</Sequence>
                <Day>Monday</Day>
                <School_x0020_Description>Test School</School_x0020_Description>
                <ContactPerson>Jane Doe</ContactPerson>
                <E-mail_x0020_adress>jane@example.com</E-mail_x0020_adress>
                <ContactCell>012345</ContactCell>
                <Telephone>011 000 0000</Telephone>
                <Fax>011 000 0001</Fax>
                <Address1>123 Street</Address1>
                <Address2>City</Address2>
                <Headmaster>Head Name</Headmaster>
                <HeadmasterCell>098765</HeadmasterCell>
                <MoneyMessage>Message</MoneyMessage>
                <Print>1</Print>
                <Taal>Afr</Taal>
                <Import>0</Import>
                <web_x0020_page>example.com#http://example.com#</web_x0020_page>
                <Naskool1_x0020_Name>Afterschool One</Naskool1_x0020_Name>
                <Naskool1_x0020_Contact>Contact One</Naskool1_x0020_Contact>
                <Naskool2_x0020_Name>Afterschool Two</Naskool2_x0020_Name>
                <Naskool2_x0020_Contact>Contact Two</Naskool2_x0020_Contact>
                <Kluis>Safe location</Kluis>
                <omsendbriewe>Yes</omsendbriewe>
                <KcowWebPageLink>kcow-link</KcowWebPageLink>
              </School>
            </dataroot>
            """);

        try
        {
            var parser = new LegacySchoolXmlParser();

            var result = parser.Parse(xmlPath, xsdPath);

            Assert.Empty(result.ValidationErrors);
            var record = Assert.Single(result.Records);
            Assert.Equal(42, record.SchoolId);
            Assert.Equal("Test", record.ShortSchool);
            Assert.Equal((byte)2, record.Trok);
            Assert.Equal(123.45, record.Price);
            Assert.Equal("10%", record.FormulaDescription);
            Assert.Equal(0.1f, record.Formula);
            Assert.Equal("SEQ", record.Sequence);
            Assert.Equal("Monday", record.Day);
            Assert.Equal("Test School", record.SchoolDescription);
            Assert.Equal("Jane Doe", record.ContactPerson);
            Assert.Equal("jane@example.com", record.EmailAddress);
            Assert.Equal("012345", record.ContactCell);
            Assert.Equal("011 000 0000", record.Telephone);
            Assert.Equal("011 000 0001", record.Fax);
            Assert.Equal("123 Street", record.Address1);
            Assert.Equal("City", record.Address2);
            Assert.Equal("Head Name", record.Headmaster);
            Assert.Equal("098765", record.HeadmasterCell);
            Assert.Equal("Message", record.MoneyMessage);
            Assert.True(record.Print);
            Assert.Equal("Afr", record.Taal);
            Assert.False(record.Import);
            Assert.Equal("example.com#http://example.com#", record.WebPage);
            Assert.Equal("Afterschool One", record.Naskool1Name);
            Assert.Equal("Contact One", record.Naskool1Contact);
            Assert.Equal("Afterschool Two", record.Naskool2Name);
            Assert.Equal("Contact Two", record.Naskool2Contact);
            Assert.Equal("Safe location", record.Kluis);
            Assert.Equal("Yes", record.Omsendbriewe);
            Assert.Equal("kcow-link", record.KcowWebPageLink);
        }
        finally
        {
            File.Delete(xmlPath);
        }
    }

    [Fact]
    public void Parse_WhenXmlInvalid_ReturnsValidationErrors()
    {
        var xsdPath = FindRepoFile("docs/legacy/1_School/School.xsd");
        var xmlPath = WriteTempFile($"""
            <?xml version="1.0" encoding="utf-8"?>
            <dataroot>
              <School>
                <Print>maybe</Print>
                <Import>1</Import>
              </School>
            </dataroot>
            """);

        try
        {
            var parser = new LegacySchoolXmlParser();

            var result = parser.Parse(xmlPath, xsdPath);

            Assert.NotEmpty(result.ValidationErrors);
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
