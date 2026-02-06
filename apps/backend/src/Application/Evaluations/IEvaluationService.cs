namespace Kcow.Application.Evaluations;

/// <summary>
/// Service interface for evaluation management operations.
/// </summary>
public interface IEvaluationService
{
    /// <summary>
    /// Gets all evaluation records.
    /// </summary>
    Task<List<EvaluationDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single evaluation record by ID.
    /// </summary>
    Task<EvaluationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets evaluation history for a specific student.
    /// </summary>
    Task<List<EvaluationDto>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new evaluation record.
    /// </summary>
    Task<EvaluationDto> CreateAsync(CreateEvaluationRequest request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing evaluation record.
    /// </summary>
    Task<EvaluationDto?> UpdateAsync(int id, UpdateEvaluationRequest request, string changedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an evaluation record by ID with audit logging.
    /// </summary>
    Task<bool> DeleteAsync(int id, string deletedBy, CancellationToken cancellationToken = default);
}
