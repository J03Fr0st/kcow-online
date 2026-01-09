using Kcow.Application.Students;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Data;
using Kcow.Infrastructure.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Kcow.Unit.Tests;

public class StudentSearchTests
{
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

    [Fact]
    public async Task SearchAsync_WithValidQuery_ReturnsMatchingResults()
    {
        // Arrange
        using var context = CreateContext();
        var service = new StudentService(context, NullLogger<StudentService>.Instance);

        context.Students.AddRange(
            new Student { Reference = "STU001", FirstName = "John", LastName = "Smith", Grade = "Grade 5", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Student { Reference = "STU002", FirstName = "Jane", LastName = "Smith", Grade = "Grade 3", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Student { Reference = "STU003", FirstName = "Bob", LastName = "Johnson", Grade = "Grade 5", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var results = await service.SearchAsync("John");

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.FullName == "John Smith");
        Assert.Contains(results, r => r.FullName == "Bob Johnson");
    }

    [Fact]
    public async Task SearchAsync_CaseInsensitive_ReturnsAllMatches()
    {
        // Arrange
        using var context = CreateContext();
        var service = new StudentService(context, NullLogger<StudentService>.Instance);

        context.Students.AddRange(
            new Student { Reference = "STU001", FirstName = "John", LastName = "Smith", Grade = "Grade 5", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Student { Reference = "STU002", FirstName = "Jane", LastName = "Smith", Grade = "Grade 3", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var results = await service.SearchAsync("smith");

        // Assert
        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task SearchAsync_OnlyActiveStudents_ReturnsActiveOnly()
    {
        // Arrange
        using var context = CreateContext();
        var service = new StudentService(context, NullLogger<StudentService>.Instance);

        context.Students.Add(
            new Student { Reference = "STU001", FirstName = "Alice", LastName = "Williams", Grade = "Grade 1", IsActive = false, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var results = await service.SearchAsync("Williams");

        // Assert
        Assert.Empty(results); // Alice is inactive
    }

    [Fact]
    public async Task SearchAsync_WithLimit_RespectsLimit()
    {
        // Arrange
        using var context = CreateContext();
        var service = new StudentService(context, NullLogger<StudentService>.Instance);

        context.Students.AddRange(
            new Student { Reference = "STU001", FirstName = "John", LastName = "Smith", Grade = "Grade 5", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Student { Reference = "STU002", FirstName = "Jane", LastName = "Smith", Grade = "Grade 3", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var results = await service.SearchAsync("Smith", limit: 1);

        // Assert
        Assert.Single(results);
    }

    [Fact]
    public async Task SearchAsync_NoMatches_ReturnsEmptyList()
    {
        // Arrange
        using var context = CreateContext();
        var service = new StudentService(context, NullLogger<StudentService>.Instance);

        // Act
        var results = await service.SearchAsync("Nonexistent");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_WithSchoolAndClassGroup_IncludesInResult()
    {
        // Arrange
        using var context = CreateContext();
        var service = new StudentService(context, NullLogger<StudentService>.Instance);

        var school = new School
        {
            Id = 1,
            Name = "Test School",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var classGroup = new ClassGroup
        {
            Id = 1,
            Name = "Class 5A",
            SchoolId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Schools.Add(school);
        context.ClassGroups.Add(classGroup);

        var student = new Student
        {
            Reference = "STU001",
            FirstName = "John",
            LastName = "Smith",
            Grade = "Grade 5",
            SchoolId = 1,
            ClassGroupId = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Students.Add(student);
        await context.SaveChangesAsync();

        // Act
        var results = await service.SearchAsync("John");

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("Test School", result.SchoolName);
        Assert.Equal("Class 5A", result.ClassGroupName);
    }

    [Fact]
    public async Task SearchAsync_WithoutSchoolOrClassGroup_DisplaysDefaults()
    {
        // Arrange
        using var context = CreateContext();
        var service = new StudentService(context, NullLogger<StudentService>.Instance);

        context.Students.Add(
            new Student { Reference = "STU001", FirstName = "John", LastName = "Smith", Grade = "Grade 5", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var results = await service.SearchAsync("John");

        // Assert
        var result = Assert.Single(results);
        Assert.Equal("No School", result.SchoolName);
        Assert.Equal("No Class", result.ClassGroupName);
    }
}
