namespace CityServicesHub.BuildingBlocks.Common.Domain;

/// <summary>
/// Interface para entidades que requieren auditoría.
/// Rastrea creación, modificación y eliminación lógica.
/// </summary>
public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    string? CreatedBy { get; }
    DateTime? UpdatedAt { get; }
    string? UpdatedBy { get; }
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
    string? DeletedBy { get; }
}

/// <summary>
/// Implementación base de entidad auditable.
/// </summary>
public abstract class AuditableEntity : Entity, IAuditableEntity
{
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    protected AuditableEntity() : base() { }
    protected AuditableEntity(Guid id) : base(id) { }

    public void SetCreatedInfo(string? userId)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = userId;
    }

    public void SetUpdatedInfo(string? userId)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = userId;
    }

    public void SetDeletedInfo(string? userId)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = userId;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
