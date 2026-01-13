using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Database.Seeders;

public static class TestDataSeeder
{
    public static async Task SeedAsync(ITruckRepository truckRepository, ILogger logger)
    {
        // Add specific test data logic here if needed for E2E tests
        // For now, we'll just log that it ran
        logger.LogInformation("Test data seeding completed (no extra data configured).");
        await Task.CompletedTask;
    }
}
