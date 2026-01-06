using Kcow.Application.Import;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Import;
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

            // Seed test trucks first (needed for school route validation)
            await SeedTrucksAsync(context, logger);

            // Seed test schools from legacy XML
            await SeedSchoolsAsync(context, logger);

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

        var trucks = LegacyTruckSeedData.Build();

        await context.Trucks.AddRangeAsync(trucks);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} sample trucks", trucks.Count);
    }

    /// <summary>
    /// Seeds school data from legacy XML files.
    /// </summary>
    private static async Task SeedSchoolsAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Schools.AnyAsync())
        {
            logger.LogInformation("Schools already exist, skipping school seeding");
            return;
        }

        try
        {
            var xmlPath = FindRepoFile("docs/legacy/1_School/School.xml");
            var xsdPath = FindRepoFile("docs/legacy/1_School/School.xsd");

            logger.LogInformation("Importing schools from {XmlPath}", xmlPath);

            var importer = new Kcow.Infrastructure.Import.LegacySchoolImportService(context);
            var summary = await importer.ImportAsync(xmlPath, xsdPath, null, null);

            logger.LogInformation("Seeded {Count} schools from legacy data", summary.ImportedCount);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to seed schools from legacy data. Continuing with empty schools.");
        }
    }

    private static string FindRepoFile(string relativePath)
    {
        var baseDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        while (baseDirectory != null)
        {
            var candidate = Path.Combine(baseDirectory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            baseDirectory = baseDirectory.Parent;
        }

        // Fallback for different environments
        var currentDir = Directory.GetCurrentDirectory();
        var fallback = Path.Combine(currentDir, relativePath);
        if (File.Exists(fallback)) return fallback;

        // Try one level up from current dir
        var parentFallback = Path.Combine(Directory.GetParent(currentDir)?.FullName ?? "", relativePath);
        if (File.Exists(parentFallback)) return parentFallback;

        throw new FileNotFoundException($"Unable to locate {relativePath}");
    }
}
