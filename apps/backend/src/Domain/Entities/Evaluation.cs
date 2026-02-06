namespace Kcow.Domain.Entities;

/// <summary>
/// Evaluation entity for tracking student progress evaluations.
/// Represents a single evaluation record for a student on a specific activity.
/// </summary>
public class Evaluation
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ActivityId { get; set; }
    public string EvaluationDate { get; set; } = string.Empty; // Stored as ISO date string (YYYY-MM-DD)
    public int? Score { get; set; }
    public decimal? SpeedMetric { get; set; }
    public decimal? AccuracyMetric { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
