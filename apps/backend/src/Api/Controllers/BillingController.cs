using System.Security.Claims;
using Kcow.Application.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kcow.Api.Controllers;

[ApiController]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly IBillingService _billingService;
    private readonly ILogger<BillingController> _logger;

    public BillingController(IBillingService billingService, ILogger<BillingController> logger)
    {
        _billingService = billingService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current authenticated user's name for audit logging.
    /// </summary>
    private string GetCurrentUser()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.Identity?.Name
            ?? "Unknown";
    }

    [HttpGet]
    [Route("api/students/{studentId}/billing")]
    [ProducesResponseType(typeof(BillingSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetBillingSummary(int studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var summary = await _billingService.GetBillingSummaryAsync(studentId, cancellationToken);
            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Student not found for billing summary: {StudentId}", studentId);
            return NotFound(new ProblemDetails
            {
                Title = "Student not found",
                Status = 404,
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving billing summary for student {StudentId}", studentId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving billing summary"));
        }
    }

    [HttpGet]
    [Route("api/students/{studentId}/invoices")]
    [ProducesResponseType(typeof(List<InvoiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetInvoices(int studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var invoices = await _billingService.GetInvoicesByStudentIdAsync(studentId, cancellationToken);
            return Ok(invoices);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Student not found for invoices: {StudentId}", studentId);
            return NotFound(new ProblemDetails
            {
                Title = "Student not found",
                Status = 404,
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoices for student {StudentId}", studentId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving invoices"));
        }
    }

    [HttpPost]
    [Route("api/students/{studentId}/invoices")]
    [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateInvoice(int studentId, [FromBody] CreateInvoiceRequest request, CancellationToken cancellationToken = default)
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
            var currentUser = GetCurrentUser();
            var invoice = await _billingService.CreateInvoiceAsync(studentId, request, currentUser, cancellationToken);
            return CreatedAtAction(nameof(GetInvoices), new { studentId }, invoice);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning(ex, "Entity not found while creating invoice for student {StudentId}", studentId);
            return NotFound(new ProblemDetails
            {
                Title = "Not found",
                Status = 404,
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation failed while creating invoice for student {StudentId}", studentId);
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Status = 400,
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice for student {StudentId}", studentId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while creating the invoice"));
        }
    }

    [HttpGet]
    [Route("api/students/{studentId}/payments")]
    [ProducesResponseType(typeof(List<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPayments(int studentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payments = await _billingService.GetPaymentsByStudentIdAsync(studentId, cancellationToken);
            return Ok(payments);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Student not found for payments: {StudentId}", studentId);
            return NotFound(new ProblemDetails
            {
                Title = "Student not found",
                Status = 404,
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payments for student {StudentId}", studentId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while retrieving payments"));
        }
    }

    [HttpPost]
    [Route("api/students/{studentId}/payments")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePayment(int studentId, [FromBody] CreatePaymentRequest request, CancellationToken cancellationToken = default)
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
            var currentUser = GetCurrentUser();
            var payment = await _billingService.CreatePaymentAsync(studentId, request, currentUser, cancellationToken);
            return CreatedAtAction(nameof(GetPayments), new { studentId }, payment);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("does not exist"))
        {
            _logger.LogWarning(ex, "Entity not found while creating payment for student {StudentId}", studentId);
            return NotFound(new ProblemDetails
            {
                Title = "Not found",
                Status = 404,
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation failed while creating payment for student {StudentId}", studentId);
            return BadRequest(new ProblemDetails
            {
                Title = "Validation failed",
                Status = 400,
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for student {StudentId}", studentId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CreateServerErrorProblemDetails("An error occurred while creating the payment"));
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
