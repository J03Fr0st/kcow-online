using Kcow.Application.ClassGroups;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.ClassGroups;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Kcow.Unit.Tests;

/// <summary>
/// Unit tests for ClassGroupService business logic.
/// </summary>
public class ClassGroupServiceTests
{
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ITruckRepository _truckRepository;
    private readonly ClassGroupService _service;

    public ClassGroupServiceTests()
    {
        _classGroupRepository = Substitute.For<IClassGroupRepository>();
        _schoolRepository = Substitute.For<ISchoolRepository>();
        _truckRepository = Substitute.For<ITruckRepository>();
        _service = new ClassGroupService(
            _classGroupRepository,
            _schoolRepository,
            _truckRepository,
            NullLogger<ClassGroupService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_Persists_ClassGroup_WithXsdFields()
    {
        // Arrange
        _schoolRepository.ExistsAsync(1, Arg.Any<CancellationToken>())
            .Returns(true);
        _classGroupRepository.CreateAsync(Arg.Any<ClassGroup>(), Arg.Any<CancellationToken>())
            .Returns(1);
        _classGroupRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new ClassGroup
            {
                Id = 1,
                Name = "CG001",
                SchoolId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 30),
                Sequence = 1,
                DayTruck = "M01",
                Description = "Test class group",
                Evaluate = true,
                ImportFlag = false,
                Notes = "Test notes",
                GroupMessage = "Welcome message",
                SendCertificates = "Yes",
                MoneyMessage = "Pay online",
                Ixl = "IXL",
                IsActive = true
            });
        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new School { Id = 1, Name = "Test School" });

