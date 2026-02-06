using Kcow.Application.Attendance;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Attendance;

/// <summary>
/// Implementation of attendance management service using Dapper repositories.
/// </summary>
public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly ILogger<AttendanceService> _logger;

    public AttendanceService(IAttendanceRepository attendanceRepository, ILogger<AttendanceService> logger)
    {
        _attendanceRepository = attendanceRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets attendance records with optional filters.
    /// </summary>
    public async Task<List<AttendanceDto>> GetFilteredAsync(
        int? studentId = null,
        int? classGroupId = null,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var records = await _attendanceRepository.GetFilteredAsync(studentId, classGroupId, fromDate, toDate, cancellationToken);

        var dtos = records.Select(MapToDto).ToList();

        _logger.LogInformation("Retrieved {Count} attendance records with filters: studentId={StudentId}, classGroupId={ClassGroupId}, fromDate={FromDate}, toDate={ToDate}",
            dtos.Count, studentId, classGroupId, fromDate, toDate);

        return dtos;
    }

    /// <summary>
    /// Gets a single attendance record by ID.
    /// </summary>
    public async Task<AttendanceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var record = await _attendanceRepository.GetByIdAsync(id, cancellationToken);

        if (record == null)
        {
            _logger.LogWarning("Attendance record with ID {AttendanceId} not found", id);
            return null;
        }

        _logger.LogInformation("Retrieved attendance record with ID {AttendanceId}", id);
        return MapToDto(record);
    }

    /// <summary>
    /// Gets attendance history for a specific student.
    /// </summary>
    public async Task<List<AttendanceDto>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var records = await _attendanceRepository.GetByStudentIdAsync(studentId, cancellationToken);
        var dtos = records.Select(MapToDto).ToList();

        _logger.LogInformation("Retrieved {Count} attendance records for student {StudentId}", dtos.Count, studentId);
        return dtos;
    }

    /// <summary>
    /// Creates a new attendance record.
    /// </summary>
    public async Task<AttendanceDto> CreateAsync(CreateAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        if (!TryParseStatus(request.Status, out var statusEnum))
        {
            throw new InvalidOperationException($"Invalid attendance status: '{request.Status}'. Valid values are: Present, Absent, Late");
        }

        var attendance = new Domain.Entities.Attendance
        {
            StudentId = request.StudentId,
            ClassGroupId = request.ClassGroupId,
            SessionDate = request.SessionDate,
            Status = statusEnum,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _attendanceRepository.CreateAsync(attendance, cancellationToken);
        attendance.Id = id;

        _logger.LogInformation("Created attendance record with ID {AttendanceId} for student {StudentId} on {SessionDate}",
            id, request.StudentId, request.SessionDate);

        // Fetch the full record with joined names
        var created = await _attendanceRepository.GetByIdAsync(id, cancellationToken);
        return MapToDto(created!);
    }

    /// <summary>
    /// Updates an existing attendance record (triggers audit via ModifiedAt).
    /// </summary>
    public async Task<AttendanceDto?> UpdateAsync(int id, UpdateAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _attendanceRepository.GetByIdAsync(id, cancellationToken);

        if (existing == null)
        {
            _logger.LogWarning("Cannot update: Attendance record with ID {AttendanceId} not found", id);
            return null;
        }

        if (!TryParseStatus(request.Status, out var statusEnum))
        {
            throw new InvalidOperationException($"Invalid attendance status: '{request.Status}'. Valid values are: Present, Absent, Late");
        }

        var attendance = new Domain.Entities.Attendance
        {
            Id = id,
            Status = statusEnum,
            Notes = request.Notes,
            ModifiedAt = DateTime.UtcNow
        };

        await _attendanceRepository.UpdateAsync(attendance, cancellationToken);

        _logger.LogInformation("Updated attendance record with ID {AttendanceId}, ModifiedAt set for audit trail", id);

        // Fetch the updated record with joined names
        var updated = await _attendanceRepository.GetByIdAsync(id, cancellationToken);
        return MapToDto(updated!);
    }

    private static AttendanceDto MapToDto(AttendanceWithNames record)
    {
        var studentName = string.Join(" ",
            new[] { record.StudentFirstName, record.StudentLastName }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        var statusValue = record.Status;
        var statusString = Enum.IsDefined(typeof(AttendanceStatus), statusValue)
            ? ((AttendanceStatus)statusValue).ToString()
            : $"Unknown({statusValue})";

        return new AttendanceDto
        {
            Id = record.Id,
            StudentId = record.StudentId,
            StudentName = string.IsNullOrWhiteSpace(studentName) ? null : studentName,
            ClassGroupId = record.ClassGroupId,
            ClassGroupName = record.ClassGroupName,
            SessionDate = record.SessionDate,
            Status = statusString,
            Notes = record.Notes,
            CreatedAt = record.CreatedAt,
            ModifiedAt = record.ModifiedAt
        };
    }

    private static bool TryParseStatus(string statusString, out AttendanceStatus status)
    {
        return Enum.TryParse(statusString, ignoreCase: true, out status);
    }
}
