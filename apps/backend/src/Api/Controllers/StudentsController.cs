using Kcow.Application.Attendance;
using Kcow.Application.Common;
using Kcow.Application.Families;
using Kcow.Application.Students;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kcow.Api.Controllers;

/// <summary>
/// Controller for student management operations.
/// </summary>
[ApiController]
[Route("api/students")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;
    private readonly IFamilyService _familyService;
    private readonly IAttendanceService _attendanceService;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IStudentService studentService, IFamilyService familyService, IAttendanceService attendanceService, ILogger<StudentsController> logger)
    {
        _studentService = studentService;
        _familyService = familyService;
        _attendanceService = attendanceService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paginated list of students with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<StudentListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? schoolId = null,
        [FromQuery] int? classGroupId = null,
        [FromQuery] string? search = null)
    {
        try
        {
            // Enforce reasonable page size limits to prevent DoS
            const int maxPageSize = 100;
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > maxPageSize) pageSize = maxPageSize;

            var result = await _studentService.GetPagedAsync(page, pageSize, schoolId, classGroupId, search);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged students");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving students"));
        }
    }

    /// <summary>
    /// Gets a specific student by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var student = await _studentService.GetByIdAsync(id);

            if (student == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Student not found",
                    Status = 404,
                    Detail = "The requested student was not found"
                });
            }

            return Ok(student);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving student with ID {StudentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving the student"));
        }
    }

    /// <summary>
    /// Creates a new student.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateStudentRequest request)
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
            var student = await _studentService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = student.Id }, student);
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
            _logger.LogError(ex, "Error creating student");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while creating the student"));
        }
    }

    /// <summary>
    /// Updates an existing student.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentRequest request)
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
            var student = await _studentService.UpdateAsync(id, request);

            if (student == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Student not found",
                    Status = 404,
                    Detail = "The requested student was not found"
                });
            }

            return Ok(student);
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
            _logger.LogError(ex, "Error updating student with ID {StudentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while updating the student"));
        }
    }

    /// <summary>
    /// Archives (soft-deletes) a student.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Archive(int id)
    {
        try
        {
            var archived = await _studentService.ArchiveAsync(id);

            if (!archived)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Student not found",
                    Status = 404,
                    Detail = "The requested student was not found"
                });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving student with ID {StudentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while archiving the student"));
        }
    }

    [HttpGet("{id}/families")]
    [ProducesResponseType(typeof(List<FamilyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFamilies(int id)
    {
        try
        {
            var families = await _familyService.GetByStudentIdAsync(id);
            return Ok(families);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving families for student {StudentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving families"));
        }
    }

    [HttpPost("{id}/families")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LinkFamily(int id, [FromBody] LinkFamilyRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var linked = await _familyService.LinkToStudentAsync(id, request);
            if (!linked)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Link failed",
                    Status = 400,
                    Detail = "Could not link family to student. Verify IDs exist."
                });
            }
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking family to student {StudentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while linking the family"));
        }
    }

    [HttpDelete("{id}/families/{familyId}")]
    [Authorize(Roles = "Admin,Teacher")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UnlinkFamily(int id, int familyId)
    {
        try
        {
            var unlinked = await _familyService.UnlinkFromStudentAsync(id, familyId);
            if (!unlinked)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Link not found",
                    Status = 404,
                    Detail = "The requested family link was not found"
                });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking family from student {StudentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while unlinking the family"));
        }
    }

    /// <summary>
    /// Searches for students by name (global search for typeahead).
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<StudentSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid search query",
                Status = 400,
                Detail = "Search query 'q' parameter is required"
            });
        }

        try
        {
            var results = await _studentService.SearchAsync(q, limit);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching students with query '{Query}'", q);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while searching for students"));
        }
    }

    /// <summary>
    /// Gets attendance history for a specific student.
    /// </summary>
    [HttpGet("{id}/attendance")]
    [ProducesResponseType(typeof(List<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAttendance(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var records = await _attendanceService.GetByStudentIdAsync(id, cancellationToken);
            return Ok(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance for student {StudentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving attendance records"));
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
