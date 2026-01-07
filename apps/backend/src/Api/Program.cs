using Kcow.Api.Middleware;
using Kcow.Application;
using Kcow.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Text.Json;

// #region agent log
static void AgentDebugLog(string runId, string hypothesisId, string location, string message, object data)
{
    try
    {
        const string logPath = @"d:\Source\kcow-online\.cursor\debug.log";
        var payload = new
        {
            sessionId = "debug-session",
            runId,
            hypothesisId,
            location,
            message,
            data,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        File.AppendAllText(logPath, JsonSerializer.Serialize(payload) + Environment.NewLine);
    }
    catch
    {
        // ignore logging failures
    }
}
// #endregion

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting KCOW API");

try
{
    AgentDebugLog("pre-fix", "H1", "Api/Program.cs:Main", "Process starting", new { argsLength = args?.Length ?? 0 });

    var builder = WebApplication.CreateBuilder(args ?? Array.Empty<string>());

    // #region agent log
    AgentDebugLog("pre-fix", "H3", "Api/Program.cs:Main", "Environment/config snapshot", new
    {
        env = builder.Environment.EnvironmentName,
        defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection")
    });
    // #endregion

    // Register database initialization hosted service for development and E2E testing
    if (builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("E2E"))
    {
        builder.Services.AddHostedService<DatabaseInitializationService>();
    }

    // Configure Serilog
    if (!builder.Environment.IsEnvironment("Testing"))
    {
        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/kcow-.log", rollingInterval: RollingInterval.Day));
    }

    // Add services to the container
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add controllers
    builder.Services.AddControllers();

    // Configure JWT Authentication
    var jwtKey = builder.Configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("JWT Key not configured");
    var jwtIssuer = builder.Configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("JWT Issuer not configured");
    var jwtAudience = builder.Configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("JWT Audience not configured");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };

        // Ensure consistent ProblemDetails on auth failures.
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException))
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                if (context.Response.HasStarted)
                {
                    return Task.CompletedTask;
                }

                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Title = "Unauthorized",
                    Status = StatusCodes.Status401Unauthorized,
                    Detail = "Authentication is required to access this resource.",
                    Instance = context.HttpContext.Request.Path
                };
                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                var payload = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                return context.Response.WriteAsync(payload);
            },
            OnForbidden = context =>
            {
                if (context.Response.HasStarted)
                {
                    return Task.CompletedTask;
                }

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/problem+json";

                var problemDetails = new ProblemDetails
                {
                    Title = "Forbidden",
                    Status = StatusCodes.Status403Forbidden,
                    Detail = "You do not have permission to access this resource.",
                    Instance = context.HttpContext.Request.Path
                };
                problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

                var payload = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                return context.Response.WriteAsync(payload);
            }
        };
    });

    builder.Services.AddAuthorization();

    // Configure ProblemDetails for exception handling
    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            context.ProblemDetails.Instance = context.HttpContext.Request.Path;
            // Some ASP.NET versions include a traceId by default; set (don't Add) to avoid duplicate-key exceptions.
            context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        };
    });

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendOrigin", policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    // Note: Database initialization moved to a hosted service for test compatibility
    app.UseGlobalExceptionHandler();
    // app.UseStatusCodePages();

    // Serilog request logging middleware requires Serilog services (DiagnosticContext).
    // In Testing environment we intentionally skip full Serilog wiring, so don't register
    // the middleware to avoid host startup failures in integration tests.
    if (!app.Environment.IsEnvironment("Testing"))
    {
        app.UseSerilogRequestLogging();
    }

    // Only redirect to HTTPS in production
    // In development, allow HTTP to avoid CORS and connection issues
    if (!app.Environment.IsDevelopment() && !app.Environment.IsEnvironment("E2E"))
    {
        app.UseHttpsRedirection();
    }
    
    app.UseCors("FrontendOrigin");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // Health check endpoints
    // - /health is the canonical endpoint
    // - /api/health is a compatibility alias used by frontend tooling/scripts
    var healthPayload = () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });

    app.MapGet("/health", healthPayload)
        .WithName("HealthCheck")
        .WithTags("Health");

    app.MapGet("/api/health", healthPayload)
        .WithName("ApiHealthCheck")
        .WithTags("Health");

    app.Run();
}
catch (Microsoft.Extensions.Hosting.HostAbortedException ex)
{
    // This is expected for EF Core design-time operations (dotnet-ef uses HostFactoryResolver and aborts the host).
    // Logging as Fatal makes tooling / extensions look like the app crashed even though the command succeeded.
    AgentDebugLog("pre-fix", "H1", "Api/Program.cs:catch(HostAbortedException)", "Host aborted (expected design-time)", new { exType = ex.GetType().FullName, exMessage = ex.Message });
    Log.Information(ex, "Host aborted (expected for design-time operations)");
}
catch (Exception ex)
{
    AgentDebugLog("pre-fix", "H2", "Api/Program.cs:catch(Exception)", "Unhandled exception during startup", new { exType = ex.GetType().FullName, exMessage = ex.Message });
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make the implicit Program class public so test projects can access it
public partial class Program { }

/// <summary>
/// Background service that initializes the database on application startup in development.
/// </summary>
public class DatabaseInitializationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(IServiceProvider serviceProvider, ILogger<DatabaseInitializationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Database initialization service starting");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            await scope.ServiceProvider.InitializeDatabaseAsync();
            _logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed");
            throw;
        }
    }
}
