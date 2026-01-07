using Kcow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Data.Seeders;

/// <summary>
/// Seeds truck data.
/// </summary>
public static class TruckSeeder
{
    /// <summary>
    /// Seeds truck data into the database.
    /// </summary>
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            // Seed Truck 1
            var truck1 = await context.Trucks.FirstOrDefaultAsync(t => t.Id == 1);
            if (truck1 == null)
            {
                truck1 = new Truck
                {
                    Id = 1,
                    Name = "Truck 1",
                    RegistrationNumber = "ABC123GP",
                    Status = "Active",
                    Notes = "Primary truck",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Trucks.Add(truck1);
                logger.LogInformation("Truck 1 seeded successfully");
            }

            // Seed Truck 2
            var truck2 = await context.Trucks.FirstOrDefaultAsync(t => t.Id == 2);
            if (truck2 == null)
            {
                truck2 = new Truck
                {
                    Id = 2,
                    Name = "Truck 2",
                    RegistrationNumber = "XYZ789GP",
                    Status = "Active",
                    Notes = "Secondary truck",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                context.Trucks.Add(truck2);
                logger.LogInformation("Truck 2 seeded successfully");
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Truck seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while seeding truck data");
            throw;
        }
    }
}
