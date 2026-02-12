namespace Kcow.Application.Interfaces;

/// <summary>
/// Repository interface for Attendance entity operations.
/// </summary>
public interface IAttendanceRepository
{
    /// <summary>
    /// Gets attendance records with optional filters.
    /// </summary>
    Task<IEnumerable<AttendanceWithNames>> GetFilteredAsync(
        int? studentId = null,
        int? classGroupId = null,
        string? fromDate = null,
        string? toDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an attendance record by ID with student and class group names.
    /// </summary>
    Task<AttendanceWithNames?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets attendance history for a specific student.
    /// </summary>
    Task<IEnumerable<AttendanceWithNames>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new attendance record.
    /// </summary>
    Task<int> CreateAsync(Kcow.Domain.Entities.Attendance attendance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing attendance record.
    /// </summary>
    Task<bool> UpdateAsync(Kcow.Domain.Entities.Attendance attendance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an attendance record exists by ID.
    /// </summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an attendance record exists for the same student, class group, and date.
    /// </summary>
    Task<bool> ExistsByStudentClassGroupDateAsync(int studentId, int classGroupId, string sessionDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch creates or updates attendance records for a class group session.
    /// Uses transaction for atomic all-or-nothing operation.
    /// </summary>
    /// <returns>Tuple with (created count, updated count)</returns>
    Task<(int created, int updated)> BatchSaveAsync(
        List<Kcow.Domain.Entities.Attendance> attendanceRecords,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Attendance record with joined student and class group names for display.
/// </summary>
public class AttendanceWithNames
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string? StudentFirstName { get; set; }
    public string? StudentLastName { get; set; }
    public int ClassGroupId { get; set; }
    public string? ClassGroupName { get; set; }
    public string SessionDate { get; set; } = string.Empty;
    public int Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
