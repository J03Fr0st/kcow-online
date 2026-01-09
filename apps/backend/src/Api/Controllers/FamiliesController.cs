using Kcow.Application.Families;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kcow.Api.Controllers;

[ApiController]
[Route("api/families")]
[Authorize]
public class FamiliesController : ControllerBase
{
    private readonly IFamilyService _familyService;
    private readonly ILogger<FamiliesController> _logger;

    public FamiliesController(IFamilyService familyService, ILogger<FamiliesController> logger)
    {
        _familyService = familyService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<FamilyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var families = await _familyService.GetAllAsync();
            return Ok(families);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all families");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving families"));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(FamilyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var family = await _familyService.GetByIdAsync(id);
            if (family == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Family not found",
                    Status = 404,
                    Detail = "The requested family was not found"
                });
            }
            return Ok(family);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving family with ID {FamilyId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving the family"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(typeof(FamilyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateFamilyRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var family = await _familyService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = family.Id }, family);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating family");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while creating the family"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(typeof(FamilyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateFamilyRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var family = await _familyService.UpdateAsync(id, request);
            if (family == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Family not found",
                    Status = 404,
                    Detail = "The requested family was not found"
                });
            }
            return Ok(family);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating family with ID {FamilyId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while updating the family"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Archive(int id)
    {
        try
        {
            var archived = await _familyService.ArchiveAsync(id);
            if (!archived)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Family not found",
                    Status = 404,
                    Detail = "The requested family was not found"
                });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving family with ID {FamilyId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while archiving the family"));
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
