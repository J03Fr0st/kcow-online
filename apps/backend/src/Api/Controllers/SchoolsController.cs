using Kcow.Application.Interfaces;
using Kcow.Application.Schools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kcow.Api.Controllers;

/// <summary>
/// Controller for school management operations.
/// </summary>
[ApiController]
[Route("api/schools")]
[Authorize]
public class SchoolsController : ControllerBase
{
    private readonly ISchoolService _schoolService;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ILogger<SchoolsController> _logger;

    public SchoolsController(ISchoolService schoolService, ISchoolRepository schoolRepository, ILogger<SchoolsController> logger)
    {
        _schoolService = schoolService;
        _schoolRepository = schoolRepository;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active schools.
    /// </summary>
    /// <returns>List of active schools</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<SchoolDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var schools = await _schoolService.GetAllAsync();
            return Ok(schools);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all schools");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving schools"));
        }
    }

    /// <summary>
    /// Gets a specific school by ID.
    /// </summary>
    /// <param name="id">School ID</param>
    /// <returns>School details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SchoolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var school = await _schoolService.GetByIdAsync(id);

            if (school == null)
            {
                // Check if school exists but is archived
                var archivedSchool = await _schoolRepository.GetByIdAsync(id);
                if (archivedSchool != null && !archivedSchool.IsActive)
                {
                    return StatusCode(StatusCodes.Status410Gone, new ProblemDetails
                    {
                        Title = "School archived",
                        Status = 410,
                        Detail = $"School with ID {id} has been archived and is no longer active"
                    });
                }

                return NotFound(new ProblemDetails
                {
                    Title = "School not found",
                    Status = 404,
                    Detail = $"No school exists with ID {id}"
                });
            }

            return Ok(school);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving school with ID {SchoolId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving the school"));
        }
    }

    /// <summary>
    /// Creates a new school.
    /// </summary>
    /// <param name="request">School creation request</param>
    /// <returns>Created school details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SchoolDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateSchoolRequest request)
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
            var school = await _schoolService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = school.Id }, school);
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
            _logger.LogError(ex, "Error creating school");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while creating the school"));
        }
    }

    /// <summary>
    /// Updates an existing school.
    /// </summary>
    /// <param name="id">School ID</param>
    /// <param name="request">School update request</param>
    /// <returns>Updated school details</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SchoolDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSchoolRequest request)
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
            var school = await _schoolService.UpdateAsync(id, request);

            if (school == null)
            {
                // Check if school exists but is archived
                var archivedSchool = await _schoolRepository.GetByIdAsync(id);
                if (archivedSchool != null && !archivedSchool.IsActive)
                {
                    return StatusCode(StatusCodes.Status410Gone, new ProblemDetails
                    {
                        Title = "School archived",
                        Status = 410,
                        Detail = $"Cannot update school with ID {id} because it has been archived"
                    });
                }

                return NotFound(new ProblemDetails
                {
                    Title = "School not found",
                    Status = 404,
                    Detail = $"No school exists with ID {id}"
                });
            }

            return Ok(school);
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
            _logger.LogError(ex, "Error updating school with ID {SchoolId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while updating the school"));
        }
    }

    /// <summary>
    /// Archives (soft-deletes) a school.
    /// </summary>
    /// <param name="id">School ID</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status410Gone)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Archive(int id)
    {
        try
        {
            var archived = await _schoolService.ArchiveAsync(id);

            if (!archived)
            {
                // Check if school exists but is already archived
                var archivedSchool = await _schoolRepository.GetByIdAsync(id);
                if (archivedSchool != null && !archivedSchool.IsActive)
                {
                    return StatusCode(StatusCodes.Status410Gone, new ProblemDetails
                    {
                        Title = "School already archived",
                        Status = 410,
                        Detail = $"School with ID {id} is already archived"
                    });
                }

                return NotFound(new ProblemDetails
                {
                    Title = "School not found",
                    Status = 404,
                    Detail = $"No school exists with ID {id}"
                });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving school with ID {SchoolId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while archiving the school"));
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
