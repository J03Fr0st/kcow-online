using Dapper;
using Kcow.Application.Attendance;
using Kcow.Application.Interfaces;
using Kcow.Application.Students;
using Kcow.Infrastructure;
using Kcow.Infrastructure.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using Kcow.Application.Auth;

namespace Kcow.Integration.Tests.Attendance;

/// <summary>
/// Integration tests for attendance management endpoints.
/// </summary>
public class AttendanceControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static bool _databaseInitialized = false;
    private static readonly object _lock = new object();

    public AttendanceControllerTests(CustomWebApplicationFactory factory)
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

    private async Task<int> EnsureStudentExistsAsync(HttpClient client)
    {
        // Create a student to use in attendance tests
        var createStudentRequest = new CreateStudentRequest
        {
            Reference = $"AT{Guid.NewGuid():N}".Substring(0, 10),
            FirstName = "Attendance",
            LastName = "TestStudent"
        };

        var response = await client.PostAsJsonAsync("/api/students", createStudentRequest);
        response.EnsureSuccessStatusCode();
        var student = await response.Content.ReadFromJsonAsync<StudentDto>();
        Assert.NotNull(student);
        return student.Id;
    }

    private int EnsureClassGroupExistsViaDb()
    {
        // Insert class group directly via SQL to avoid TimeOnly Dapper mapping issue in ClassGroup API
        using var scope = _factory.Services.CreateScope();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        using var connection = connectionFactory.Create();

        // First create a school (include all NOT NULL columns)
        var schoolId = connection.QuerySingle<int>(
            @"INSERT INTO schools (name, is_active, print_invoice, import_flag, created_at)
              VALUES (@Name, 1, 0, 0, datetime('now')) RETURNING id",
            new { Name = $"AttSch{Guid.NewGuid():N}".Substring(0, 10) });

        // Then create a class group (include all NOT NULL columns)
        var classGroupId = connection.QuerySingle<int>(
            @"INSERT INTO class_groups (name, school_id, day_of_week, start_time, end_time, sequence, evaluate, is_active, import_flag, created_at)
              VALUES (@Name, @SchoolId, 1, '08:00', '09:00', 1, 0, 1, 0, datetime('now')) RETURNING id",
            new { Name = $"AG{Guid.NewGuid():N}".Substring(0, 10), SchoolId = schoolId });

        return classGroupId;
    }

    [Fact]
    public async Task GetAll_WithAuthentication_ReturnsAttendanceList()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/attendance");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var records = await response.Content.ReadFromJsonAsync<List<AttendanceDto>>();
        Assert.NotNull(records);
        Assert.IsType<List<AttendanceDto>>(records);
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = CreateHttpsClient();

        // Act
        var response = await client.GetAsync("/api/attendance");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAttendance()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var studentId = await EnsureStudentExistsAsync(client);
        var classGroupId = EnsureClassGroupExistsViaDb();

        var request = new CreateAttendanceRequest
        {
            StudentId = studentId,
            ClassGroupId = classGroupId,
            SessionDate = "2026-02-05",
            Status = "Present",
            Notes = "On time"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/attendance", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var record = await response.Content.ReadFromJsonAsync<AttendanceDto>();
        Assert.NotNull(record);
        Assert.Equal(studentId, record.StudentId);
        Assert.Equal(classGroupId, record.ClassGroupId);
        Assert.Equal("2026-02-05", record.SessionDate);
        Assert.Equal("Present", record.Status);
        Assert.Equal("On time", record.Notes);
        Assert.NotNull(record.StudentName);
        Assert.NotNull(record.ClassGroupName);
    }

    [Fact]
    public async Task Create_WithInvalidStatus_ReturnsBadRequest()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var studentId = await EnsureStudentExistsAsync(client);
        var classGroupId = EnsureClassGroupExistsViaDb();

        var request = new CreateAttendanceRequest
        {
            StudentId = studentId,
            ClassGroupId = classGroupId,
            SessionDate = "2026-02-05",
            Status = "InvalidStatus"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/attendance", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsAttendance()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var studentId = await EnsureStudentExistsAsync(client);
        var classGroupId = EnsureClassGroupExistsViaDb();

        var createRequest = new CreateAttendanceRequest
        {
            StudentId = studentId,
            ClassGroupId = classGroupId,
            SessionDate = "2026-02-04",
            Status = "Absent"
        };

        var createResponse = await client.PostAsJsonAsync("/api/attendance", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<AttendanceDto>();
        Assert.NotNull(created);

        // Act
        var response = await client.GetAsync($"/api/attendance/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var record = await response.Content.ReadFromJsonAsync<AttendanceDto>();
        Assert.NotNull(record);
        Assert.Equal(created.Id, record.Id);
        Assert.Equal("Absent", record.Status);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        // Act
        var response = await client.GetAsync("/api/attendance/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_WithValidData_ReturnsUpdatedAttendance()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var studentId = await EnsureStudentExistsAsync(client);
        var classGroupId = EnsureClassGroupExistsViaDb();

        var createRequest = new CreateAttendanceRequest
        {
            StudentId = studentId,
            ClassGroupId = classGroupId,
            SessionDate = "2026-02-03",
            Status = "Present"
        };

        var createResponse = await client.PostAsJsonAsync("/api/attendance", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<AttendanceDto>();
        Assert.NotNull(created);

        var updateRequest = new UpdateAttendanceRequest
        {
            Status = "Late",
            Notes = "Arrived 10 minutes late"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/attendance/{created.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var record = await response.Content.ReadFromJsonAsync<AttendanceDto>();
        Assert.NotNull(record);
        Assert.Equal("Late", record.Status);
        Assert.Equal("Arrived 10 minutes late", record.Notes);
        Assert.NotNull(record.ModifiedAt); // Audit trail triggered
    }

    [Fact]
    public async Task Update_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var updateRequest = new UpdateAttendanceRequest
        {
            Status = "Present"
        };

        // Act
        var response = await client.PutAsJsonAsync("/api/attendance/99999", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_SetsModifiedAt_ForAuditTrail()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var studentId = await EnsureStudentExistsAsync(client);
        var classGroupId = EnsureClassGroupExistsViaDb();

        var createRequest = new CreateAttendanceRequest
        {
            StudentId = studentId,
            ClassGroupId = classGroupId,
            SessionDate = "2026-02-02",
            Status = "Present"
        };

        var createResponse = await client.PostAsJsonAsync("/api/attendance", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<AttendanceDto>();
        Assert.NotNull(created);
        Assert.Null(created.ModifiedAt); // Not yet modified

        // Act - Update to trigger audit
        var updateRequest = new UpdateAttendanceRequest
        {
            Status = "Absent",
            Notes = "Student was actually absent"
        };

        var response = await client.PutAsJsonAsync($"/api/attendance/{created.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<AttendanceDto>();
        Assert.NotNull(updated);
        Assert.NotNull(updated.ModifiedAt); // ModifiedAt should be set
    }

    [Fact]
    public async Task GetAll_WithStudentIdFilter_ReturnsFilteredResults()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var studentId = await EnsureStudentExistsAsync(client);
        var classGroupId = EnsureClassGroupExistsViaDb();

        var request = new CreateAttendanceRequest
        {
            StudentId = studentId,
            ClassGroupId = classGroupId,
            SessionDate = "2026-02-01",
            Status = "Present"
        };

        await client.PostAsJsonAsync("/api/attendance", request);

        // Act
        var response = await client.GetAsync($"/api/attendance?studentId={studentId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var records = await response.Content.ReadFromJsonAsync<List<AttendanceDto>>();
        Assert.NotNull(records);
        Assert.All(records, r => Assert.Equal(studentId, r.StudentId));
    }

    [Fact]
    public async Task GetStudentAttendance_NestedRoute_ReturnsStudentHistory()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var studentId = await EnsureStudentExistsAsync(client);
        var classGroupId = EnsureClassGroupExistsViaDb();

        // Create some attendance records
        await client.PostAsJsonAsync("/api/attendance", new CreateAttendanceRequest
        {
            StudentId = studentId,
            ClassGroupId = classGroupId,
            SessionDate = "2026-01-15",
            Status = "Present"
        });

        await client.PostAsJsonAsync("/api/attendance", new CreateAttendanceRequest
        {
            StudentId = studentId,
            ClassGroupId = classGroupId,
            SessionDate = "2026-01-16",
            Status = "Late"
        });

        // Act - Use the nested route on StudentsController
        var response = await client.GetAsync($"/api/students/{studentId}/attendance");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var records = await response.Content.ReadFromJsonAsync<List<AttendanceDto>>();
        Assert.NotNull(records);
        Assert.True(records.Count >= 2);
        Assert.All(records, r => Assert.Equal(studentId, r.StudentId));
    }

    [Fact]
    public async Task Create_WithAllStatuses_HandlesCorrectly()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var studentId = await EnsureStudentExistsAsync(client);
        var classGroupId = EnsureClassGroupExistsViaDb();

        var statuses = new[] { "Present", "Absent", "Late" };
        var dateBase = "2026-01-";

        foreach (var (status, index) in statuses.Select((s, i) => (s, i)))
        {
            var request = new CreateAttendanceRequest
            {
                StudentId = studentId,
                ClassGroupId = classGroupId,
                SessionDate = $"{dateBase}{(20 + index):D2}",
                Status = status
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/attendance", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var record = await response.Content.ReadFromJsonAsync<AttendanceDto>();
            Assert.NotNull(record);
            Assert.Equal(status, record.Status);
        }
    }

    [Fact]
    public async Task BatchAttendance_WithValidData_ReturnsCreatedCounts()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var student1Id = await EnsureStudentExistsAsync(client);
        var student2Id = await EnsureStudentExistsAsync(client);
        var classGroupId = EnsureClassGroupExistsViaDb();

        var request = new BatchAttendanceRequest
        {
            ClassGroupId = classGroupId,
            SessionDate = "2026-02-09",
            Entries = new List<BatchAttendanceEntry>
            {
                new() { StudentId = student1Id, Status = "Present" },
                new() { StudentId = student2Id, Status = "Absent", Notes = "Sick" }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/class-groups/{classGroupId}/attendance", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BatchAttendanceResponse>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Created);
        Assert.Equal(0, result.Updated);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    public async Task BatchAttendance_UpdateExisting_ReturnsUpdatedCounts()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var studentId = await EnsureStudentExistsAsync(client);
        var classGroupId = EnsureClassGroupExistsViaDb();

        // Create initial attendance
        var initialRequest = new BatchAttendanceRequest
        {
            ClassGroupId = classGroupId,
            SessionDate = "2026-02-08",
            Entries = new List<BatchAttendanceEntry>
            {
                new() { StudentId = studentId, Status = "Present" }
            }
        };

        await client.PostAsJsonAsync($"/api/class-groups/{classGroupId}/attendance", initialRequest);

        // Update with different status
        var updateRequest = new BatchAttendanceRequest
        {
            ClassGroupId = classGroupId,
            SessionDate = "2026-02-08",
            Entries = new List<BatchAttendanceEntry>
            {
                new() { StudentId = studentId, Status = "Late", Notes = "Updated" }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/class-groups/{classGroupId}/attendance", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BatchAttendanceResponse>();
        Assert.NotNull(result);
        Assert.Equal(0, result.Created);
        Assert.Equal(1, result.Updated);
        Assert.Equal(0, result.Failed);
    }

    [Fact]
    public async Task BatchAttendance_WithMismatchedClassGroupId_ReturnsBadRequest()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = await CreateAuthenticatedClientAsync();

        var classGroupId = EnsureClassGroupExistsViaDb();

        var request = new BatchAttendanceRequest
        {
            ClassGroupId = 999, // Mismatched with URL
            SessionDate = "2026-02-09",
            Entries = new List<BatchAttendanceEntry>
            {
                new() { StudentId = 1, Status = "Present" }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync($"/api/class-groups/{classGroupId}/attendance", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BatchAttendance_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        EnsureDatabaseInitialized();
        using var client = CreateHttpsClient();

        var request = new BatchAttendanceRequest
        {
            ClassGroupId = 1,
            SessionDate = "2026-02-09",
            Entries = new List<BatchAttendanceEntry>
            {
                new() { StudentId = 1, Status = "Present" }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/class-groups/1/attendance", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
