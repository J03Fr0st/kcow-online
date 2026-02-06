namespace Kcow.Application.Evaluations;

/// <summary>
/// Data transfer object for evaluation information.
/// Includes related student and activity names for display.
/// </summary>
public class EvaluationDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string? StudentName { get; set; }
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
