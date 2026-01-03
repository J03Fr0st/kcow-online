namespace Kcow.Application.Auth;

/// <summary>
/// Response model for successful login.
/// </summary>
public class LoginResponse
{
    public required string Token { get; set; }
    public required UserDto User { get; set; }
}
