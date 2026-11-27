using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CityServicesHub.IntegrationTests;

/// <summary>
/// Emite tokens con la clave de prueba, con el mismo formato que Identity (HS256, claims sub y rol),
/// para ejercitar la autenticación y la autorización reales de los servicios.
/// </summary>
public static class TestJwt
{
    public const string SecretKey = "integration-test-only-key-0123456789-0123456789";
    public const string Issuer = "CityServicesHub";
    public const string Audience = "CityServicesHub";

    public static string Create(Guid userId, params string[] roles)
    {
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, userId.ToString()) };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            Issuer, Audience, claims,
            notBefore: DateTime.UtcNow.AddMinutes(-1),
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
