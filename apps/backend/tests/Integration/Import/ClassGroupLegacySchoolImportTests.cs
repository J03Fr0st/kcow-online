using Dapper;
using Kcow.Application.Import;
using Kcow.Infrastructure;
using Kcow.Infrastructure.Database;
using Kcow.Infrastructure.Import;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Kcow.Integration.Tests.Import;

public class ClassGroupLegacySchoolImportTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ClassGroupLegacySchoolImportTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task ExecuteAsync_ResolvesLegacySchoolIdToGeneratedPrimaryKey()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.InitializeDatabaseAsync();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
        using var connection = connectionFactory.Create();
        var schoolId = await connection.ExecuteScalarAsync<long>(
            "INSERT INTO schools (name, legacy_id) VALUES ('Legacy school', '42'); SELECT last_insert_rowid();");
        Assert.NotEqual(42, schoolId);

        var parser = Substitute.For<ILegacyParser>();
        parser.ParseClassGroups(Arg.Any<string>(), Arg.Any<string>()).Returns(new ParseResult<LegacyClassGroupRecord>
        {
            Records = new List<LegacyClassGroupRecord>
            {
                new("CG-42", null, "Legacy group", "09:00", 42, "1", "08:00",
                    false, null, true, "1", null, null, null, null)
            }
        });

        var root = Path.Combine(Path.GetTempPath(), $"kcow-class-group-import-{Guid.NewGuid():N}");
        var folder = Path.Combine(root, "2_Class_Group");
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "Class Group.xml"), "");
        File.WriteAllText(Path.Combine(folder, "Class Group.xsd"), "");

        try
        {
            var importer = new ImportExecutionService(connectionFactory, parser);
            var result = await importer.ExecuteAsync(root);

            Assert.Equal(1, result.ClassGroups.Imported);
            Assert.Empty(result.Exceptions);
            var linkedSchoolId = await connection.QuerySingleAsync<long>(
                "SELECT school_id FROM class_groups WHERE legacy_id = 'CG-42'");
            Assert.Equal(schoolId, linkedSchoolId);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
