using AttendanceStatus = Kcow.Domain.Entities.AttendanceStatus;

namespace Kcow.Application.Import;

/// <summary>
/// Input record for attendance import mapping.
/// </summary>
public sealed record LegacyAttendanceImportRecord(
    int StudentId,
    int ClassGroupId,
    string SessionDate,
    string Status,
    string? Notes,
    DateTime? OriginalCreatedAt,
    DateTime? OriginalModifiedAt);

/// <summary>
/// Result of mapping a legacy attendance record to the Attendance entity.
/// </summary>
public sealed record LegacyAttendanceMappingResult(Kcow.Domain.Entities.Attendance? Attendance, IReadOnlyList<string> Warnings);

/// <summary>
/// Maps legacy attendance data to Attendance entities.
/// Validates student and class group references and maps status values.
/// </summary>
public sealed class LegacyAttendanceMapper
{
    private readonly HashSet<int> _validStudentIds;
    private readonly HashSet<int> _validClassGroupIds;

    public LegacyAttendanceMapper(IEnumerable<int> validStudentIds, IEnumerable<int> validClassGroupIds)
    {
        _validStudentIds = new HashSet<int>(validStudentIds);
        _validClassGroupIds = new HashSet<int>(validClassGroupIds);
    }

    public LegacyAttendanceMappingResult Map(LegacyAttendanceImportRecord record)
    {
        var warnings = new List<string>();

        // Validate student reference
        if (!_validStudentIds.Contains(record.StudentId))
        {
            warnings.Add($"Student ID {record.StudentId} not found in database. Skipping attendance record.");
            return new LegacyAttendanceMappingResult(null, warnings);
        }

        // Validate class group reference
        if (!_validClassGroupIds.Contains(record.ClassGroupId))
        {
            warnings.Add($"Class Group ID {record.ClassGroupId} not found in database. Skipping attendance record.");
            return new LegacyAttendanceMappingResult(null, warnings);
        }

        // Validate session date
        var isoDate = LegacyAttendanceEvaluationXmlParser.ParseDateToIso(record.SessionDate);
        if (isoDate == null)
        {
            warnings.Add($"Invalid session date '{record.SessionDate}' for Student {record.StudentId}. Skipping.");
            return new LegacyAttendanceMappingResult(null, warnings);
        }

        // Map status
        var status = MapStatus(record.Status, warnings, record.StudentId);

        // Preserve historical timestamps
        var createdAt = record.OriginalCreatedAt ?? DateTime.UtcNow;
        var modifiedAt = record.OriginalModifiedAt;

        var attendance = new Kcow.Domain.Entities.Attendance
        {
            StudentId = record.StudentId,
            ClassGroupId = record.ClassGroupId,
            SessionDate = isoDate,
            Status = status,
            Notes = record.Notes,
            CreatedAt = createdAt,
            ModifiedAt = modifiedAt
        };

        return new LegacyAttendanceMappingResult(attendance, warnings);
    }

    private static AttendanceStatus MapStatus(string? statusValue, List<string> warnings, int studentId)
    {
        if (string.IsNullOrWhiteSpace(statusValue))
        {
            return AttendanceStatus.Present; // Default
        }

        var normalized = statusValue.Trim().ToLowerInvariant();

        return normalized switch
        {
            "present" or "p" or "1" or "true" or "yes" or "teenwoordig" => AttendanceStatus.Present,
            "absent" or "a" or "0" or "false" or "no" or "afwesig" => AttendanceStatus.Absent,
            "late" or "l" or "laat" => AttendanceStatus.Late,
            _ => HandleUnknownStatus(statusValue, warnings, studentId)
        };
    }

    private static AttendanceStatus HandleUnknownStatus(string statusValue, List<string> warnings, int studentId)
    {
        warnings.Add($"Unknown attendance status '{statusValue}' for Student {studentId}. Defaulting to Present.");
        return AttendanceStatus.Present;
    }
}
