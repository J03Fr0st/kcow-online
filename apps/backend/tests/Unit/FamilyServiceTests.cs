using Kcow.Application.Families;
using Kcow.Domain.Entities;
using Kcow.Domain.Enums;
using Kcow.Infrastructure.Data;
using Kcow.Infrastructure.Families;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Kcow.Unit.Tests;

public class FamilyServiceTests
{
    [Fact]
    public async Task CreateAsync_Persists_Family()
    {
        using var context = CreateContext();
        var service = new FamilyService(context, NullLogger<FamilyService>.Instance);

        var request = new CreateFamilyRequest
        {
            FamilyName = "Smith",
            PrimaryContactName = "John Smith",
            Phone = "1234567890",
            Email = "john@smith.com"
        };

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Smith", result.FamilyName);
        Assert.True(result.IsActive);
        Assert.Equal(1, await context.Families.CountAsync());
    }

    [Fact]
    public async Task LinkToStudentAsync_Creates_Link()
    {
        using var context = CreateContext();
        var student = new Student { Reference = "S1", FirstName = "Child", IsActive = true };
        var family = new Family { FamilyName = "Family", PrimaryContactName = "Parent", IsActive = true };
        context.Students.Add(student);
        context.Families.Add(family);
        await context.SaveChangesAsync();

        var service = new FamilyService(context, NullLogger<FamilyService>.Instance);
        var request = new LinkFamilyRequest { FamilyId = family.Id, RelationshipType = RelationshipType.Parent };

        var linked = await service.LinkToStudentAsync(student.Id, request);

        Assert.True(linked);
        var link = await context.StudentFamilies.FirstOrDefaultAsync(sf => sf.StudentId == student.Id && sf.FamilyId == family.Id);
        Assert.NotNull(link);
        Assert.Equal(RelationshipType.Parent, link!.RelationshipType);
    }

    [Fact]
    public async Task UnlinkFromStudentAsync_Removes_Link()
    {
        using var context = CreateContext();
        var student = new Student { Reference = "S1", FirstName = "Child", IsActive = true };
        var family = new Family { FamilyName = "Family", PrimaryContactName = "Parent", IsActive = true };
        context.Students.Add(student);
        context.Families.Add(family);
        await context.SaveChangesAsync();

        var link = new StudentFamily { StudentId = student.Id, FamilyId = family.Id, RelationshipType = RelationshipType.Parent };
        context.StudentFamilies.Add(link);
        await context.SaveChangesAsync();

        var service = new FamilyService(context, NullLogger<FamilyService>.Instance);
        var unlinked = await service.UnlinkFromStudentAsync(student.Id, family.Id);

        Assert.True(unlinked);
        Assert.Equal(0, await context.StudentFamilies.CountAsync());
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
