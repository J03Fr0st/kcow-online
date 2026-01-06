using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Kcow.Application.Import;

public sealed record LegacyXmlValidationError(string Message, int? LineNumber, int? LinePosition);

public sealed record LegacySchoolImportResult(
    IReadOnlyList<LegacySchoolRecord> Records,
    IReadOnlyList<LegacyXmlValidationError> ValidationErrors);

public sealed record LegacySchoolRecord(
    int SchoolId,
    string? ShortSchool,
    byte? Trok,
    double? Price,
    string? FormulaDescription,
    float? Formula,
    string? Sequence,
    string? Day,
    string? SchoolDescription,
    string? ContactPerson,
    string? EmailAddress,
    string? ContactCell,
    string? Telephone,
    string? Fax,
    string? Address1,
    string? Address2,
    string? Headmaster,
    string? HeadmasterCell,
    string? MoneyMessage,
    bool Print,
    string? Taal,
    bool Import,
    string? WebPage,
    string? Naskool1Name,
    string? Naskool1Contact,
    string? Naskool2Name,
    string? Naskool2Contact,
    string? Kluis,
    string? Omsendbriewe,
    string? KcowWebPageLink);

public sealed class LegacySchoolXmlParser
{
    public LegacySchoolImportResult Parse(string xmlPath, string xsdPath)
    {
        if (string.IsNullOrWhiteSpace(xmlPath))
        {
            throw new ArgumentException("XML path is required.", nameof(xmlPath));
        }

        if (string.IsNullOrWhiteSpace(xsdPath))
        {
            throw new ArgumentException("XSD path is required.", nameof(xsdPath));
        }

        var errors = new List<LegacyXmlValidationError>();
        var settings = BuildValidationSettings(xsdPath, errors);

        XDocument document;
        try
        {
            using var stream = new FileStream(xmlPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = XmlReader.Create(stream, settings);
            document = XDocument.Load(reader);
        }
        catch (XmlException ex)
        {
            errors.Add(new LegacyXmlValidationError(ex.Message, ex.LineNumber, ex.LinePosition));
            return new LegacySchoolImportResult(Array.Empty<LegacySchoolRecord>(), errors);
        }
        catch (Exception ex)
        {
            errors.Add(new LegacyXmlValidationError(ex.Message, null, null));
            return new LegacySchoolImportResult(Array.Empty<LegacySchoolRecord>(), errors);
        }

        var records = new List<LegacySchoolRecord>();
        foreach (var schoolElement in document.Root?.Elements("School") ?? Enumerable.Empty<XElement>())
        {
            var record = new LegacySchoolRecord(
                SchoolId: ParseRequiredInt(GetValue(schoolElement, "School_x0020_Id"), errors, "School_x0020_Id"),
                ShortSchool: NormalizeString(GetValue(schoolElement, "Short_x0020_School")),
                Trok: ParseByte(GetValue(schoolElement, "Trok"), errors, "Trok"),
                Price: ParseDouble(GetValue(schoolElement, "Price"), errors, "Price"),
                FormulaDescription: NormalizeString(GetValue(schoolElement, "F_x0020_Descr")),
                Formula: ParseFloat(GetValue(schoolElement, "Formula"), errors, "Formula"),
                Sequence: NormalizeString(GetValue(schoolElement, "Sequence")),
                Day: NormalizeString(GetValue(schoolElement, "Day")),
                SchoolDescription: NormalizeString(GetValue(schoolElement, "School_x0020_Description")),
                ContactPerson: NormalizeString(GetValue(schoolElement, "ContactPerson")),
                EmailAddress: NormalizeString(GetValue(schoolElement, "E-mail_x0020_adress")),
                ContactCell: NormalizeString(GetValue(schoolElement, "ContactCell")),
                Telephone: NormalizeString(GetValue(schoolElement, "Telephone")),
                Fax: NormalizeString(GetValue(schoolElement, "Fax")),
                Address1: NormalizeString(GetValue(schoolElement, "Address1")),
                Address2: NormalizeString(GetValue(schoolElement, "Address2")),
                Headmaster: NormalizeString(GetValue(schoolElement, "Headmaster")),
                HeadmasterCell: NormalizeString(GetValue(schoolElement, "HeadmasterCell")),
                MoneyMessage: NormalizeString(GetValue(schoolElement, "MoneyMessage")),
                Print: ParseRequiredBool(GetValue(schoolElement, "Print"), errors, "Print"),
                Taal: NormalizeString(GetValue(schoolElement, "Taal")),
                Import: ParseRequiredBool(GetValue(schoolElement, "Import"), errors, "Import"),
                WebPage: NormalizeString(GetValue(schoolElement, "web_x0020_page")),
                Naskool1Name: NormalizeString(GetValue(schoolElement, "Naskool1_x0020_Name")),
                Naskool1Contact: NormalizeString(GetValue(schoolElement, "Naskool1_x0020_Contact")),
                Naskool2Name: NormalizeString(GetValue(schoolElement, "Naskool2_x0020_Name")),
                Naskool2Contact: NormalizeString(GetValue(schoolElement, "Naskool2_x0020_Contact")),
                Kluis: NormalizeString(GetValue(schoolElement, "Kluis")),
                Omsendbriewe: NormalizeString(GetValue(schoolElement, "omsendbriewe")),
                KcowWebPageLink: NormalizeString(GetValue(schoolElement, "KcowWebPageLink")));

            records.Add(record);
        }

        return new LegacySchoolImportResult(records, errors);
    }

    private static XmlReaderSettings BuildValidationSettings(string xsdPath, ICollection<LegacyXmlValidationError> errors)
    {
        var schemas = new XmlSchemaSet();
        schemas.Add(null, xsdPath);

        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            ValidationType = ValidationType.Schema,
            Schemas = schemas,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true
        };
        settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
        settings.ValidationEventHandler += (_, args) =>
        {
            var exception = args.Exception;
            errors.Add(new LegacyXmlValidationError(
                args.Message,
                exception?.LineNumber,
                exception?.LinePosition));
        };

        return settings;
    }

