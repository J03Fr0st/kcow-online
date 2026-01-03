using Kcow.Domain;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Data.Seeders;

/// <summary>
/// Seeds authentication data (roles and default admin user).
/// </summary>
public static class AuthSeeder
{
    /// <summary>
    /// Seeds authentication data into the database.
    /// </summary>
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            // Seed Admin role
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == Constants.Roles.Admin);
            if (adminRole == null)
            {
                adminRole = new Role
                {
                    Name = Constants.Roles.Admin,
                    CreatedAt = DateTime.UtcNow
                };
                context.Roles.Add(adminRole);
                await context.SaveChangesAsync();
                logger.LogInformation("Admin role seeded successfully");
            }

            // Seed default admin user
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@kcow.local");
            if (adminUser == null)
            {
                var passwordHasher = new PasswordHasher();
                var passwordHash = passwordHasher.HashPassword("Admin123!");

                adminUser = new User
                {
                    Email = "admin@kcow.local",
                    PasswordHash = passwordHash,
                    Name = "Administrator",
                    RoleId = adminRole.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
                logger.LogInformation("Default admin user seeded successfully (Email: admin@kcow.local)");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while seeding authentication data");
            throw;
        }
    }
}
