using Kcow.Application.Auth;
using Kcow.Application.ClassGroups;
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

namespace Kcow.Integration.Tests.ClassGroups;

/// <summary>
/// Integration tests for ClassGroup management endpoints.
/// Tests authentication, CRUD operations, filtering, validation, and XSD compliance.
/// </summary>
public class ClassGroupsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static bool _databaseInitialized = false;
    private static readonly object _lock = new object();

    public ClassGroupsControllerTests(CustomWebApplicationFactory factory)
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

        // Login to get JWT token
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

    private async Task<(int schoolId, int? truckId)> CreateTestDataAsync(HttpClient client)
    {
        // Create a school
        var schoolRequest = new
        {
            Name = "Test School",
            Address = "123 Test St",
            ContactCell = "555-0100",
            IsActive = true
        };
        var schoolResponse = await client.PostAsJsonAsync("/api/schools", schoolRequest);
        schoolResponse.EnsureSuccessStatusCode();
        var school = await schoolResponse.Content.ReadFromJsonAsync<SchoolDto>();
        Assert.NotNull(school);

        // Create a truck
        var truckRequest = new
        {
            Name = "Test Truck",
            RegistrationNumber = $"TRUCK-{Guid.NewGuid():N}",
            Status = "Active",
            IsActive = true
        };
        var truckResponse = await client.PostAsJsonAsync("/api/trucks", truckRequest);
        truckResponse.EnsureSuccessStatusCode();
        var truck = await truckResponse.Content.ReadFromJsonAsync<TruckDto>();
        Assert.NotNull(truck);

        return (school.Id, truck.Id);
    }

    #region Authentication Tests

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = CreateHttpsClient();

        // Act
        var response = await client.GetAsync("/api/class-groups");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status401Unauthorized, problem!.Status);
    }

    [Fact]
    public async Task GetById_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = CreateHttpsClient();

        // Act
        var response = await client.GetAsync("/api/class-groups/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = CreateHttpsClient();
        var request = new CreateClassGroupRequest
        {
            Name = "Test",
            SchoolId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/class-groups", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = CreateHttpsClient();
        var request = new UpdateClassGroupRequest
        {
            Name = "Test",
            SchoolId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/class-groups/1", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Delete_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = CreateHttpsClient();

        // Act
        var response = await client.DeleteAsync("/api/class-groups/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    #endregion

    #region CRUD Tests

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedClassGroup()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();
        var (schoolId, truckId) = await CreateTestDataAsync(client);

        var request = new CreateClassGroupRequest
        {
            Name = "CG001",
            SchoolId = schoolId,
            TruckId = truckId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 30),
            Sequence = 1,
            // XSD fields
            DayTruck = "M01",
            Description = "Morning class",
            Evaluate = true,
            ImportFlag = false,
            Notes = "Test notes",
            GroupMessage = "Welcome",
            SendCertificates = "Yes",
            MoneyMessage = "Pay online",
            Ixl = "IXL"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/class-groups", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var cg = await response.Content.ReadFromJsonAsync<ClassGroupDto>();
        Assert.NotNull(cg);
        Assert.Equal("CG001", cg.Name);
        Assert.Equal(schoolId, cg.SchoolId);
        Assert.Equal(truckId, cg.TruckId);
        Assert.Equal(DayOfWeek.Monday, cg.DayOfWeek);
        Assert.Equal(new TimeOnly(9, 0), cg.StartTime);
        Assert.Equal(new TimeOnly(10, 30), cg.EndTime);
        // Verify XSD fields
        Assert.Equal("M01", cg.DayTruck);
        Assert.Equal("Morning class", cg.Description);
        Assert.True(cg.Evaluate);
        Assert.False(cg.ImportFlag);
        Assert.Equal("Test notes", cg.Notes);
        Assert.Equal("Welcome", cg.GroupMessage);
        Assert.Equal("Yes", cg.SendCertificates);
        Assert.Equal("Pay online", cg.MoneyMessage);
        Assert.Equal("IXL", cg.Ixl);
    }

    [Fact]
    public async Task Create_WithInvalidSchoolId_ReturnsBadRequest()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var request = new CreateClassGroupRequest
        {
            Name = "CG001",
            SchoolId = 99999, // Invalid school
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/class-groups", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem!.Status);
    }

    [Fact]
    public async Task Create_WithInvalidName_ReturnsValidationError()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();
        var (schoolId, _) = await CreateTestDataAsync(client);

        var request = new CreateClassGroupRequest
        {
            Name = "VeryLongName", // Exceeds XSD max of 10 characters
            SchoolId = schoolId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/class-groups", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("Name", problem!.Title);
    }

    [Fact]
    public async Task GetAll_ReturnsClassGroupsWithSchoolAndTruckDetails()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();
        var (schoolId, truckId) = await CreateTestDataAsync(client);

        // Create a class group
        var createRequest = new CreateClassGroupRequest
        {
            Name = "CG001",
            SchoolId = schoolId,
            TruckId = truckId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };
        await client.PostAsJsonAsync("/api/class-groups", createRequest);

        // Act
        var response = await client.GetAsync("/api/class-groups");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cgs = await response.Content.ReadFromJsonAsync<List<ClassGroupDto>>();
        Assert.NotNull(cgs);
        Assert.NotEmpty(cgs);
        var cg = cgs.First();
        Assert.NotNull(cg.School);
        Assert.NotNull(cg.Truck);
    }

    [Fact]
    public async Task GetAll_WithSchoolFilter_ReturnsFilteredResults()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();
        var (schoolId1, _) = await CreateTestDataAsync(client);
        var (schoolId2, _) = await CreateTestDataAsync(client);

        // Create class groups for different schools
        await client.PostAsJsonAsync("/api/class-groups", new CreateClassGroupRequest
        {
            Name = "CG1",
            SchoolId = schoolId1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        });

        await client.PostAsJsonAsync("/api/class-groups", new CreateClassGroupRequest
        {
            Name = "CG2",
            SchoolId = schoolId2,
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0)
        });

        // Act
        var response = await client.GetAsync($"/api/class-groups?schoolId={schoolId1}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cgs = await response.Content.ReadFromJsonAsync<List<ClassGroupDto>>();
        Assert.NotNull(cgs);
        Assert.Single(cgs);
        Assert.Equal("CG1", cgs[0].Name);
        Assert.Equal(schoolId1, cgs[0].SchoolId);
    }

    [Fact]
    public async Task GetAll_WithTruckFilter_ReturnsFilteredResults()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();
        var (schoolId, truckId1) = await CreateTestDataAsync(client);
        var (_, truckId2) = await CreateTestDataAsync(client);

        // Create class groups with different trucks
        await client.PostAsJsonAsync("/api/class-groups", new CreateClassGroupRequest
        {
            Name = "CG1",
            SchoolId = schoolId,
            TruckId = truckId1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        });

        await client.PostAsJsonAsync("/api/class-groups", new CreateClassGroupRequest
        {
            Name = "CG2",
            SchoolId = schoolId,
            TruckId = truckId2,
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0)
        });

        // Act
        var response = await client.GetAsync($"/api/class-groups?truckId={truckId1}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cgs = await response.Content.ReadFromJsonAsync<List<ClassGroupDto>>();
        Assert.NotNull(cgs);
        Assert.Single(cgs);
        Assert.Equal("CG1", cgs[0].Name);
        Assert.Equal(truckId1, cgs[0].TruckId);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsClassGroupWithDetails()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();
        var (schoolId, truckId) = await CreateTestDataAsync(client);

        var createRequest = new CreateClassGroupRequest
        {
            Name = "CG001",
            SchoolId = schoolId,
            TruckId = truckId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };
        var createResponse = await client.PostAsJsonAsync("/api/class-groups", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ClassGroupDto>();
        Assert.NotNull(created);

        // Act
        var response = await client.GetAsync($"/api/class-groups/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cg = await response.Content.ReadFromJsonAsync<ClassGroupDto>();
        Assert.NotNull(cg);
        Assert.Equal(created.Id, cg.Id);
        Assert.NotNull(cg.School);
        Assert.NotNull(cg.Truck);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/class-groups/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem!.Status);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedClassGroup()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();
        var (schoolId, truckId) = await CreateTestDataAsync(client);

        var createRequest = new CreateClassGroupRequest
        {
            Name = "Original",
            SchoolId = schoolId,
            TruckId = truckId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };
        var createResponse = await client.PostAsJsonAsync("/api/class-groups", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ClassGroupDto>();
        Assert.NotNull(created);

        var updateRequest = new UpdateClassGroupRequest
        {
            Name = "Updated",
            SchoolId = schoolId,
            TruckId = truckId,
            DayOfWeek = DayOfWeek.Tuesday,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 30),
            Sequence = 2,
            // XSD fields
            DayTruck = "T02",
            Description = "Updated description",
            Evaluate = false,
            ImportFlag = true,
            Notes = "Updated notes",
            GroupMessage = "Updated message",
            SendCertificates = "No",
            MoneyMessage = "Updated payment",
            Ixl = "IX2"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/class-groups/{created.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cg = await response.Content.ReadFromJsonAsync<ClassGroupDto>();
        Assert.NotNull(cg);
        Assert.Equal("Updated", cg.Name);
        Assert.Equal(DayOfWeek.Tuesday, cg.DayOfWeek);
        Assert.Equal(new TimeOnly(10, 0), cg.StartTime);
        Assert.Equal(new TimeOnly(11, 30), cg.EndTime);
        Assert.Equal(2, cg.Sequence);
        // Verify XSD field updates
        Assert.Equal("T02", cg.DayTruck);
        Assert.Equal("Updated description", cg.Description);
        Assert.False(cg.Evaluate);
        Assert.True(cg.ImportFlag);
        Assert.Equal("Updated notes", cg.Notes);
        Assert.Equal("Updated message", cg.GroupMessage);
        Assert.Equal("No", cg.SendCertificates);
        Assert.Equal("Updated payment", cg.MoneyMessage);
        Assert.Equal("IX2", cg.Ixl);
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var updateRequest = new UpdateClassGroupRequest
        {
            Name = "Test",
            SchoolId = 1,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/class-groups/99999", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem!.Status);
    }

    [Fact]
    public async Task Archive_WithValidId_SetsIsActiveFalse()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();
        var (schoolId, _) = await CreateTestDataAsync(client);

        var createRequest = new CreateClassGroupRequest
        {
            Name = "To Archive",
            SchoolId = schoolId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };
        var createResponse = await client.PostAsJsonAsync("/api/class-groups", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ClassGroupDto>();
        Assert.NotNull(created);

        // Act
        var response = await client.DeleteAsync($"/api/class-groups/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's archived (not in GetAll)
        var getAllResponse = await client.GetAsync("/api/class-groups");
        var cgs = await getAllResponse.Content.ReadFromJsonAsync<List<ClassGroupDto>>();
        Assert.NotNull(cgs);
        Assert.DoesNotContain(cgs, cg => cg.Id == created.Id);
    }

    [Fact]
    public async Task Archive_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.DeleteAsync("/api/class-groups/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem!.Status);
    }

    [Fact]
    public async Task SoftDelete_ArchivedClassGroupNotReturnedInGetAll()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();
        var (schoolId, _) = await CreateTestDataAsync(client);

        var createRequest = new CreateClassGroupRequest
        {
            Name = "Test CG",
            SchoolId = schoolId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };
        var createResponse = await client.PostAsJsonAsync("/api/class-groups", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ClassGroupDto>();
        Assert.NotNull(created);

        // Verify it's in initial list
        var initialResponse = await client.GetAsync("/api/class-groups");
        var initialCgs = await initialResponse.Content.ReadFromJsonAsync<List<ClassGroupDto>>();
        Assert.NotNull(initialCgs);
        Assert.Contains(initialCgs, cg => cg.Id == created.Id);

        // Archive it
        await client.DeleteAsync($"/api/class-groups/{created.Id}");

        // Act - Get all after archive
        var afterResponse = await client.GetAsync("/api/class-groups");

        // Assert - Should not be in list
        var afterCgs = await afterResponse.Content.ReadFromJsonAsync<List<ClassGroupDto>>();
        Assert.NotNull(afterCgs);
        Assert.DoesNotContain(afterCgs, cg => cg.Id == created.Id);
    }

    #endregion

    #region XSD Compliance Tests

    [Fact]
    public async Task XsdCompliance_AllFieldsPersistAndRetrieve()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();
        var (schoolId, truckId) = await CreateTestDataAsync(client);

        var request = new CreateClassGroupRequest
        {
            Name = "XSD01", // Within 10 char limit
            SchoolId = schoolId,
            TruckId = truckId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0),
            Sequence = 1,
            // All 8 new XSD fields
            DayTruck = "M01",       // 6 chars max
            Description = "Desc",   // 35 chars max
            Evaluate = true,        // boolean
            ImportFlag = false,     // boolean
            Notes = "Note",         // 255 chars max
            GroupMessage = "GM",    // 255 chars max
            SendCertificates = "SC", // 255 chars max
            MoneyMessage = "MM",    // 50 chars max
            Ixl = "IXL"             // 3 chars max
        };

        // Act
        var createResponse = await client.PostAsJsonAsync("/api/class-groups", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ClassGroupDto>();
        Assert.NotNull(created);

        // Get by ID to verify retrieval
        var getResponse = await client.GetAsync($"/api/class-groups/{created.Id}");
        var retrieved = await getResponse.Content.ReadFromJsonAsync<ClassGroupDto>();

        // Assert - All XSD fields persisted
        Assert.NotNull(retrieved);
        Assert.Equal("XSD01", retrieved.Name);
        Assert.Equal("M01", retrieved.DayTruck);
        Assert.Equal("Desc", retrieved.Description);
        Assert.True(retrieved.Evaluate);
        Assert.False(retrieved.ImportFlag);
        Assert.Equal("Note", retrieved.Notes);
        Assert.Equal("GM", retrieved.GroupMessage);
        Assert.Equal("SC", retrieved.SendCertificates);
        Assert.Equal("MM", retrieved.MoneyMessage);
        Assert.Equal("IXL", retrieved.Ixl);
    }

    [Fact]
    public async Task XsdValidation_FieldMaxLengthsEnforced()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();
        var (schoolId, _) = await CreateTestDataAsync(client);

        // Test Name field (10 char max)
        var request = new CreateClassGroupRequest
        {
            Name = "0123456789ABC", // 13 characters - exceeds XSD max of 10
            SchoolId = schoolId,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/class-groups", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("Name", problem!.Title);
    }

    #endregion
}
