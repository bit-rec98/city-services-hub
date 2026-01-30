namespace CityServicesHub.BuildingBlocks.Common.Domain;

/// <summary>
/// Interface para raíces de agregados.
/// Los agregados son la unidad de consistencia transaccional.
/// </summary>
public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}

/// <summary>
/// Clase base para raíces de agregados.
/// </summary>
public abstract class AggregateRoot : AuditableEntity, IAggregateRoot
{
    private int _versionNumber;
    
    /// <summary>
    /// Número de versión para control de concurrencia optimista.
    /// </summary>
    public int VersionNumber 
    { 
        get => _versionNumber;
        private set => _versionNumber = value;
    }

    protected AggregateRoot() : base() { }
    protected AggregateRoot(Guid id) : base(id) { }

    public void IncrementVersion()
    {
        _versionNumber++;
    }
}
