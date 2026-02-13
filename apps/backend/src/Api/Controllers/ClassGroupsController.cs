using Kcow.Application.Attendance;
using Kcow.Application.ClassGroups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kcow.Api.Controllers;

/// <summary>
/// Controller for class group management operations.
/// </summary>
[ApiController]
[Route("api/class-groups")]
[Authorize]
public class ClassGroupsController : ControllerBase
{
    private readonly IClassGroupService _classGroupService;
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<ClassGroupsController> _logger;

    public ClassGroupsController(
        IClassGroupService classGroupService,
        IAttendanceService attendanceService,
        ILogger<ClassGroupsController> logger)
    {
        _classGroupService = classGroupService;
        _attendanceService = attendanceService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active class groups with optional filtering by school or truck.
    /// </summary>
    /// <param name="schoolId">Optional school ID filter</param>
    /// <param name="truckId">Optional truck ID filter</param>
    /// <returns>List of active class groups</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ClassGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll([FromQuery] int? schoolId, [FromQuery] int? truckId, CancellationToken cancellationToken)
    {
        try
        {
            var classGroups = await _classGroupService.GetAllAsync(schoolId, truckId, cancellationToken);
            return Ok(classGroups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving class groups (SchoolId: {SchoolId}, TruckId: {TruckId})", schoolId, truckId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving class groups"));
        }
    }

    /// <summary>
    /// Gets a specific class group by ID.
    /// </summary>
    /// <param name="id">Class group ID</param>
    /// <returns>Class group details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ClassGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            var classGroup = await _classGroupService.GetByIdAsync(id, cancellationToken);

            if (classGroup == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Class group not found",
                    Status = 404,
                    Detail = $"Class group with ID {id} was not found"
                });
            }

            return Ok(classGroup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving class group with ID {ClassGroupId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving the class group"));
        }
    }

    /// <summary>
    /// Creates a new class group.
    /// </summary>
    /// <param name="request">Class group creation request</param>
    /// <returns>Created class group details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ClassGroupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateClassGroupRequest request, CancellationToken cancellationToken)
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
            var classGroup = await _classGroupService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = classGroup.Id }, classGroup);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Status = 400,
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating class group");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while creating the class group"));
        }
    }

    /// <summary>
    /// Updates an existing class group.
    /// </summary>
    /// <param name="id">Class group ID</param>
    /// <param name="request">Class group update request</param>
    /// <returns>Updated class group details</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ClassGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassGroupRequest request, CancellationToken cancellationToken)
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
            var classGroup = await _classGroupService.UpdateAsync(id, request, cancellationToken);

            if (classGroup == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Class group not found",
                    Status = 404,
                    Detail = $"Class group with ID {id} was not found"
                });
            }

            return Ok(classGroup);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Status = 400,
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating class group with ID {ClassGroupId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while updating the class group"));
        }
    }

    /// <summary>
    /// Archives (soft-deletes) a class group.
    /// </summary>
    /// <param name="id">Class group ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Archive(int id, CancellationToken cancellationToken)
    {
        try
        {
            var archived = await _classGroupService.ArchiveAsync(id, cancellationToken);

            if (!archived)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Class group not found",
                    Status = 404,
                    Detail = $"Class group with ID {id} was not found"
                });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving class group with ID {ClassGroupId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while archiving the class group"));
        }
    }

    /// <summary>
    /// Checks for scheduling conflicts with existing class groups.
    /// </summary>
    /// <param name="request">Conflict check request parameters</param>
    /// <returns>Conflict check response with list of conflicting class groups</returns>
    [HttpPost("check-conflicts")]
    [ProducesResponseType(typeof(CheckConflictsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CheckConflicts([FromBody] CheckConflictsRequest request, CancellationToken cancellationToken)
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
            var result = await _classGroupService.CheckConflictsAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking conflicts for TruckId: {TruckId}", request.TruckId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while checking for conflicts"));
        }
    }

    /// <summary>
    /// Batch saves attendance records for a class group session.
    /// Creates new records or updates existing ones in a single atomic transaction.
    /// </summary>
    /// <param name="id">Class group ID</param>
    /// <param name="request">Batch attendance request</param>
    /// <returns>Batch attendance response with created/updated/failed counts</returns>
    [HttpPost("{id}/attendance")]
    [ProducesResponseType(typeof(BatchAttendanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> BatchAttendance(int id, [FromBody] BatchAttendanceRequest request, CancellationToken cancellationToken = default)
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

        // Ensure the class group ID in the URL matches the one in the request
        if (request.ClassGroupId != id)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Status = 400,
                Detail = $"ClassGroupId in request ({request.ClassGroupId}) does not match ID in URL ({id})"
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

            var result = await _attendanceService.BatchSaveAsync(request, userEmail, cancellationToken);

            if (result.Failed > 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Partial or complete failure",
                    Status = 400,
                    Detail = $"Batch save completed with {result.Failed} failures.",
                    Extensions = { ["errors"] = result.Errors ?? new List<string>() }
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving batch attendance for ClassGroupId {ClassGroupId} on {SessionDate}",
                request.ClassGroupId, request.SessionDate);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while saving batch attendance"));
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
