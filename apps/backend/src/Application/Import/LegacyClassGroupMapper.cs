using Kcow.Domain.Entities;

namespace Kcow.Application.Import;

public sealed record LegacyClassGroupMappingResult(ClassGroup? ClassGroup, IReadOnlyList<string> Warnings);

public sealed class LegacyClassGroupMapper
{
    private static readonly IReadOnlySet<int> EmptySchoolIds = new HashSet<int>();
    private static readonly IReadOnlySet<int> EmptyTruckIds = new HashSet<int>();

    private readonly IReadOnlySet<int> _validSchoolIds;
    private readonly IReadOnlySet<int> _validTruckIds;

    public LegacyClassGroupMapper() : this(EmptySchoolIds, EmptyTruckIds) { }

    public LegacyClassGroupMapper(IReadOnlySet<int> validSchoolIds, IReadOnlySet<int> validTruckIds)
    {
        _validSchoolIds = validSchoolIds;
        _validTruckIds = validTruckIds;
    }

    public LegacyClassGroupMappingResult Map(LegacyClassGroupRecord record)
    {
        var warnings = new List<string>();

        // Skip if Import flag is false
        if (!record.Import)
        {
            return new LegacyClassGroupMappingResult(null, Array.Empty<string>());
        }

        // Use Description as primary name, fall back to ClassGroup
        var name = Trim(record.Description) ?? Trim(record.ClassGroup) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            warnings.Add($"Class Group is missing a description and name.");
            return new LegacyClassGroupMappingResult(null, warnings);
        }

        // Validate SchoolId foreign key
        if (_validSchoolIds.Count > 0 && !_validSchoolIds.Contains(record.SchoolId))
        {
            warnings.Add($"Class Group references invalid SchoolId {record.SchoolId}. Skipping import.");
            return new LegacyClassGroupMappingResult(null, warnings);
        }

        // Parse DayId to DayOfWeek (1=Monday, 2=Tuesday, etc.)
        var dayOfWeek = DayOfWeek.Monday; // Default
        if (!string.IsNullOrWhiteSpace(record.DayId))
        {
            if (int.TryParse(record.DayId, out var dayNum) && dayNum >= 1 && dayNum <= 5)
            {
                dayOfWeek = (DayOfWeek)(dayNum);
            }
            else
            {
                warnings.Add($"Invalid DayId '{record.DayId}'. Defaulting to Monday.");
            }
        }

        // Parse Sequence (try to parse as int, default to 1)
        var sequence = 1;
        if (!string.IsNullOrWhiteSpace(record.Sequence) && int.TryParse(record.Sequence, out var seqNum))
        {
            sequence = seqNum > 0 ? seqNum : 1;
        }

        // Parse Start Time and End Time
        TimeOnly? startTime = null;
        TimeOnly? endTime = null;

        if (!string.IsNullOrWhiteSpace(record.StartTime))
        {
            if (TimeOnly.TryParse(record.StartTime, out var start))
            {
                startTime = start;
            }
            else
            {
                warnings.Add($"Invalid Start Time '{record.StartTime}'. Class Group will not be imported.");
                return new LegacyClassGroupMappingResult(null, warnings);
            }
        }

        if (!string.IsNullOrWhiteSpace(record.EndTime))
        {
            if (TimeOnly.TryParse(record.EndTime, out var end))
            {
                endTime = end;
            }
            else
            {
                warnings.Add($"Invalid End Time '{record.EndTime}'. Class Group will not be imported.");
                return new LegacyClassGroupMappingResult(null, warnings);
            }
        }

        // Validate time range
        if (startTime.HasValue && endTime.HasValue && endTime <= startTime)
        {
            warnings.Add($"End Time must be after Start Time. Class Group will not be imported.");
            return new LegacyClassGroupMappingResult(null, warnings);
        }

        // Extract TruckId from DayTruck field (format might be like "1", "Truck1", etc.)
        int? truckId = null;
        if (!string.IsNullOrWhiteSpace(record.DayTruck))
        {
            // Try parsing as integer first
            if (int.TryParse(record.DayTruck, out var truckNum))
            {
                if (_validTruckIds.Count > 0 && _validTruckIds.Contains(truckNum))
                {
                    truckId = truckNum;
                }
                else if (_validTruckIds.Count == 0)
                {
                    truckId = truckNum; // No validation set, use as-is
                }
                else
                {
                    warnings.Add($"Class Group references invalid TruckId {truckNum}. Truck will be set to null.");
                }
            }
        }

        var classGroup = new ClassGroup
        {
            Name = name,
            DayTruck = Trim(record.DayTruck),
            Description = Trim(record.Description),
            SchoolId = record.SchoolId,
            TruckId = truckId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime ?? new TimeOnly(8, 0), // Default 8:00 AM
            EndTime = endTime ?? new TimeOnly(9, 0),     // Default 9:00 AM
            Sequence = sequence,
            Evaluate = record.Evaluate,
            Notes = Trim(record.Note),
            ImportFlag = record.Import,
            GroupMessage = Trim(record.GroupMessage),
            SendCertificates = Trim(record.SendCertificates),
            MoneyMessage = Trim(record.MoneyMessage),
            Ixl = Trim(record.IXL),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return new LegacyClassGroupMappingResult(classGroup, warnings);
    }

    private static string? Trim(string? value) => value?.Trim();
}
