using Kcow.Domain.Entities;

namespace Kcow.Application.Import;

/// <summary>
/// Result of mapping a legacy Activity record to the Activity entity.
/// </summary>
public sealed record LegacyActivityMappingResult(Activity Activity, IReadOnlyList<string> Warnings);

/// <summary>
/// Maps legacy Activity XML records to Activity entities.
/// Handles field length validation and data transformation.
/// </summary>
public sealed class LegacyActivityMapper
{
    private const int MaxFieldLength = 255;

    public LegacyActivityMappingResult Map(LegacyActivityRecord record)
    {
        var warnings = new List<string>();

        // Validate and truncate field lengths
        var code = Truncate(record.Program, MaxFieldLength);
        var name = Truncate(record.ProgramName, MaxFieldLength);
        var folder = Truncate(record.Folder, MaxFieldLength);
        var gradeLevel = Truncate(record.Grade, MaxFieldLength);
        var description = record.EducationalFocus;

        // Track truncation warnings
        if (record.Program != null && record.Program.Length > MaxFieldLength)
        {
            warnings.Add($"Activity {record.ActivityId}: Program (Code) truncated from {record.Program.Length} to {MaxFieldLength} characters.");
        }
        if (record.ProgramName != null && record.ProgramName.Length > MaxFieldLength)
        {
            warnings.Add($"Activity {record.ActivityId}: ProgramName (Name) truncated from {record.ProgramName.Length} to {MaxFieldLength} characters.");
        }
        if (record.Folder != null && record.Folder.Length > MaxFieldLength)
        {
            warnings.Add($"Activity {record.ActivityId}: Folder truncated from {record.Folder.Length} to {MaxFieldLength} characters.");
        }
        if (record.Grade != null && record.Grade.Length > MaxFieldLength)
        {
            warnings.Add($"Activity {record.ActivityId}: Grade (GradeLevel) truncated from {record.Grade.Length} to {MaxFieldLength} characters.");
        }

        // Log large icon sizes for awareness
        if (!string.IsNullOrEmpty(record.Icon))
        {
            if (record.Icon.Length > 100_000) // ~100KB base64 = ~75KB image
            {
                var sizeKB = record.Icon.Length / 1024;
                warnings.Add($"Activity {record.ActivityId}: Large icon data detected ({sizeKB}KB base64).");
            }
            
            // Warning for potential OLE header (simple heuristic: valid images usually start with /9j/ (JPG) or iVBOR (PNG))
            // OLE wrappers often make the string start differently.
            if (!record.Icon.StartsWith("/9j/") && !record.Icon.StartsWith("iVBOR"))
            {
                warnings.Add($"Activity {record.ActivityId}: Icon data may contain OLE wrapper (does not start with standard JPG/PNG signature). Verify rendering.");
            }
        }

        var activity = new Activity
        {
            Id = record.ActivityId,
            Code = code,
            Name = name,
            Description = description,
            Folder = folder,
            GradeLevel = gradeLevel,
            Icon = record.Icon, // Already base64 from XML
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return new LegacyActivityMappingResult(activity, warnings);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return value.Length > maxLength ? value.Substring(0, maxLength) : value;
    }
}