    private static string? GetValue(XElement parent, string elementName)
    {
        return parent.Element(elementName)?.Value;
    }

    private static string? NormalizeString(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static int ParseRequiredInt(string? value, ICollection<LegacyXmlValidationError> errors, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new LegacyXmlValidationError($"{fieldName} is required.", null, null));
            return 0;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ||
            int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out result))
        {
            return result;
        }

        errors.Add(new LegacyXmlValidationError($"Invalid {fieldName} value: '{value}'.", null, null));
        return 0;
    }

    private static byte? ParseByte(string? value, ICollection<LegacyXmlValidationError> errors, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ||
            byte.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out result))
        {
            return result;
        }

        errors.Add(new LegacyXmlValidationError($"Invalid {fieldName} value: '{value}'.", null, null));
        return null;
    }

    private static double? ParseDouble(string? value, ICollection<LegacyXmlValidationError> errors, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ||
            double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
        {
            return result;
        }

        errors.Add(new LegacyXmlValidationError($"Invalid {fieldName} value: '{value}'.", null, null));
        return null;
    }

    private static float? ParseFloat(string? value, ICollection<LegacyXmlValidationError> errors, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ||
            float.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
        {
            return result;
        }

        errors.Add(new LegacyXmlValidationError($"Invalid {fieldName} value: '{value}'.", null, null));
        return null;
    }

    private static bool ParseRequiredBool(string? value, ICollection<LegacyXmlValidationError> errors, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new LegacyXmlValidationError($"{fieldName} is required.", null, null));
            return false;
        }

        var normalized = value.Trim();
        if (normalized == "1")
        {
            return true;
        }

        if (normalized == "0")
        {
            return false;
        }

        if (bool.TryParse(normalized, out var result))
        {
            return result;
        }

        errors.Add(new LegacyXmlValidationError($"Invalid {fieldName} value: '{value}'.", null, null));
        return false;
    }
}
