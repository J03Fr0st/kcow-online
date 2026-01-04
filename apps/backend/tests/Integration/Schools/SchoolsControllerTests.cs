using Kcow.Application.Schools;
using Kcow.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using Kcow.Application.Auth;
using Microsoft.AspNetCore.Http;

namespace Kcow.Integration.Tests.Schools;

/// <summary>
/// Integration tests for school management endpoints.
/// </summary>
public class SchoolsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static bool _databaseInitialized = false;
    private static readonly object _lock = new object();

    public SchoolsControllerTests(CustomWebApplicationFactory factory)
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

        // First, login to get a token
        var loginRequest = new
        {
            Email = "admin@kcow.local",
            Password = "Admin123!"
        };

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.EnsureSuccessStatusCode();

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.Token);

        // Add authorization header
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", loginResult.Token);

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
                    using var tempClient = _factory.CreateClient();
                    using var scope = _factory.Services.CreateScope();
                    scope.ServiceProvider.InitializeDatabaseAsync().GetAwaiter().GetResult();
                    _databaseInitialized = true;
                }
            }
        }
    }

    private string GenerateUniqueSchoolName(string prefix = "TEST")
    {
        return $"{prefix} School {Guid.NewGuid():N}";
    }

    [Fact]
    public async Task GetAll_WithAuthentication_ReturnsSchoolsList()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/schools");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var schools = await response.Content.ReadFromJsonAsync<List<SchoolDto>>();
        Assert.NotNull(schools);
        Assert.IsType<List<SchoolDto>>(schools);
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = CreateHttpsClient();

        // Act
        var response = await client.GetAsync("/api/schools");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem!.Status);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedSchool()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var schoolName = GenerateUniqueSchoolName("CREATE");
        var request = new CreateSchoolRequest
        {
            Name = schoolName,
            Address = "123 Test Street",
            ContactName = "John Doe",
            ContactPhone = "555-1234",
            ContactEmail = "test@school.edu",
            BillingSettings = new BillingSettingsDto
            {
                DefaultSessionRate = 50.00m,
                BillingCycle = "Monthly",
                BillingNotes = "Test billing notes"
            },
            Notes = "Test school notes"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/schools", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var school = await response.Content.ReadFromJsonAsync<SchoolDto>();
        Assert.NotNull(school);
        Assert.Equal(schoolName, school.Name);
        Assert.Equal("123 Test Street", school.Address);
        Assert.Equal("John Doe", school.ContactName);
        Assert.Equal("555-1234", school.ContactPhone);
        Assert.Equal("test@school.edu", school.ContactEmail);
        Assert.NotNull(school.BillingSettings);
        Assert.Equal(50.00m, school.BillingSettings.DefaultSessionRate);
        Assert.Equal("Monthly", school.BillingSettings.BillingCycle);
        Assert.Equal("Test billing notes", school.BillingSettings.BillingNotes);
        Assert.Equal("Test school notes", school.Notes);
        Assert.True(school.IsActive);
    }

    [Fact]
    public async Task Create_WithMinimalData_ReturnsCreatedSchool()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var schoolName = GenerateUniqueSchoolName("MINIMAL");
        var request = new CreateSchoolRequest
        {
            Name = schoolName,
            Address = null,
            ContactName = null,
            ContactPhone = null,
            ContactEmail = null,
            BillingSettings = null,
            Notes = null
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/schools", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var school = await response.Content.ReadFromJsonAsync<SchoolDto>();
        Assert.NotNull(school);
        Assert.Equal(schoolName, school.Name);
        Assert.Null(school.Address);
        Assert.Null(school.ContactName);
        Assert.Null(school.ContactPhone);
        Assert.Null(school.ContactEmail);
        Assert.Null(school.BillingSettings);
        Assert.Null(school.Notes);
        Assert.True(school.IsActive);
    }

    [Fact]
    public async Task Create_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var request = new CreateSchoolRequest
        {
            Name = "", // Invalid: empty name
            Address = "123 Test Street",
            ContactName = "John Doe"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/schools", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem!.Status);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsSchool()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var schoolName = GenerateUniqueSchoolName("GET");
        var createRequest = new CreateSchoolRequest
        {
            Name = schoolName,
            Address = "123 Get Test",
            ContactName = "Get Contact",
            BillingSettings = new BillingSettingsDto
            {
                DefaultSessionRate = 75.00m,
                BillingCycle = "Weekly"
            }
        };

        var createResponse = await client.PostAsJsonAsync("/api/schools", createRequest);
        var createdSchool = await createResponse.Content.ReadFromJsonAsync<SchoolDto>();
        Assert.NotNull(createdSchool);

        // Act
        var response = await client.GetAsync($"/api/schools/{createdSchool.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var school = await response.Content.ReadFromJsonAsync<SchoolDto>();
        Assert.NotNull(school);
        Assert.Equal(createdSchool.Id, school.Id);
        Assert.Equal(schoolName, school.Name);
        Assert.Equal("123 Get Test", school.Address);
        Assert.Equal("Get Contact", school.ContactName);
        Assert.NotNull(school.BillingSettings);
        Assert.Equal(75.00m, school.BillingSettings.DefaultSessionRate);
        Assert.Equal("Weekly", school.BillingSettings.BillingCycle);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/schools/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem!.Status);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedSchool()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var schoolName = GenerateUniqueSchoolName("UPDATE");
        var createRequest = new CreateSchoolRequest
        {
            Name = schoolName,
            Address = "Original Address",
            ContactName = "Original Contact",
            ContactPhone = "555-0000",
            ContactEmail = "original@school.edu",
            BillingSettings = new BillingSettingsDto
            {
                DefaultSessionRate = 50.00m,
                BillingCycle = "Monthly"
            },
            Notes = "Original notes"
        };

        var createResponse = await client.PostAsJsonAsync("/api/schools", createRequest);
        var createdSchool = await createResponse.Content.ReadFromJsonAsync<SchoolDto>();
        Assert.NotNull(createdSchool);

        var updateRequest = new UpdateSchoolRequest
        {
            Name = "Updated School Name",
            Address = "Updated Address",
            ContactName = "Updated Contact",
            ContactPhone = "555-9999",
            ContactEmail = "updated@school.edu",
            BillingSettings = new BillingSettingsDto
            {
                DefaultSessionRate = 100.00m,
                BillingCycle = "Bi-Weekly",
                BillingNotes = "Updated billing notes"
            },
            Notes = "Updated notes"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/schools/{createdSchool.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var school = await response.Content.ReadFromJsonAsync<SchoolDto>();
        Assert.NotNull(school);
        Assert.Equal("Updated School Name", school.Name);
        Assert.Equal("Updated Address", school.Address);
        Assert.Equal("Updated Contact", school.ContactName);
        Assert.Equal("555-9999", school.ContactPhone);
        Assert.Equal("updated@school.edu", school.ContactEmail);
        Assert.NotNull(school.BillingSettings);
        Assert.Equal(100.00m, school.BillingSettings.DefaultSessionRate);
        Assert.Equal("Bi-Weekly", school.BillingSettings.BillingCycle);
        Assert.Equal("Updated billing notes", school.BillingSettings.BillingNotes);
        Assert.Equal("Updated notes", school.Notes);
        Assert.NotNull(school.UpdatedAt);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var updateRequest = new UpdateSchoolRequest
        {
            Name = "Updated Name",
            Address = "Updated Address"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/schools/99999", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem!.Status);
    }

    [Fact]
    public async Task Archive_WithValidId_ReturnsNoContent()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var schoolName = GenerateUniqueSchoolName("ARCHIVE");
        var createRequest = new CreateSchoolRequest
        {
            Name = schoolName,
            Address = "123 Archive Street"
        };

        var createResponse = await client.PostAsJsonAsync("/api/schools", createRequest);
        var createdSchool = await createResponse.Content.ReadFromJsonAsync<SchoolDto>();
        Assert.NotNull(createdSchool);

        // Act
        var response = await client.DeleteAsync($"/api/schools/{createdSchool.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify school is archived (not returned in GetAll)
        var getAllResponse = await client.GetAsync("/api/schools");
        var schools = await getAllResponse.Content.ReadFromJsonAsync<List<SchoolDto>>();
        Assert.NotNull(schools);
        Assert.DoesNotContain(schools, s => s.Id == createdSchool.Id);
    }

    [Fact]
    public async Task Archive_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.DeleteAsync("/api/schools/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem!.Status);
    }

    [Fact]
    public async Task SoftDelete_ArchivedSchoolNotReturnedInGetAll()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var schoolName = GenerateUniqueSchoolName("SOFT");
        var createRequest = new CreateSchoolRequest
        {
            Name = schoolName,
            Address = "123 Soft Delete Street"
        };

        var createResponse = await client.PostAsJsonAsync("/api/schools", createRequest);
        var createdSchool = await createResponse.Content.ReadFromJsonAsync<SchoolDto>();
        Assert.NotNull(createdSchool);

        // Verify school is in initial list
        var initialListResponse = await client.GetAsync("/api/schools");
        var initialSchools = await initialListResponse.Content.ReadFromJsonAsync<List<SchoolDto>>();
        Assert.NotNull(initialSchools);
        Assert.Contains(initialSchools, s => s.Id == createdSchool.Id);

        // Archive the school
        await client.DeleteAsync($"/api/schools/{createdSchool.Id}");

        // Act - Get all schools after archive
        var afterArchiveResponse = await client.GetAsync("/api/schools");

        // Assert - School should not be in list
        var afterArchiveSchools = await afterArchiveResponse.Content.ReadFromJsonAsync<List<SchoolDto>>();
        Assert.NotNull(afterArchiveSchools);
        Assert.DoesNotContain(afterArchiveSchools, s => s.Id == createdSchool.Id);
    }

    [Fact]
    public async Task GetAll_OnlyReturnsActiveSchools_InNameOrder()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Create multiple schools
        var school1 = await CreateSchoolAsync(client, "Zebra School", "123 Zebra St");
        var school2 = await CreateSchoolAsync(client, "Alpha School", "123 Alpha St");
        var school3 = await CreateSchoolAsync(client, "Beta School", "123 Beta St");

        // Archive one school
        await client.DeleteAsync($"/api/schools/{school2.Id}");

        // Act
        var response = await client.GetAsync("/api/schools");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var schools = await response.Content.ReadFromJsonAsync<List<SchoolDto>>();
        Assert.NotNull(schools);

        // Should not include archived school
        Assert.DoesNotContain(schools, s => s.Id == school2.Id);

        // Should be in name order (Alpha, Beta, Zebra after Zebra archived)
        var activeSchools = schools.Where(s => s.Id != school2.Id).ToList();
        Assert.True(activeSchools[0].Name.CompareTo(activeSchools[1].Name) <= 0);
    }

    private async Task<SchoolDto> CreateSchoolAsync(HttpClient client, string name, string address)
    {
        var request = new CreateSchoolRequest
        {
            Name = name,
            Address = address
        };

        var response = await client.PostAsJsonAsync("/api/schools", request);
        response.EnsureSuccessStatusCode();

        var school = await response.Content.ReadFromJsonAsync<SchoolDto>();
        Assert.NotNull(school);
        return school;
    }
}
