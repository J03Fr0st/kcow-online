namespace Kcow.Application.Import;

/// <summary>
/// Unified legacy XML parser that orchestrates parsing of all entity types.
/// Wraps individual entity parsers and provides a consistent interface.
/// </summary>
public sealed class LegacyParser : ILegacyParser
{
    private readonly LegacySchoolXmlParser _schoolParser = new();
    private readonly LegacyClassGroupXmlParser _classGroupParser = new();
    private readonly LegacyActivityXmlParser _activityParser = new();
    private readonly LegacyChildXmlParser _childParser = new();

    /// <inheritdoc />
    public ParseResult<LegacySchoolRecord> ParseSchools(string xmlPath, string xsdPath)
    {
        var result = new ParseResult<LegacySchoolRecord> { SourceFile = xmlPath };

        try
        {
            var parseResult = _schoolParser.Parse(xmlPath, xsdPath);
            result.Records.AddRange(parseResult.Records);
            result.Errors.AddRange(parseResult.ValidationErrors
                .Select(e => new ParseError(xmlPath, e.LineNumber, e.Message)));
        }
        catch (Exception ex)
        {
            result.Errors.Add(new ParseError(xmlPath, null, ex.Message));
        }

        return result;
    }

    /// <inheritdoc />
    public ParseResult<LegacyClassGroupRecord> ParseClassGroups(string xmlPath, string xsdPath)
    {
        var result = new ParseResult<LegacyClassGroupRecord> { SourceFile = xmlPath };

        try
        {
            var parseResult = _classGroupParser.Parse(xmlPath, xsdPath);
            result.Records.AddRange(parseResult.Records);
            result.Errors.AddRange(parseResult.ValidationErrors
                .Select(e => new ParseError(xmlPath, e.LineNumber, e.Message)));
        }
        catch (Exception ex)
        {
            result.Errors.Add(new ParseError(xmlPath, null, ex.Message));
        }

        return result;
    }

    /// <inheritdoc />
    public ParseResult<LegacyActivityRecord> ParseActivities(string xmlPath, string xsdPath)
    {
        var result = new ParseResult<LegacyActivityRecord> { SourceFile = xmlPath };

        try
        {
            var parseResult = _activityParser.Parse(xmlPath, xsdPath);
            result.Records.AddRange(parseResult.Records);
            result.Errors.AddRange(parseResult.ValidationErrors
                .Select(e => new ParseError(xmlPath, e.LineNumber, e.Message)));
        }
        catch (Exception ex)
        {
            result.Errors.Add(new ParseError(xmlPath, null, ex.Message));
        }

        return result;
    }

    /// <inheritdoc />
    public ParseResult<LegacyChildRecord> ParseChildren(string xmlPath, string xsdPath)
    {
        var result = new ParseResult<LegacyChildRecord> { SourceFile = xmlPath };

        try
        {
            var parseResult = _childParser.Parse(xmlPath, xsdPath);
            result.Records.AddRange(parseResult.Records);
            result.Errors.AddRange(parseResult.ValidationErrors
                .Select(e => new ParseError(xmlPath, e.LineNumber, e.Message)));
        }
        catch (Exception ex)
        {
            result.Errors.Add(new ParseError(xmlPath, null, ex.Message));
        }

        return result;
    }
}
