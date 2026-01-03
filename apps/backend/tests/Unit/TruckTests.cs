using Kcow.Domain.Entities;

namespace Kcow.Unit.Tests;

public class TruckTests
{
    [Fact]
    public void Truck_Defaults_Are_Set()
    {
        var truck = new Truck();

        Assert.Equal("Active", truck.Status);
        Assert.True(truck.IsActive);
        Assert.Equal(string.Empty, truck.Name);
        Assert.Equal(string.Empty, truck.RegistrationNumber);
    }
}
