using Kcow.Application.Interfaces;
using Kcow.Application.Students;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Students;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Kcow.Unit.Tests;

public class StudentSearchTests
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IClassGroupRepository _classGroupRepository;
    private readonly StudentService _service;

    public StudentSearchTests()
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
    public async Task SearchAsync_WithValidQuery_ReturnsMatchingResults()
    {
        // Arrange
        var students = new List<Student>
        {
            new Student { Id = 1, Reference = "STU001", FirstName = "John", LastName = "Smith", Grade = "Grade 5", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Student { Id = 2, Reference = "STU002", FirstName = "Jane", LastName = "Smith", Grade = "Grade 3", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Student { Id = 3, Reference = "STU003", FirstName = "Bob", LastName = "Johnson", Grade = "Grade 5", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        _studentRepository.SearchByNameAsync("John", Arg.Any<CancellationToken>())
            .Returns(students.Where(s => s.FirstName.Contains("John") || s.LastName.Contains("John")));

        // Act
        var results = await _service.SearchAsync("John");

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.FullName == "John Smith");
        Assert.Contains(results, r => r.FullName == "Bob Johnson");
    }

    [Fact]
    public async Task SearchAsync_WithLimit_RespectsLimit()
    {
        // Arrange
        var students = new List<Student>
        {
            new Student { Id = 1, Reference = "STU001", FirstName = "John", LastName = "Smith", Grade = "Grade 5", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Student { Id = 2, Reference = "STU002", FirstName = "Jane", LastName = "Smith", Grade = "Grade 3", IsActive = true, CreatedAt = DateTime.UtcNow }
        };

        _studentRepository.SearchByNameAsync("Smith", Arg.Any<CancellationToken>())
            .Returns(students);

        // Act
        var results = await _service.SearchAsync("Smith", limit: 1);

        // Assert
        Assert.Single(results);
    }

    [Fact]
    public async Task SearchAsync_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        _studentRepository.SearchByNameAsync("Nonexistent", Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<Student>());

        // Act
        var results = await _service.SearchAsync("Nonexistent");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_WithSchoolAndClassGroup_IncludesInResult()
    {
        // Arrange
        var student = new Student
        {
            Id = 1,
            Reference = "STU001",
            FirstName = "John",
            LastName = "Smith",
            Grade = "Grade 5",
            SchoolId = 1,
            ClassGroupId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _studentRepository.SearchByNameAsync("John", Arg.Any<CancellationToken>())
            .Returns(new[] { student });
        _schoolRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new School { Id = 1, Name = "Test School" });
        _classGroupRepository.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(new ClassGroup { Id = 1, Name = "Class 5A" });

        // Act
        var results = await _service.SearchAsync("John");

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("Test School", result.SchoolName);
        Assert.Equal("Class 5A", result.ClassGroupName);
    }

    [Fact]
    public async Task SearchAsync_WithoutSchoolOrClassGroup_DisplaysDefaults()
    {
        // Arrange
        var student = new Student
        {
            Id = 1,
            Reference = "STU001",
            FirstName = "John",
            LastName = "Smith",
            Grade = "Grade 5",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _studentRepository.SearchByNameAsync("John", Arg.Any<CancellationToken>())
            .Returns(new[] { student });

        // Act
        var results = await _service.SearchAsync("John");

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("No School", result.SchoolName);
        Assert.Equal("No Class", result.ClassGroupName);
    }

    [Fact]
    public async Task SearchAsync_EnforcesMaxLimit()
    {
        // Arrange
        var students = Enumerable.Range(1, 100)
            .Select(i => new Student
            {
                Id = i,
                Reference = $"STU{i:D3}",
                FirstName = "John",
                LastName = $"Doe{i}",
                IsActive = true
            })
            .ToList();

        _studentRepository.SearchByNameAsync("John", Arg.Any<CancellationToken>())
            .Returns(students);

        // Act - request more than max limit (50)
        var results = await _service.SearchAsync("John", limit: 100);

        // Assert - should be capped at 50
        Assert.Equal(50, results.Count);
    }
}
