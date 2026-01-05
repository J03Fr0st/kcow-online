using Kcow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Data.Seeders;

/// <summary>
/// Seeds test data for E2E testing.
/// This seeder adds sample data that E2E tests can interact with.
/// </summary>
public static class TestDataSeeder
{
    /// <summary>
    /// Seeds test data into the database.
/// This should only be called in testing/E2E environments.
    /// </summary>
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Seeding test data for E2E tests");

            // Seed test trucks
            await SeedTrucksAsync(context, logger);

            logger.LogInformation("Test data seeded successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while seeding test data");
            throw;
        }
    }

    /// <summary>
    /// Seeds sample truck data for testing.
    /// </summary>
    private static async Task SeedTrucksAsync(AppDbContext context, ILogger logger)
    {
        // Check if trucks already exist
        if (await context.Trucks.AnyAsync())
        {
            logger.LogInformation("Trucks already exist, skipping truck seeding");
            return;
        }

        var trucks = new List<Truck>
        {
            new()
            {
                Name = "KCOW-Alpha",
                RegistrationNumber = "KCOW-001",
                Status = "Active",
                Notes = "Primary truck for downtown visits",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "KCOW-Bravo",
                RegistrationNumber = "KCOW-002",
                Status = "Active",
                Notes = "Secondary truck for suburban visits",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "KCOW-Charlie",
                RegistrationNumber = "KCOW-003",
                Status = "Maintenance",
                Notes = "Currently undergoing scheduled maintenance",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "KCOW-Delta",
                RegistrationNumber = "KCOW-004",
                Status = "Active",
                Notes = "Newest truck in fleet",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "KCOW- legacy",
                RegistrationNumber = "KCOW-005",
                Status = "Inactive",
                Notes = "Old truck being phased out",
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Trucks.AddRangeAsync(trucks);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} sample trucks", trucks.Count);
    }
}
