using System.ComponentModel.DataAnnotations;

namespace Kcow.Application.Auth;

/// <summary>
/// Request model for user login.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User's email address for authentication.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(256, ErrorMessage = "Email is too long")]
    public required string Email { get; set; }

    /// <summary>
    /// User's password for authentication.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [MaxLength(128, ErrorMessage = "Password is too long")]
    public required string Password { get; set; }
}
