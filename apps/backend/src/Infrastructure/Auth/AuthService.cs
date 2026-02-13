using Kcow.Application.Auth;
using Kcow.Application.Interfaces;
using Kcow.Domain;
using Kcow.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Kcow.Infrastructure.Auth;

/// <summary>
/// Implementation of authentication service using Dapper repositories.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly JwtService _jwtService;
    private readonly PasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        JwtService jwtService,
        PasswordHasher passwordHasher,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// Uses constant-time comparison to prevent timing attacks.
    /// </summary>
    public async Task<LoginResponse?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

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

            // Load role for the user
            var role = await _roleRepository.GetByIdAsync(user.RoleId, cancellationToken);
            var roleName = role?.Name ?? Constants.Roles.User;

            // Generate JWT token
            var token = _jwtService.GenerateToken(
                user.Id,
                user.Email,
                user.Name,
                roleName
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
                    Role = roleName
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
