using Dapper;
using Kcow.Application.Interfaces;
using Kcow.Infrastructure.Database;

namespace Kcow.Infrastructure.Repositories;

/// <summary>
/// Dapper implementation of IEvaluationRepository.
/// </summary>
public class EvaluationRepository : IEvaluationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public EvaluationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<EvaluationWithNames>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT e.id, e.student_id, s.first_name AS student_first_name, s.last_name AS student_last_name,
                   e.activity_id, a.name AS activity_name,
                   e.evaluation_date, e.score, e.speed_metric, e.accuracy_metric,
                   e.notes, e.created_at, e.modified_at
            FROM evaluations e
            INNER JOIN students s ON e.student_id = s.id
            INNER JOIN activities a ON e.activity_id = a.id
            ORDER BY e.evaluation_date DESC, s.last_name, s.first_name";
        return await connection.QueryAsync<EvaluationWithNames>(sql);
    }

    public async Task<EvaluationWithNames?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT e.id, e.student_id, s.first_name AS student_first_name, s.last_name AS student_last_name,
                   e.activity_id, a.name AS activity_name,
                   e.evaluation_date, e.score, e.speed_metric, e.accuracy_metric,
                   e.notes, e.created_at, e.modified_at
            FROM evaluations e
            INNER JOIN students s ON e.student_id = s.id
            INNER JOIN activities a ON e.activity_id = a.id
            WHERE e.id = @Id";
        return await connection.QueryFirstOrDefaultAsync<EvaluationWithNames>(sql, new { Id = id });
    }

    public async Task<IEnumerable<EvaluationWithNames>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            SELECT e.id, e.student_id, s.first_name AS student_first_name, s.last_name AS student_last_name,
                   e.activity_id, a.name AS activity_name,
                   e.evaluation_date, e.score, e.speed_metric, e.accuracy_metric,
                   e.notes, e.created_at, e.modified_at
            FROM evaluations e
            INNER JOIN students s ON e.student_id = s.id
            INNER JOIN activities a ON e.activity_id = a.id
            WHERE e.student_id = @StudentId
            ORDER BY e.evaluation_date DESC";
        return await connection.QueryAsync<EvaluationWithNames>(sql, new { StudentId = studentId });
    }

    public async Task<int> CreateAsync(Kcow.Domain.Entities.Evaluation evaluation, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            INSERT INTO evaluations (student_id, activity_id, evaluation_date, score, speed_metric, accuracy_metric, notes, created_at, modified_at)
            VALUES (@StudentId, @ActivityId, @EvaluationDate, @Score, @SpeedMetric, @AccuracyMetric, @Notes, @CreatedAt, @ModifiedAt)
            RETURNING id";
        return await connection.QuerySingleAsync<int>(sql, evaluation);
    }

    public async Task<bool> UpdateAsync(Kcow.Domain.Entities.Evaluation evaluation, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = @"
            UPDATE evaluations
            SET evaluation_date = @EvaluationDate,
                score = @Score,
                speed_metric = @SpeedMetric,
                accuracy_metric = @AccuracyMetric,
                notes = @Notes,
                modified_at = @ModifiedAt
            WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, evaluation);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "DELETE FROM evaluations WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();
        const string sql = "SELECT COUNT(1) FROM evaluations WHERE id = @Id";
        var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}
