using Kcow.Application.Audit;
using Kcow.Application.Evaluations;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Evaluations;

/// <summary>
/// Implementation of evaluation management service using Dapper repositories.
/// </summary>
public class EvaluationService : IEvaluationService
{
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IActivityRepository _activityRepository;
    private readonly ILogger<EvaluationService> _logger;
    private readonly IAuditService _auditService;

    public EvaluationService(
        IEvaluationRepository evaluationRepository,
        IStudentRepository studentRepository,
        IActivityRepository activityRepository,
        ILogger<EvaluationService> logger,
        IAuditService auditService)
    {
        _evaluationRepository = evaluationRepository;
        _studentRepository = studentRepository;
        _activityRepository = activityRepository;
        _logger = logger;
        _auditService = auditService;
    }

    /// <summary>
    /// Gets all evaluation records.
    /// </summary>
    public async Task<List<EvaluationDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await _evaluationRepository.GetAllAsync(cancellationToken);

        var dtos = records.Select(MapToDto).ToList();

        _logger.LogInformation("Retrieved {Count} evaluation records", dtos.Count);

        return dtos;
    }

    /// <summary>
    /// Gets a single evaluation record by ID.
    /// </summary>
    public async Task<EvaluationDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var record = await _evaluationRepository.GetByIdAsync(id, cancellationToken);

        if (record == null)
        {
            _logger.LogWarning("Evaluation record with ID {EvaluationId} not found", id);
            return null;
        }

        _logger.LogInformation("Retrieved evaluation record with ID {EvaluationId}", id);
        return MapToDto(record);
    }

    /// <summary>
    /// Gets evaluation history for a specific student.
    /// </summary>
    public async Task<List<EvaluationDto>> GetByStudentIdAsync(int studentId, CancellationToken cancellationToken = default)
    {
        var records = await _evaluationRepository.GetByStudentIdAsync(studentId, cancellationToken);
        var dtos = records.Select(MapToDto).ToList();

        _logger.LogInformation("Retrieved {Count} evaluation records for student {StudentId}", dtos.Count, studentId);
        return dtos;
    }

    /// <summary>
    /// Creates a new evaluation record.
    /// </summary>
    public async Task<EvaluationDto> CreateAsync(CreateEvaluationRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        // Validate StudentId exists
        var studentExists = await _studentRepository.ExistsAsync(request.StudentId, cancellationToken);
        if (!studentExists)
        {
            throw new InvalidOperationException($"Student with ID {request.StudentId} does not exist.");
        }

        // Validate ActivityId exists
        var activityExists = await _activityRepository.ExistsAsync(request.ActivityId, cancellationToken);
        if (!activityExists)
        {
            throw new InvalidOperationException($"Activity with ID {request.ActivityId} does not exist.");
        }

        var evaluation = new Domain.Entities.Evaluation
        {
            StudentId = request.StudentId,
            ActivityId = request.ActivityId,
            EvaluationDate = request.EvaluationDate,
            Score = request.Score,
            SpeedMetric = request.SpeedMetric,
            AccuracyMetric = request.AccuracyMetric,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _evaluationRepository.CreateAsync(evaluation, cancellationToken);
        evaluation.Id = id;

        // Log audit entry for record creation
        await _auditService.LogChangeAsync(
            "Evaluation",
            id,
            "Created",
            null,
            $"Record created for StudentId: {request.StudentId}, ActivityId: {request.ActivityId}",
            createdBy,
            cancellationToken);

        _logger.LogInformation("Created evaluation record with ID {EvaluationId} for student {StudentId}",
            id, request.StudentId);

        // Fetch the full record with joined names
        var created = await _evaluationRepository.GetByIdAsync(id, cancellationToken);
        return MapToDto(created!);
    }

    /// <summary>
    /// Updates an existing evaluation record with audit logging.
    /// </summary>
    public async Task<EvaluationDto?> UpdateAsync(int id, UpdateEvaluationRequest request, string changedBy, CancellationToken cancellationToken = default)
    {
        var existing = await _evaluationRepository.GetByIdAsync(id, cancellationToken);

        if (existing == null)
        {
            _logger.LogWarning("Cannot update: Evaluation record with ID {EvaluationId} not found", id);
            return null;
        }

        // Track changes for audit
        var changes = new Dictionary<string, (string? oldVal, string? newVal)>();

        // Check if evaluation date changed
        if (existing.EvaluationDate != request.EvaluationDate)
        {
            changes["EvaluationDate"] = (existing.EvaluationDate, request.EvaluationDate);
        }

        // Check if score changed
        var oldScore = existing.Score?.ToString() ?? "null";
        var newScore = request.Score?.ToString() ?? "null";
        if (oldScore != newScore)
        {
            changes["Score"] = (oldScore, newScore);
        }

        // Check if speed metric changed
        var oldSpeed = existing.SpeedMetric?.ToString() ?? "null";
        var newSpeed = request.SpeedMetric?.ToString() ?? "null";
        if (oldSpeed != newSpeed)
        {
            changes["SpeedMetric"] = (oldSpeed, newSpeed);
        }

        // Check if accuracy metric changed
        var oldAccuracy = existing.AccuracyMetric?.ToString() ?? "null";
        var newAccuracy = request.AccuracyMetric?.ToString() ?? "null";
        if (oldAccuracy != newAccuracy)
        {
            changes["AccuracyMetric"] = (oldAccuracy, newAccuracy);
        }

        // Check if notes changed
        var oldNotes = existing.Notes ?? "";
        var newNotes = request.Notes ?? "";
        if (oldNotes != newNotes)
        {
            changes["Notes"] = (oldNotes, newNotes);
        }

        // Update the record
        var evaluation = new Domain.Entities.Evaluation
        {
            Id = id,
            EvaluationDate = request.EvaluationDate,
            Score = request.Score,
            SpeedMetric = request.SpeedMetric,
            AccuracyMetric = request.AccuracyMetric,
            Notes = request.Notes,
            ModifiedAt = DateTime.UtcNow
        };

        // Skip update if no actual changes detected
        if (changes.Count == 0)
        {
            _logger.LogInformation("No changes detected for evaluation record with ID {EvaluationId}, skipping update", id);
            // Still return the current record with joined names
            return MapToDto(existing);
        }

        await _evaluationRepository.UpdateAsync(evaluation, cancellationToken);

        // Log audit changes
        await _auditService.LogChangesAsync(
            "Evaluation",
            id,
            changes,
            changedBy,
            cancellationToken);

        _logger.LogInformation("Updated evaluation record with ID {EvaluationId}, {ChangeCount} changes audited", id, changes.Count);

        // Fetch the updated record with joined names
        var updated = await _evaluationRepository.GetByIdAsync(id, cancellationToken);
        return MapToDto(updated!);
    }

    /// <summary>
    /// Deletes an evaluation record by ID with audit logging.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var existing = await _evaluationRepository.GetByIdAsync(id, cancellationToken);

        if (existing == null)
        {
            _logger.LogWarning("Cannot delete: Evaluation record with ID {EvaluationId} not found", id);
            return false;
        }

        // Log audit entry before deletion - capture the record details
        await _auditService.LogChangeAsync(
            "Evaluation",
            id,
            "Deleted",
            $"StudentId: {existing.StudentId}, ActivityId: {existing.ActivityId}, Date: {existing.EvaluationDate}, Score: {existing.Score}",
            null,
            deletedBy,
            cancellationToken);

        var result = await _evaluationRepository.DeleteAsync(id, cancellationToken);

        if (result)
        {
            _logger.LogInformation("Deleted evaluation record with ID {EvaluationId}", id);
        }

        return result;
    }

    private static EvaluationDto MapToDto(EvaluationWithNames record)
    {
        var studentName = string.Join(" ",
            new[] { record.StudentFirstName, record.StudentLastName }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        return new EvaluationDto
        {
            Id = record.Id,
            StudentId = record.StudentId,
            StudentName = string.IsNullOrWhiteSpace(studentName) ? null : studentName,
            ActivityId = record.ActivityId,
            ActivityName = record.ActivityName,
            EvaluationDate = record.EvaluationDate,
            Score = record.Score,
            SpeedMetric = record.SpeedMetric,
            AccuracyMetric = record.AccuracyMetric,
            Notes = record.Notes,
            CreatedAt = record.CreatedAt,
            ModifiedAt = record.ModifiedAt
        };
    }
}
