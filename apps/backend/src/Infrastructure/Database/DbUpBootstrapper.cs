using DbUp;
using DbUp.Engine;
using DbUp.Engine.Output;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Database;

/// <summary>
/// Bootstrapper for configuring and running DbUp database migrations.
/// Migrations are SQL scripts located in the Migrations/Scripts directory.
/// </summary>
public class DbUpBootstrapper
{
    private readonly string _connectionString;
    private readonly ILogger<DbUpBootstrapper> _logger;
    private readonly string _scriptsPath;

    /// <summary>
    /// Initializes a new instance of the DbUpBootstrapper.
    /// </summary>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="logger">Logger for recording migration activity.</param>
    /// <param name="scriptsPath">Path to the migration scripts directory (relative to assembly).</param>
    public DbUpBootstrapper(
        string connectionString,
        ILogger<DbUpBootstrapper> logger,
        string scriptsPath = "Migrations/Scripts")
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _scriptsPath = scriptsPath;
    }

    /// <summary>
    /// Configures and returns a DbUp upgrader for the database.
    /// </summary>
    /// <returns>A configured DatabaseUpgradeResult upgrader.</returns>
    private UpgradeEngine GetUpgrader()
    {
        return DeployChanges.To
            .SQLiteDatabase(_connectionString)
            .WithScriptsFromFileSystem(_scriptsPath)
            .WithTransaction()
            .LogTo(new DbUpLogger(_logger))
            .Build();
    }

    /// <summary>
    /// Runs the database migrations to bring the schema up to date.
    /// </summary>
    /// <returns>True if migrations were successful, false otherwise.</returns>
    public bool RunMigrations()
    {
        _logger.LogInformation("Starting database migrations from path: {ScriptsPath}", _scriptsPath);

        var upgrader = GetUpgrader();
        var result = upgrader.PerformUpgrade();

        if (result.Successful)
        {
            if (result.Scripts.Any())
            {
                _logger.LogInformation("Database migrations completed successfully. {ScriptCount} scripts executed.", result.Scripts.Count());
                foreach (var script in result.Scripts)
                {
                    _logger.LogDebug("Executed migration script: {ScriptName}", script.Name);
                }
            }
            else
            {
                _logger.LogInformation("Database is up to date. No migrations needed.");
            }
            return true;
        }
        else
        {
            _logger.LogError(result.Error, "Database migrations failed.");
            return false;
        }
    }

    /// <summary>
    /// Checks if there are pending migrations to run.
    /// </summary>
    /// <returns>True if there are pending migrations, false otherwise.</returns>
    public bool HasPendingMigrations()
    {
        var upgrader = GetUpgrader();
        return upgrader.GetScriptsToExecute().Any();
    }

    /// <summary>
    /// Gets a list of scripts that would be executed.
    /// </summary>
    /// <returns>List of script names to be executed.</returns>
    public IEnumerable<string> GetPendingScripts()
    {
        var upgrader = GetUpgrader();
        return upgrader.GetScriptsToExecute().Select(s => s.Name);
    }

    /// <summary>
    /// Internal logger adapter for DbUp to use Microsoft.Extensions.Logging.
    /// </summary>
    private class DbUpLogger : IUpgradeLog
    {
        private readonly ILogger _logger;

        public DbUpLogger(ILogger logger)
        {
            _logger = logger;
        }

        public void WriteInformation(string format, params object[] args)
        {
            _logger.LogInformation(format, args);
        }

        public void WriteWarning(string format, params object[] args)
        {
            _logger.LogWarning(format, args);
        }

        public void WriteError(string format, params object[] args)
        {
            _logger.LogError(format, args);
        }

        public void WriteError(Exception ex, string format, params object[] args)
        {
            _logger.LogError(ex, format, args);
        }
    }
}
