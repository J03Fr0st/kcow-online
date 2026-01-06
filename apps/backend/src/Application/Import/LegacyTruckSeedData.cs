using Kcow.Domain.Entities;

namespace Kcow.Application.Import;

public static class LegacyTruckSeedData
{
    public static IReadOnlyList<Truck> Build()
    {
        return new List<Truck>
        {
            new()
            {
                Name = "Alpha",
                RegistrationNumber = "KCOW-001",
                Status = "Active",
                Notes = "Primary truck",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Name = "Bravo",
                RegistrationNumber = "KCOW-002",
                Status = "Active",
                Notes = "Secondary truck",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
            
        };
    }
}
