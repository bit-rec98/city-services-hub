namespace Identity.Application.Interfaces;

/// <summary>
/// Servicio para hashear y verificar contraseñas.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

/// <summary>
/// Servicio para generar y validar tokens JWT.
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(Guid userId, string email, IEnumerable<string> roles);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
}

/// <summary>
/// Servicio para envío de emails.
/// </summary>
public interface IEmailService
{
    Task SendEmailVerificationAsync(string email, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string email, string token, CancellationToken cancellationToken = default);
    Task SendWelcomeEmailAsync(string email, string firstName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Servicio para generación de códigos de verificación.
/// </summary>
public interface IVerificationCodeService
{
    string GenerateCode(int length = 6);
    string GenerateToken();
    Task<bool> StoreCodeAsync(string key, string code, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task<string?> GetCodeAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ValidateAndRemoveCodeAsync(string key, string code, CancellationToken cancellationToken = default);
}
