namespace Kcow.Application.Import.Mappers;

/// <summary>
/// Result of a mapping operation containing the mapped data, warnings, and errors.
/// </summary>
/// <typeparam name="T">The type of the mapped result</typeparam>
public sealed class MappingResult<T>
{
    /// <summary>Mapped entity, or null if mapping failed.</summary>
    public T? Data { get; set; }

    /// <summary>True when Data is populated and no errors occurred.</summary>
    public bool Success { get; set; }

    /// <summary>Non-fatal issues encountered during mapping (e.g., truncated fields, default values used).</summary>
    public List<MappingWarning> Warnings { get; set; } = new();

    /// <summary>Fatal issues that prevented successful mapping.</summary>
    public List<MappingError> Errors { get; set; } = new();

    /// <summary>True if any warnings were recorded.</summary>
    public bool HasWarnings => Warnings.Count > 0;

    /// <summary>True if any errors were recorded.</summary>
    public bool HasErrors => Errors.Count > 0;

    /// <summary>Creates a successful mapping result.</summary>
    public static MappingResult<T> Ok(T data) => new()
    {
        Data = data,
        Success = true
    };

    /// <summary>Creates a failed mapping result with a single error.</summary>
    public static MappingResult<T> Fail(string field, string message) => new()
    {
        Success = false,
        Errors = { new MappingError(field, message) }
    };

    /// <summary>Creates a skipped mapping result (record intentionally not imported).</summary>
    public static MappingResult<T> Skipped(string reason) => new()
    {
        Success = false,
        Warnings = { new MappingWarning("_skip", reason) }
    };
}

/// <summary>
/// Non-fatal issue encountered during mapping.
/// Records what field was affected and what transformation occurred.
/// </summary>
public sealed class MappingWarning
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? OriginalValue { get; set; }
    public string? MappedValue { get; set; }

    public MappingWarning() { }

    public MappingWarning(string field, string message, string? originalValue = null, string? mappedValue = null)
    {
        Field = field;
        Message = message;
        OriginalValue = originalValue;
        MappedValue = mappedValue;
    }
}

/// <summary>
/// Fatal issue that prevented a record from being mapped.
/// </summary>
public sealed class MappingError
{
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public MappingError() { }

    public MappingError(string field, string message)
    {
        Field = field;
        Message = message;
    }
}
