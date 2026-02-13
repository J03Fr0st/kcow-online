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

        // Icon: strip OLE wrapper and extract raw image data
        var icon = StripOleWrapper(source.Icon, source.ActivityId, result);

        result.Data = new Activity
        {
            Id = source.ActivityId,
            Code = code,
            Name = name,
            Description = source.EducationalFocus,
            Folder = folder,
            GradeLevel = gradeLevel,
            Icon = icon,
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

    /// <summary>
    /// Strips OLE Object wrapper from base64-encoded icon data exported from MS Access.
    /// OLE objects contain a header followed by the actual image (typically BMP).
    /// We find the BMP signature (0x42 0x4D = "BM") and return only the image data.
    /// </summary>
    private static string? StripOleWrapper(string? base64Data, int activityId, MappingResult<Activity> result)
    {
        if (string.IsNullOrEmpty(base64Data))
            return null;

        try
        {
            var raw = Convert.FromBase64String(base64Data);

            // Already a valid image? (JPEG, PNG, BMP at offset 0)
            if (raw.Length >= 2 && raw[0] == 0xFF && raw[1] == 0xD8) return base64Data; // JPEG
            if (raw.Length >= 4 && raw[0] == 0x89 && raw[1] == 0x50) return base64Data; // PNG
            if (raw.Length >= 2 && raw[0] == 0x42 && raw[1] == 0x4D) return base64Data; // BMP

            // Search for BMP signature within OLE wrapper
            for (var i = 1; i < Math.Min(raw.Length - 1, 512); i++)
            {
                if (raw[i] == 0x42 && raw[i + 1] == 0x4D) // "BM"
                {
                    var imageData = new byte[raw.Length - i];
                    Array.Copy(raw, i, imageData, 0, imageData.Length);
                    return Convert.ToBase64String(imageData);
                }
            }

            // Search for JPEG signature
            for (var i = 1; i < Math.Min(raw.Length - 2, 512); i++)
            {
                if (raw[i] == 0xFF && raw[i + 1] == 0xD8 && raw[i + 2] == 0xFF)
                {
                    var imageData = new byte[raw.Length - i];
                    Array.Copy(raw, i, imageData, 0, imageData.Length);
                    return Convert.ToBase64String(imageData);
                }
            }

            // No recognizable image found, return as-is with warning
            result.Warnings.Add(new MappingWarning("Icon",
                $"Activity {activityId}: Could not find image data in OLE wrapper."));
            return base64Data;
        }
        catch (FormatException)
        {
            result.Warnings.Add(new MappingWarning("Icon",
                $"Activity {activityId}: Invalid base64 data in icon field."));
            return null;
        }
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return null;
        return value.Length > maxLength ? value[..maxLength] : value;
    }
}
