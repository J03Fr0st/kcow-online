using Kcow.Domain.Entities;

namespace Kcow.Application.Import.Mappers;

/// <summary>
/// Maps LegacyClassGroupRecord to ClassGroup entity.
/// Mapping rules:
///   Description → Name (fallback to ClassGroup code)
///   DayId (string "1"-"5") → DayOfWeek enum (1=Monday)
///   StartTime/EndTime (string) → TimeOnly
///   DayTruck (string) → TruckId (validated against known truck IDs)
///   Sequence (string) → Sequence (int, defaults to 1)
///   Records with Import=false are skipped.
///   Records with invalid SchoolId are rejected.
/// </summary>
public sealed class ClassGroupDataMapper : IDataMapper<LegacyClassGroupRecord, ClassGroup>
{
    private static readonly IReadOnlySet<int> EmptyIds = new HashSet<int>();
    private readonly IReadOnlySet<int> _validSchoolIds;
    private readonly IReadOnlySet<int> _validTruckIds;

    public ClassGroupDataMapper() : this(EmptyIds, EmptyIds) { }

    public ClassGroupDataMapper(IReadOnlySet<int> validSchoolIds)
        : this(validSchoolIds, EmptyIds) { }

    public ClassGroupDataMapper(IReadOnlySet<int> validSchoolIds, IReadOnlySet<int> validTruckIds)
    {
        _validSchoolIds = validSchoolIds;
        _validTruckIds = validTruckIds;
    }

    public MappingResult<ClassGroup> Map(LegacyClassGroupRecord source)
    {
        // Skip if Import flag is false
        if (!source.Import)
        {
            return MappingResult<ClassGroup>.Skipped("Import flag is false");
        }

        var name = Trim(source.Description) ?? Trim(source.ClassGroup) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            return MappingResult<ClassGroup>.Fail("Name", "Class Group is missing a description and name.");
        }

        // Validate SchoolId
        if (_validSchoolIds.Count > 0 && !_validSchoolIds.Contains(source.SchoolId))
        {
            return MappingResult<ClassGroup>.Fail("SchoolId",
                $"Class Group references invalid SchoolId {source.SchoolId}.");
        }

        var result = new MappingResult<ClassGroup> { Success = true };

        // Parse DayId to DayOfWeek (1=Monday, 2=Tuesday, etc.)
        var dayOfWeek = DayOfWeek.Monday;
        if (!string.IsNullOrWhiteSpace(source.DayId))
        {
            if (int.TryParse(source.DayId, out var dayNum) && dayNum >= 1 && dayNum <= 5)
            {
                dayOfWeek = (DayOfWeek)dayNum;
            }
            else
            {
                result.Warnings.Add(new MappingWarning("DayId",
                    $"Invalid DayId '{source.DayId}'. Defaulting to Monday.",
                    source.DayId, "1"));
            }
        }

        // Parse Sequence
        var sequence = 1;
        if (!string.IsNullOrWhiteSpace(source.Sequence) && int.TryParse(source.Sequence, out var seqNum))
        {
            sequence = seqNum > 0 ? seqNum : 1;
        }

        // Parse Start/End Time
        TimeOnly? startTime = null;
        if (!string.IsNullOrWhiteSpace(source.StartTime))
        {
            if (TimeOnly.TryParse(source.StartTime, out var start))
                startTime = start;
            else
                return MappingResult<ClassGroup>.Fail("StartTime",
                    $"Invalid Start Time '{source.StartTime}'.");
        }

        TimeOnly? endTime = null;
        if (!string.IsNullOrWhiteSpace(source.EndTime))
        {
            if (TimeOnly.TryParse(source.EndTime, out var end))
                endTime = end;
            else
                return MappingResult<ClassGroup>.Fail("EndTime",
                    $"Invalid End Time '{source.EndTime}'.");
        }

        if (startTime.HasValue && endTime.HasValue && endTime <= startTime)
        {
            return MappingResult<ClassGroup>.Fail("EndTime",
                "End Time must be after Start Time.");
        }

        // Parse TruckId from DayTruck
        int? truckId = null;
        if (!string.IsNullOrWhiteSpace(source.DayTruck) && int.TryParse(source.DayTruck, out var truckNum))
        {
            if (_validTruckIds.Count > 0 && _validTruckIds.Contains(truckNum))
            {
                truckId = truckNum;
            }
            else if (_validTruckIds.Count == 0)
            {
                truckId = truckNum;
            }
            else
            {
                result.Warnings.Add(new MappingWarning("TruckId",
                    $"Class Group references invalid TruckId {truckNum}. Truck will be set to null.",
                    truckNum.ToString(), null));
            }
        }

        result.Data = new ClassGroup
        {
            Name = name,
            DayTruck = Trim(source.DayTruck),
            Description = Trim(source.Description),
            SchoolId = source.SchoolId,
            TruckId = truckId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime ?? new TimeOnly(8, 0),
            EndTime = endTime ?? new TimeOnly(9, 0),
            Sequence = sequence,
            Evaluate = source.Evaluate,
            Notes = Trim(source.Note),
            ImportFlag = source.Import,
            GroupMessage = Trim(source.GroupMessage),
            SendCertificates = Trim(source.SendCertificates),
            MoneyMessage = Trim(source.MoneyMessage),
            Ixl = Trim(source.IXL),
            IsActive = true,
            LegacyId = source.ClassGroup,
            CreatedAt = DateTime.UtcNow
        };

        return result;
    }

    public MappingResult<List<ClassGroup>> MapMany(IEnumerable<LegacyClassGroupRecord> sources)
    {
        var groups = new List<ClassGroup>();
        var allWarnings = new List<MappingWarning>();
        var allErrors = new List<MappingError>();

        foreach (var source in sources)
        {
            var result = Map(source);
            if (result.Success && result.Data is not null)
            {
                groups.Add(result.Data);
            }
            allWarnings.AddRange(result.Warnings);
            allErrors.AddRange(result.Errors);
        }

        return new MappingResult<List<ClassGroup>>
        {
            Data = groups,
            Success = allErrors.Count == 0,
            Warnings = allWarnings,
            Errors = allErrors
        };
    }

    private static string? Trim(string? value) => value?.Trim();
}
