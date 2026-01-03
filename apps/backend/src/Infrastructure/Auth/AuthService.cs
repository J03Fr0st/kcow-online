using Kcow.Application.Auth;
using Kcow.Domain;
using Kcow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Auth;

/// <summary>
/// Implementation of authentication service.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;
    private readonly PasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AppDbContext context, JwtService jwtService, PasswordHasher passwordHasher, ILogger<AuthService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// Uses constant-time comparison to prevent timing attacks.
    /// </summary>
    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        try
        {
            // Find user by email with role
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            // Timing attack prevention: Always verify password hash, even for non-existent users
            // Use a dummy hash for non-existent users to maintain consistent timing
            var passwordHash = user?.PasswordHash ?? "$2a$12$dummy.hash.for.timing.attack.prevention";

            // Verify password (always runs BCrypt, even if user doesn't exist)
            var passwordValid = _passwordHasher.VerifyPassword(password, passwordHash);

            if (user == null || !passwordValid)
            {
                // Generic log message prevents user enumeration
                _logger.LogWarning("Login attempt failed: Invalid credentials");
                return null;
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(
                user.Id,
                user.Email,
                user.Name,
                user.Role?.Name ?? Constants.Roles.User
            );

            _logger.LogInformation("User {Email} logged in successfully", email);

            return new LoginResponse
            {
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Name = user.Name,
                    Role = user.Role?.Name ?? Constants.Roles.User
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email {Email}", email);
            throw;
        }
    }
}
