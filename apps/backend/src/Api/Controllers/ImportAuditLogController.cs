using Kcow.Application.Interfaces;
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
        var logs = await _repository.GetRecentAsync(count, cancellationToken);
        return Ok(logs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        var log = await _repository.GetByIdAsync(id, cancellationToken);
        if (log is null) return NotFound();
        return Ok(log);
    }
}
