using Kcow.Application.Interfaces;
using Kcow.Application.Students;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Students;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Kcow.Unit.Tests;

public class StudentServiceTests
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _studentRepository = Substitute.For<IStudentRepository>();
        _schoolRepository = Substitute.For<ISchoolRepository>();
        _classGroupRepository = Substitute.For<IClassGroupRepository>();
        _service = new StudentService(
            _studentRepository,
            _schoolRepository,
            _classGroupRepository,
            NullLogger<StudentService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_Persists_Student()
    {
        // Arrange
        var request = new CreateStudentRequest
        {
            Reference = "REF001",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };

        _studentRepository.ExistsByReferenceAsync("REF001", Arg.Any<CancellationToken>())
            .Returns(false);
        _studentRepository.CreateAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>())
            .Returns(1);
        _studentRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new Student { Id = 1, Reference = "REF001", FirstName = "John", LastName = "Doe", IsActive = true });

        // Act
        var result = await _service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("REF001", result.Reference);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.True(result.IsActive);
        await _studentRepository.Received(1).CreateAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateReference_Throws()
    {
        // Arrange
        var request = new CreateStudentRequest
        {
            Reference = "DUP001",
            FirstName = "Original",
            LastName = "Student"
        };

        _studentRepository.ExistsByReferenceAsync("DUP001", Arg.Any<CancellationToken>())
            .Returns(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
    }

    [Fact]
    public async Task GetPagedAsync_Returns_Paginated_Results()
    {
        // Arrange
        var students = Enumerable.Range(1, 25)
            .Select(i => new Student
            {
                Id = i,
                Reference = $"REF{i:D3}",
                FirstName = $"First{i}",
                LastName = $"Last{i}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            })
            .ToList();

        _studentRepository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(students);

        // Act - Page 1
        var result1 = await _service.GetPagedAsync(1, 10);

        // Assert
        Assert.Equal(10, result1.Items.Count());
        Assert.Equal(25, result1.TotalCount);
        Assert.Equal(3, result1.TotalPages);

        // Act - Page 3
        var result3 = await _service.GetPagedAsync(3, 10);
        Assert.Equal(5, result3.Items.Count());
    }

    [Fact]
    public async Task GetPagedAsync_Filters_By_SearchTerm()
    {
        // Arrange
        var students = new List<Student>
        {
            new Student { Id = 1, Reference = "REF1", FirstName = "Alice", LastName = "Smith", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Student { Id = 2, Reference = "REF2", FirstName = "Bob", LastName = "Jones", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Student { Id = 3, Reference = "REF3", FirstName = "Charlie", LastName = "Smith", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        _studentRepository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(students);

        // Act
        var result = await _service.GetPagedAsync(1, 10, search: "Smith");

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, s => Assert.Equal("Smith", s.LastName));
    }

    [Fact]
    public async Task GetPagedAsync_Filters_By_SchoolId()
    {
        // Arrange
        var students = new List<Student>
        {
            new Student { Id = 1, Reference = "S1", FirstName = "S1", SchoolId = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Student { Id = 2, Reference = "S2", FirstName = "S2", SchoolId = 2, IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        _studentRepository.GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(students);
        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new School { Id = 1, Name = "School 1" });

        // Act
        var result = await _service.GetPagedAsync(1, 10, schoolId: 1);

        // Assert
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("S1", result.Items.First().FirstName);
    }

    [Fact]
    public async Task ArchiveAsync_Sets_IsActive_False()
    {
        // Arrange
        var student = new Student { Id = 1, Reference = "ARC001", FirstName = "Archive", LastName = "Me", IsActive = true, CreatedAt = DateTime.UtcNow };
        _studentRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(student);
        _studentRepository.UpdateAsync(Arg.Any<Student>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var archived = await _service.ArchiveAsync(1);

        // Assert
        Assert.True(archived);
        await _studentRepository.Received(1).UpdateAsync(
            Arg.Is<Student>(s => s.IsActive == false),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        _studentRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Student?)null);

        // Act
        var result = await _service.ArchiveAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsStudent()
    {
        // Arrange
        var student = new Student
        {
            Id = 1,
            Reference = "REF001",
            FirstName = "John",
            LastName = "Doe",
            SchoolId = 1,
            IsActive = true
        };

        _studentRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(student);
        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new School { Id = 1, Name = "Test School" });

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _studentRepository.GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((Student?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }
}
