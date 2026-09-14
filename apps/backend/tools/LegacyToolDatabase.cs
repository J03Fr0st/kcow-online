using Dapper;
using Kcow.Infrastructure.Database;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

/// <summary>Shared database setup for retained legacy command-line adapters.</summary>
internal static class LegacyToolDatabase
{
    public static SqliteConnectionFactory Open(bool migrate, string fallback = "Data Source=kcow.db")
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());
        var settings = new SqliteConnectionStringBuilder(
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ?? fallback)
        {
            Pooling = false,
            Mode = migrate ? SqliteOpenMode.ReadWriteCreate : SqliteOpenMode.ReadOnly
        };
        if (migrate)
        {
            var scripts = Path.Combine(AppContext.BaseDirectory, "Migrations", "Scripts");
            if (!new DbUpBootstrapper(settings.ConnectionString, NullLogger<DbUpBootstrapper>.Instance, scripts).RunMigrations())
                throw new InvalidOperationException("Database migration failed; import was not started.");
        }
        return new SqliteConnectionFactory(settings.ConnectionString);
    }
}
