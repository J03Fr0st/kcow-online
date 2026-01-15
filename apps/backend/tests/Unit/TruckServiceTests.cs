using Kcow.Application.Interfaces;
using Kcow.Application.Trucks;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Trucks;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Kcow.Unit.Tests;

public class TruckServiceTests
{
    private readonly ITruckRepository _truckRepository;
    private readonly TruckService _service;

    public TruckServiceTests()
    {
        _truckRepository = Substitute.For<ITruckRepository>();
        _service = new TruckService(_truckRepository, NullLogger<TruckService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_Persists_Truck()
    {
        // Arrange
        var request = new CreateTruckRequest
        {
            Name = "Unit Truck",
            RegistrationNumber = "UNIT-001",
            Status = "Active",
            Notes = "Notes"
        };

        _truckRepository.ExistsByRegistrationNumberAsync("UNIT-001", Arg.Any<CancellationToken>())
            .Returns(false);
        _truckRepository.CreateAsync(Arg.Any<Truck>(), Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unit Truck", result.Name);
        Assert.Equal("UNIT-001", result.RegistrationNumber);
        Assert.True(result.IsActive);
        await _truckRepository.Received(1).CreateAsync(Arg.Any<Truck>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateRegistrationNumber_Throws()
    {
        // Arrange
        var request = new CreateTruckRequest
        {
            Name = "Unit Truck",
            RegistrationNumber = "DUP-001",
            Status = "Active"
        };

        _truckRepository.ExistsByRegistrationNumberAsync("DUP-001", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task GetAllAsync_OnlyReturnsActiveTrucks_InNameOrder()
    {
        // Arrange
        var trucks = new List<Truck>
        {
            new Truck { Id = 1, Name = "Beta", RegistrationNumber = "B1", Status = "Active", IsActive = true },
            new Truck { Id = 2, Name = "Alpha", RegistrationNumber = "A1", Status = "Active", IsActive = true }
        };

        _truckRepository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(trucks);

        // Act
        var results = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Alpha", results[0].Name);
        Assert.Equal("Beta", results[1].Name);
    }

    [Fact]
    public async Task ArchiveAsync_Sets_IsActive_False()
    {
        // Arrange
        var truck = new Truck { Id = 1, Name = "Archive Me", RegistrationNumber = "ARC-001", Status = "Active", IsActive = true };
        _truckRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(truck);
        _truckRepository.UpdateAsync(Arg.Any<Truck>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var archived = await _service.ArchiveAsync(1);

        // Assert
        Assert.True(archived);
        await _truckRepository.Received(1).UpdateAsync(
            Arg.Is<Truck>(t => t.IsActive == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        _truckRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Truck?)null);

        // Act
        var result = await _service.ArchiveAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsTruck()
    {
        // Arrange
        var truck = new Truck
        {
            Id = 1,
            Name = "Test Truck",
            RegistrationNumber = "TEST-001",
            Status = "Active",
            IsActive = true
        };

        _truckRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(truck);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Truck", result.Name);
        Assert.Equal("TEST-001", result.RegistrationNumber);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _truckRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Truck?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_UpdatesTruck()
    {
        // Arrange
        var truck = new Truck
        {
            Id = 1,
            Name = "Original Name",
            RegistrationNumber = "ORIG-001",
            Status = "Active",
            IsActive = true
        };

        _truckRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(truck);
        _truckRepository.GetByRegistrationNumberAsync("UPDATED-001", Arg.Any<CancellationToken>())
            .Returns((Truck?)null);
        _truckRepository.UpdateAsync(Arg.Any<Truck>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var request = new UpdateTruckRequest
        {
            Name = "Updated Name",
            RegistrationNumber = "UPDATED-001",
            Status = "Maintenance",
            Notes = "Updated notes"
        };

        // Act
        var result = await _service.UpdateAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("UPDATED-001", result.RegistrationNumber);
        Assert.Equal("Maintenance", result.Status);
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateRegistrationNumber_Throws()
    {
        // Arrange
        var truck = new Truck
        {
            Id = 1,
            Name = "Original Name",
            RegistrationNumber = "ORIG-001",
            Status = "Active",
            IsActive = true
        };

        var existingTruck = new Truck
        {
            Id = 2,
            Name = "Other Truck",
            RegistrationNumber = "EXISTING-001",
            Status = "Active",
            IsActive = true
        };

        _truckRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(truck);
        _truckRepository.GetByRegistrationNumberAsync("EXISTING-001", Arg.Any<CancellationToken>())
            .Returns(existingTruck);

        var request = new UpdateTruckRequest
        {
            Name = "Updated Name",
            RegistrationNumber = "EXISTING-001",
            Status = "Active"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(1, request));
    }
}
