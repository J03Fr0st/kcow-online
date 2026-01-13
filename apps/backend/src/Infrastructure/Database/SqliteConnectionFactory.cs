using System.Data;
using Microsoft.Data.Sqlite;

namespace Kcow.Infrastructure.Database;

/// <summary>
/// SQLite implementation of IDbConnectionFactory.
/// Creates and manages SQLite database connections using Microsoft.Data.Sqlite.
/// </summary>
public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the SqliteConnectionFactory.
    /// </summary>
    /// <param name="connectionString">The SQLite connection string.</param>
    public SqliteConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <inheritdoc />
    public IDbConnection Create()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    /// <inheritdoc />
    public async Task<IDbConnection> CreateAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
