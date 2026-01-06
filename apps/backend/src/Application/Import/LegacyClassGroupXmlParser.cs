using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Kcow.Application.Import;

public sealed record LegacyClassGroupImportResult(
    IReadOnlyList<LegacyClassGroupRecord> Records,
    IReadOnlyList<LegacyXmlValidationError> ValidationErrors);

public sealed record LegacyClassGroupRecord(
    string ClassGroup,
    string? DayTruck,
    string? Description,
    string? EndTime,
    short SchoolId,
    string? DayId,
    string? StartTime,
    bool Evaluate,
    string? Note,
    bool Import,
    string? Sequence,
    string? GroupMessage,
    string? SendCertificates,
    string? MoneyMessage,
    string? IXL);

public sealed class LegacyClassGroupXmlParser
{
    public LegacyClassGroupImportResult Parse(string xmlPath, string xsdPath)
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
            return new LegacyClassGroupImportResult(Array.Empty<LegacyClassGroupRecord>(), errors);
        }
        catch (Exception ex)
        {
            errors.Add(new LegacyXmlValidationError(ex.Message, null, null));
            return new LegacyClassGroupImportResult(Array.Empty<LegacyClassGroupRecord>(), errors);
        }

        var records = new List<LegacyClassGroupRecord>();
        foreach (var classGroupElement in document.Root?.Elements("Class_x0020_Group") ?? Enumerable.Empty<XElement>())
        {
            var record = new LegacyClassGroupRecord(
                ClassGroup: NormalizeString(GetValue(classGroupElement, "Class_x0020_Group")) ?? string.Empty,
                DayTruck: NormalizeString(GetValue(classGroupElement, "DayTruck")),
                Description: NormalizeString(GetValue(classGroupElement, "Description")),
                EndTime: NormalizeString(GetValue(classGroupElement, "End_x0020_Time")),
                SchoolId: ParseRequiredShort(GetValue(classGroupElement, "School_x0020_Id"), errors, "School_x0020_Id"),
                DayId: NormalizeString(GetValue(classGroupElement, "DayId")),
                StartTime: NormalizeString(GetValue(classGroupElement, "Start_x0020_Time")),
                Evaluate: ParseBool(GetValue(classGroupElement, "Evaluate"), false),
                Note: NormalizeString(GetValue(classGroupElement, "Note")),
                Import: ParseBool(GetValue(classGroupElement, "Import"), false),
                Sequence: NormalizeString(GetValue(classGroupElement, "Sequence")),
                GroupMessage: NormalizeString(GetValue(classGroupElement, "GroupMessage")),
                SendCertificates: NormalizeString(GetValue(classGroupElement, "Send_x0020_Certificates")),
                MoneyMessage: NormalizeString(GetValue(classGroupElement, "Money_x0020_Message")),
                IXL: NormalizeString(GetValue(classGroupElement, "IXL")));

            records.Add(record);
        }

        return new LegacyClassGroupImportResult(records, errors);
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

    private static short ParseRequiredShort(string? value, ICollection<LegacyXmlValidationError> errors, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new LegacyXmlValidationError($"{fieldName} is required.", null, null));
            return 0;
        }

        if (short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ||
            short.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out result))
        {
            return result;
        }

        errors.Add(new LegacyXmlValidationError($"Invalid {fieldName} value: '{value}'.", null, null));
        return 0;
    }

    private static bool ParseBool(string? value, bool defaultValue)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
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

        return defaultValue;
    }
}
