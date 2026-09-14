using System.Data;
using Microsoft.Extensions.Logging;
using Dapper;
using Kcow.Application.ClassGroups;
using Kcow.Application.Families;
using Kcow.Application.Import;
using Kcow.Application.Import.Mappers;
using Kcow.Domain.Entities;
using Kcow.Infrastructure.Database;
using Kcow.Infrastructure.Import;
using Kcow.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Kcow.Unit.Tests;

public sealed class ArchitectureContractTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"kcow-contracts-{Guid.NewGuid():N}");
    private readonly string _connectionString;
    private readonly CountingConnections _connections;
    private readonly string _scripts = Path.Combine(AppContext.BaseDirectory, "Migrations", "Scripts");

    public ArchitectureContractTests()
    {
        Directory.CreateDirectory(_directory);
        _connectionString = $"Data Source={Path.Combine(_directory, "test.db")};Pooling=False";
        _connections = new CountingConnections(new SqliteConnectionFactory(_connectionString));
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
    }

    private void Migrate() => Assert.True(new DbUpBootstrapper(_connectionString, NullLogger<DbUpBootstrapper>.Instance, _scripts).RunMigrations());

    [Fact]
    public async Task ReadinessRequiresACompleteSchemaAndDoesNotCreateFiles()
    {
        var ready = new DatabaseReadiness(_connectionString, _scripts, NullLogger<DatabaseReadiness>.Instance);
        Assert.False(await ready.IsReadyAsync(default));
        Assert.False(File.Exists(Path.Combine(_directory, "test.db")));
        Migrate();
        Assert.True(await ready.IsReadyAsync(default));
        using var db = _connections.Create(); db.Execute("DROP TABLE billing_commands");
        Assert.False(await ready.IsReadyAsync(default));
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task ListsUseBoundedQueriesAndPreserveRelatedData(int count)
    {
        Migrate();
        using (var db = _connections.Create())
        {
            db.Execute("INSERT INTO trucks(id,name,registration_number) VALUES(1,'Truck','REG'); INSERT INTO schools(id,name,short_name) VALUES(1,'School','SCH'); INSERT INTO students(id,reference) VALUES(1,'STUDENT')");
            using var tx = db.BeginTransaction();
            for (var i=1; i<=count; i++)
                db.Execute("""
                    INSERT INTO class_groups(id,name,school_id,truck_id,day_of_week,start_time,end_time) VALUES(@Id,'Group',1,1,1,'08:00:00','09:00:00');
                    INSERT INTO families(id,family_name,primary_contact_name) VALUES(@Id,'Family','Guardian');
                    INSERT INTO student_families(student_id,family_id,relationship_type) VALUES(1,@Id,'Parent')
                    """, new { Id = i }, tx);
            tx.Commit();
        }
        _connections.Count = 0;
        var queries = new ClassGroupQueries(_connections);
        var groups = await queries.ListAsync(1, 1, default);
        Assert.Equal(count, groups.Count); Assert.Equal(1, _connections.Count);
        Assert.All(groups, g => { Assert.Equal("School", g.School?.Name); Assert.Equal("REG", g.Truck?.RegistrationNumber); });
        Assert.Empty(await queries.ListAsync(2, null, default));
        Assert.Empty(await queries.ListAsync(null, 2, default));
        var service = new FamilyService(new FamilyRepository(_connections), new StudentRepository(_connections),
            new FamilyRelationships(_connections), NullLogger<FamilyService>.Instance);
        _connections.Count = 0;
        var families = await service.GetAllAsync();
        Assert.Equal(count, families.Count); Assert.Equal(2, _connections.Count);
        Assert.All(families, family => Assert.Equal(1, Assert.Single(family.Students).StudentId));
        using (var db = _connections.Create())
            db.Execute("INSERT INTO class_groups(id,name,day_of_week,start_time,end_time) VALUES(@Id,'No links',1,'07:00:00','08:00:00'); INSERT INTO class_groups(id,name,is_active,day_of_week,start_time,end_time) VALUES(@Inactive,'Hidden',0,1,'07:00:00','08:00:00')", new { Id = count + 1, Inactive = count + 2 });
        var unfiltered = await queries.ListAsync(null, null, default);
        Assert.Equal(count + 1, unfiltered.Count);
        Assert.Equal("No links", unfiltered[0].Name);
        Assert.Null(unfiltered[0].School); Assert.Null(unfiltered[0].Truck);

    }

    [Fact]
    public async Task ImportExecutesImmutablePlanAndReportsPartialConflict()
    {
        Migrate();
        var parser = Substitute.For<ILegacyParser>();
        var folder = Path.Combine(_directory, "3_Activity"); Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "Activity.xml"), "original");
        File.WriteAllText(Path.Combine(folder, "Activity.xsd"), "schema");
        var parsed = new ParseResult<LegacyActivityRecord> { Records = [new(1, "ONE", "Original", null, null, null, null)] };
        parser.ParseActivities(Arg.Any<string>(), Arg.Any<string>()).Returns(parsed);
        var plan = new ImportPlanBuilder(parser).Build(_directory);
        File.WriteAllText(Path.Combine(folder, "Activity.xml"), "changed after preview");
        plan.Activities.GetMapping().Data![0].Name = "mutated clone";
        var executor = new ImportExecutionService(_connections, parser);
        var first = await executor.ExecuteAsync(plan);
        Assert.Equal(1, first.TotalImported); Assert.Equal(plan.SourceFingerprint, first.SourceFingerprint);
        Assert.True(first.Activities.Committed);
        Assert.False(first.Schools.Committed);
        var rerun = await executor.ExecuteAsync(plan);
        Assert.Equal(1, rerun.TotalFailed); Assert.Equal(0, rerun.TotalImported);
        using var db = _connections.Create(); Assert.Equal("Original", db.QuerySingle<string>("SELECT name FROM activities"));
        parser.Received(1).ParseActivities(Arg.Any<string>(), Arg.Any<string>());
        parsed.Records.Add(new(2, "TWO", "Accepted", null, null, null, null));
        var mixedPlan = new ImportPlanBuilder(parser).Build(_directory);
        var mixed = await executor.ExecuteAsync(mixedPlan);
        Assert.Equal(1, mixed.TotalFailed); Assert.Equal(1, mixed.TotalImported);
        Assert.True(mixed.Activities.Committed);
        Assert.Equal("Accepted", db.QuerySingle<string>("SELECT name FROM activities WHERE id = 2"));
        using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => executor.ExecuteAsync(plan, cancelled.Token));
    }

    [Fact]
    public async Task CancellationAfterEntityCommitPreservesThatCommitAndStopsTheNextEntity()
    {
        Migrate();
        var mapping = new MappingResult<List<Activity>> { Success = true, Data = [new Activity { Id = 7, Name = "Committed", LegacyId = "7" }] };
        var plan = new ImportPlan(_directory, "test-snapshot", ConflictResolutionMode.FailOnConflict,
            new EntityPlan<School>(0, true, [], new() { Data = [] }),
            new EntityPlan<ClassGroup>(0, true, [], new() { Data = [] }),
            new EntityPlan<Activity>(1, false, [], mapping),
            new EntityPlan<StudentMappingData>(0, true, [], new() { Data = [] }));
        using var cancellation = new CancellationTokenSource();
        var logger = new OutcomeLogger(message =>
        {
            if (message.Contains("entity Activities committed=True")) cancellation.Cancel();
        });
        var executor = new ImportExecutionService(_connections, Substitute.For<ILegacyParser>(), logger);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => executor.ExecuteAsync(plan, cancellation.Token));
        using var db = _connections.Create();
        Assert.Equal("Committed", db.QuerySingle<string>("SELECT name FROM activities WHERE legacy_id='7'"));
    }

    private sealed class OutcomeLogger(Action<string> onMessage) : ILogger<ImportExecutionService>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel level) => true;
        public void Log<TState>(LogLevel level, EventId id, TState state, Exception? error, Func<TState, Exception?, string> formatter) => onMessage(formatter(state, error));
    }

    public void Dispose()
    {
        // Only this test's GUID-named directory is removed.
        var resolved = Path.GetFullPath(_directory);
        if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("kcow-contracts-"))
            throw new InvalidOperationException("Unexpected cleanup directory");
        Directory.Delete(resolved, true);
    }

    private sealed class CountingConnections(IDbConnectionFactory inner) : IDbConnectionFactory
    {
        public int Count { get; set; }
        public IDbConnection Create() { Count++; return inner.Create(); }
        public Task<IDbConnection> CreateAsync(CancellationToken ct = default) { Count++; return inner.CreateAsync(ct); }
    }
}
