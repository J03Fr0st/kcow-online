namespace Kcow.Application.Attendance;

/// <summary>
/// Service interface for attendance management operations.
/// </summary>
public interface IAttendanceService
{
    /// <summary>
    /// Gets attendance records with optional filters.
    /// </summary>
    Task<List<AttendanceDto>> GetFilteredAsync(
        int? studentId = null,
        int? classGroupId = null,
        string? fromDate = null,
        string? toDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single attendance record by ID.
    /// </summary>
    Task<AttendanceDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets attendance history for a specific student.
    /// </summary>
    Task<List<AttendanceDto>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new attendance record.
    /// </summary>
    Task<AttendanceDto> CreateAsync(CreateAttendanceRequest request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing attendance record with audit logging.
    /// </summary>
    Task<AttendanceDto?> UpdateAsync(int id, UpdateAttendanceRequest request, string changedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch creates or updates attendance records for a class group session.
    /// Uses transaction for atomic all-or-nothing operation.
    /// </summary>
    Task<BatchAttendanceResponse> BatchSaveAsync(BatchAttendanceRequest request, string createdBy, CancellationToken cancellationToken = default);
}
