using Kcow.Application.Schools;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Data;
using Kcow.Infrastructure.Schools;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kcow.Unit.Tests;

public class SchoolServiceTests
{
    [Fact]
    public async Task CreateAsync_Persists_School()
    {
        using var context = CreateContext();
        var service = new SchoolService(context, NullLogger<SchoolService>.Instance);

        var request = new CreateSchoolRequest
        {
            Name = "Test School",
            Address = "123 Test St",
            ContactName = "John Doe",
            ContactPhone = "555-1234",
            ContactEmail = "test@school.edu",
            BillingSettings = new BillingSettingsDto
            {
                DefaultSessionRate = 50.00m,
                BillingCycle = "Monthly",
                BillingNotes = "Test billing notes"
            },
            Notes = "Test notes"
        };

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Test School", result.Name);
        Assert.Equal("123 Test St", result.Address);
        Assert.Equal("John Doe", result.ContactName);
        Assert.Equal("555-1234", result.ContactPhone);
        Assert.Equal("test@school.edu", result.ContactEmail);
        Assert.NotNull(result.BillingSettings);
        Assert.Equal(50.00m, result.BillingSettings.DefaultSessionRate);
        Assert.Equal("Monthly", result.BillingSettings.BillingCycle);
        Assert.Equal("Test billing notes", result.BillingSettings.BillingNotes);
        Assert.Equal("Test notes", result.Notes);
        Assert.True(result.IsActive);
        Assert.Equal(1, await context.Schools.CountAsync());
    }

    [Fact]
    public async Task CreateAsync_WithoutBillingSettings_Persists_School()
    {
        using var context = CreateContext();
        var service = new SchoolService(context, NullLogger<SchoolService>.Instance);

        var request = new CreateSchoolRequest
        {
            Name = "Test School",
            Address = "123 Test St",
            ContactName = "John Doe",
            ContactPhone = "555-1234",
            ContactEmail = "test@school.edu",
            BillingSettings = null,
            Notes = null
        };

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Test School", result.Name);
        Assert.Null(result.BillingSettings);
        Assert.Null(result.Notes);
        Assert.True(result.IsActive);
        Assert.Equal(1, await context.Schools.CountAsync());
    }

