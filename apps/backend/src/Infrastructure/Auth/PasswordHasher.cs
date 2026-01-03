namespace Kcow.Infrastructure.Auth;

/// <summary>
/// Service for hashing and verifying passwords.
/// </summary>
public class PasswordHasher
{
    private const int WorkFactor = 12;

    /// <summary>
    /// Hashes a password using BCrypt.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <returns>The hashed password.</returns>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: WorkFactor);
    }

    /// <summary>
    /// Verifies a password against a hash using BCrypt.
    /// </summary>
    /// <param name="password">The plain text password.</param>
    /// <param name="hash">The hashed password.</param>
    /// <returns>True if the password matches the hash, otherwise false.</returns>
    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
