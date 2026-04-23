using CityServicesHub.BuildingBlocks.Common.Application;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Identity.Domain.Interfaces;

namespace Identity.Application.Commands;

/// <summary>
/// Handler para el comando de registro de usuario.
/// </summary>
public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IVerificationCodeService _verificationCodeService;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IVerificationCodeService verificationCodeService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _verificationCodeService = verificationCodeService;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Verificar si el email ya existe
        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            return Result.Failure<UserDto>(new Error(
                "User.EmailExists", 
                "Ya existe un usuario registrado con este email."));
        }

        // Verificar si el documento ya existe
        if (await _userRepository.DocumentNumberExistsAsync(request.DocumentNumber, cancellationToken))
        {
            return Result.Failure<UserDto>(new Error(
                "User.DocumentExists", 
                "Ya existe un usuario registrado con este número de documento."));
        }

        // Crear el usuario
        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = User.Create(
            request.FirstName,
            request.LastName,
            request.Email,
            request.DocumentNumber,
            (DocumentType)request.DocumentType,
            passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);

        // Generar token de verificación de email
        var verificationToken = _verificationCodeService.GenerateToken();
        await _verificationCodeService.StoreCodeAsync(
            $"email_verification:{user.Id}", 
            verificationToken, 
            TimeSpan.FromHours(24), 
            cancellationToken);

        // Enviar email de verificación
        await _emailService.SendEmailVerificationAsync(user.Email.Value, verificationToken, cancellationToken);

        // Mapear a DTO
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
            CreatedAt = user.CreatedAt
        };

        return Result.Success(userDto);
    }
}
