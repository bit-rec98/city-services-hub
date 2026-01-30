namespace CityServicesHub.BuildingBlocks.Common.Interfaces;

/// <summary>
/// Interface para acceder al usuario actual del contexto.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    string? Email { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    bool HasClaim(string claimType, string claimValue);
}
