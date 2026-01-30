using CityServicesHub.BuildingBlocks.Common.Application;
using Identity.Application.DTOs;

namespace Identity.Application.Commands;

/// <summary>
/// Comando para iniciar sesión.
/// </summary>
public record LoginCommand(
    string Email,
    string Password,
    string? TwoFactorCode = null
) : ICommand<AuthResponseDto>;
