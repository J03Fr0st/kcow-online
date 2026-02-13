using Kcow.Domain.Entities;

namespace Kcow.Application.Import.Mappers;

/// <summary>
/// Maps LegacyActivityRecord to Activity entity.
/// Mapping rules:
///   Program → Code (truncated to 255 chars)
///   ProgramName → Name (truncated to 255 chars)
///   EducationalFocus → Description (no limit)
///   Folder → Folder (truncated to 255 chars)
///   Grade → GradeLevel (truncated to 255 chars)
///   Icon → Icon (base64 string, warns if large or has OLE wrapper)
/// </summary>
public sealed class ActivityDataMapper : IDataMapper<LegacyActivityRecord, Activity>
{
    private const int MaxFieldLength = 255;

    public MappingResult<Activity> Map(LegacyActivityRecord source)
    {
        var result = new MappingResult<Activity> { Success = true };

        var code = Truncate(source.Program, MaxFieldLength);
        var name = Truncate(source.ProgramName, MaxFieldLength);
        var folder = Truncate(source.Folder, MaxFieldLength);
        var gradeLevel = Truncate(source.Grade, MaxFieldLength);

        // Track truncation warnings
        if (source.Program?.Length > MaxFieldLength)
            result.Warnings.Add(new MappingWarning("Code",
                $"Activity {source.ActivityId}: Program (Code) truncated from {source.Program.Length} to {MaxFieldLength} characters."));

        if (source.ProgramName?.Length > MaxFieldLength)
            result.Warnings.Add(new MappingWarning("Name",
                $"Activity {source.ActivityId}: ProgramName (Name) truncated from {source.ProgramName.Length} to {MaxFieldLength} characters."));

        if (source.Folder?.Length > MaxFieldLength)
            result.Warnings.Add(new MappingWarning("Folder",
                $"Activity {source.ActivityId}: Folder truncated from {source.Folder.Length} to {MaxFieldLength} characters."));

        if (source.Grade?.Length > MaxFieldLength)
            result.Warnings.Add(new MappingWarning("GradeLevel",
                $"Activity {source.ActivityId}: Grade (GradeLevel) truncated from {source.Grade.Length} to {MaxFieldLength} characters."));

        // Icon validation
        if (!string.IsNullOrEmpty(source.Icon))
        {
            if (source.Icon.Length > 100_000)
            {
                var sizeKB = source.Icon.Length / 1024;
                result.Warnings.Add(new MappingWarning("Icon",
                    $"Activity {source.ActivityId}: Large icon data detected ({sizeKB}KB base64)."));
            }

            if (!source.Icon.StartsWith("/9j/") && !source.Icon.StartsWith("iVBOR"))
            {
                result.Warnings.Add(new MappingWarning("Icon",
                    $"Activity {source.ActivityId}: Icon data may contain OLE wrapper (does not start with standard JPG/PNG signature). Verify rendering."));
            }
        }

        result.Data = new Activity
        {
            Id = source.ActivityId,
            Code = code,
            Name = name,
            Description = source.EducationalFocus,
            Folder = folder,
            GradeLevel = gradeLevel,
            Icon = source.Icon,
            IsActive = true,
            LegacyId = source.ActivityId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        return result;
    }

    public MappingResult<List<Activity>> MapMany(IEnumerable<LegacyActivityRecord> sources)
    {
        var activities = new List<Activity>();
        var allWarnings = new List<MappingWarning>();
        var allErrors = new List<MappingError>();

        foreach (var source in sources)
        {
            var result = Map(source);
            if (result.Success && result.Data is not null)
            {
                activities.Add(result.Data);
            }
            allWarnings.AddRange(result.Warnings);
            allErrors.AddRange(result.Errors);
        }

        return new MappingResult<List<Activity>>
        {
            Data = activities,
            Success = true,
            Warnings = allWarnings,
            Errors = allErrors
        };
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return null;
        return value.Length > maxLength ? value[..maxLength] : value;
    }
}
