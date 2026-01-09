using Kcow.Application.Students;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Data;
using Kcow.Infrastructure.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kcow.Unit.Tests;

public class StudentServiceTests
{
    [Fact]
    public async Task CreateAsync_Persists_Student()
    {
        using var context = CreateContext();
        var service = new StudentService(context, NullLogger<StudentService>.Instance);

        var request = new CreateStudentRequest
        {
            Reference = "REF001",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("REF001", result.Reference);
        Assert.Equal("John", result.FirstName);
        Assert.Equal("Doe", result.LastName);
        Assert.True(result.IsActive);
        Assert.Equal(1, await context.Students.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateReference_Throws()
    {
        using var context = CreateContext();
        var service = new StudentService(context, NullLogger<StudentService>.Instance);

        var request = new CreateStudentRequest
        {
            Reference = "DUP001",
            FirstName = "Original",
            LastName = "Student"
        };

        await service.CreateAsync(request);

        var duplicateRequest = new CreateStudentRequest
        {
            Reference = "DUP001",
            FirstName = "Duplicate",
            LastName = "Student"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(duplicateRequest));
    }

    [Fact]
    public async Task GetPagedAsync_Returns_Paginated_Results()
    {
        using var context = CreateContext();
        for (int i = 1; i <= 25; i++)
        {
            context.Students.Add(new Student 
            { 
                Reference = $"REF{i:D3}", 
                FirstName = $"First{i}", 
                LastName = $"Last{i}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();

        var service = new StudentService(context, NullLogger<StudentService>.Instance);
        
        // Page 1
        var result1 = await service.GetPagedAsync(1, 10);
        Assert.Equal(10, result1.Items.Count());
        Assert.Equal(25, result1.TotalCount);
        Assert.Equal(3, result1.TotalPages);

        // Page 3
        var result3 = await service.GetPagedAsync(3, 10);
        Assert.Equal(5, result3.Items.Count());
    }

    [Fact]
    public async Task GetPagedAsync_Filters_By_SearchTerm()
    {
        using var context = CreateContext();
        context.Students.AddRange(
            new Student { Reference = "REF1", FirstName = "Alice", LastName = "Smith", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Student { Reference = "REF2", FirstName = "Bob", LastName = "Jones", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Student { Reference = "REF3", FirstName = "Charlie", LastName = "Smith", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new StudentService(context, NullLogger<StudentService>.Instance);
        
        var result = await service.GetPagedAsync(1, 10, search: "Smith");

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, s => Assert.Equal("Smith", s.LastName));
    }

    [Fact]
    public async Task GetPagedAsync_Filters_By_SchoolAndClassGroup()
    {
        using var context = CreateContext();
        var school1 = new School { Name = "School 1", IsActive = true };
        var school2 = new School { Name = "School 2", IsActive = true };
        context.Schools.AddRange(school1, school2);
        await context.SaveChangesAsync();

        var cg1 = new ClassGroup { Name = "CG1", SchoolId = school1.Id, IsActive = true };
        var cg2 = new ClassGroup { Name = "CG2", SchoolId = school2.Id, IsActive = true };
        context.ClassGroups.AddRange(cg1, cg2);
        await context.SaveChangesAsync();

        context.Students.AddRange(
            new Student { Reference = "S1", FirstName = "S1", SchoolId = school1.Id, ClassGroupId = cg1.Id, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Student { Reference = "S2", FirstName = "S2", SchoolId = school2.Id, ClassGroupId = cg2.Id, IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new StudentService(context, NullLogger<StudentService>.Instance);
        
        var result = await service.GetPagedAsync(1, 10, schoolId: school1.Id);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("S1", result.Items.First().FirstName);
    }

    [Fact]
    public async Task ArchiveAsync_Sets_IsActive_False()
    {
        using var context = CreateContext();
        var student = new Student { Reference = "ARC001", FirstName = "Archive", LastName = "Me", IsActive = true, CreatedAt = DateTime.UtcNow };
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var service = new StudentService(context, NullLogger<StudentService>.Instance);
        var archived = await service.ArchiveAsync(student.Id);

        Assert.True(archived);
        var reloaded = await context.Students.FindAsync(student.Id);
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
