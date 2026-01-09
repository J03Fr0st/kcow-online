using Kcow.Application.Activities;
using Kcow.Application.Auth;
using Kcow.Application.ClassGroups;
using Kcow.Application.Families;
using Kcow.Application.Schools;
using Kcow.Application.Students;
using Kcow.Application.Trucks;
using Kcow.Infrastructure.Activities;
using Kcow.Infrastructure.Auth;
using Kcow.Infrastructure.ClassGroups;
using Kcow.Infrastructure.Data;
using Kcow.Infrastructure.Data.Seeders;
using Kcow.Infrastructure.Families;
using Kcow.Infrastructure.Schools;
using Kcow.Infrastructure.Students;
using Kcow.Infrastructure.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure;

/// <summary>
/// Extension methods for configuring Infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Infrastructure services to the DI container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=kcow.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

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

        return services;
    }

    /// <summary>
    /// Ensures the database is created on application startup.
    /// TODO: Replace with EF Core migrations in production.
    /// </summary>
    public static async Task EnsureDatabaseCreatedAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// Seeds authentication data (roles and admin user) on application startup.
    /// Should only be called in development environment.
    /// </summary>
    public static async Task SeedAuthenticationDataAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        await AuthSeeder.SeedAsync(dbContext, logger);
    }

    /// <summary>
    /// Initializes the database (creates and seeds data) for development.
    /// This method is designed to work with WebApplicationFactory in tests.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        // Apply migrations to ensure database schema is up to date
        await dbContext.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully");

        // Seed authentication data
        await AuthSeeder.SeedAsync(dbContext, logger);
        // Seed truck data
        await TruckSeeder.SeedAsync(dbContext, logger);

        // Seed test data if in E2E test mode
        var seedTestData = Environment.GetEnvironmentVariable("DOTNET_SEED_TEST_DATA");
        if (seedTestData == "true" || seedTestData == "1")
        {
            await TestDataSeeder.SeedAsync(dbContext, logger);
        }
    }
}
