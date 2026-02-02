namespace CityServicesHub.BuildingBlocks.Common.Interfaces;

/// <summary>
/// Patrón Unit of Work para gestión de transacciones.
/// Coordina la escritura de cambios de múltiples repositorios.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
