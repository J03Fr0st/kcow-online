namespace Kcow.Application.Auth;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="password">User's password</param>
    /// <returns>LoginResponse with JWT token and user info, or null if authentication fails</returns>
    Task<LoginResponse?> LoginAsync(string email, string password);
}
