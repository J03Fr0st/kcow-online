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
        string? toDate = null);

    /// <summary>
    /// Gets a single attendance record by ID.
    /// </summary>
    Task<AttendanceDto?> GetByIdAsync(int id);

    /// <summary>
    /// Gets attendance history for a specific student.
    /// </summary>
    Task<List<AttendanceDto>> GetByStudentIdAsync(int studentId);

    /// <summary>
    /// Creates a new attendance record.
    /// </summary>
    Task<AttendanceDto> CreateAsync(CreateAttendanceRequest request);

    /// <summary>
    /// Updates an existing attendance record (triggers audit via ModifiedAt).
    /// </summary>
    Task<AttendanceDto?> UpdateAsync(int id, UpdateAttendanceRequest request);
}