    [Fact]
    public async Task GetAllAsync_OnlyReturnsActiveSchools_InNameOrder()
    {
        using var context = CreateContext();
        context.Schools.AddRange(
            new School
            {
                Name = "Beta School",
                Address = "Address B",
                ContactName = "Contact B",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new School
            {
                Name = "Alpha School",
                Address = "Address A",
                ContactName = "Contact A",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new School
            {
                Name = "Inactive School",
                Address = "Address I",
                ContactName = "Contact I",
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        var service = new SchoolService(context, NullLogger<SchoolService>.Instance);
        var results = await service.GetAllAsync();

        Assert.Equal(2, results.Count);
        Assert.Collection(results,
            first => Assert.Equal("Alpha School", first.Name),
            second => Assert.Equal("Beta School", second.Name));
    }

    [Fact]
    public async Task GetAllAsync_WithBillingSettings_ReturnsCorrectData()
    {
        using var context = CreateContext();
        context.Schools.Add(
            new School
            {
                Name = "Test School",
                Address = "123 Test St",
                ContactName = "John Doe",
                ContactPhone = "555-1234",
                ContactEmail = "test@school.edu",
                BillingSettings = new Domain.Entities.BillingSettings
                {
                    DefaultSessionRate = 75.50m,
                    BillingCycle = "Weekly",
                    BillingNotes = "Weekly billing"
                },
                Notes = "Test notes",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        var service = new SchoolService(context, NullLogger<SchoolService>.Instance);
        var results = await service.GetAllAsync();

        Assert.Single(results);
        var school = results[0];
        Assert.Equal("Test School", school.Name);
        Assert.NotNull(school.BillingSettings);
        Assert.Equal(75.50m, school.BillingSettings.DefaultSessionRate);
        Assert.Equal("Weekly", school.BillingSettings.BillingCycle);
        Assert.Equal("Weekly billing", school.BillingSettings.BillingNotes);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsSchool()
    {
        using var context = CreateContext();
        var school = new School
        {
            Name = "Test School",
            Address = "123 Test St",
            ContactName = "John Doe",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Schools.Add(school);
        await context.SaveChangesAsync();

        var service = new SchoolService(context, NullLogger<SchoolService>.Instance);
        var result = await service.GetByIdAsync(school.Id);

        Assert.NotNull(result);
        Assert.Equal("Test School", result.Name);
        Assert.Equal("123 Test St", result.Address);
        Assert.Equal("John Doe", result.ContactName);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        using var context = CreateContext();
        var service = new SchoolService(context, NullLogger<SchoolService>.Instance);

        var result = await service.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_WithInactiveId_ReturnsNull()
    {
        using var context = CreateContext();
        var school = new School
        {
            Name = "Inactive School",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        context.Schools.Add(school);
        await context.SaveChangesAsync();

        var service = new SchoolService(context, NullLogger<SchoolService>.Instance);
        var result = await service.GetByIdAsync(school.Id);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_WithValidId_UpdatesSchool()
    {
        using var context = CreateContext();
        var school = new School
        {
            Name = "Original Name",
            Address = "Original Address",
            ContactName = "Original Contact",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Schools.Add(school);
        await context.SaveChangesAsync();

        var service = new SchoolService(context, NullLogger<SchoolService>.Instance);
        var request = new UpdateSchoolRequest
        {
            Name = "Updated Name",
            Address = "Updated Address",
            ContactName = "Updated Contact",
            ContactPhone = "555-9999",
            ContactEmail = "updated@school.edu",
            BillingSettings = new BillingSettingsDto
            {
                DefaultSessionRate = 100.00m,
                BillingCycle = "Bi-Weekly",
                BillingNotes = "Updated billing"
            },
            Notes = "Updated notes"
        };

        var result = await service.UpdateAsync(school.Id, request);

        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated Address", result.Address);
        Assert.Equal("Updated Contact", result.ContactName);
        Assert.Equal("555-9999", result.ContactPhone);
        Assert.Equal("updated@school.edu", result.ContactEmail);
        Assert.NotNull(result.BillingSettings);
        Assert.Equal(100.00m, result.BillingSettings.DefaultSessionRate);
        Assert.Equal("Bi-Weekly", result.BillingSettings.BillingCycle);
        Assert.Equal("Updated billing", result.BillingSettings.BillingNotes);
        Assert.Equal("Updated notes", result.Notes);
        Assert.NotNull(result.UpdatedAt);

        // Verify database was updated
        var reloaded = await context.Schools.FindAsync(school.Id);
        Assert.NotNull(reloaded);
        Assert.Equal("Updated Name", reloaded.Name);
    }

    [Fact]
    public async Task UpdateAsync_WithInvalidId_ReturnsNull()
    {
        using var context = CreateContext();
        var service = new SchoolService(context, NullLogger<SchoolService>.Instance);
        var request = new UpdateSchoolRequest
        {
            Name = "Updated Name"
        };

        var result = await service.UpdateAsync(999, request);

        Assert.Null(result);
    }

    [Fact]
    public async Task ArchiveAsync_WithValidId_SetsIsActiveFalse()
    {
        using var context = CreateContext();
        var school = new School
        {
            Name = "To Archive",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Schools.Add(school);
        await context.SaveChangesAsync();

        var service = new SchoolService(context, NullLogger<SchoolService>.Instance);
        var archived = await service.ArchiveAsync(school.Id);

        Assert.True(archived);
        var reloaded = await context.Schools.FindAsync(school.Id);
        Assert.NotNull(reloaded);
        Assert.False(reloaded!.IsActive);
        Assert.NotNull(reloaded.UpdatedAt);
    }

    [Fact]
    public async Task ArchiveAsync_WithInvalidId_ReturnsFalse()
    {
        using var context = CreateContext();
        var service = new SchoolService(context, NullLogger<SchoolService>.Instance);

        var archived = await service.ArchiveAsync(999);

        Assert.False(archived);
    }

    [Fact]
    public async Task ArchiveAsync_WithInactiveId_ReturnsFalse()
    {
        using var context = CreateContext();
        var school = new School
        {
            Name = "Already Inactive",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
        context.Schools.Add(school);
        await context.SaveChangesAsync();

        var service = new SchoolService(context, NullLogger<SchoolService>.Instance);
        var archived = await service.ArchiveAsync(school.Id);

        Assert.False(archived);
    }

    [Fact]
    public async Task BillingSettings_JsonSerialization_RoundTrip()
    {
        using var context = CreateContext();
        var service = new SchoolService(context, NullLogger<SchoolService>.Instance);

        // Create school with billing settings
        var request = new CreateSchoolRequest
        {
            Name = "JSON Test School",
            BillingSettings = new BillingSettingsDto
            {
                DefaultSessionRate = 123.45m,
                BillingCycle = "Termly",
                BillingNotes = "Complex JSON serialization test with special chars: @#$%"
            }
        };

        var created = await service.CreateAsync(request);
        var schoolId = created.Id;

        // Clear context to force fresh database read
        context.ChangeTracker.Clear();

        // Retrieve from database and verify JSON deserialization
        var retrieved = await service.GetByIdAsync(schoolId);

        Assert.NotNull(retrieved);
        Assert.NotNull(retrieved.BillingSettings);
        Assert.Equal(123.45m, retrieved.BillingSettings.DefaultSessionRate);
        Assert.Equal("Termly", retrieved.BillingSettings.BillingCycle);
        Assert.Equal("Complex JSON serialization test with special chars: @#$%", retrieved.BillingSettings.BillingNotes);

        // Verify direct database query also deserializes correctly
        var schoolEntity = await context.Schools.FindAsync(schoolId);
        Assert.NotNull(schoolEntity);
        Assert.NotNull(schoolEntity.BillingSettings);
        Assert.Equal(123.45m, schoolEntity.BillingSettings.DefaultSessionRate);
        Assert.Equal("Termly", schoolEntity.BillingSettings.BillingCycle);
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
