using Kcow.Application.Families;
using Kcow.Domain.Enums;
using Kcow.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using Kcow.Application.Auth;
using Kcow.Application.Students;

namespace Kcow.Integration.Tests.Families;

public class FamiliesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static bool _databaseInitialized = false;
    private static readonly object _lock = new object();

    public FamiliesControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
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
        var loginRequest = new { Email = "admin@kcow.local", Password = "Admin123!" };
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult!.Token);
        return client;
    }

    private void EnsureDatabaseInitialized()
    {
        if (!_databaseInitialized)
        {
            lock (_lock)
            {
                if (!_databaseInitialized)
                {
                    using var scope = _factory.Services.CreateScope();
                    scope.ServiceProvider.InitializeDatabaseAsync().GetAwaiter().GetResult();
                    _databaseInitialized = true;
                }
            }
        }
    }

    private string GenerateUniqueReference(string prefix = "S")
    {
        int maxRandomLength = 10 - prefix.Length;
        if (maxRandomLength < 0) maxRandomLength = 0;
        string randomPart = Guid.NewGuid().ToString("N");
        if (randomPart.Length > maxRandomLength) randomPart = randomPart.Substring(0, maxRandomLength);
        
        return $"{prefix}{randomPart}".ToUpper();
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedFamily()
    {
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var request = new CreateFamilyRequest
        {
            FamilyName = "Jones",
            PrimaryContactName = "Mary Jones",
            Email = "mary@jones.com"
        };

        var response = await client.PostAsJsonAsync("/api/families", request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var family = await response.Content.ReadFromJsonAsync<FamilyDto>();
        Assert.NotNull(family);
        Assert.Equal("Jones", family.FamilyName);
    }

    [Fact]
    public async Task StudentLink_Cycle_Works()
    {
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // 1. Create Student
        var reference = GenerateUniqueReference("LINK");
        var studentReq = new CreateStudentRequest { Reference = reference, FirstName = "LinkChild", IsActive = true };
        var studentResp = await client.PostAsJsonAsync("/api/students", studentReq);
        studentResp.EnsureSuccessStatusCode();
        var student = await studentResp.Content.ReadFromJsonAsync<StudentDto>();

        // 2. Create Family
        var familyReq = new CreateFamilyRequest { FamilyName = "LinkFamily", PrimaryContactName = "LinkParent" };
        var familyResp = await client.PostAsJsonAsync("/api/families", familyReq);
        familyResp.EnsureSuccessStatusCode();
        var family = await familyResp.Content.ReadFromJsonAsync<FamilyDto>();

        // 3. Link Student to Family
        var linkReq = new LinkFamilyRequest { FamilyId = family!.Id, RelationshipType = RelationshipType.Parent };
        var linkResp = await client.PostAsJsonAsync($"/api/students/{student!.Id}/families", linkReq);
        linkResp.EnsureSuccessStatusCode();

        // 4. Verify Link via Student endpoint
        var getStudFamiliesResp = await client.GetAsync($"/api/students/{student.Id}/families");
        var families = await getStudFamiliesResp.Content.ReadFromJsonAsync<List<FamilyDto>>();
        Assert.Contains(families!, f => f.Id == family.Id);

        // 5. Unlink
        var unlinkResp = await client.DeleteAsync($"/api/students/{student.Id}/families/{family.Id}");
        Assert.Equal(HttpStatusCode.NoContent, unlinkResp.StatusCode);

        // 6. Verify Unlink
        var getStudFamiliesResp2 = await client.GetAsync($"/api/students/{student.Id}/families");
        var families2 = await getStudFamiliesResp2.Content.ReadFromJsonAsync<List<FamilyDto>>();
        Assert.DoesNotContain(families2!, f => f.Id == family.Id);
    }
}
