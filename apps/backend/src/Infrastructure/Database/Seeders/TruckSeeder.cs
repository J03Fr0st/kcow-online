using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Database.Seeders;

public static class TruckSeeder
{
    public static async Task SeedAsync(ITruckRepository truckRepository, ILogger logger)
    {
        var trucks = new[]
        {
            new Truck { Name = "Blue Truck", RegistrationNumber = "CA 123-456", Status = "Active", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Truck { Name = "Red Truck", RegistrationNumber = "CA 789-012", Status = "Active", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Truck { Name = "Yellow Truck", RegistrationNumber = "CA 345-678", Status = "Active", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        foreach (var truck in trucks)
        {
            if (!await truckRepository.ExistsByRegistrationNumberAsync(truck.RegistrationNumber))
            {
                logger.LogInformation("Seeding truck: {TruckName}", truck.Name);
                await truckRepository.CreateAsync(truck);
            }
        }
    }
}
