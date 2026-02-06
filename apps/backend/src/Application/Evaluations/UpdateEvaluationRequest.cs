using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Evaluations;

/// <summary>
/// Request model for updating an existing evaluation record.
/// </summary>
public class UpdateEvaluationRequest
{
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
