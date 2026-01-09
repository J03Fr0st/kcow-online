using Kcow.Application.Students;
using Kcow.Application.Common;
using Kcow.Application.Auth;
using Kcow.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace Kcow.Integration.Tests.Students;

/// <summary>
/// Integration tests for student management endpoints.
/// </summary>
public class StudentsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static bool _databaseInitialized = false;
    private static readonly object _lock = new object();

    public StudentsControllerTests(CustomWebApplicationFactory factory)
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

    private string GenerateUniqueReference(string prefix = "S")
    {
        int maxRandomLength = 10 - prefix.Length;
        if (maxRandomLength < 0) maxRandomLength = 0;
        string randomPart = Guid.NewGuid().ToString("N");
        if (randomPart.Length > maxRandomLength) randomPart = randomPart.Substring(0, maxRandomLength);
        
        return $"{prefix}{randomPart}".ToUpper();
    }

    [Fact]
    public async Task GetPaged_WithAuthentication_ReturnsPagedList()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/students?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResponse<StudentListDto>>();
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedStudent()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var reference = GenerateUniqueReference();
        var request = new CreateStudentRequest
        {
            Reference = reference,
            FirstName = "Integration",
            LastName = "Test",
            IsActive = true
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/students", request);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Create failed with status {response.StatusCode}. Response: {content}");
        }
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var student = await response.Content.ReadFromJsonAsync<StudentDto>();
        Assert.NotNull(student);
        Assert.Equal(reference, student.Reference);
        Assert.Equal("Integration", student.FirstName);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsStudent()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var reference = GenerateUniqueReference("GET");
        var request = new CreateStudentRequest
        {
            Reference = reference,
            FirstName = "GetBy",
            LastName = "Id"
        };

        var createResponse = await client.PostAsJsonAsync("/api/students", request);
        createResponse.EnsureSuccessStatusCode();
        var createdStudent = await createResponse.Content.ReadFromJsonAsync<StudentDto>();
        Assert.NotNull(createdStudent);

        // Act
        var response = await client.GetAsync($"/api/students/{createdStudent.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var student = await response.Content.ReadFromJsonAsync<StudentDto>();
        Assert.NotNull(student);
        Assert.Equal(createdStudent.Id, student.Id);
        Assert.Equal(reference, student.Reference);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedStudent()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var reference = GenerateUniqueReference("UPD");
        var createRequest = new CreateStudentRequest
        {
            Reference = reference,
            FirstName = "Original",
            LastName = "Name"
        };

        var createResponse = await client.PostAsJsonAsync("/api/students", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdStudent = await createResponse.Content.ReadFromJsonAsync<StudentDto>();
        Assert.NotNull(createdStudent);

        var updateRequest = new UpdateStudentRequest
        {
            Reference = reference,
            FirstName = "Updated",
            LastName = "Name",
            IsActive = true
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/students/{createdStudent.Id}", updateRequest);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Update failed with status {response.StatusCode}. Response: {content}");
        }
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var student = await response.Content.ReadFromJsonAsync<StudentDto>();
        Assert.NotNull(student);
        Assert.Equal("Updated", student.FirstName);
    }

    [Fact]
    public async Task Archive_WithValidId_ReturnsNoContent()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var reference = GenerateUniqueReference("ARC");
        var createRequest = new CreateStudentRequest
        {
            Reference = reference,
            FirstName = "To Archive",
            LastName = "Student"
        };

        var createResponse = await client.PostAsJsonAsync("/api/students", createRequest);
        createResponse.EnsureSuccessStatusCode();
        var createdStudent = await createResponse.Content.ReadFromJsonAsync<StudentDto>();
        Assert.NotNull(createdStudent);

        // Act
        var response = await client.DeleteAsync($"/api/students/{createdStudent.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify student is archived (IsActive should be false in GetById)
        var getResponse = await client.GetAsync($"/api/students/{createdStudent.Id}");
        var student = await getResponse.Content.ReadFromJsonAsync<StudentDto>();
        Assert.NotNull(student);
        Assert.False(student.IsActive);
    }
}
