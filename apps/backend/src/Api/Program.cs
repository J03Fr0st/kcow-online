using Kcow.Api.CliCommands;
using Kcow.Api.Middleware;
using Kcow.Application;
using Kcow.Application.Import;
using Kcow.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Text.Json;

// Deployment owns schema changes explicitly; this command never seeds authentication data.
if (args.Length == 2 && args[0] == "database" && args[1] == "migrate")
{
    try
    {
        var migrationBuilder = WebApplication.CreateBuilder(Array.Empty<string>());
        migrationBuilder.Services.AddInfrastructure(migrationBuilder.Configuration);
        await using var migrationApp = migrationBuilder.Build();
        await migrationApp.Services.EnsureDatabaseCreatedAsync();
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Migration failed: {ex.Message}");
        Environment.ExitCode = 1;
    }
    return;
}

// Handle CLI commands before starting the web host
if (ImportParseCommand.IsImportParseCommand(args))
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();

    var parser = new LegacyParser();
    var exitCode = await ImportParseCommand.ExecuteAsync(args, parser, Console.Out);
    Environment.Exit(exitCode);
    return;
}

if (ImportRunCommand.IsImportRunCommand(args))
{
    Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .CreateBootstrapLogger();

    var parser = new LegacyParser();

    using var cancellation = new CancellationTokenSource();
    ConsoleCancelEventHandler cancelHandler = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
    Console.CancelKeyPress += cancelHandler;
    WebApplication? cliApp = null;
    IServiceScope? cliScope = null;
    try
    {
        var exitCode = await ImportRunCommand.ExecuteAsync(args, parser, Console.Out, importServiceFactory: async () =>
        {
            var cliBuilder = WebApplication.CreateBuilder(Array.Empty<string>());
            cliBuilder.Services.AddInfrastructure(cliBuilder.Configuration);
            cliApp = cliBuilder.Build();
            await cliApp.Services.EnsureDatabaseCreatedAsync();
            cliScope = cliApp.Services.CreateScope();
            // Legacy school records reference the fixed truck catalogue; authentication is not seeded.
            await Kcow.Infrastructure.Database.Seeders.TruckSeeder.SeedAsync(
                cliScope.ServiceProvider.GetRequiredService<Kcow.Application.Interfaces.ITruckRepository>(),
                cliScope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Import"));
            return cliScope.ServiceProvider.GetRequiredService<IImportExecutionService>();
        }, cancellationToken: cancellation.Token);
        Environment.ExitCode = exitCode;
    }
    catch (OperationCanceledException)
    {
        Console.Error.WriteLine("Import cancelled. Current entity rolled back; earlier entity commits remain. Reconcile before rerunning.");
        Environment.ExitCode = 130;
    }
    finally
    {
        Console.CancelKeyPress -= cancelHandler;
        cliScope?.Dispose();
        if (cliApp is not null) await cliApp.DisposeAsync();
    }
    return;
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting KCOW API");

try
{
    var builder = WebApplication.CreateBuilder(args ?? Array.Empty<string>());

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
        ?? throw new InvalidOperationException("JWT Key not configured. Set via environment variable Jwt__Key or dotnet user-secrets.");

    // Reject known placeholder keys in non-development environments
    if (!builder.Environment.IsDevelopment() && !builder.Environment.IsEnvironment("Testing") && !builder.Environment.IsEnvironment("E2E"))
    {
        if (jwtKey.Contains("development-only") || jwtKey.Contains("minimum-32-char"))
        {
            throw new InvalidOperationException("Production JWT key not configured. Do not use development placeholder keys in production.");
        }
    }
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
    var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? new[] { "http://localhost:4200" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendOrigin", policy =>
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });

    var app = builder.Build();
    if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("E2E"))
        await app.Services.InitializeDatabaseAsync();

    // Configure the HTTP request pipeline
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

    app.MapGet("/health/ready", async (Kcow.Application.Common.IDatabaseReadiness database, CancellationToken ct) =>
        await database.IsReadyAsync(ct) ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503));

    app.Run();
}
catch (Microsoft.Extensions.Hosting.HostAbortedException ex)
{
    // This is expected for EF Core design-time operations (dotnet-ef uses HostFactoryResolver and aborts the host).
    // NOTE: We're now using DbUp instead of EF Core, but this catch is kept for compatibility.
    // Logging as Fatal makes tooling / extensions look like the app crashed even though the command succeeded.
    Log.Information(ex, "Host aborted (expected for design-time operations)");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}

// Make the implicit Program class public so test projects can access it
public partial class Program { }
