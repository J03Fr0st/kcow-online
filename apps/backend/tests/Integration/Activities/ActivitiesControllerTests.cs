using Kcow.Application.Activities;
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

namespace Kcow.Integration.Tests.Activities;

/// <summary>
/// Integration tests for activity management endpoints.
/// </summary>
public class ActivitiesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static bool _databaseInitialized = false;
    private static readonly object _lock = new object();

    public ActivitiesControllerTests(CustomWebApplicationFactory factory)
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

    private string GenerateUniqueCode(string prefix = "TEST")
    {
        return $"{prefix}-{Guid.NewGuid():N}".Substring(0, 20);
    }

    [Fact]
    public async Task GetAll_WithAuthentication_ReturnsActivitiesList()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/activities");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var activities = await response.Content.ReadFromJsonAsync<List<ActivityDto>>();
        Assert.NotNull(activities);
        Assert.IsType<List<ActivityDto>>(activities);
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = CreateHttpsClient();

        // Act
        var response = await client.GetAsync("/api/activities");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem!.Status);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedActivity()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var code = GenerateUniqueCode("ACT");
        var request = new CreateActivityRequest
        {
            Code = code,
            Name = "Test Activity",
            Description = "Test activity description",
            Folder = "TestFolder",
            GradeLevel = "Grade 3"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/activities", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var activity = await response.Content.ReadFromJsonAsync<ActivityDto>();
        Assert.NotNull(activity);
        Assert.Equal(code, activity.Code);
        Assert.Equal("Test Activity", activity.Name);
        Assert.Equal("Test activity description", activity.Description);
        Assert.True(activity.IsActive);
    }

    [Fact]
    public async Task Create_WithDuplicateCode_ReturnsBadRequest()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var code = GenerateUniqueCode("DUP");
        var request = new CreateActivityRequest
        {
            Code = code,
            Name = "Test Activity",
            Description = "Test activity"
        };

        // Create first activity
        await client.PostAsJsonAsync("/api/activities", request);

        // Act - Try to create duplicate
        var response = await client.PostAsJsonAsync("/api/activities", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem!.Status);
    }

    [Fact]
    public async Task Create_WithSpecificId_ReturnsCreatedActivityWithThatId()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var specificId = 99900 + new Random().Next(1, 99);
        var code = GenerateUniqueCode("ID");
        var request = new CreateActivityRequest
        {
            Id = specificId,
            Code = code,
            Name = "Activity with Specific ID"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/activities", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var activity = await response.Content.ReadFromJsonAsync<ActivityDto>();
        Assert.NotNull(activity);
        Assert.Equal(specificId, activity.Id);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsActivity()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var code = GenerateUniqueCode("GET");
        var request = new CreateActivityRequest
        {
            Code = code,
            Name = "Test Activity"
        };

        var createResponse = await client.PostAsJsonAsync("/api/activities", request);
        var createdActivity = await createResponse.Content.ReadFromJsonAsync<ActivityDto>();
        Assert.NotNull(createdActivity);

        // Act
        var response = await client.GetAsync($"/api/activities/{createdActivity.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var activity = await response.Content.ReadFromJsonAsync<ActivityDto>();
        Assert.NotNull(activity);
        Assert.Equal(createdActivity.Id, activity.Id);
        Assert.Equal("Test Activity", activity.Name);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/activities/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem!.Status);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedActivity()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var code = GenerateUniqueCode("UPD");
        var createRequest = new CreateActivityRequest
        {
            Code = code,
            Name = "Original Name",
            Description = "Original description"
        };

        var createResponse = await client.PostAsJsonAsync("/api/activities", createRequest);
        var createdActivity = await createResponse.Content.ReadFromJsonAsync<ActivityDto>();
        Assert.NotNull(createdActivity);

        var updateRequest = new UpdateActivityRequest
        {
            Code = code,
            Name = "Updated Name",
            Description = "Updated description",
            GradeLevel = "Grade 5"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/activities/{createdActivity.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var activity = await response.Content.ReadFromJsonAsync<ActivityDto>();
        Assert.NotNull(activity);
        Assert.Equal("Updated Name", activity.Name);
        Assert.Equal("Updated description", activity.Description);
        Assert.Equal("Grade 5", activity.GradeLevel);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var updateRequest = new UpdateActivityRequest
        {
            Code = "TEST",
            Name = "Updated Name"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/activities/99999", updateRequest);

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

        var code = GenerateUniqueCode("ARC");
        var createRequest = new CreateActivityRequest
        {
            Code = code,
            Name = "To Archive"
        };

        var createResponse = await client.PostAsJsonAsync("/api/activities", createRequest);
        var createdActivity = await createResponse.Content.ReadFromJsonAsync<ActivityDto>();
        Assert.NotNull(createdActivity);

        // Act
        var response = await client.DeleteAsync($"/api/activities/{createdActivity.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify activity is archived (not returned in GetAll)
        var getAllResponse = await client.GetAsync("/api/activities");
        var activities = await getAllResponse.Content.ReadFromJsonAsync<List<ActivityDto>>();
        Assert.NotNull(activities);
        Assert.DoesNotContain(activities, a => a.Id == createdActivity.Id);
    }

    [Fact]
    public async Task Archive_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.DeleteAsync("/api/activities/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem!.Status);
    }

    [Fact]
    public async Task SoftDelete_ArchivedActivityNotReturnedInGetAll()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var code = GenerateUniqueCode("SOFT");
        var createRequest = new CreateActivityRequest
        {
            Code = code,
            Name = "Test Activity"
        };

        var createResponse = await client.PostAsJsonAsync("/api/activities", createRequest);
        var createdActivity = await createResponse.Content.ReadFromJsonAsync<ActivityDto>();
        Assert.NotNull(createdActivity);

        // Verify activity is in initial list
        var initialListResponse = await client.GetAsync("/api/activities");
        var initialActivities = await initialListResponse.Content.ReadFromJsonAsync<List<ActivityDto>>();
        Assert.NotNull(initialActivities);
        Assert.Contains(initialActivities, a => a.Id == createdActivity.Id);

        // Archive the activity
        await client.DeleteAsync($"/api/activities/{createdActivity.Id}");

        // Act - Get all activities after archive
        var afterArchiveResponse = await client.GetAsync("/api/activities");

        // Assert - Activity should not be in list
        var afterArchiveActivities = await afterArchiveResponse.Content.ReadFromJsonAsync<List<ActivityDto>>();
        Assert.NotNull(afterArchiveActivities);
        Assert.DoesNotContain(afterArchiveActivities, a => a.Id == createdActivity.Id);
    }

    [Fact]
    public async Task Create_WithNullCode_ReturnsCreatedActivity()
    {
        // Arrange - Code is optional for activities
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var request = new CreateActivityRequest
        {
            Name = "Activity Without Code",
            Description = "Testing optional code field"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/activities", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var activity = await response.Content.ReadFromJsonAsync<ActivityDto>();
        Assert.NotNull(activity);
        Assert.Null(activity.Code);
        Assert.Equal("Activity Without Code", activity.Name);
    }
}
