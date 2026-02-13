namespace Kcow.Application.Import;

/// <summary>
/// Unified interface for parsing legacy XML data files.
/// Implementations handle XSD validation, encoding issues, and error reporting.
/// </summary>
public interface ILegacyParser
{
    /// <summary>
    /// Parses School XML file against XSD schema.
    /// </summary>
    ParseResult<LegacySchoolRecord> ParseSchools(string xmlPath, string xsdPath);

    /// <summary>
    /// Parses ClassGroup XML file against XSD schema.
    /// </summary>
    ParseResult<LegacyClassGroupRecord> ParseClassGroups(string xmlPath, string xsdPath);

    /// <summary>
    /// Parses Activity XML file against XSD schema.
    /// </summary>
    ParseResult<LegacyActivityRecord> ParseActivities(string xmlPath, string xsdPath);

    /// <summary>
    /// Parses Children (Student) XML file against XSD schema.
    /// </summary>
    ParseResult<LegacyChildRecord> ParseChildren(string xmlPath, string xsdPath);
}

/// <summary>
/// Generic result container for legacy XML parsing operations.
/// </summary>
/// <typeparam name="T">The type of parsed record</typeparam>
public sealed class ParseResult<T>
{
    /// <summary>
    /// Successfully parsed records.
    /// </summary>
    public List<T> Records { get; set; } = new();

    /// <summary>
    /// Errors encountered during parsing or validation.
    /// </summary>
    public List<ParseError> Errors { get; set; } = new();

    /// <summary>
    /// Indicates whether any errors occurred.
    /// </summary>
    public bool HasErrors => Errors.Any();

    /// <summary>
    /// The source file that was parsed.
    /// </summary>
    public string? SourceFile { get; set; }
}

/// <summary>
/// Represents a parsing error with location information.
/// </summary>
public sealed class ParseError
{
    /// <summary>
    /// The file where the error occurred.
    /// </summary>
    public string File { get; set; } = string.Empty;

    /// <summary>
    /// Line number where the error occurred, if available.
    /// </summary>
    public int? Line { get; set; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    public ParseError() { }

    public ParseError(string file, int? line, string message)
    {
        File = file;
        Line = line;
        Message = message;
    }

    public override string ToString() => Line.HasValue
        ? $"{File}:{Line}: {Message}"
        : $"{File}: {Message}";
}
