using System.Linq.Expressions;

namespace CityServicesHub.BuildingBlocks.Common.Interfaces;

/// <summary>
/// Patrón Repository genérico para acceso a datos.
/// Abstrae la persistencia de entidades del dominio.
/// </summary>
public interface IRepository<TEntity, TId> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate, 
        CancellationToken cancellationToken = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null, 
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository con Id de tipo Guid por defecto.
/// </summary>
public interface IRepository<TEntity> : IRepository<TEntity, Guid> where TEntity : class
{
}
