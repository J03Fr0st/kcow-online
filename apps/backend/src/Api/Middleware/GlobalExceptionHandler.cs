using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Kcow.Api.Middleware;

/// <summary>
/// Global exception handler that converts exceptions to ProblemDetails responses.
/// </summary>
public static class GlobalExceptionHandler
{
    /// <summary>
    /// Configures custom exception handling with ProblemDetails.
    /// </summary>
    public static void UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                if (exceptionHandlerFeature == null) return;

                var exception = exceptionHandlerFeature.Error;
                context.Response.StatusCode = exception switch
                {
                    ArgumentException => StatusCodes.Status400BadRequest,
                    UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                    KeyNotFoundException => StatusCodes.Status404NotFound,
                    InvalidOperationException => StatusCodes.Status422UnprocessableEntity,
                    _ => StatusCodes.Status500InternalServerError
                };

                context.Response.ContentType = "application/problem+json";

                var isServerError = context.Response.StatusCode >= 500;
                var problemDetails = new ProblemDetails
                {
                    Type = $"https://httpstatuses.com/{context.Response.StatusCode}",
                    Title = isServerError ? "Internal Server Error" : exception.GetType().Name,
                    Detail = isServerError ? "An unexpected error occurred." : exception.Message,
                    Status = context.Response.StatusCode,
                    Instance = context.Request.Path
                };

                // Add trace ID for correlation
                problemDetails.Extensions["traceId"] = context.TraceIdentifier;

                await context.Response.WriteAsJsonAsync(problemDetails);
            });
        });
    }
}
