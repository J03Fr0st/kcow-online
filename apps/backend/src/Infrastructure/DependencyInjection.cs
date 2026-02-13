using Dapper;
using Kcow.Application.Activities;
using Kcow.Application.Audit;
using Kcow.Application.Attendance;
using Kcow.Application.Auth;
using Kcow.Application.Billing;
using Kcow.Application.ClassGroups;
using Kcow.Application.Evaluations;
using Kcow.Application.Families;
using Kcow.Application.Import;
using Kcow.Application.Interfaces;
using Kcow.Application.Schools;
using Kcow.Application.Students;
using Kcow.Application.Trucks;
using Kcow.Infrastructure.Activities;
using Kcow.Infrastructure.Audit;
using Kcow.Infrastructure.Auth;
using Kcow.Infrastructure.Billing;
using Kcow.Infrastructure.ClassGroups;
using Kcow.Infrastructure.Database.Seeders;
using Kcow.Infrastructure.Database;
using Kcow.Infrastructure.Evaluations;
using Kcow.Infrastructure.Families;
using Kcow.Infrastructure.Import;
using Kcow.Infrastructure.Repositories;
using Kcow.Infrastructure.Schools;
using Kcow.Infrastructure.Students;
using Kcow.Infrastructure.Trucks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure services.
/// Updated to use Dapper repositories and DbUp instead of EF Core.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure services to the DI container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Enable Dapper snake_case column to PascalCase property mapping
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=kcow.db";

        // Register Dapper connection factory
        services.AddSingleton<IDbConnectionFactory>(sp => new SqliteConnectionFactory(connectionString));

        // Register DbUp bootstrapper
        services.AddSingleton(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<DbUpBootstrapper>();
            var scriptsPath = Path.Combine(AppContext.BaseDirectory, "Migrations", "Scripts");
            return new DbUpBootstrapper(connectionString, logger, scriptsPath);
        });

        // Register Dapper repositories
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITruckRepository, TruckRepository>();
        services.AddScoped<ISchoolRepository, SchoolRepository>();
        services.AddScoped<IClassGroupRepository, ClassGroupRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IImportAuditLogRepository, ImportAuditLogRepository>();

        // Register import services
        services.AddScoped<ILegacyParser, LegacyParser>();
        services.AddScoped<IImportExecutionService, ImportExecutionService>();

        // Register authentication services
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<JwtService>();
        services.AddSingleton<PasswordHasher>();

        // Register truck services
        services.AddScoped<ITruckService, TruckService>();

        // Register school services
        services.AddScoped<ISchoolService, SchoolService>();

        // Register class group services
        services.AddScoped<IClassGroupService, ClassGroupService>();

        // Register student services
        services.AddScoped<IStudentService, StudentService>();

        // Register family services
        services.AddScoped<IFamilyService, FamilyService>();

        // Register activity services
        services.AddScoped<IActivityService, ActivityService>();

        // Register attendance services
        services.AddScoped<IAttendanceService, Kcow.Infrastructure.Attendance.AttendanceService>();

        // Register audit services
        services.AddScoped<IAuditService, AuditService>();

        // Register evaluation services
        services.AddScoped<IEvaluationService, EvaluationService>();

        // Register billing services
        services.AddScoped<IBillingService, BillingService>();

        return services;
    }

    /// <summary>
    /// Ensures the database is created and migrations are run on application startup.
    /// Uses DbUp for migrations instead of EF Core.
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider services)
    {
        var dbUpBootstrapper = services.GetRequiredService<DbUpBootstrapper>();
        var logger = services.GetRequiredService<ILogger<DbUpBootstrapper>>();

        logger.LogInformation("Running database migrations...");
        var success = dbUpBootstrapper.RunMigrations();

        if (!success)
        {
            throw new InvalidOperationException("Database migrations failed. See logs for details.");
        }

        logger.LogInformation("Database migrations completed successfully");
    }

    /// <summary>
    /// Seeds authentication data (roles and admin user) on application startup.
    /// Should only be called in development environment.
    /// Updated to use repositories instead of AppDbContext.
    /// </summary>
    public static async Task SeedAuthenticationDataAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbUpBootstrapper>>();

        await AuthSeeder.SeedAsync(roleRepository, userRepository, logger);
    }

    /// <summary>
    /// Initializes the database (creates and seeds data) for development.
    /// This method is designed to work with WebApplicationFactory in tests.
    /// Updated to use DbUp for migrations and repositories for seeding.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbUpBootstrapper = scope.ServiceProvider.GetRequiredService<DbUpBootstrapper>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbUpBootstrapper>>();

        // Run DbUp migrations
        logger.LogInformation("Running database migrations...");
        var migrationSuccess = dbUpBootstrapper.RunMigrations();
        if (!migrationSuccess)
        {
            throw new InvalidOperationException("Database migrations failed. See logs for details.");
        }
        logger.LogInformation("Database migrations applied successfully");

        // Get repositories for seeding
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var truckRepository = scope.ServiceProvider.GetRequiredService<ITruckRepository>();

        // Seed authentication data
        await AuthSeeder.SeedAsync(roleRepository, userRepository, logger);

        // Seed truck data
        await TruckSeeder.SeedAsync(truckRepository, logger);

        // Seed test data if in E2E test mode
        var seedTestData = Environment.GetEnvironmentVariable("DOTNET_SEED_TEST_DATA");
        if (seedTestData == "true" || seedTestData == "1")
        {
            await TestDataSeeder.SeedAsync(truckRepository, logger);
        }
    }
}
