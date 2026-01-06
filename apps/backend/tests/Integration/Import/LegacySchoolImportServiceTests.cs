using Kcow.Application.Auth;
using Kcow.Application.Schools;
using Kcow.Infrastructure;
using Kcow.Infrastructure.Data;
using Kcow.Infrastructure.Import;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Kcow.Integration.Tests.Import;

public class LegacySchoolImportServiceTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static bool _databaseInitialized;
    private static readonly object _lock = new object();

    public LegacySchoolImportServiceTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ImportAsync_AddsSchoolAndIsVisibleInApi()
    {
        EnsureDatabaseInitialized();

        var schoolId = Random.Shared.Next(100000, 999999);
        var xmlPath = WriteTempFile($"""
            <?xml version="1.0" encoding="utf-8"?>
            <dataroot>
              <School>
                <Short_x0020_School>Import Test</Short_x0020_School>
                <School_x0020_Id>{schoolId}</School_x0020_Id>
                <Print>1</Print>
                <Import>1</Import>
              </School>
            </dataroot>
            """);
        var xsdPath = FindRepoFile("docs/legacy/1_School/School.xsd");
        var auditPath = Path.GetTempFileName();
        var summaryPath = Path.GetTempFileName();

        try
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var importer = new LegacySchoolImportService(context);
                var summary = await importer.ImportAsync(xmlPath, xsdPath, auditPath, summaryPath);
                Assert.Equal(1, summary.ImportedCount);
                Assert.Equal(0, summary.SkippedCount);
            }

            using var client = await CreateAuthenticatedClientAsync();
            var response = await client.GetAsync("/api/schools");
            response.EnsureSuccessStatusCode();

            var schools = await response.Content.ReadFromJsonAsync<List<SchoolDto>>();
            Assert.NotNull(schools);
            Assert.Contains(schools, s => s.Id == schoolId);

            // Verify field mappings - AC #2 validation
            var importedSchool = schools.First(s => s.Id == schoolId);
            Assert.Equal("Import Test", importedSchool.Name); // ShortSchool → Name fallback
            Assert.True(importedSchool.PrintInvoice); // Print=1 mapped correctly
            Assert.True(importedSchool.ImportFlag); // Import=1 mapped correctly
        }
        finally
        {
            File.Delete(xmlPath);
            File.Delete(auditPath);
            File.Delete(summaryPath);
        }
    }

    private HttpClient CreateHttpsClient()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = CreateHttpsClient();

        var loginRequest = new
        {
            Email = "admin@kcow.local",
            Password = "Admin123!"
        };

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult!.Token);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);

        return client;
    }

    private void EnsureDatabaseInitialized()
    {
        if (_databaseInitialized)
        {
            return;
        }

        lock (_lock)
        {
            if (_databaseInitialized)
            {
                return;
            }

            using var scope = _factory.Services.CreateScope();
            scope.ServiceProvider.InitializeDatabaseAsync().GetAwaiter().GetResult();
            _databaseInitialized = true;
        }
    }

    private static string WriteTempFile(string contents)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, contents);
        return path;
    }

    private static string FindRepoFile(string relativePath)
    {
        var baseDirectory = new DirectoryInfo(AppContext.BaseDirectory);
        while (baseDirectory != null)
        {
            var candidate = Path.Combine(baseDirectory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            baseDirectory = baseDirectory.Parent;
        }

        throw new FileNotFoundException($"Unable to locate {relativePath} from {AppContext.BaseDirectory}");
    }
}
