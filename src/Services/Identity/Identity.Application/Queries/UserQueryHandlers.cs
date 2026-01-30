using CityServicesHub.BuildingBlocks.Common.Application;
using Identity.Application.DTOs;
using Identity.Domain.Interfaces;

namespace Identity.Application.Queries;

/// <summary>
/// Handler para obtener un usuario por ID.
/// </summary>
public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user is null)
        {
            return Result.Failure<UserDto>(new Error(
                "User.NotFound", 
                "Usuario no encontrado."));
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email.Value,
            DocumentNumber = user.DocumentNumber.Value,
            DocumentType = user.DocumentType,
            PhoneNumber = user.PhoneNumber?.ToString(),
            Status = user.Status,
            EmailVerified = user.EmailVerified,
            TwoFactorEnabled = user.TwoFactorEnabled,
            Roles = user.Roles.Select(r => r.ToString()),
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };

        return Result.Success(userDto);
    }
}

/// <summary>
/// Handler para obtener el usuario actual.
/// </summary>
public class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        
        if (user is null)
        {
            return Result.Failure<UserDto>(new Error(
                "User.NotFound", 
                "Usuario no encontrado."));
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email.Value,
            DocumentNumber = user.DocumentNumber.Value,
            DocumentType = user.DocumentType,
            PhoneNumber = user.PhoneNumber?.ToString(),
            Status = user.Status,
            EmailVerified = user.EmailVerified,
            TwoFactorEnabled = user.TwoFactorEnabled,
            Roles = user.Roles.Select(r => r.ToString()),
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        };

        return Result.Success(userDto);
    }
}
