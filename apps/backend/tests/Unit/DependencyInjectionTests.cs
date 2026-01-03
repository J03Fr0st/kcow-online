using Kcow.Application;

namespace Kcow.Unit.Tests;

/// <summary>
/// Unit tests for Application layer dependency injection.
/// </summary>
public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_Returns_ServiceCollection()
    {
        // Arrange
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        // Act
        var result = services.AddApplication();

        // Assert
        Assert.NotNull(result);
        Assert.Same(services, result);
    }
}
