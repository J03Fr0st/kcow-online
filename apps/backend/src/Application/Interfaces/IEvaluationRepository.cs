namespace Kcow.Application.Interfaces;

/// <summary>
/// Repository interface for Evaluation entity operations.
/// </summary>
public interface IEvaluationRepository
{
    /// <summary>
    /// Gets all evaluation records with student and activity names.
    /// </summary>
    Task<IEnumerable<EvaluationWithNames>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an evaluation record by ID with student and activity names.
    /// </summary>
    Task<EvaluationWithNames?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets evaluation history for a specific student.
    /// </summary>
    Task<IEnumerable<EvaluationWithNames>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new evaluation record.
    /// </summary>
    Task<int> CreateAsync(Kcow.Domain.Entities.Evaluation evaluation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing evaluation record.
    /// </summary>
    Task<bool> UpdateAsync(Kcow.Domain.Entities.Evaluation evaluation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an evaluation record by ID.
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an evaluation record exists by ID.
    /// </summary>
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Evaluation record with joined student and activity names for display.
/// </summary>
public class EvaluationWithNames
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string? StudentFirstName { get; set; }
    public string? StudentLastName { get; set; }
    public int ActivityId { get; set; }
    public string? ActivityName { get; set; }
    public string EvaluationDate { get; set; } = string.Empty;
    public int? Score { get; set; }
    public decimal? SpeedMetric { get; set; }
    public decimal? AccuracyMetric { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
