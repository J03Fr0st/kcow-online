using System.Data;

namespace Kcow.Infrastructure.Database;

/// <summary>
/// Factory for creating database connections.
/// Connections created by this factory should be disposed after use.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates a new open database connection.
    /// </summary>
    /// <returns>An open IDatabase connection.</returns>
    IDbConnection Create();

    /// <summary>
    /// Creates a new open database connection asynchronously.
    /// </summary>
    /// <returns>A task representing the async operation with an open IDatabase connection.</returns>
    Task<IDbConnection> CreateAsync(CancellationToken cancellationToken = default);
}
