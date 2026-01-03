using Kcow.Application.Trucks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kcow.Api.Controllers;

/// <summary>
/// Controller for truck management operations.
/// </summary>
[ApiController]
[Route("api/trucks")]
[Authorize]
public class TrucksController : ControllerBase
{
    private readonly ITruckService _truckService;
    private readonly ILogger<TrucksController> _logger;

    public TrucksController(ITruckService truckService, ILogger<TrucksController> logger)
    {
        _truckService = truckService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all active trucks.
    /// </summary>
    /// <returns>List of active trucks</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<TruckDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var trucks = await _truckService.GetAllAsync();
            return Ok(trucks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all trucks");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving trucks"));
        }
    }

    /// <summary>
    /// Gets a specific truck by ID.
    /// </summary>
    /// <param name="id">Truck ID</param>
    /// <returns>Truck details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TruckDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var truck = await _truckService.GetByIdAsync(id);

            if (truck == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Truck not found",
                    Status = 404,
                    Detail = $"Truck with ID {id} was not found"
                });
            }

            return Ok(truck);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving truck with ID {TruckId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving the truck"));
        }
    }

    /// <summary>
    /// Creates a new truck.
    /// </summary>
    /// <param name="request">Truck creation request</param>
    /// <returns>Created truck details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TruckDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateTruckRequest request)
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
            var truck = await _truckService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = truck.Id }, truck);
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
            _logger.LogError(ex, "Error creating truck");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while creating the truck"));
        }
    }

    /// <summary>
    /// Updates an existing truck.
    /// </summary>
    /// <param name="id">Truck ID</param>
    /// <param name="request">Truck update request</param>
    /// <returns>Updated truck details</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TruckDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTruckRequest request)
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
            var truck = await _truckService.UpdateAsync(id, request);

            if (truck == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Truck not found",
                    Status = 404,
                    Detail = $"Truck with ID {id} was not found"
                });
            }

            return Ok(truck);
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
            _logger.LogError(ex, "Error updating truck with ID {TruckId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while updating the truck"));
        }
    }

    /// <summary>
    /// Archives (soft-deletes) a truck.
    /// </summary>
    /// <param name="id">Truck ID</param>
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
            var archived = await _truckService.ArchiveAsync(id);

            if (!archived)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Truck not found",
                    Status = 404,
                    Detail = $"Truck with ID {id} was not found"
                });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving truck with ID {TruckId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while archiving the truck"));
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
