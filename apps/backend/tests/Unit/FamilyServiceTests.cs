using Dapper;
using Kcow.Application.Families;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using Kcow.Infrastructure.Families;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Kcow.Unit.Tests;

public class FamilyServiceTests : IDisposable
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly FamilyService _service;
    private readonly SqliteConnection _keepAlive;

    public FamilyServiceTests()
    {
        _familyRepository = Substitute.For<IFamilyRepository>();
        _studentRepository = Substitute.For<IStudentRepository>();
        _connectionFactory = Substitute.For<IDbConnectionFactory>();
        var connectionString = $"Data Source=family-unit-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        _keepAlive = new SqliteConnection(connectionString);
        _keepAlive.Open();
        _keepAlive.Execute("""
            CREATE TABLE students (id INTEGER PRIMARY KEY, first_name TEXT, last_name TEXT,
                reference TEXT, is_active INTEGER);
            CREATE TABLE student_families (student_id INTEGER, family_id INTEGER,
                relationship_type TEXT);
            CREATE TABLE families (id INTEGER PRIMARY KEY, family_name TEXT,
                primary_contact_name TEXT, phone TEXT, email TEXT, address TEXT,
                notes TEXT, is_active INTEGER, created_at TEXT, updated_at TEXT);
            """);
        _connectionFactory.Create().Returns(_ => new SqliteConnection(connectionString));
        _service = new FamilyService(
            _familyRepository,
            _studentRepository,
            _connectionFactory,
            NullLogger<FamilyService>.Instance);
    }

    public void Dispose() => _keepAlive.Dispose();

    [Fact]
    public async Task GetByStudentIdAsync_MapsOnlyLinkedFamilies()
    {
        _keepAlive.Execute("""
            INSERT INTO families (id, family_name, primary_contact_name, phone, email,
                address, notes, is_active, created_at, updated_at)
            VALUES (1, 'Smith', 'John Smith', '555-1234', 'smith@example.com',
                'Main Street', 'Primary', 1, '2024-01-01', '2024-02-01'),
                   (2, 'Jones', 'Jane Jones', NULL, NULL, NULL, NULL, 1, '2024-01-02', NULL),
                   (3, 'Other', 'Other Contact', NULL, NULL, NULL, NULL, 1, '2024-01-03', NULL);
            INSERT INTO student_families (student_id, family_id, relationship_type)
            VALUES (10, 1, 'Parent'), (10, 2, 'Guardian'), (11, 3, 'Parent');
            """);

        var families = await _service.GetByStudentIdAsync(10);

        Assert.Equal(2, families.Count);
        var smith = Assert.Single(families, family => family.Id == 1);
        Assert.Equal("Smith", smith.FamilyName);
        Assert.Equal("John Smith", smith.PrimaryContactName);
        Assert.Equal("555-1234", smith.Phone);
        Assert.Equal("smith@example.com", smith.Email);
        Assert.Equal("Main Street", smith.Address);
        Assert.Equal("Primary", smith.Notes);
        Assert.True(smith.IsActive);
        Assert.Equal(new DateTime(2024, 1, 1), smith.CreatedAt);
        Assert.Equal(new DateTime(2024, 2, 1), smith.UpdatedAt);
        Assert.Empty(smith.Students);
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
