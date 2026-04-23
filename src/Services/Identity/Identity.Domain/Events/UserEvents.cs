using CityServicesHub.BuildingBlocks.Common.Domain;
using Identity.Domain.Enums;

namespace Identity.Domain.Events;

/// <summary>
/// Evento de dominio: Usuario creado.
/// </summary>
public record UserCreatedEvent(Guid UserId, string Email, string DocumentNumber) : DomainEvent
{
    public override string EventType => "UserCreated";
}

/// <summary>
/// Evento de dominio: Perfil de usuario actualizado.
/// </summary>
public record UserProfileUpdatedEvent(Guid UserId) : DomainEvent
{
    public override string EventType => "UserProfileUpdated";
}

/// <summary>
/// Evento de dominio: Email de usuario cambiado.
/// </summary>
public record UserEmailChangedEvent(Guid UserId, string NewEmail) : DomainEvent
{
    public override string EventType => "UserEmailChanged";
}

/// <summary>
/// Evento de dominio: Email verificado.
/// </summary>
public record UserEmailVerifiedEvent(Guid UserId) : DomainEvent
{
    public override string EventType => "UserEmailVerified";
}

/// <summary>
/// Evento de dominio: Contraseña cambiada.
/// </summary>
public record UserPasswordChangedEvent(Guid UserId) : DomainEvent
{
    public override string EventType => "UserPasswordChanged";
}

/// <summary>
/// Evento de dominio: Rol agregado al usuario.
/// </summary>
public record UserRoleAddedEvent(Guid UserId, UserRole Role) : DomainEvent
{
    public override string EventType => "UserRoleAdded";
}

/// <summary>
/// Evento de dominio: Usuario inició sesión.
/// </summary>
public record UserLoggedInEvent(Guid UserId) : DomainEvent
{
    public override string EventType => "UserLoggedIn";
}

/// <summary>
/// Evento de dominio: Usuario bloqueado por intentos fallidos.
/// </summary>
public record UserLockedOutEvent(Guid UserId, DateTime LockoutEnd) : DomainEvent
{
    public override string EventType => "UserLockedOut";
}

/// <summary>
/// Evento de dominio: Estado de usuario cambiado.
/// </summary>
public record UserStatusChangedEvent(Guid UserId, UserStatus NewStatus) : DomainEvent
{
    public override string EventType => "UserStatusChanged";
}
