using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Kcow.Application.Import;

/// <summary>
/// Result of parsing legacy Activity XML for attendance and evaluation data extraction.
/// The Activity XML is the source for understanding which programs/activities exist.
/// Attendance and evaluation records are derived from student-class group-activity relationships.
/// </summary>
public sealed record LegacyAttendanceEvaluationParseResult(
    IReadOnlyList<LegacyActivityRecord> ActivityRecords,
    IReadOnlyList<LegacyXmlValidationError> ValidationErrors);

/// <summary>
/// Parses legacy Activity XML files to extract activity context for attendance/evaluation import.
/// Reuses the Activity XSD validation and extends parsing for relationship mapping.
/// </summary>
public sealed class LegacyAttendanceEvaluationXmlParser
{
    /// <summary>
    /// Parses Activity XML to extract activity records that serve as the basis
    /// for attendance and evaluation imports. Validates against XSD schema.
    /// </summary>
    public LegacyAttendanceEvaluationParseResult Parse(string xmlPath, string xsdPath)
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
            return new LegacyAttendanceEvaluationParseResult(Array.Empty<LegacyActivityRecord>(), errors);
        }
        catch (Exception ex)
        {
            errors.Add(new LegacyXmlValidationError(ex.Message, null, null));
            return new LegacyAttendanceEvaluationParseResult(Array.Empty<LegacyActivityRecord>(), errors);
        }

        var records = new List<LegacyActivityRecord>();
        foreach (var activityElement in document.Root?.Elements("Activity") ?? Enumerable.Empty<XElement>())
        {
            var activityId = ParseRequiredInt(GetValue(activityElement, "ActivityID"), errors, "ActivityID");
            if (!activityId.HasValue)
            {
                // Skip this record due to parsing error
                continue;
            }

            var record = new LegacyActivityRecord(
                ActivityId: activityId.Value,
                Program: NormalizeString(GetValue(activityElement, "Program")),
                ProgramName: NormalizeString(GetValue(activityElement, "ProgramName")),
                EducationalFocus: NormalizeString(GetValue(activityElement, "Educational_x0020_Focus")),
                Folder: NormalizeString(GetValue(activityElement, "Folder")),
                Grade: NormalizeString(GetValue(activityElement, "Grade")),
                Icon: GetValue(activityElement, "Icon")
            );

            records.Add(record);
        }

        return new LegacyAttendanceEvaluationParseResult(records, errors);
    }

    /// <summary>
    /// Parses date/time format variations from legacy data.
    /// Supports ISO 8601, Access datetime format, and common date formats.
    /// </summary>
    public static string? ParseDateToIso(string? dateValue)
    {
        if (string.IsNullOrWhiteSpace(dateValue))
        {
            return null;
        }

        var formats = new[]
        {
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd",
            "MM/dd/yyyy",
            "dd/MM/yyyy",
            "M/d/yyyy",
            "d/M/yyyy",
            "yyyy-MM-dd HH:mm:ss"
        };

        if (DateTime.TryParseExact(dateValue.Trim(), formats,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out var parsed))
        {
            return parsed.ToString("yyyy-MM-dd");
        }

        if (DateTime.TryParse(dateValue.Trim(), System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var fallback))
        {
            return fallback.ToString("yyyy-MM-dd");
        }

        return null;
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
            IgnoreWhitespace = true,
        };

        settings.ValidationEventHandler += (sender, e) =>
        {
            int? lineNumber = null;
            int? linePosition = null;
            if (e.Exception is XmlSchemaValidationException schemaEx)
            {
                lineNumber = schemaEx.LineNumber;
                linePosition = schemaEx.LinePosition;
            }
            errors.Add(new LegacyXmlValidationError(e.Message, lineNumber, linePosition));
        };

        return settings;
    }

    private static string? GetValue(XElement element, string name)
    {
        return element.Element(name)?.Value;
    }

    private static string? NormalizeString(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static int? ParseRequiredInt(string? value, ICollection<LegacyXmlValidationError> errors, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new LegacyXmlValidationError($"{fieldName} is required but was missing.", null, null));
            return null;
        }

        if (!int.TryParse(value, out var result))
        {
            errors.Add(new LegacyXmlValidationError($"{fieldName} must be a valid integer but was '{value}'.", null, null));
            return null;
        }

        return result;
    }
}
