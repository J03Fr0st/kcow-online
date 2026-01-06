namespace Kcow.Application.Import;

public sealed record LegacyImportAuditEntry(
    DateTime Timestamp,
    string SourceFile,
    string Message,
    int? LineNumber,
    int? LinePosition);

public sealed class LegacyImportAuditLog
{
    private readonly List<LegacyImportAuditEntry> _entries = new();

    public IReadOnlyList<LegacyImportAuditEntry> Entries => _entries;

    public void AddValidationErrors(string sourceFile, IEnumerable<LegacyXmlValidationError> errors)
    {
        foreach (var error in errors)
        {
            _entries.Add(new LegacyImportAuditEntry(
                DateTime.UtcNow,
                sourceFile,
                error.Message,
                error.LineNumber,
                error.LinePosition));
        }
    }

    public void WriteTo(TextWriter writer)
    {
        foreach (var entry in _entries)
        {
            writer.WriteLine(
                $"Import validation error in {entry.SourceFile} (Line {entry.LineNumber}, Position {entry.LinePosition}): {entry.Message}");
        }
    }
}
