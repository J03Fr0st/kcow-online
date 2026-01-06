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
    private readonly ILogger<ClassGroupsController> _logger;

    public ClassGroupsController(IClassGroupService classGroupService, ILogger<ClassGroupsController> logger)
    {
        _classGroupService = classGroupService;
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
    public async Task<IActionResult> GetAll([FromQuery] int? schoolId, [FromQuery] int? truckId)
    {
        try
        {
            var classGroups = await _classGroupService.GetAllAsync(schoolId, truckId);
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
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var classGroup = await _classGroupService.GetByIdAsync(id);

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
    public async Task<IActionResult> Create([FromBody] CreateClassGroupRequest request)
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
            var classGroup = await _classGroupService.CreateAsync(request);
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
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassGroupRequest request)
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
            var classGroup = await _classGroupService.UpdateAsync(id, request);

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
    public async Task<IActionResult> Archive(int id)
    {
        try
        {
            var archived = await _classGroupService.ArchiveAsync(id);

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
    public async Task<IActionResult> CheckConflicts([FromBody] CheckConflictsRequest request)
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
            var result = await _classGroupService.CheckConflictsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking conflicts for TruckId: {TruckId}", request.TruckId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while checking for conflicts"));
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
