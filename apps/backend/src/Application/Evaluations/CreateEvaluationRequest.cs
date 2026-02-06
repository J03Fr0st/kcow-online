using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Evaluations;

/// <summary>
/// Request model for creating a new evaluation record.
/// </summary>
public class CreateEvaluationRequest
{
    /// <summary>
    /// The student ID (required).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a positive integer")]
    public int StudentId { get; set; }

    /// <summary>
    /// The activity ID (required).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "ActivityId must be a positive integer")]
    public int ActivityId { get; set; }

    /// <summary>
    /// The evaluation date in ISO format (YYYY-MM-DD) (required).
    /// </summary>
    [Required(ErrorMessage = "EvaluationDate is required")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "EvaluationDate must be in ISO format (YYYY-MM-DD)")]
    public string EvaluationDate { get; set; } = string.Empty;

    /// <summary>
    /// Optional score for the evaluation.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Score must be a non-negative integer")]
    public int? Score { get; set; }

    /// <summary>
    /// Optional speed metric for the evaluation.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "SpeedMetric must be a non-negative number")]
    public decimal? SpeedMetric { get; set; }

    /// <summary>
    /// Optional accuracy metric for the evaluation.
    /// </summary>
    [Range(0, 1, ErrorMessage = "AccuracyMetric must be between 0 and 1")]
    public decimal? AccuracyMetric { get; set; }

    /// <summary>
    /// Optional notes for the evaluation record.
    /// </summary>
    public string? Notes { get; set; }
}
