using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Auth;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Database.Seeders;

public static class AuthSeeder
{
    public static async Task SeedAsync(IRoleRepository roleRepository, IUserRepository userRepository, ILogger logger)
    {
        // Seed Roles
        if (!await roleRepository.ExistsByNameAsync("Admin"))
        {
            logger.LogInformation("Seeding Admin role...");
            await roleRepository.CreateAsync(new Role { Name = "Admin", CreatedAt = DateTime.UtcNow });
        }

        if (!await roleRepository.ExistsByNameAsync("User"))
        {
            logger.LogInformation("Seeding User role...");
            await roleRepository.CreateAsync(new Role { Name = "User", CreatedAt = DateTime.UtcNow });
        }

        // Seed Admin User
        var adminEmail = "admin@kcow.local";
        var existingAdmin = await userRepository.GetByEmailAsync(adminEmail);
        
        if (existingAdmin == null)
        {
            logger.LogInformation("Seeding Admin user...");
            var adminRole = await roleRepository.GetByNameAsync("Admin");
            
            if (adminRole != null)
            {
                var passwordHasher = new PasswordHasher();
                var adminUser = new User
                {
                    Email = adminEmail,
                    Name = "System Admin",
                    RoleId = adminRole.Id,
                    PasswordHash = passwordHasher.HashPassword("Admin123!"),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                await userRepository.CreateAsync(adminUser);
            }
            else
            {
                logger.LogError("Admin role not found. Skipping Admin user seeding.");
            }
        }
    }
}
