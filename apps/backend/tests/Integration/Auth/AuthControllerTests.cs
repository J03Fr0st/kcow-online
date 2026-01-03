using Kcow.Application.Auth;
using Kcow.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace Kcow.Integration.Tests.Auth;

/// <summary>
/// Integration tests for authentication endpoints.
/// </summary>
public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static bool _databaseInitialized = false;
    private static readonly object _lock = new object();

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateHttpsClient()
    {
        // The API enables UseHttpsRedirection(). If the test client starts at http://,
        // the request may be redirected and the Authorization header can be dropped,
        // causing [Authorize] endpoints (e.g. /me, /logout) to return 401 intermittently.
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private void EnsureDatabaseInitialized()
    {
        if (!_databaseInitialized)
        {
            lock (_lock)
            {
                if (!_databaseInitialized)
                {
                    // Create a client first to ensure the server is started
                    using var tempClient = _factory.CreateClient();
                    using var scope = _factory.Services.CreateScope();
                    scope.ServiceProvider.InitializeDatabaseAsync().GetAwaiter().GetResult();
                    _databaseInitialized = true;
                }
            }
        }
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndUserInfo()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = _factory.CreateClient();
        var loginRequest = new LoginRequest
        {
            Email = "admin@kcow.local",
            Password = "Admin123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResponse);
        Assert.NotNull(loginResponse.Token);
        Assert.NotEmpty(loginResponse.Token);
        Assert.Equal("admin@kcow.local", loginResponse.User.Email);
        Assert.Equal("Administrator", loginResponse.User.Name);
        Assert.Equal("Admin", loginResponse.User.Role);
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ReturnsUnauthorized()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = _factory.CreateClient();
        var loginRequest = new LoginRequest
        {
            Email = "nonexistent@kcow.local",
            Password = "Admin123!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = _factory.CreateClient();
        var loginRequest = new LoginRequest
        {
            Email = "admin@kcow.local",
            Password = "WrongPassword!"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithValidToken_ReturnsUserInfo()
    {
        // Arrange - Login first to get token
        EnsureDatabaseInitialized();
        using var client = CreateHttpsClient();
        var loginRequest = new LoginRequest
        {
            Email = "admin@kcow.local",
            Password = "Admin123!"
        };

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult);

        // Add token to request
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // Act
        var response = await client.GetAsync("/api/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var userDto = await response.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(userDto);
        Assert.Equal("admin@kcow.local", userDto.Email);
        Assert.Equal("Administrator", userDto.Name);
        Assert.Equal("Admin", userDto.Role);
    }

    [Fact]
    public async Task Logout_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsync("/api/auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_WithValidToken_ReturnsOk()
    {
        // Arrange - Login first to get token
        EnsureDatabaseInitialized();
        using var client = CreateHttpsClient();
        var loginRequest = new LoginRequest
        {
            Email = "admin@kcow.local",
            Password = "Admin123!"
        };

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginResult);

        // Add token to request
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Token);

        // Act
        var response = await client.PostAsync("/api/auth/logout", null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
