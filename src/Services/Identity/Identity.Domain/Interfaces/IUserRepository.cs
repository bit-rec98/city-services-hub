using CityServicesHub.BuildingBlocks.Common.Interfaces;
using Identity.Domain.Entities;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Interfaces;

/// <summary>
/// Repositorio específico para la entidad User.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByDocumentNumberAsync(string documentNumber, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> DocumentNumberExistsAsync(string documentNumber, CancellationToken cancellationToken = default);
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
