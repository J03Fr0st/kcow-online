using Kcow.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Kcow.Integration.Tests;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(), $"kcow-integration-{Guid.NewGuid():N}.db");

    public string DatabasePath => _databasePath;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databasePath}"
            });
        });
        builder.ConfigureServices(services =>
        {
            // The application registers these before test configuration is applied.
            var connectionString = $"Data Source={_databasePath};Pooling=False";
            services.RemoveAll<IDbConnectionFactory>();
            services.RemoveAll<DbUpBootstrapper>();
            services.AddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(connectionString));
            services.AddSingleton(serviceProvider => new DbUpBootstrapper(
                connectionString,
                serviceProvider.GetRequiredService<ILogger<DbUpBootstrapper>>(),
                Path.Combine(AppContext.BaseDirectory, "Migrations", "Scripts")));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing && File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
