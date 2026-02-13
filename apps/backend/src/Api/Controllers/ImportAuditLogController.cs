using Kcow.Application.Import;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kcow.Api.Controllers;

[ApiController]
[Route("api/import/audit-log")]
[Authorize]
public class ImportAuditLogController : ControllerBase
{
    private readonly IImportAuditLogRepository _repository;

    public ImportAuditLogController(IImportAuditLogRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent([FromQuery] int count = 10, CancellationToken cancellationToken = default)
    {
        if (count < 1) count = 1;
        if (count > 100) count = 100;

        var logs = await _repository.GetRecentAsync(count, cancellationToken);
        return Ok(logs.Select(MapToDto));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var log = await _repository.GetByIdAsync(id, cancellationToken);
        if (log is null) return NotFound();
        return Ok(MapToDto(log));
    }

    private static ImportAuditLogDto MapToDto(ImportAuditLog log) => new()
    {
        Id = log.Id,
        StartedAt = log.StartedAt,
        CompletedAt = log.CompletedAt,
        RunBy = log.RunBy,
        SourcePath = log.SourcePath,
        Status = log.Status,
        SchoolsCreated = log.SchoolsCreated,
        ClassGroupsCreated = log.ClassGroupsCreated,
        ActivitiesCreated = log.ActivitiesCreated,
        StudentsCreated = log.StudentsCreated,
        TotalCreated = log.TotalCreated,
        TotalFailed = log.TotalFailed,
        TotalSkipped = log.TotalSkipped,
        ExceptionsFilePath = log.ExceptionsFilePath,
        Notes = log.Notes
    };
}
