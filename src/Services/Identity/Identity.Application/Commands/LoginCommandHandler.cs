using CityServicesHub.BuildingBlocks.Common.Application;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces;

namespace Identity.Application.Commands;

/// <summary>
/// Handler para el comando de login.
/// </summary>
public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthResponseDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Buscar usuario por email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        
        if (user is null)
        {
            return Result.Failure<AuthResponseDto>(new Error(
                "Auth.InvalidCredentials", 
                "Credenciales inválidas."));
        }

        // Verificar si está bloqueado
        if (user.IsLockedOut)
        {
            return Result.Failure<AuthResponseDto>(new Error(
                "Auth.AccountLocked", 
                "La cuenta está temporalmente bloqueada. Intente nuevamente más tarde."));
        }

        // Verificar estado
        if (user.Status != UserStatus.Active && user.Status != UserStatus.Pending)
        {
            return Result.Failure<AuthResponseDto>(new Error(
                "Auth.AccountInactive", 
                "La cuenta no está activa."));
        }

        // Verificar contraseña
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _userRepository.UpdateAsync(user, cancellationToken);
            
            return Result.Failure<AuthResponseDto>(new Error(
                "Auth.InvalidCredentials", 
                "Credenciales inválidas."));
        }

        // TODO: Verificar 2FA si está habilitado
        if (user.TwoFactorEnabled && string.IsNullOrEmpty(request.TwoFactorCode))
        {
            return Result.Failure<AuthResponseDto>(new Error(
                "Auth.TwoFactorRequired", 
                "Se requiere el código de verificación de dos factores."));
        }

        // Generar tokens
        var accessToken = _tokenService.GenerateAccessToken(
            user.Id, 
            user.Email.Value, 
            user.Roles.Select(r => r.ToString()));
        
        var refreshToken = _tokenService.GenerateRefreshToken();
        var refreshTokenExpires = DateTime.UtcNow.AddDays(7);
        
        user.AddRefreshToken(refreshToken, refreshTokenExpires);
        user.RecordLogin();
        
        await _userRepository.UpdateAsync(user, cancellationToken);

        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email.Value,
            DocumentNumber = user.DocumentNumber.Value,
            DocumentType = user.DocumentType,
            Status = user.Status,
            EmailVerified = user.EmailVerified,
            TwoFactorEnabled = user.TwoFactorEnabled,
            Roles = user.Roles.Select(r => r.ToString()),
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };

        return Result.Success(new AuthResponseDto
        {
            User = userDto,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }
}