        var request = new CreateClassGroupRequest
        {
            Name = "CG001",
            SchoolId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 30),
            Sequence = 1,
            DayTruck = "M01",
            Description = "Test class group",
            Evaluate = true,
            ImportFlag = false,
            Notes = "Test notes",
            GroupMessage = "Welcome message",
            SendCertificates = "Yes",
            MoneyMessage = "Pay online",
            Ixl = "IXL"
        };

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CG001", result.Name);
        Assert.Equal(1, result.SchoolId);
        Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
        Assert.Equal(new TimeOnly(9, 0), result.StartTime);
        Assert.Equal(new TimeOnly(10, 30), result.EndTime);
        Assert.Equal("M01", result.DayTruck);
        Assert.Equal("Test class group", result.Description);
        Assert.True(result.Evaluate);
        Assert.False(result.ImportFlag);
        Assert.Equal("Test notes", result.Notes);
        await _classGroupRepository.Received(1).CreateAsync(Arg.Any<ClassGroup>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithInvalidSchoolId_Throws()
    {
        // Arrange
        _schoolRepository.ExistsAsync(999, Arg.Any<CancellationToken>())
            .Returns(false);

        var request = new CreateClassGroupRequest
        {
            Name = "CG001",
            SchoolId = 999,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_WithInvalidTimeRange_Throws()
    {
        // Arrange
        _schoolRepository.ExistsAsync(1, Arg.Any<CancellationToken>())
            .Returns(true);

        var request = new CreateClassGroupRequest
        {
            Name = "CG001",
            SchoolId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(9, 0) // End before start
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsActiveClassGroups()
    {
        // Arrange
        var classGroups = new List<ClassGroup>
        {
            new ClassGroup
            {
                Id = 1,
                Name = "Active1",
                SchoolId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ClassGroup
            {
                Id = 2,
                Name = "Active2",
                SchoolId = 1,
                DayOfWeek = DayOfWeek.Wednesday,
                StartTime = new TimeOnly(11, 0),
                EndTime = new TimeOnly(12, 0),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _classGroupRepository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(classGroups);
        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new School { Id = 1, Name = "Test School" });

        // Act
        var results = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, cg => Assert.True(cg.IsActive));
    }

    [Fact]
    public async Task GetAllAsync_WithSchoolFilter_ReturnsFilteredResults()
    {
        // Arrange
        var classGroups = new List<ClassGroup>
        {
            new ClassGroup { Id = 1, Name = "CG1", SchoolId = 1, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), IsActive = true },
            new ClassGroup { Id = 2, Name = "CG2", SchoolId = 2, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), IsActive = true }
        };

        _classGroupRepository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(classGroups);
        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new School { Id = 1, Name = "School 1" });

        // Act
        var results = await _service.GetAllAsync(schoolId: 1);

        // Assert
        Assert.Single(results);
        Assert.Equal("CG1", results[0].Name);
        Assert.Equal(1, results[0].SchoolId);
    }

    [Fact]
    public async Task GetAllAsync_WithTruckFilter_ReturnsFilteredResults()
    {
        // Arrange
        var classGroups = new List<ClassGroup>
        {
            new ClassGroup { Id = 1, Name = "CG1", SchoolId = 1, TruckId = 1, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(10, 0), IsActive = true },
            new ClassGroup { Id = 2, Name = "CG2", SchoolId = 1, TruckId = 2, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), IsActive = true }
        };

        _classGroupRepository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(classGroups);
        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new School { Id = 1, Name = "Test School" });
        _truckRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new Truck { Id = 1, Name = "Truck 1" });

        // Act
        var results = await _service.GetAllAsync(truckId: 1);

        // Assert
        Assert.Single(results);
        Assert.Equal("CG1", results[0].Name);
        Assert.Equal(1, results[0].TruckId);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsClassGroupWithDetails()
    {
        // Arrange
        var classGroup = new ClassGroup
        {
            Id = 1,
            Name = "Test CG",
            SchoolId = 1,
            TruckId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _classGroupRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(classGroup);
        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new School { Id = 1, Name = "Test School" });
        _truckRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new Truck { Id = 1, Name = "Test Truck" });

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test CG", result.Name);
        Assert.NotNull(result.School);
        Assert.Equal("Test School", result.School.Name);
        Assert.NotNull(result.Truck);
        Assert.Equal("Test Truck", result.Truck.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _classGroupRepository.GetByIdAsync(99999, Arg.Any<CancellationToken>())
            .Returns((ClassGroup?)null);

        // Act
        var result = await _service.GetByIdAsync(99999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ArchiveAsync_WithValidId_SetsIsActiveFalse()
    {
        // Arrange
        var classGroup = new ClassGroup
        {
            Id = 1,
            Name = "To Archive",
            SchoolId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _classGroupRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(classGroup);
        _classGroupRepository.UpdateAsync(Arg.Any<ClassGroup>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var archived = await _service.ArchiveAsync(1);

        // Assert
        Assert.True(archived);
        await _classGroupRepository.Received(1).UpdateAsync(
            Arg.Is<ClassGroup>(cg => cg.IsActive == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        _classGroupRepository.GetByIdAsync(99999, Arg.Any<CancellationToken>())
            .Returns((ClassGroup?)null);

        // Act
        var result = await _service.ArchiveAsync(99999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckConflictsAsync_WithOverlappingSchedule_ReturnsConflicts()
    {
        // Arrange
        var classGroups = new List<ClassGroup>
        {
            new ClassGroup
            {
                Id = 1,
                Name = "Existing CG",
                SchoolId = 1,
                TruckId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true
            }
        };

        _classGroupRepository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(classGroups);
        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new School { Id = 1, Name = "Test School" });

        var request = new CheckConflictsRequest
        {
            TruckId = 1,
            DayOfWeek = (int)DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 30),
            EndTime = new TimeOnly(10, 30)
        };

        // Act
        var result = await _service.CheckConflictsAsync(request);

        // Assert
        Assert.True(result.HasConflicts);
        Assert.Single(result.Conflicts);
        Assert.Equal("Existing CG", result.Conflicts[0].Name);
    }

    [Fact]
    public async Task CheckConflictsAsync_WithNoOverlap_ReturnsNoConflicts()
    {
        // Arrange
        var classGroups = new List<ClassGroup>
        {
            new ClassGroup
            {
                Id = 1,
                Name = "Existing CG",
                SchoolId = 1,
                TruckId = 1,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true
            }
        };

        _classGroupRepository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(classGroups);

        var request = new CheckConflictsRequest
        {
            TruckId = 1,
            DayOfWeek = (int)DayOfWeek.Monday,
            StartTime = new TimeOnly(11, 0),
            EndTime = new TimeOnly(12, 0)
        };

        // Act
        var result = await _service.CheckConflictsAsync(request);

        // Assert
        Assert.False(result.HasConflicts);
        Assert.Empty(result.Conflicts);
    }
}
