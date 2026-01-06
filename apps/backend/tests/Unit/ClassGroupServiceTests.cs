using Kcow.Application.ClassGroups;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Data;
using Kcow.Infrastructure.ClassGroups;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kcow.Unit.Tests;

/// <summary>
/// Unit tests for ClassGroupService business logic.
/// </summary>
public class ClassGroupServiceTests
{
    [Fact]
    public async Task CreateAsync_Persists_ClassGroup_WithXsdFields()
    {
        // Arrange
        using var context = CreateContext();
        var school = new School { Name = "Test School", IsActive = true };
        context.Schools.Add(school);
        await context.SaveChangesAsync();

        var service = new ClassGroupService(context, NullLogger<ClassGroupService>.Instance);

        var request = new CreateClassGroupRequest
        {
            Name = "CG001",
            SchoolId = school.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 30),
            Sequence = 1,
            // XSD fields
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
        var result = await service.CreateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CG001", result.Name);
        Assert.Equal(school.Id, result.SchoolId);
        Assert.Equal(DayOfWeek.Monday, result.DayOfWeek);
        Assert.Equal(new TimeOnly(9, 0), result.StartTime);
        Assert.Equal(new TimeOnly(10, 30), result.EndTime);
        // Verify XSD fields
        Assert.Equal("M01", result.DayTruck);
        Assert.Equal("Test class group", result.Description);
        Assert.True(result.Evaluate);
        Assert.False(result.ImportFlag);
        Assert.Equal("Test notes", result.Notes);
        Assert.Equal("Welcome message", result.GroupMessage);
        Assert.Equal("Yes", result.SendCertificates);
        Assert.Equal("Pay online", result.MoneyMessage);
        Assert.Equal("IXL", result.Ixl);
        Assert.Equal(1, await context.ClassGroups.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_WithInvalidSchoolId_Throws()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ClassGroupService(context, NullLogger<ClassGroupService>.Instance);

        var request = new CreateClassGroupRequest
        {
            Name = "CG001",
            SchoolId = 999, // Non-existent school
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsActiveClassGroups_WithSchoolAndTruck()
    {
        // Arrange
        using var context = CreateContext();
        var school = new School { Name = "Test School", IsActive = true };
        var truck = new Truck { Name = "Test Truck", RegistrationNumber = "TRUCK01", Status = "Active", IsActive = true };
        context.Schools.Add(school);
        context.Trucks.Add(truck);
        context.ClassGroups.AddRange(
            new ClassGroup
            {
                Name = "Active1",
                SchoolId = school.Id,
                TruckId = truck.Id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ClassGroup
            {
                Name = "Inactive",
                SchoolId = school.Id,
                DayOfWeek = DayOfWeek.Tuesday,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 0),
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            },
            new ClassGroup
            {
                Name = "Active2",
                SchoolId = school.Id,
                DayOfWeek = DayOfWeek.Wednesday,
                StartTime = new TimeOnly(11, 0),
                EndTime = new TimeOnly(12, 0),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        var service = new ClassGroupService(context, NullLogger<ClassGroupService>.Instance);

        // Act
        var results = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, cg => Assert.True(cg.IsActive));
        Assert.All(results, cg => Assert.NotNull(cg.School));
    }

    [Fact]
    public async Task GetAllAsync_WithSchoolFilter_ReturnsFilteredResults()
    {
        // Arrange
        using var context = CreateContext();
        var school1 = new School { Name = "School 1", IsActive = true };
        var school2 = new School { Name = "School 2", IsActive = true };
        context.Schools.AddRange(school1, school2);
        context.ClassGroups.AddRange(
            new ClassGroup
            {
                Name = "CG1",
                SchoolId = school1.Id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ClassGroup
            {
                Name = "CG2",
                SchoolId = school2.Id,
                DayOfWeek = DayOfWeek.Tuesday,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 0),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        var service = new ClassGroupService(context, NullLogger<ClassGroupService>.Instance);

        // Act
        var results = await service.GetAllAsync(schoolId: school1.Id);

        // Assert
        Assert.Single(results);
        Assert.Equal("CG1", results[0].Name);
        Assert.Equal(school1.Id, results[0].SchoolId);
    }

    [Fact]
    public async Task GetAllAsync_WithTruckFilter_ReturnsFilteredResults()
    {
        // Arrange
        using var context = CreateContext();
        var school = new School { Name = "Test School", IsActive = true };
        var truck1 = new Truck { Name = "Truck 1", RegistrationNumber = "TRUCK01", Status = "Active", IsActive = true };
        var truck2 = new Truck { Name = "Truck 2", RegistrationNumber = "TRUCK02", Status = "Active", IsActive = true };
        context.Schools.Add(school);
        context.Trucks.AddRange(truck1, truck2);
        context.ClassGroups.AddRange(
            new ClassGroup
            {
                Name = "CG1",
                SchoolId = school.Id,
                TruckId = truck1.Id,
                DayOfWeek = DayOfWeek.Monday,
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(10, 0),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ClassGroup
            {
                Name = "CG2",
                SchoolId = school.Id,
                TruckId = truck2.Id,
                DayOfWeek = DayOfWeek.Tuesday,
                StartTime = new TimeOnly(10, 0),
                EndTime = new TimeOnly(11, 0),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        var service = new ClassGroupService(context, NullLogger<ClassGroupService>.Instance);

        // Act
        var results = await service.GetAllAsync(truckId: truck1.Id);

        // Assert
        Assert.Single(results);
        Assert.Equal("CG1", results[0].Name);
        Assert.Equal(truck1.Id, results[0].TruckId);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsClassGroupWithDetails()
    {
        // Arrange
        using var context = CreateContext();
        var school = new School { Name = "Test School", IsActive = true };
        var truck = new Truck { Name = "Test Truck", RegistrationNumber = "TRUCK01", Status = "Active", IsActive = true };
        context.Schools.Add(school);
        context.Trucks.Add(truck);
        var classGroup = new ClassGroup
        {
            Name = "Test CG",
            SchoolId = school.Id,
            TruckId = truck.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.ClassGroups.Add(classGroup);
        await context.SaveChangesAsync();

        var service = new ClassGroupService(context, NullLogger<ClassGroupService>.Instance);

        // Act
        var result = await service.GetByIdAsync(classGroup.Id);

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
        using var context = CreateContext();
        var service = new ClassGroupService(context, NullLogger<ClassGroupService>.Instance);

        // Act
        var result = await service.GetByIdAsync(99999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithValidData_UpdatesClassGroup()
    {
        // Arrange
        using var context = CreateContext();
        var school = new School { Name = "Test School", IsActive = true };
        context.Schools.Add(school);
        var classGroup = new ClassGroup
        {
            Name = "Original",
            SchoolId = school.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.ClassGroups.Add(classGroup);
        await context.SaveChangesAsync();

        var service = new ClassGroupService(context, NullLogger<ClassGroupService>.Instance);

        var request = new UpdateClassGroupRequest
        {
            Name = "Updated",
            SchoolId = school.Id,
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 30),
            Sequence = 2,
            // XSD fields
            DayTruck = "T02",
            Description = "Updated description",
            Evaluate = false,
            ImportFlag = true,
            Notes = "Updated notes",
            GroupMessage = "Updated message",
            SendCertificates = "No",
            MoneyMessage = "Updated payment",
            Ixl = "IX2"
        };

        // Act
        var result = await service.UpdateAsync(classGroup.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
        Assert.Equal(DayOfWeek.Tuesday, result.DayOfWeek);
        Assert.Equal(new TimeOnly(10, 0), result.StartTime);
        Assert.Equal(new TimeOnly(11, 30), result.EndTime);
        Assert.Equal(2, result.Sequence);
        // Verify XSD field updates
        Assert.Equal("T02", result.DayTruck);
        Assert.Equal("Updated description", result.Description);
        Assert.False(result.Evaluate);
        Assert.True(result.ImportFlag);
        Assert.Equal("Updated notes", result.Notes);
        Assert.Equal("Updated message", result.GroupMessage);
        Assert.Equal("No", result.SendCertificates);
        Assert.Equal("Updated payment", result.MoneyMessage);
        Assert.Equal("IX2", result.Ixl);
        Assert.NotNull(result.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ClassGroupService(context, NullLogger<ClassGroupService>.Instance);

        var request = new UpdateClassGroupRequest
        {
            Name = "Test",
            SchoolId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        // Act
        var result = await service.UpdateAsync(99999, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ArchiveAsync_WithValidId_SetsIsActiveFalse()
    {
        // Arrange
        using var context = CreateContext();
        var school = new School { Name = "Test School", IsActive = true };
        context.Schools.Add(school);
        var classGroup = new ClassGroup
        {
            Name = "To Archive",
            SchoolId = school.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.ClassGroups.Add(classGroup);
        await context.SaveChangesAsync();

        var service = new ClassGroupService(context, NullLogger<ClassGroupService>.Instance);

        // Act
        var archived = await service.ArchiveAsync(classGroup.Id);

        // Assert
        Assert.True(archived);
        var reloaded = await context.ClassGroups.FindAsync(classGroup.Id);
        Assert.NotNull(reloaded);
        Assert.False(reloaded!.IsActive);
        Assert.NotNull(reloaded.UpdatedAt);
    }

    [Fact]
    public async Task ArchiveAsync_WithInvalidId_ReturnsFalse()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ClassGroupService(context, NullLogger<ClassGroupService>.Instance);

        // Act
        var result = await service.ArchiveAsync(99999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task XsdFieldValidation_AllXsdFieldsPersisted()
    {
        // Arrange
        using var context = CreateContext();
        var school = new School { Name = "Test School", IsActive = true };
        var truck = new Truck { Name = "Test Truck", RegistrationNumber = "TRUCK01", Status = "Active", IsActive = true };
        context.Schools.Add(school);
        context.Trucks.Add(truck);
        await context.SaveChangesAsync();

        var service = new ClassGroupService(context, NullLogger<ClassGroupService>.Instance);

        // Name within 10 character limit
        var request = new CreateClassGroupRequest
        {
            Name = "CG01", // Within XSD max of 10 characters
            SchoolId = school.Id,
            TruckId = truck.Id,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            // All 8 XSD fields
            DayTruck = "M01",
            Description = "A test class group",
            Evaluate = true,
            ImportFlag = false,
            Notes = "Test notes",
            GroupMessage = "Welcome",
            SendCertificates = "Yes",
            MoneyMessage = "Pay",
            Ixl = "IX1"
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert - All XSD fields persisted correctly
        Assert.NotNull(result);
        Assert.Equal("CG01", result.Name);
        Assert.Equal("M01", result.DayTruck);
        Assert.Equal("A test class group", result.Description);
        Assert.True(result.Evaluate);
        Assert.False(result.ImportFlag);
        Assert.Equal("Test notes", result.Notes);
        Assert.Equal("Welcome", result.GroupMessage);
        Assert.Equal("Yes", result.SendCertificates);
        Assert.Equal("Pay", result.MoneyMessage);
        Assert.Equal("IX1", result.Ixl);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory;")
            .Options;

        var context = new AppDbContext(options);
        context.Database.OpenConnection();
        context.Database.EnsureCreated();

        // Enable foreign key enforcement for SQLite
        context.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");

        return context;
    }
}
