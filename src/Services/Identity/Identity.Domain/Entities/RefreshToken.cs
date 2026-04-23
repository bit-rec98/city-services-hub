using CityServicesHub.BuildingBlocks.Common.Domain;

namespace Identity.Domain.Entities;

/// <summary>
/// Entidad que representa un refresh token para JWT.
/// </summary>
public class RefreshToken : Entity
{
    public string Token { get; private set; } = null!;
    public DateTime Expires { get; private set; }
    public DateTime Created { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTime? Revoked { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? ReasonRevoked { get; private set; }
    public Guid UserId { get; private set; }

    private RefreshToken() { }

    public RefreshToken(string token, DateTime expires, Guid userId, string? createdByIp = null)
        : base(Guid.NewGuid())
    {
        Token = token;
        Expires = expires;
        Created = DateTime.UtcNow;
        CreatedByIp = createdByIp;
        UserId = userId;
    }

    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsRevoked => Revoked != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke(string reason, string? revokedByIp = null, string? replacedByToken = null)
    {
        Revoked = DateTime.UtcNow;
        RevokedByIp = revokedByIp;
        ReasonRevoked = reason;
        ReplacedByToken = replacedByToken;
    }
}
