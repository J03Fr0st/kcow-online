using Kcow.Application.Interfaces;
using Kcow.Application.Schools;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Schools;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Kcow.Unit.Tests;

public class SchoolServiceTests
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly SchoolService _service;

    public SchoolServiceTests()
    {
        _schoolRepository = Substitute.For<ISchoolRepository>();
        _service = new SchoolService(_schoolRepository, NullLogger<SchoolService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_Persists_School()
    {
        // Arrange
        var request = new CreateSchoolRequest
        {
            Name = "Test School",
            Address = "123 Test St",
            ContactPerson = "John Doe",
            Phone = "555-1234",
            Email = "test@school.edu",
            SchedulingNotes = "Test notes",
            PrintInvoice = false,
            ImportFlag = false
        };

        _schoolRepository.CreateAsync(Arg.Any<School>(), Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test School", result.Name);
        Assert.Equal("123 Test St", result.Address);
        Assert.Equal("John Doe", result.ContactPerson);
        Assert.Equal("555-1234", result.Phone);
        Assert.Equal("test@school.edu", result.Email);
        Assert.Equal("Test notes", result.SchedulingNotes);
        Assert.True(result.IsActive);
        await _schoolRepository.Received(1).CreateAsync(Arg.Any<School>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithMinimalData_Persists_School()
    {
        // Arrange
        var request = new CreateSchoolRequest
        {
            Name = "Test School",
            Address = "123 Test St",
            ContactPerson = "John Doe",
            Phone = "555-1234",
            Email = "test@school.edu",
            SchedulingNotes = null
        };

        _schoolRepository.CreateAsync(Arg.Any<School>(), Arg.Any<CancellationToken>())
            .Returns(1);

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test School", result.Name);
        Assert.Null(result.SchedulingNotes);
        Assert.True(result.IsActive);
    }

    [Fact]
    public async Task GetAllAsync_OnlyReturnsActiveSchools_InNameOrder()
    {
        // Arrange
        var schools = new List<School>
        {
            new School { Id = 1, Name = "Beta School", Address = "Address B", ContactPerson = "Contact B", IsActive = true, CreatedAt = DateTime.UtcNow },
            new School { Id = 2, Name = "Alpha School", Address = "Address A", ContactPerson = "Contact A", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        _schoolRepository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(schools);

        // Act
        var results = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("Alpha School", results[0].Name);
        Assert.Equal("Beta School", results[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSchool()
    {
        // Arrange
        var school = new School
        {
            Id = 1,
            Name = "Test School",
            Address = "123 Test St",
            ContactPerson = "John Doe",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(school);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test School", result.Name);
        Assert.Equal("123 Test St", result.Address);
        Assert.Equal("John Doe", result.ContactPerson);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _schoolRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((School?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithInactiveId_ReturnsNull()
    {
        // Arrange
        var school = new School
        {
            Id = 1,
            Name = "Inactive School",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(school);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_UpdatesSchool()
    {
        // Arrange
        var school = new School
        {
            Id = 1,
            Name = "Original Name",
            Address = "Original Address",
            ContactPerson = "Original Contact",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(school);
        _schoolRepository.UpdateAsync(Arg.Any<School>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var request = new UpdateSchoolRequest
        {
            Name = "Updated Name",
            Address = "Updated Address",
            ContactPerson = "Updated Contact",
            Phone = "555-9999",
            Email = "updated@school.edu",
            SchedulingNotes = "Updated notes"
        };

        // Act
        var result = await _service.UpdateAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated Address", result.Address);
        Assert.Equal("Updated Contact", result.ContactPerson);
        Assert.Equal("555-9999", result.Phone);
        Assert.Equal("updated@school.edu", result.Email);
        Assert.Equal("Updated notes", result.SchedulingNotes);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _schoolRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((School?)null);

        var request = new UpdateSchoolRequest
        {
            Name = "Updated Name"
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
        var school = new School
        {
            Id = 1,
            Name = "To Archive",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(school);
        _schoolRepository.UpdateAsync(Arg.Any<School>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var archived = await _service.ArchiveAsync(1);

        // Assert
        Assert.True(archived);
        await _schoolRepository.Received(1).UpdateAsync(
            Arg.Is<School>(s => s.IsActive == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        _schoolRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((School?)null);

        // Act
        var archived = await _service.ArchiveAsync(999);

        // Assert
        Assert.False(archived);
    }

    [Fact]
    public async Task ArchiveAsync_WithInactiveId_ReturnsFalse()
    {
        // Arrange
        var school = new School
        {
            Id = 1,
            Name = "Already Inactive",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(school);

        // Act
        var archived = await _service.ArchiveAsync(1);

        // Assert
        Assert.False(archived);
    }
}
