using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Infrastructure.Database;

namespace Kcow.Infrastructure.Repositories;

/// <summary>
/// Dapper implementation of IAttendanceRepository.
/// </summary>
public class AttendanceRepository : IAttendanceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AttendanceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<AttendanceWithNames>> GetFilteredAsync(
        int? studentId = null,
        int? classGroupId = null,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();

        var sql = @"
            SELECT a.id, a.student_id, s.first_name AS student_first_name, s.last_name AS student_last_name,
                   a.class_group_id, cg.name AS class_group_name,
                   a.session_date, a.status, a.notes, a.created_at, a.modified_at
            FROM attendance a
            INNER JOIN students s ON a.student_id = s.id
            INNER JOIN class_groups cg ON a.class_group_id = cg.id
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (studentId.HasValue)
        {
            sql += " AND a.student_id = @StudentId";
            parameters.Add("StudentId", studentId.Value);
        }

        if (classGroupId.HasValue)
        {
            sql += " AND a.class_group_id = @ClassGroupId";
            parameters.Add("ClassGroupId", classGroupId.Value);
        }

        if (!string.IsNullOrWhiteSpace(fromDate))
        {
            sql += " AND a.session_date >= @FromDate";
            parameters.Add("FromDate", fromDate);
        }

        if (!string.IsNullOrWhiteSpace(toDate))
        {
            sql += " AND a.session_date <= @ToDate";
            parameters.Add("ToDate", toDate);
        }

        sql += " ORDER BY a.session_date DESC, s.last_name, s.first_name";

        return await connection.QueryAsync<AttendanceWithNames>(sql, parameters);
    }

    public async Task<AttendanceWithNames?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT a.id, a.student_id, s.first_name AS student_first_name, s.last_name AS student_last_name,
                   a.class_group_id, cg.name AS class_group_name,
                   a.session_date, a.status, a.notes, a.created_at, a.modified_at
            FROM attendance a
            INNER JOIN students s ON a.student_id = s.id
            INNER JOIN class_groups cg ON a.class_group_id = cg.id
            WHERE a.id = @Id";
        return await connection.QueryFirstOrDefaultAsync<AttendanceWithNames>(sql, new { Id = id });
    }

    public async Task<IEnumerable<AttendanceWithNames>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT a.id, a.student_id, s.first_name AS student_first_name, s.last_name AS student_last_name,
                   a.class_group_id, cg.name AS class_group_name,
                   a.session_date, a.status, a.notes, a.created_at, a.modified_at
            FROM attendance a
            INNER JOIN students s ON a.student_id = s.id
            INNER JOIN class_groups cg ON a.class_group_id = cg.id
            WHERE a.student_id = @StudentId
            ORDER BY a.session_date DESC";
        return await connection.QueryAsync<AttendanceWithNames>(sql, new { StudentId = studentId });
    }

    public async Task<int> CreateAsync(Kcow.Domain.Entities.Attendance attendance, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            INSERT INTO attendance (student_id, class_group_id, session_date, status, notes, created_at, modified_at)
            VALUES (@StudentId, @ClassGroupId, @SessionDate, @Status, @Notes, @CreatedAt, @ModifiedAt)
            RETURNING id";
        return await connection.QuerySingleAsync<int>(sql, attendance);
    }

    public async Task<bool> UpdateAsync(Kcow.Domain.Entities.Attendance attendance, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            UPDATE attendance
            SET status = @Status,
                notes = @Notes,
                modified_at = @ModifiedAt
            WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, attendance);
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "SELECT COUNT(1) FROM attendance WHERE id = @Id";
        var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }

    public async Task<(int created, int updated)> BatchSaveAsync(
        List<Kcow.Domain.Entities.Attendance> attendanceRecords,
        CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        connection.Open();

        using var transaction = connection.BeginTransaction();
        try
        {
            int createdCount = 0;
            int updatedCount = 0;

            foreach (var record in attendanceRecords)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Check if record exists for this student, class group, and date
                const string checkSql = @"
                    SELECT id FROM attendance
                    WHERE student_id = @StudentId AND class_group_id = @ClassGroupId AND session_date = @SessionDate
                    LIMIT 1";

                var checkCommand = new CommandDefinition(
                    checkSql,
                    new { record.StudentId, record.ClassGroupId, record.SessionDate },
                    transaction,
                    cancellationToken: cancellationToken);

                var existingId = await connection.QuerySingleOrDefaultAsync<int?>(checkCommand);

                if (existingId.HasValue)
                {
                    // Update existing record
                    const string updateSql = @"
                        UPDATE attendance
                        SET status = @Status,
                            notes = @Notes,
                            modified_at = @ModifiedAt
                        WHERE id = @Id";

                    var updateCommand = new CommandDefinition(
                        updateSql,
                        new { Id = existingId.Value, record.Status, record.Notes, record.ModifiedAt },
                        transaction,
                        cancellationToken: cancellationToken);

                    await connection.ExecuteAsync(updateCommand);
                    updatedCount++;
                }
                else
                {
                    // Insert new record
                    const string insertSql = @"
                        INSERT INTO attendance (student_id, class_group_id, session_date, status, notes, created_at, modified_at)
                        VALUES (@StudentId, @ClassGroupId, @SessionDate, @Status, @Notes, @CreatedAt, @ModifiedAt)
                        RETURNING id";

                    var insertCommand = new CommandDefinition(
                        insertSql,
                        record,
                        transaction,
                        cancellationToken: cancellationToken);

                    await connection.QuerySingleAsync<int>(insertCommand);
                    createdCount++;
                }
            }

            transaction.Commit();
            return (createdCount, updatedCount);
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
