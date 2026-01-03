using Kcow.Application.Trucks;
using Kcow.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using Kcow.Application.Auth;

namespace Kcow.Integration.Tests.Trucks;

/// <summary>
/// Integration tests for truck management endpoints.
/// </summary>
public class TrucksControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static bool _databaseInitialized = false;
    private static readonly object _lock = new object();

    public TrucksControllerTests(CustomWebApplicationFactory factory)
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

    private string GenerateUniqueRegistrationNumber(string prefix = "TEST")
    {
        return $"{prefix}-{Guid.NewGuid():N}";
    }

    [Fact]
    public async Task GetAll_WithAuthentication_ReturnsTrucksList()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/trucks");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var trucks = await response.Content.ReadFromJsonAsync<List<TruckDto>>();
        Assert.NotNull(trucks);
        Assert.IsType<List<TruckDto>>(trucks);
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = CreateHttpsClient();

        // Act
        var response = await client.GetAsync("/api/trucks");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem!.Status);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedTruck()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var registrationNumber = GenerateUniqueRegistrationNumber("TEST");
        var request = new CreateTruckRequest
        {
            Name = "Test Truck",
            RegistrationNumber = registrationNumber,
            Status = "Active",
            Notes = "Test truck notes"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/trucks", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var truck = await response.Content.ReadFromJsonAsync<TruckDto>();
        Assert.NotNull(truck);
        Assert.Equal("Test Truck", truck.Name);
        Assert.Equal(registrationNumber, truck.RegistrationNumber);
        Assert.Equal("Active", truck.Status);
        Assert.True(truck.IsActive);
    }

    [Fact]
    public async Task Create_WithDuplicateRegistrationNumber_ReturnsBadRequest()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var registrationNumber = GenerateUniqueRegistrationNumber("DUP");
        var request = new CreateTruckRequest
        {
            Name = "Test Truck",
            RegistrationNumber = registrationNumber,
            Status = "Active"
        };

        // Create first truck
        await client.PostAsJsonAsync("/api/trucks", request);

        // Act - Try to create duplicate
        var response = await client.PostAsJsonAsync("/api/trucks", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem!.Status);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsTruck()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var registrationNumber = GenerateUniqueRegistrationNumber("GET");
        var request = new CreateTruckRequest
        {
            Name = "Test Truck",
            RegistrationNumber = registrationNumber,
            Status = "Active"
        };

        var createResponse = await client.PostAsJsonAsync("/api/trucks", request);
        var createdTruck = await createResponse.Content.ReadFromJsonAsync<TruckDto>();
        Assert.NotNull(createdTruck);

        // Act
        var response = await client.GetAsync($"/api/trucks/{createdTruck.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var truck = await response.Content.ReadFromJsonAsync<TruckDto>();
        Assert.NotNull(truck);
        Assert.Equal(createdTruck.Id, truck.Id);
        Assert.Equal("Test Truck", truck.Name);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/trucks/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem!.Status);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedTruck()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var registrationNumber = GenerateUniqueRegistrationNumber("UPD");
        var createRequest = new CreateTruckRequest
        {
            Name = "Original Name",
            RegistrationNumber = registrationNumber,
            Status = "Active"
        };

        var createResponse = await client.PostAsJsonAsync("/api/trucks", createRequest);
        var createdTruck = await createResponse.Content.ReadFromJsonAsync<TruckDto>();
        Assert.NotNull(createdTruck);

        var updateRequest = new UpdateTruckRequest
        {
            Name = "Updated Name",
            RegistrationNumber = registrationNumber,
            Status = "Maintenance",
            Notes = "Updated notes"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/trucks/{createdTruck.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var truck = await response.Content.ReadFromJsonAsync<TruckDto>();
        Assert.NotNull(truck);
        Assert.Equal("Updated Name", truck.Name);
        Assert.Equal("Maintenance", truck.Status);
        Assert.Equal("Updated notes", truck.Notes);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var registrationNumber = GenerateUniqueRegistrationNumber("UPD");
        var updateRequest = new UpdateTruckRequest
        {
            Name = "Updated Name",
            RegistrationNumber = registrationNumber,
            Status = "Active"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/trucks/99999", updateRequest);

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

        var registrationNumber = GenerateUniqueRegistrationNumber("ARC");
        var createRequest = new CreateTruckRequest
        {
            Name = "To Archive",
            RegistrationNumber = registrationNumber,
            Status = "Active"
        };

        var createResponse = await client.PostAsJsonAsync("/api/trucks", createRequest);
        var createdTruck = await createResponse.Content.ReadFromJsonAsync<TruckDto>();
        Assert.NotNull(createdTruck);

        // Act
        var response = await client.DeleteAsync($"/api/trucks/{createdTruck.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify truck is archived (not returned in GetAll)
        var getAllResponse = await client.GetAsync("/api/trucks");
        var trucks = await getAllResponse.Content.ReadFromJsonAsync<List<TruckDto>>();
        Assert.NotNull(trucks);
        Assert.DoesNotContain(trucks, t => t.Id == createdTruck.Id);
    }

    [Fact]
    public async Task Archive_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.DeleteAsync("/api/trucks/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem!.Status);
    }

    [Fact]
    public async Task Create_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var request = new CreateTruckRequest
        {
            Name = "", // Invalid: empty name
            RegistrationNumber = "", // Invalid: empty registration
            Status = "Active"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/trucks", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem!.Status);
    }

    [Fact]
    public async Task SoftDelete_ArchivedTruckNotReturnedInGetAll()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var registrationNumber = GenerateUniqueRegistrationNumber("SOFT");
        var createRequest = new CreateTruckRequest
        {
            Name = "Test Truck",
            RegistrationNumber = registrationNumber,
            Status = "Active"
        };

        var createResponse = await client.PostAsJsonAsync("/api/trucks", createRequest);
        var createdTruck = await createResponse.Content.ReadFromJsonAsync<TruckDto>();
        Assert.NotNull(createdTruck);

        // Verify truck is in initial list
        var initialListResponse = await client.GetAsync("/api/trucks");
        var initialTrucks = await initialListResponse.Content.ReadFromJsonAsync<List<TruckDto>>();
        Assert.NotNull(initialTrucks);
        Assert.Contains(initialTrucks, t => t.Id == createdTruck.Id);

        // Archive the truck
        await client.DeleteAsync($"/api/trucks/{createdTruck.Id}");

        // Act - Get all trucks after archive
        var afterArchiveResponse = await client.GetAsync("/api/trucks");

        // Assert - Truck should not be in list
        var afterArchiveTrucks = await afterArchiveResponse.Content.ReadFromJsonAsync<List<TruckDto>>();
        Assert.NotNull(afterArchiveTrucks);
        Assert.DoesNotContain(afterArchiveTrucks, t => t.Id == createdTruck.Id);
    }
}
