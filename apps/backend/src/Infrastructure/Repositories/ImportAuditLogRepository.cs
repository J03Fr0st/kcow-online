using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;

namespace Kcow.Infrastructure.Repositories;

public sealed class ImportAuditLogRepository : IImportAuditLogRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ImportAuditLogRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(ImportAuditLog auditLog, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            INSERT INTO import_audit_log (started_at, run_by, source_path, status)
            VALUES (@StartedAt, @RunBy, @SourcePath, @Status)
            RETURNING id";
        return await connection.QuerySingleAsync<int>(sql, auditLog);
    }

    public async Task UpdateAsync(ImportAuditLog auditLog, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            UPDATE import_audit_log
            SET completed_at = @CompletedAt,
                status = @Status,
                schools_created = @SchoolsCreated,
                class_groups_created = @ClassGroupsCreated,
                activities_created = @ActivitiesCreated,
                students_created = @StudentsCreated,
                total_failed = @TotalFailed,
                total_skipped = @TotalSkipped,
                exceptions_file_path = @ExceptionsFilePath,
                notes = @Notes
            WHERE id = @Id";
        await connection.ExecuteAsync(sql, auditLog);
    }

    public async Task<ImportAuditLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, started_at, completed_at, run_by, source_path, status,
                   schools_created, class_groups_created, activities_created,
                   students_created, total_failed, total_skipped,
                   exceptions_file_path, notes
            FROM import_audit_log
            WHERE id = @Id";
        return await connection.QuerySingleOrDefaultAsync<ImportAuditLog>(sql, new { Id = id });
    }

    public async Task<IEnumerable<ImportAuditLog>> GetRecentAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT id, started_at, completed_at, run_by, source_path, status,
                   schools_created, class_groups_created, activities_created,
                   students_created, total_failed, total_skipped,
                   exceptions_file_path, notes
            FROM import_audit_log
            ORDER BY started_at DESC
            LIMIT @Count";
        return await connection.QueryAsync<ImportAuditLog>(sql, new { Count = count });
    }
}
