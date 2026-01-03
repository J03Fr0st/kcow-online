using Kcow.Domain.Entities;
using Kcow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Kcow.Unit.Tests;

public class TruckConfigurationTests
{
    [Fact]
    public void TruckConfiguration_Uses_SnakeCase_Table_And_Columns()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var context = new AppDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(Truck));

        Assert.NotNull(entityType);
        Assert.Equal("trucks", entityType!.GetTableName());

        var storeObject = StoreObjectIdentifier.Table("trucks", null);

        Assert.Equal("id", entityType.FindProperty(nameof(Truck.Id))?.GetColumnName(storeObject));
        Assert.Equal("name", entityType.FindProperty(nameof(Truck.Name))?.GetColumnName(storeObject));
        Assert.Equal("registration_number", entityType.FindProperty(nameof(Truck.RegistrationNumber))?.GetColumnName(storeObject));
        Assert.Equal("status", entityType.FindProperty(nameof(Truck.Status))?.GetColumnName(storeObject));
        Assert.Equal("notes", entityType.FindProperty(nameof(Truck.Notes))?.GetColumnName(storeObject));
        Assert.Equal("is_active", entityType.FindProperty(nameof(Truck.IsActive))?.GetColumnName(storeObject));
        Assert.Equal("created_at", entityType.FindProperty(nameof(Truck.CreatedAt))?.GetColumnName(storeObject));
        Assert.Equal("updated_at", entityType.FindProperty(nameof(Truck.UpdatedAt))?.GetColumnName(storeObject));
    }
}
