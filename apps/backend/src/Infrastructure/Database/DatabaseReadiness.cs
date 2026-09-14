using Dapper;
using Kcow.Application.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Database;

/// <summary>Read-only, bounded schema probe. It never creates or migrates a database.</summary>
public sealed class DatabaseReadiness(string connectionString, string scriptsPath, ILogger<DatabaseReadiness> logger) : IDatabaseReadiness
{
    public async Task<bool> IsReadyAsync(CancellationToken cancellationToken)
    {
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(2));
        try
        {
            var expected = Directory.GetFiles(scriptsPath, "*.sql").Select(Path.GetFileName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (expected.Count == 0) return false;
            var settings = new SqliteConnectionStringBuilder(connectionString) { Mode = SqliteOpenMode.ReadOnly, DefaultTimeout = 2, Pooling = false };
            using var db = new SqliteConnection(settings.ConnectionString);
            await db.OpenAsync(deadline.Token);
            var applied = (await db.QueryAsync<string>(new CommandDefinition("SELECT ScriptName FROM SchemaVersions", commandTimeout: 2, cancellationToken: deadline.Token))).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (!expected.IsSubsetOf(applied)) return false;
            await db.ExecuteScalarAsync<int>(new CommandDefinition("SELECT COUNT(*) FROM billing_commands WHERE 1=0", commandTimeout: 2, cancellationToken: deadline.Token));
            return true;
        }
        catch (Exception ex) when (ex is SqliteException or IOException or UnauthorizedAccessException or OperationCanceledException)
        {
            logger.LogWarning("Database readiness unavailable ({FailureType})", ex.GetType().Name);
            return false;
        }
    }
}
