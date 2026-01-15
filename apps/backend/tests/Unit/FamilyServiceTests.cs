using Kcow.Application.Families;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using Kcow.Infrastructure.Families;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Kcow.Unit.Tests;

public class FamilyServiceTests
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly FamilyService _service;

    public FamilyServiceTests()
    {
        _familyRepository = Substitute.For<IFamilyRepository>();
        _studentRepository = Substitute.For<IStudentRepository>();
        _connectionFactory = Substitute.For<IDbConnectionFactory>();
        _service = new FamilyService(
            _familyRepository,
            _studentRepository,
            _connectionFactory,
            NullLogger<FamilyService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_Persists_Family()
    {
        // Arrange
        var request = new CreateFamilyRequest
        {
            FamilyName = "Smith Family",
            PrimaryContactName = "John Smith",
            Phone = "555-1234",
            Email = "smith@email.com",
            Address = "123 Main St",
            Notes = "Test notes"
        };

        _familyRepository.CreateAsync(Arg.Any<Family>(), Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Smith Family", result.FamilyName);
        Assert.Equal("John Smith", result.PrimaryContactName);
        Assert.Equal("555-1234", result.Phone);
        Assert.Equal("smith@email.com", result.Email);
        Assert.Equal("123 Main St", result.Address);
        Assert.True(result.IsActive);
        await _familyRepository.Received(1).CreateAsync(Arg.Any<Family>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsFamily()
    {
        // Arrange
        var family = new Family
        {
            Id = 1,
            FamilyName = "Smith Family",
            PrimaryContactName = "John Smith",
            Phone = "555-1234",
            Email = "smith@email.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _familyRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(family);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Smith Family", result.FamilyName);
        Assert.Equal("John Smith", result.PrimaryContactName);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _familyRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Family?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithInactiveFamily_ReturnsNull()
    {
        // Arrange
        var family = new Family
        {
            Id = 1,
            FamilyName = "Inactive Family",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _familyRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(family);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsActiveFamilies_InNameOrder()
    {
        // Arrange
        var families = new List<Family>
        {
            new Family { Id = 1, FamilyName = "Beta Family", PrimaryContactName = "Contact B", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Family { Id = 2, FamilyName = "Alpha Family", PrimaryContactName = "Contact A", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        _familyRepository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(families);

        // Act
        var results = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Alpha Family", results[0].FamilyName);
        Assert.Equal("Beta Family", results[1].FamilyName);
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_UpdatesFamily()
    {
        // Arrange
        var family = new Family
        {
            Id = 1,
            FamilyName = "Original Name",
            PrimaryContactName = "Original Contact",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _familyRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(family);
        _familyRepository.UpdateAsync(Arg.Any<Family>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var request = new UpdateFamilyRequest
        {
            FamilyName = "Updated Name",
            PrimaryContactName = "Updated Contact",
            Phone = "555-9999",
            Email = "updated@email.com",
            Address = "456 New St",
            Notes = "Updated notes",
            IsActive = true
        };

        // Act
        var result = await _service.UpdateAsync(1, request);

        // Assert
        Assert.NotNull(result);
        await _familyRepository.Received(1).UpdateAsync(
            Arg.Is<Family>(f => f.FamilyName == "Updated Name"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _familyRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Family?)null);

        var request = new UpdateFamilyRequest
        {
            FamilyName = "Updated Name",
            PrimaryContactName = "Updated Contact"
        };

        // Act
        var result = await _service.UpdateAsync(999, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ArchiveAsync_WithValidId_SetsIsActiveFalse()
    {
        // Arrange
        var family = new Family
        {
            Id = 1,
            FamilyName = "To Archive",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _familyRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(family);
        _familyRepository.UpdateAsync(Arg.Any<Family>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var archived = await _service.ArchiveAsync(1);

        // Assert
        Assert.True(archived);
        await _familyRepository.Received(1).UpdateAsync(
            Arg.Is<Family>(f => f.IsActive == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        _familyRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Family?)null);

        // Act
        var archived = await _service.ArchiveAsync(999);

        // Assert
        Assert.False(archived);
    }

    [Fact]
    public async Task ArchiveAsync_WithInactiveFamily_ReturnsFalse()
    {
        // Arrange
        var family = new Family
        {
            Id = 1,
            FamilyName = "Already Inactive",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _familyRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(family);

        // Act
        var archived = await _service.ArchiveAsync(1);

        // Assert
        Assert.False(archived);
    }
}
