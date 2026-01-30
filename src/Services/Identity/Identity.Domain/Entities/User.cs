using CityServicesHub.BuildingBlocks.Common.Domain;
using Identity.Domain.Enums;
using Identity.Domain.Events;
using Identity.Domain.ValueObjects;

namespace Identity.Domain.Entities;

/// <summary>
/// Entidad User - Raíz del agregado de identidad.
/// Representa un ciudadano o empleado del sistema.
/// </summary>
public class User : AggregateRoot
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public DocumentNumber DocumentNumber { get; private set; } = null!;
    public DocumentType DocumentType { get; private set; }
    public PhoneNumber? PhoneNumber { get; private set; }
    public Address? Address { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public UserStatus Status { get; private set; }
    public string PasswordHash { get; private set; } = null!;
    public bool EmailVerified { get; private set; }
    public bool PhoneVerified { get; private set; }
    public bool TwoFactorEnabled { get; private set; }
    public string? TwoFactorSecret { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    
    private readonly List<UserRole> _roles = new();
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();
    
    private readonly List<RefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    private User(
        Guid id,
        string firstName,
        string lastName,
        Email email,
        DocumentNumber documentNumber,
        DocumentType documentType,
        string passwordHash) : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        DocumentNumber = documentNumber;
        DocumentType = documentType;
        PasswordHash = passwordHash;
        Status = UserStatus.Pending;
        EmailVerified = false;
        PhoneVerified = false;
        TwoFactorEnabled = false;
        FailedLoginAttempts = 0;
        _roles.Add(UserRole.Citizen);
    }

    /// <summary>
    /// Factory method para crear un nuevo usuario.
    /// </summary>
    public static User Create(
        string firstName,
        string lastName,
        string email,
        string documentNumber,
        DocumentType documentType,
        string passwordHash)
    {
        var user = new User(
            Guid.NewGuid(),
            firstName,
            lastName,
            Email.Create(email),
            DocumentNumber.Create(documentNumber),
            documentType,
            passwordHash);

        user.AddDomainEvent(new UserCreatedEvent(user.Id, user.Email.Value, user.DocumentNumber.Value));
        
        return user;
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateProfile(
        string firstName, 
        string lastName, 
        PhoneNumber? phoneNumber, 
        Address? address, 
        DateTime? dateOfBirth)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Address = address;
        DateOfBirth = dateOfBirth;
        
        AddDomainEvent(new UserProfileUpdatedEvent(Id));
    }

    public void ChangeEmail(string newEmail)
    {
        var email = Email.Create(newEmail);
        Email = email;
        EmailVerified = false;
        
        AddDomainEvent(new UserEmailChangedEvent(Id, newEmail));
    }

    public void VerifyEmail()
    {
        EmailVerified = true;
        if (Status == UserStatus.Pending)
            Status = UserStatus.Active;
        
        AddDomainEvent(new UserEmailVerifiedEvent(Id));
    }

    public void VerifyPhone()
    {
        PhoneVerified = true;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        AddDomainEvent(new UserPasswordChangedEvent(Id));
    }

    public void EnableTwoFactor(string secret)
    {
        TwoFactorSecret = secret;
        TwoFactorEnabled = true;
    }

    public void DisableTwoFactor()
    {
        TwoFactorSecret = null;
        TwoFactorEnabled = false;
    }

    public void AddRole(UserRole role)
    {
        if (!_roles.Contains(role))
        {
            _roles.Add(role);
            AddDomainEvent(new UserRoleAddedEvent(Id, role));
        }
    }

    public void RemoveRole(UserRole role)
    {
        if (_roles.Contains(role) && role != UserRole.Citizen)
        {
            _roles.Remove(role);
        }
    }

    public bool HasRole(UserRole role) => _roles.Contains(role);

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockoutEnd = null;
        
        AddDomainEvent(new UserLoggedInEvent(Id));
    }

    public void RecordFailedLogin()
    {
        FailedLoginAttempts++;
        
        // Bloquear después de 5 intentos fallidos
        if (FailedLoginAttempts >= 5)
        {
            LockoutEnd = DateTime.UtcNow.AddMinutes(15);
            AddDomainEvent(new UserLockedOutEvent(Id, LockoutEnd.Value));
        }
    }

    public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;

    public void Activate()
    {
        Status = UserStatus.Active;
        AddDomainEvent(new UserStatusChangedEvent(Id, Status));
    }

    public void Suspend(string reason)
    {
        Status = UserStatus.Suspended;
        AddDomainEvent(new UserStatusChangedEvent(Id, Status));
    }

    public void Block(string reason)
    {
        Status = UserStatus.Blocked;
        AddDomainEvent(new UserStatusChangedEvent(Id, Status));
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
        AddDomainEvent(new UserStatusChangedEvent(Id, Status));
    }

    public RefreshToken AddRefreshToken(string token, DateTime expires)
    {
        var refreshToken = new RefreshToken(token, expires, Id);
        _refreshTokens.Add(refreshToken);
        
        // Limpiar tokens expirados
        var expiredTokens = _refreshTokens.Where(t => t.IsExpired || t.IsRevoked).ToList();
        foreach (var expiredToken in expiredTokens)
        {
            if (_refreshTokens.Count > 5) // Mantener al menos los últimos 5 tokens
                _refreshTokens.Remove(expiredToken);
        }
        
        return refreshToken;
    }

    public RefreshToken? GetActiveRefreshToken(string token)
    {
        return _refreshTokens.FirstOrDefault(t => t.Token == token && t.IsActive);
    }

    public void RevokeRefreshToken(string token, string reason = "Revoked by user")
    {
        var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
        refreshToken?.Revoke(reason);
    }

    public void RevokeAllRefreshTokens(string reason = "Revoked all tokens")
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke(reason);
        }
    }
}
