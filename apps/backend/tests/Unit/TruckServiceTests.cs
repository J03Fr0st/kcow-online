using Kcow.Application.Trucks;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Data;
using Kcow.Infrastructure.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kcow.Unit.Tests;

public class TruckServiceTests
{
    [Fact]
    public async Task CreateAsync_Persists_Truck()
    {
        using var context = CreateContext();
        var service = new TruckService(context, NullLogger<TruckService>.Instance);

        var request = new CreateTruckRequest
        {
            Name = "Unit Truck",
            RegistrationNumber = "UNIT-001",
            Status = "Active",
            Notes = "Notes"
        };

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Unit Truck", result.Name);
        Assert.Equal("UNIT-001", result.RegistrationNumber);
        Assert.True(result.IsActive);
        Assert.Equal(1, await context.Trucks.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateRegistrationNumber_Throws()
    {
        using var context = CreateContext();
        var service = new TruckService(context, NullLogger<TruckService>.Instance);

        var request = new CreateTruckRequest
        {
            Name = "Unit Truck",
            RegistrationNumber = "DUP-001",
            Status = "Active"
        };

        await service.CreateAsync(request);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task GetAllAsync_OnlyReturnsActiveTrucks_InNameOrder()
    {
        using var context = CreateContext();
        context.Trucks.AddRange(
            new Truck { Name = "Beta", RegistrationNumber = "B1", Status = "Active", IsActive = true },
            new Truck { Name = "Alpha", RegistrationNumber = "A1", Status = "Active", IsActive = true },
            new Truck { Name = "Inactive", RegistrationNumber = "I1", Status = "Retired", IsActive = false }
        );
        await context.SaveChangesAsync();

        var service = new TruckService(context, NullLogger<TruckService>.Instance);
        var results = await service.GetAllAsync();

        Assert.Equal(2, results.Count);
        Assert.Collection(results,
            first => Assert.Equal("Alpha", first.Name),
            second => Assert.Equal("Beta", second.Name));
    }

    [Fact]
    public async Task ArchiveAsync_Sets_IsActive_False()
    {
        using var context = CreateContext();
        var truck = new Truck { Name = "Archive Me", RegistrationNumber = "ARC-001", Status = "Active" };
        context.Trucks.Add(truck);
        await context.SaveChangesAsync();

        var service = new TruckService(context, NullLogger<TruckService>.Instance);
        var archived = await service.ArchiveAsync(truck.Id);

        Assert.True(archived);
        var reloaded = await context.Trucks.FindAsync(truck.Id);
        Assert.NotNull(reloaded);
        Assert.False(reloaded!.IsActive);
        Assert.NotNull(reloaded.UpdatedAt);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new AppDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();
        return context;
    }
}
