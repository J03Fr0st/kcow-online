using Kcow.Application.Audit;
using Kcow.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kcow.Api.Controllers;

/// <summary>
/// Controller for audit log retrieval operations.
/// </summary>
[ApiController]
[Route("api/audit-log")]
[Authorize]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<AuditLogController> _logger;

    public AuditLogController(IAuditLogRepository auditLogRepository, ILogger<AuditLogController> logger)
    {
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets audit log entries for a specific entity.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByEntity(
        [FromQuery] string entityType,
        [FromQuery] int entityId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entityType))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Validation failed",
                    Status = 400,
                    Detail = "entityType query parameter is required"
                });
            }

            var entries = await _auditLogRepository.GetByEntityAsync(entityType, entityId, cancellationToken);

            var dtos = entries.Select(e => new AuditLogDto
            {
                Id = e.Id,
                EntityType = e.EntityType,
                EntityId = e.EntityId,
                Field = e.Field,
                OldValue = e.OldValue,
                NewValue = e.NewValue,
                ChangedBy = e.ChangedBy,
                ChangedAt = e.ChangedAt
            }).ToList();

            _logger.LogInformation("Retrieved {Count} audit log entries for entity type {EntityType}, ID {EntityId}",
                dtos.Count, entityType, entityId);

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit log entries for entity type {EntityType}, ID {EntityId}",
                entityType, entityId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving audit log entries"));
        }
    }

    private ProblemDetails CreateServerErrorProblemDetails(string title)
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred.",
            Instance = HttpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;
        return problemDetails;
    }
}
