using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Kcow.Integration.Tests;

/// <summary>
/// Integration tests for the health endpoint.
/// </summary>
public class HealthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public HealthEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthEndpoint_ReturnsOk_WithHealthyStatus()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(content);
        Assert.Equal("healthy", content.Status);
    }

    [Fact]
    public void EachFactoryUsesItsOwnDatabase()
    {
        using var client = _factory.CreateClient();
        var configuration = _factory.Services.GetRequiredService<IConfiguration>();

        Assert.Equal($"Data Source={_factory.DatabasePath}",
            configuration.GetConnectionString("DefaultConnection"));
        var connectionFactory = _factory.Services.GetRequiredService<Kcow.Infrastructure.Database.IDbConnectionFactory>();
        using var connection = connectionFactory.Create();
        Assert.Equal(_factory.DatabasePath,
            ((Microsoft.Data.Sqlite.SqliteConnection)connection).DataSource);
    }

    private record HealthResponse(string Status, DateTime Timestamp);
}
