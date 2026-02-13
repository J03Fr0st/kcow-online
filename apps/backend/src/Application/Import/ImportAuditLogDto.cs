namespace Kcow.Application.Import;

/// <summary>
/// Data transfer object for import audit log entries.
/// </summary>
public class ImportAuditLogDto
{
    public int Id { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string RunBy { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int SchoolsCreated { get; set; }
    public int ClassGroupsCreated { get; set; }
    public int ActivitiesCreated { get; set; }
    public int StudentsCreated { get; set; }
    public int TotalCreated { get; set; }
    public int TotalFailed { get; set; }
    public int TotalSkipped { get; set; }
    public string? ExceptionsFilePath { get; set; }
    public string? Notes { get; set; }
}
