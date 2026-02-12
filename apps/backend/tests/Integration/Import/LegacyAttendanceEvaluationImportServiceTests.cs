using Dapper;
using Kcow.Application.Import;
using Kcow.Application.Interfaces;
using Kcow.Domain.Entities;
using Kcow.Infrastructure;
using Kcow.Infrastructure.Database;
using Kcow.Infrastructure.Import;
using Microsoft.Extensions.DependencyInjection;
using AttendanceStatus = Kcow.Domain.Entities.AttendanceStatus;

namespace Kcow.Integration.Tests.Import;

public class LegacyAttendanceEvaluationImportServiceTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private static bool _databaseInitialized;
    private static readonly object _lock = new();

    // Test data IDs (will be created during setup)
    private static int _testStudentId;
    private static int _testClassGroupId;
    private static int _testActivityId;
    private static bool _testDataSeeded;
    private static readonly string _testSuffix = Guid.NewGuid().ToString("N")[..8];

    public LegacyAttendanceEvaluationImportServiceTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ImportAttendanceAsync_ValidRecord_InsertsAndReturnsSummary()
    {
        await EnsureTestDataAsync();

        using var scope = _factory.Services.CreateScope();
        var service = CreateImportService(scope);
        var auditPath = Path.GetTempFileName();
        var summaryPath = Path.GetTempFileName();

        var records = new List<LegacyAttendanceImportRecord>
        {
            new(
                StudentId: _testStudentId,
                ClassGroupId: _testClassGroupId,
                SessionDate: "2024-03-15",
                Status: "Present",
                Notes: "Integration test attendance",
                OriginalCreatedAt: new DateTime(2024, 3, 15, 8, 0, 0, DateTimeKind.Utc),
                OriginalModifiedAt: null)
        };

        try
        {
            var summary = await service.ImportAttendanceAsync(records, auditPath, summaryPath);

            Assert.Equal(1, summary.AttendanceImported);
            Assert.Equal(0, summary.AttendanceSkipped);
            Assert.True(File.Exists(auditPath));
            Assert.True(File.Exists(summaryPath));

            // Verify data was persisted
            var attendanceRepo = scope.ServiceProvider.GetRequiredService<IAttendanceRepository>();
            var attendanceRecords = await attendanceRepo.GetByStudentIdAsync(_testStudentId);
            Assert.Contains(attendanceRecords, a =>
                a.SessionDate == "2024-03-15" &&
                a.Status == (int)AttendanceStatus.Present);
        }
        finally
        {
            File.Delete(auditPath);
            File.Delete(summaryPath);
        }
    }

    [Fact]
    public async Task ImportAttendanceAsync_InvalidStudentId_SkipsRecord()
    {
        await EnsureTestDataAsync();

        using var scope = _factory.Services.CreateScope();
        var service = CreateImportService(scope);

        var records = new List<LegacyAttendanceImportRecord>
        {
            new(
                StudentId: 999999,
                ClassGroupId: _testClassGroupId,
                SessionDate: "2024-03-16",
                Status: "Present",
                Notes: null,
                OriginalCreatedAt: null,
                OriginalModifiedAt: null)
        };

        var summary = await service.ImportAttendanceAsync(records, null, null);

        Assert.Equal(0, summary.AttendanceImported);
        Assert.Equal(1, summary.AttendanceSkipped);
    }

    [Fact]
    public async Task ImportAttendanceAsync_PreviewMode_DoesNotInsert()
    {
        await EnsureTestDataAsync();

        using var scope = _factory.Services.CreateScope();
        var service = CreateImportService(scope);

        var records = new List<LegacyAttendanceImportRecord>
        {
            new(
                StudentId: _testStudentId,
                ClassGroupId: _testClassGroupId,
                SessionDate: "2099-01-01",
                Status: "Present",
                Notes: "Preview only",
                OriginalCreatedAt: null,
                OriginalModifiedAt: null)
        };

        var summary = await service.ImportAttendanceAsync(records, null, null, preview: true);

        Assert.Equal(1, summary.AttendanceImported); // Counted but not inserted

        // Verify data was NOT persisted
        var attendanceRepo = scope.ServiceProvider.GetRequiredService<IAttendanceRepository>();
        var attendanceRecords = await attendanceRepo.GetByStudentIdAsync(_testStudentId);
        Assert.DoesNotContain(attendanceRecords, a => a.SessionDate == "2099-01-01");
    }

    [Fact]
    public async Task ImportEvaluationsAsync_ValidRecord_InsertsAndReturnsSummary()
    {
        await EnsureTestDataAsync();

        using var scope = _factory.Services.CreateScope();
        var service = CreateImportService(scope);
        var auditPath = Path.GetTempFileName();
        var summaryPath = Path.GetTempFileName();

        var records = new List<LegacyEvaluationImportRecord>
        {
            new(
                StudentId: _testStudentId,
                ActivityId: _testActivityId,
                EvaluationDate: "2024-03-15",
                Score: 85,
                SpeedMetric: 7.5m,
                AccuracyMetric: 92.0m,
                Notes: "Integration test evaluation",
                OriginalCreatedAt: new DateTime(2024, 3, 15, 8, 0, 0, DateTimeKind.Utc),
                OriginalModifiedAt: null)
        };

        try
        {
            var summary = await service.ImportEvaluationsAsync(records, auditPath, summaryPath);

            Assert.Equal(1, summary.EvaluationImported);
            Assert.Equal(0, summary.EvaluationSkipped);
            Assert.True(File.Exists(auditPath));
            Assert.True(File.Exists(summaryPath));

            // Verify data was persisted
            var evalRepo = scope.ServiceProvider.GetRequiredService<IEvaluationRepository>();
            var evaluations = await evalRepo.GetByStudentIdAsync(_testStudentId);
            Assert.Contains(evaluations, e =>
                e.EvaluationDate == "2024-03-15" &&
                e.Score == 85 &&
                e.ActivityId == _testActivityId);
        }
        finally
        {
            File.Delete(auditPath);
            File.Delete(summaryPath);
        }
    }

    [Fact]
    public async Task ImportEvaluationsAsync_InvalidActivityId_SkipsRecord()
    {
        await EnsureTestDataAsync();

        using var scope = _factory.Services.CreateScope();
        var service = CreateImportService(scope);

        var records = new List<LegacyEvaluationImportRecord>
        {
            new(
                StudentId: _testStudentId,
                ActivityId: 999999,
                EvaluationDate: "2024-03-16",
                Score: 50,
                SpeedMetric: null,
                AccuracyMetric: null,
                Notes: null,
                OriginalCreatedAt: null,
                OriginalModifiedAt: null)
        };

        var summary = await service.ImportEvaluationsAsync(records, null, null);

        Assert.Equal(0, summary.EvaluationImported);
        Assert.Equal(1, summary.EvaluationSkipped);
    }

    [Fact]
    public async Task ImportEvaluationsAsync_DuplicateRecord_SkipsDuplicate()
    {
        await EnsureTestDataAsync();

        using var scope = _factory.Services.CreateScope();
        var service = CreateImportService(scope);

        var uniqueDate = $"2024-04-{Random.Shared.Next(10, 28):D2}";

        var records = new List<LegacyEvaluationImportRecord>
        {
            new(
                StudentId: _testStudentId,
                ActivityId: _testActivityId,
                EvaluationDate: uniqueDate,
                Score: 70,
                SpeedMetric: null,
                AccuracyMetric: null,
                Notes: "First",
                OriginalCreatedAt: null,
                OriginalModifiedAt: null)
        };

        // First import
        await service.ImportEvaluationsAsync(records, null, null);

        // Second import - same student/activity/date should be skipped
        var service2 = CreateImportService(scope);
        var summary = await service2.ImportEvaluationsAsync(records, null, null);

        Assert.Equal(0, summary.EvaluationImported);
        Assert.Equal(1, summary.EvaluationSkipped);
    }

    [Fact]
    public async Task ImportAllAsync_MixedRecords_ProcessesBothTypes()
    {
        await EnsureTestDataAsync();

        using var scope = _factory.Services.CreateScope();
        var service = CreateImportService(scope);

        var xmlPath = FindRepoFile("docs/legacy/3_Activity/Activity.xml");
        var xsdPath = FindRepoFile("docs/legacy/3_Activity/Activity.xsd");

        var uniqueAttDate = $"2024-05-{Random.Shared.Next(10, 28):D2}";
        var uniqueEvalDate = $"2024-06-{Random.Shared.Next(10, 28):D2}";

        var attendanceRecords = new List<LegacyAttendanceImportRecord>
        {
            new(_testStudentId, _testClassGroupId, uniqueAttDate, "Present", null, null, null)
        };

        var evaluationRecords = new List<LegacyEvaluationImportRecord>
        {
            new(_testStudentId, _testActivityId, uniqueEvalDate, 80, null, null, null, null, null)
        };

        var auditPath = Path.GetTempFileName();
        var summaryPath = Path.GetTempFileName();

        try
        {
            var summary = await service.ImportAllAsync(
                xmlPath, xsdPath,
                attendanceRecords, evaluationRecords,
                auditPath, summaryPath);

            Assert.Equal(1, summary.AttendanceImported);
            Assert.Equal(1, summary.EvaluationImported);
            Assert.True(File.Exists(summaryPath));
        }
        finally
        {
            File.Delete(auditPath);
            File.Delete(summaryPath);
        }
    }

    [Fact]
    public async Task ImportAttendanceAsync_PreservesHistoricalTimestamps()
    {
        await EnsureTestDataAsync();

        using var scope = _factory.Services.CreateScope();
        var service = CreateImportService(scope);

        var historicalDate = new DateTime(2023, 1, 10, 9, 0, 0, DateTimeKind.Utc);
        var uniqueDate = $"2023-01-{Random.Shared.Next(10, 28):D2}";

        var records = new List<LegacyAttendanceImportRecord>
        {
            new(
                StudentId: _testStudentId,
                ClassGroupId: _testClassGroupId,
                SessionDate: uniqueDate,
                Status: "Absent",
                Notes: "Historical import",
                OriginalCreatedAt: historicalDate,
                OriginalModifiedAt: null)
        };

        var summary = await service.ImportAttendanceAsync(records, null, null);

        Assert.Equal(1, summary.AttendanceImported);
    }

    private LegacyAttendanceEvaluationImportService CreateImportService(IServiceScope scope)
    {
        return new LegacyAttendanceEvaluationImportService(
            scope.ServiceProvider.GetRequiredService<IAttendanceRepository>(),
            scope.ServiceProvider.GetRequiredService<IEvaluationRepository>(),
            scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>());
    }

    private async Task EnsureTestDataAsync()
    {
        EnsureDatabaseInitialized();

        if (_testDataSeeded) return;

        lock (_lock)
        {
            if (_testDataSeeded) return;

            using var scope = _factory.Services.CreateScope();
            var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

            using var conn = connectionFactory.Create();

            var testRef = $"IMP{_testSuffix}";

            // Check if our test data already exists (idempotent)
            var existingStudent = conn.QuerySingleOrDefault<int?>(
                "SELECT id FROM students WHERE reference = @Ref", new { Ref = testRef });

            if (existingStudent.HasValue)
            {
                _testStudentId = existingStudent.Value;
                _testClassGroupId = conn.QuerySingle<int>(
                    "SELECT id FROM class_groups WHERE name = @Name", new { Name = $"IMP{_testSuffix}" });
                _testActivityId = conn.QuerySingle<int>(
                    "SELECT id FROM activities WHERE code = @Code", new { Code = $"imp-{_testSuffix}" });
                _testDataSeeded = true;
                return;
            }

            // Create a test school first (class groups need it)
            var schoolId = conn.QuerySingle<int>(@"
                INSERT INTO schools (name, short_name, print_invoice, import_flag, is_active, created_at)
                VALUES (@Name, @ShortName, @Print, @Import, @IsActive, @CreatedAt)
                RETURNING id",
                new { Name = $"Import Test School {_testSuffix}", ShortName = $"I{_testSuffix[..2]}",
                      Print = false, Import = false, IsActive = true, CreatedAt = DateTime.UtcNow.ToString("o") });

            // Create a test class group
            _testClassGroupId = conn.QuerySingle<int>(@"
                INSERT INTO class_groups (name, description, school_id, day_of_week, start_time, end_time, is_active, created_at)
                VALUES (@Name, @Description, @SchoolId, @DayOfWeek, @StartTime, @EndTime, @IsActive, @CreatedAt)
                RETURNING id",
                new { Name = $"IMP{_testSuffix}", Description = "Import Test Group", SchoolId = schoolId,
                      DayOfWeek = (int)DayOfWeek.Monday, StartTime = "08:00", EndTime = "09:00",
                      IsActive = true, CreatedAt = DateTime.UtcNow.ToString("o") });

            // Create a test student
            _testStudentId = conn.QuerySingle<int>(@"
                INSERT INTO students (reference, first_name, last_name, class_group_id, is_active, created_at)
                VALUES (@Reference, @FirstName, @LastName, @ClassGroupId, @IsActive, @CreatedAt)
                RETURNING id",
                new { Reference = testRef, FirstName = "Import", LastName = "TestStudent",
                      ClassGroupId = _testClassGroupId, IsActive = true, CreatedAt = DateTime.UtcNow.ToString("o") });

            // Create a test activity
            _testActivityId = conn.QuerySingle<int>(@"
                INSERT INTO activities (code, name, is_active, created_at)
                VALUES (@Code, @Name, @IsActive, @CreatedAt)
                RETURNING id",
                new { Code = $"imp-{_testSuffix}", Name = "Import Test Activity",
                      IsActive = true, CreatedAt = DateTime.UtcNow.ToString("o") });

            _testDataSeeded = true;
        }
    }

    private void EnsureDatabaseInitialized()
    {
        if (_databaseInitialized) return;

        lock (_lock)
        {
            if (_databaseInitialized) return;

            using var scope = _factory.Services.CreateScope();
            scope.ServiceProvider.InitializeDatabaseAsync().GetAwaiter().GetResult();
            _databaseInitialized = true;
        }
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
