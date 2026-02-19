using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Database.Seeders;

public static class TestDataSeeder
{
    public static async Task SeedAsync(ITruckRepository truckRepository, IStudentRepository studentRepository, ILogger logger)
    {
        // Seed a minimal student so E2E tests that navigate to /students/:id/edit
        // have at least one record to work with instead of skipping.
        var existing = await studentRepository.GetAllAsync();
        if (!existing.Any())
        {
            logger.LogInformation("Seeding E2E test student...");
            await studentRepository.CreateAsync(new Student
            {
                Reference = "E2E-001",
                FirstName = "Test",
                LastName = "Student",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
            logger.LogInformation("E2E test student seeded.");
        }

        logger.LogInformation("Test data seeding completed.");
    }
}
