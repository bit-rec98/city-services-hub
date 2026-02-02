using Identity.Domain.Enums;

namespace Identity.Application.DTOs;

/// <summary>
/// DTO para datos de usuario.
/// </summary>
public record UserDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string DocumentNumber { get; init; } = null!;
    public DocumentType DocumentType { get; init; }
    public string? PhoneNumber { get; init; }
    public UserStatus Status { get; init; }
    public bool EmailVerified { get; init; }
    public bool TwoFactorEnabled { get; init; }
    public IEnumerable<string> Roles { get; init; } = Enumerable.Empty<string>();
    public DateTime? LastLoginAt { get; init; }
    public DateTime CreatedAt { get; init; }
}

/// <summary>
/// DTO para respuesta de autenticación.
/// </summary>
public record AuthResponseDto
{
    public UserDto User { get; init; } = null!;
    public string AccessToken { get; init; } = null!;
    public string RefreshToken { get; init; } = null!;
    public DateTime ExpiresAt { get; init; }
}

/// <summary>
/// DTO para registro de usuario.
/// </summary>
public record RegisterUserDto
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string DocumentNumber { get; init; } = null!;
    public DocumentType DocumentType { get; init; }
    public string Password { get; init; } = null!;
    public string ConfirmPassword { get; init; } = null!;
}

/// <summary>
/// DTO para login de usuario.
/// </summary>
public record LoginDto
{
    public string Email { get; init; } = null!;
    public string Password { get; init; } = null!;
    public string? TwoFactorCode { get; init; }
}

/// <summary>
/// DTO para actualización de perfil.
/// </summary>
public record UpdateProfileDto
{
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string? PhoneNumber { get; init; }
    public AddressDto? Address { get; init; }
    public DateTime? DateOfBirth { get; init; }
}

/// <summary>
/// DTO para dirección.
/// </summary>
public record AddressDto
{
    public string Street { get; init; } = null!;
    public string Number { get; init; } = null!;
    public string? Floor { get; init; }
    public string? Apartment { get; init; }
    public string City { get; init; } = null!;
    public string Province { get; init; } = null!;
    public string PostalCode { get; init; } = null!;
}

/// <summary>
/// DTO para cambio de contraseña.
/// </summary>
public record ChangePasswordDto
{
    public string CurrentPassword { get; init; } = null!;
    public string NewPassword { get; init; } = null!;
    public string ConfirmNewPassword { get; init; } = null!;
}

/// <summary>
/// DTO para solicitud de reset de contraseña.
/// </summary>
public record ForgotPasswordDto
{
    public string Email { get; init; } = null!;
}

/// <summary>
/// DTO para reset de contraseña.
/// </summary>
public record ResetPasswordDto
{
    public string Token { get; init; } = null!;
    public string NewPassword { get; init; } = null!;
    public string ConfirmNewPassword { get; init; } = null!;
}

/// <summary>
/// DTO para refresh token.
/// </summary>
public record RefreshTokenDto
{
    public string RefreshToken { get; init; } = null!;
}
