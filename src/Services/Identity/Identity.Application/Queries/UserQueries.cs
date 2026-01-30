using CityServicesHub.BuildingBlocks.Common.Application;
using Identity.Application.DTOs;

namespace Identity.Application.Queries;

/// <summary>
/// Query para obtener un usuario por ID.
/// </summary>
public record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;

/// <summary>
/// Query para obtener el perfil del usuario actual.
/// </summary>
public record GetCurrentUserQuery(Guid UserId) : IQuery<UserDto>;

/// <summary>
/// Query para listar usuarios con paginación.
/// </summary>
public record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    int? Status = null
) : IQuery<PagedResult<UserDto>>;
