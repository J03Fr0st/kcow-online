using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Kcow.Application.Import;

/// <summary>
/// Result of parsing legacy Activity XML file.
/// </summary>
public sealed record LegacyActivityImportResult(
    IReadOnlyList<LegacyActivityRecord> Records,
    IReadOnlyList<LegacyXmlValidationError> ValidationErrors);

/// <summary>
/// Legacy Activity XML record matching the XSD structure.
/// Field names match the legacy XML schema for clarity.
/// </summary>
/// <param name="ActivityId">ActivityID - Primary key from legacy system</param>
/// <param name="Program">Program - Activity code/identifier (255 chars max)</param>
/// <param name="ProgramName">ProgramName - Display name (255 chars max)</param>
/// <param name="EducationalFocus">Educational Focus (encoded as Educational_x0020_Focus) - Long text description</param>
/// <param name="Folder">Folder - File system folder (255 chars max)</param>
/// <param name="Grade">Grade - Target grade level (255 chars max)</param>
/// <param name="Icon">Icon - Base64 encoded OLE object image data</param>
public sealed record LegacyActivityRecord(
    int ActivityId,
    string? Program,
    string? ProgramName,
    string? EducationalFocus,
    string? Folder,
    string? Grade,
    string? Icon);

/// <summary>
/// Parses legacy Activity XML files from the Access export format.
/// Handles XML encoding (e.g., Educational_x0020_Focus for "Educational Focus").
/// </summary>
public sealed class LegacyActivityXmlParser
{
    public LegacyActivityImportResult Parse(string xmlPath, string xsdPath)
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
            return new LegacyActivityImportResult(Array.Empty<LegacyActivityRecord>(), errors);
        }
        catch (Exception ex)
        {
            errors.Add(new LegacyXmlValidationError(ex.Message, null, null));
            return new LegacyActivityImportResult(Array.Empty<LegacyActivityRecord>(), errors);
        }

        var records = new List<LegacyActivityRecord>();
        foreach (var activityElement in document.Root?.Elements("Activity") ?? Enumerable.Empty<XElement>())
        {
            // Note: Educational Focus is encoded as "Educational_x0020_Focus" in XML
            var record = new LegacyActivityRecord(
                ActivityId: ParseRequiredInt(GetValue(activityElement, "ActivityID"), errors, "ActivityID"),
                Program: NormalizeString(GetValue(activityElement, "Program")),
                ProgramName: NormalizeString(GetValue(activityElement, "ProgramName")),
                EducationalFocus: NormalizeString(GetValue(activityElement, "Educational_x0020_Focus")),
                Folder: NormalizeString(GetValue(activityElement, "Folder")),
                Grade: NormalizeString(GetValue(activityElement, "Grade")),
                Icon: GetValue(activityElement, "Icon") // Keep raw base64, no normalization
            );

            records.Add(record);
        }

        return new LegacyActivityImportResult(records, errors);
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

        settings.ValidationEvents += (sender, e) =>
        {
            errors.Add(new LegacyXmlValidationError(e.Message, null, null));
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

    private static int ParseRequiredInt(string? value, ICollection<LegacyXmlValidationError> errors, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new LegacyXmlValidationError($"{fieldName} is required but was missing.", null, null));
            return 0;
        }

        if (!int.TryParse(value, out var result))
        {
            errors.Add(new LegacyXmlValidationError($"{fieldName} must be a valid integer but was '{value}'.", null, null));
            return 0;
        }

        return result;
    }
}
