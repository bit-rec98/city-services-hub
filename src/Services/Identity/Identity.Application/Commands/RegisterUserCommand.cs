using CityServicesHub.BuildingBlocks.Common.Application;
using Identity.Application.DTOs;

namespace Identity.Application.Commands;

/// <summary>
/// Comando para registrar un nuevo usuario.
/// </summary>
public record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string DocumentNumber,
    int DocumentType,
    string Password,
    string ConfirmPassword
) : ICommand<UserDto>;
