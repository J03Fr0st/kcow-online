using Kcow.Application.Import;

namespace Kcow.Integration.Tests.Import;

public class LegacyTruckSeedDataTests
{
    [Fact]
    public void Build_ReturnsStandardTruckSeedData()
    {
        var trucks = LegacyTruckSeedData.Build();

        Assert.Equal(5, trucks.Count);
        Assert.Equal("Alpha", trucks[0].Name);
        Assert.Equal("KCOW-001", trucks[0].RegistrationNumber);
        Assert.Equal("Bravo", trucks[1].Name);
        Assert.Equal("KCOW-002", trucks[1].RegistrationNumber);
    }
}
