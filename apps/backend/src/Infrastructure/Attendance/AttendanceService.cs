using Kcow.Application.Attendance;
using Kcow.Application.Audit;
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
    private readonly IAuditService _auditService;

    public AttendanceService(
        IAttendanceRepository attendanceRepository,
        ILogger<AttendanceService> logger,
        IAuditService auditService)
    {
        _attendanceRepository = attendanceRepository;
        _logger = logger;
        _auditService = auditService;
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
    public async Task<AttendanceDto> CreateAsync(CreateAttendanceRequest request, string createdBy, CancellationToken cancellationToken = default)
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

        // Log audit entry for record creation
        await _auditService.LogChangeAsync(
            "Attendance",
            id,
            "Created",
            null,
            $"Record created with Status: {request.Status}",
            createdBy,
            cancellationToken);

        _logger.LogInformation("Created attendance record with ID {AttendanceId} for student {StudentId} on {SessionDate}",
            id, request.StudentId, request.SessionDate);

        // Fetch the full record with joined names
        var created = await _attendanceRepository.GetByIdAsync(id, cancellationToken);
        return MapToDto(created!);
    }

    /// <summary>
    /// Updates an existing attendance record with audit logging.
    /// </summary>
    public async Task<AttendanceDto?> UpdateAsync(int id, UpdateAttendanceRequest request, string changedBy, CancellationToken cancellationToken = default)
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

        // Track changes for audit
        var changes = new Dictionary<string, (string? oldVal, string? newVal)>();

        // Check if status changed
        var oldStatusString = Enum.IsDefined(typeof(AttendanceStatus), existing.Status)
            ? ((AttendanceStatus)existing.Status).ToString()
            : $"Unknown({existing.Status})";
        if (oldStatusString != request.Status)
        {
            changes["Status"] = (oldStatusString, request.Status);
        }

        // Check if notes changed
        var oldNotes = existing.Notes ?? "";
        var newNotes = request.Notes ?? "";
        if (oldNotes != newNotes)
        {
            changes["Notes"] = (oldNotes, newNotes);
        }

        // Update the record
        var attendance = new Domain.Entities.Attendance
        {
            Id = id,
            Status = statusEnum,
            Notes = request.Notes,
            ModifiedAt = DateTime.UtcNow
        };

        await _attendanceRepository.UpdateAsync(attendance, cancellationToken);

        // Log audit changes if any
        if (changes.Count > 0)
        {
            await _auditService.LogChangesAsync(
                "Attendance",
                id,
                changes,
                changedBy,
                cancellationToken);
        }

        _logger.LogInformation("Updated attendance record with ID {AttendanceId}, {ChangeCount} changes audited", id, changes.Count);

        // Fetch the updated record with joined names
        var updated = await _attendanceRepository.GetByIdAsync(id, cancellationToken);
        return MapToDto(updated!);
    }

    /// <summary>
    /// Batch creates or updates attendance records for a class group session.
    /// Uses transaction for atomic all-or-nothing operation.
    /// </summary>
    public async Task<BatchAttendanceResponse> BatchSaveAsync(BatchAttendanceRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var attendanceRecords = new List<Domain.Entities.Attendance>();

        // Validate all entries and convert to domain entities
        foreach (var entry in request.Entries)
        {
            if (!TryParseStatus(entry.Status, out var statusEnum))
            {
                errors.Add($"Student {entry.StudentId}: Invalid status '{entry.Status}'. Valid values are: Present, Absent, Late");
                continue;
            }

            attendanceRecords.Add(new Domain.Entities.Attendance
            {
                StudentId = entry.StudentId,
                ClassGroupId = request.ClassGroupId,
                SessionDate = request.SessionDate,
                Status = statusEnum,
                Notes = entry.Notes,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            });
        }

        var response = new BatchAttendanceResponse();

        if (errors.Count > 0)
        {
            response.Failed = errors.Count;
            response.Errors = errors;
            _logger.LogWarning("Batch attendance validation failed with {ErrorCount} errors", errors.Count);
            return response;
        }

        try
        {
            // Perform batch save in transaction
            var (created, updated) = await _attendanceRepository.BatchSaveAsync(attendanceRecords, cancellationToken);

            response.Created = created;
            response.Updated = updated;
            response.Failed = 0;

            // Log audit entry for the batch operation
            await _auditService.LogChangeAsync(
                "Attendance",
                request.ClassGroupId,
                "BatchSave",
                null,
                $"Batch attendance save for ClassGroup {request.ClassGroupId} on {request.SessionDate}: {created} created, {updated} updated",
                createdBy,
                cancellationToken);

            _logger.LogInformation("Batch attendance save completed for ClassGroup {ClassGroupId} on {SessionDate}: {Created} created, {Updated} updated",
                request.ClassGroupId, request.SessionDate, created, updated);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Batch attendance save failed for ClassGroup {ClassGroupId} on {SessionDate}",
                request.ClassGroupId, request.SessionDate);

            response.Failed = request.Entries.Count;
            response.Errors = new List<string> { "Batch save operation failed. All changes were rolled back." };
            return response;
        }
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
