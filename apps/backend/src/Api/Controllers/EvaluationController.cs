using Kcow.Application.Evaluations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kcow.Api.Controllers;

/// <summary>
/// Controller for evaluation management operations.
/// </summary>
[ApiController]
[Route("api/evaluations")]
[Authorize]
public class EvaluationController : ControllerBase
{
    private readonly IEvaluationService _evaluationService;
    private readonly ILogger<EvaluationController> _logger;

    public EvaluationController(IEvaluationService evaluationService, ILogger<EvaluationController> logger)
    {
        _evaluationService = evaluationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all evaluation records.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<EvaluationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var records = await _evaluationService.GetAllAsync(cancellationToken);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving evaluation records");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving evaluation records"));
        }
    }

    /// <summary>
    /// Gets a specific evaluation record by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(EvaluationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var record = await _evaluationService.GetByIdAsync(id, cancellationToken);

            if (record == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Evaluation record not found",
                    Status = 404,
                    Detail = $"Evaluation record with ID {id} was not found"
                });
            }

            return Ok(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving evaluation record with ID {EvaluationId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving the evaluation record"));
        }
    }

    /// <summary>
    /// Gets evaluation history for a specific student.
    /// </summary>
    [HttpGet]
    [Route("/api/students/{studentId}/evaluations")]
    [ProducesResponseType(typeof(List<EvaluationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByStudentId(int studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var records = await _evaluationService.GetByStudentIdAsync(studentId, cancellationToken);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving evaluation records for student {StudentId}", studentId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving evaluation records for the student"));
        }
    }

    /// <summary>
    /// Creates a new evaluation record.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EvaluationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateEvaluationRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Status = 400,
                Detail = "One or more validation errors occurred",
                Extensions = { ["errors"] = ModelState }
            });
        }

        try
        {
            // Get current user from JWT claims - require authenticated user
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? User.FindFirst("email")?.Value;

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Authentication required",
                    Status = 401,
                    Detail = "Unable to identify user from authentication token. Please log in again."
                });
            }

            var record = await _evaluationService.CreateAsync(request, userEmail, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation failed while creating evaluation record");
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Status = 400,
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating evaluation record");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while creating the evaluation record"));
        }
    }

    /// <summary>
    /// Updates an existing evaluation record with audit logging.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(EvaluationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEvaluationRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Status = 400,
                Detail = "One or more validation errors occurred",
                Extensions = { ["errors"] = ModelState }
            });
        }

        try
        {
            // Get current user from JWT claims - require authenticated user
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? User.FindFirst("email")?.Value;

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Authentication required",
                    Status = 401,
                    Detail = "Unable to identify user from authentication token. Audit logging requires valid user identity."
                });
            }

            var record = await _evaluationService.UpdateAsync(id, request, userEmail, cancellationToken);

            if (record == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Evaluation record not found",
                    Status = 404,
                    Detail = $"Evaluation record with ID {id} was not found"
                });
            }

            return Ok(record);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating evaluation record with ID {EvaluationId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while updating the evaluation record"));
        }
    }

    /// <summary>
    /// Deletes an evaluation record by ID.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current user from JWT claims - require authenticated user
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                ?? User.FindFirst("email")?.Value;

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Authentication required",
                    Status = 401,
                    Detail = "Unable to identify user from authentication token. Audit logging requires valid user identity."
                });
            }

            var result = await _evaluationService.DeleteAsync(id, userEmail, cancellationToken);

            if (!result)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Evaluation record not found",
                    Status = 404,
                    Detail = $"Evaluation record with ID {id} was not found"
                });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting evaluation record with ID {EvaluationId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while deleting the evaluation record"));
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
