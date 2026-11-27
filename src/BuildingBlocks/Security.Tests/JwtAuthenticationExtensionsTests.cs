using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CityServicesHub.BuildingBlocks.Security.Tests;

/// <summary>
/// Verifica el contrato de validación JWT compartido por todos los servicios.
/// </summary>
public class JwtAuthenticationExtensionsTests
{
    private const string Secret = "test-only-secret-key-with-at-least-32-bytes!";
    private const string Issuer = "TestIssuer";
    private const string Audience = "TestAudience";

    private static IConfiguration BuildConfiguration(
        string? secret = Secret, string? issuer = Issuer, string? audience = Audience)
    {
        var values = new Dictionary<string, string?>
        {
            ["JwtSettings:SecretKey"] = secret,
            ["JwtSettings:Issuer"] = issuer,
            ["JwtSettings:Audience"] = audience
        };

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static TokenValidationParameters GetValidationParameters(IConfiguration configuration)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCityServicesJwtAuthentication(configuration);
        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme)
            .TokenValidationParameters;
    }

    private static string CreateToken(
        string secret = Secret,
        string issuer = Issuer,
        string audience = Audience,
        string algorithm = SecurityAlgorithms.HmacSha256,
        DateTime? expires = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, algorithm);
        var token = new JwtSecurityToken(
            issuer,
            audience,
            new[] { new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()) },
            notBefore: DateTime.UtcNow.AddMinutes(-10),
            expires: expires ?? DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static bool IsValid(string token, TokenValidationParameters parameters)
    {
        try
        {
            new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
            return true;
        }
        catch (SecurityTokenException)
        {
            return false;
        }
    }

    [Fact]
    public void AddCityServicesJwtAuthentication_WithValidConfiguration_AcceptsCorrectlySignedToken()
    {
        var parameters = GetValidationParameters(BuildConfiguration());

        IsValid(CreateToken(), parameters).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddCityServicesJwtAuthentication_WithoutSecretKey_ThrowsAtStartup(string? secret)
    {
        var act = () => GetValidationParameters(BuildConfiguration(secret: secret));

        act.Should().Throw<InvalidOperationException>().WithMessage("*SecretKey*");
    }

    [Fact]
    public void AddCityServicesJwtAuthentication_WithShortSecretKey_ThrowsAtStartup()
    {
        var act = () => GetValidationParameters(BuildConfiguration(secret: "too-short"));

        act.Should().Throw<InvalidOperationException>().WithMessage("*32 bytes*");
    }

    [Fact]
    public void AddCityServicesJwtAuthentication_WithoutIssuer_ThrowsAtStartup()
    {
        var act = () => GetValidationParameters(BuildConfiguration(issuer: null));

        act.Should().Throw<InvalidOperationException>().WithMessage("*Issuer*");
    }

    [Fact]
    public void AddCityServicesJwtAuthentication_WithoutAudience_ThrowsAtStartup()
    {
        var act = () => GetValidationParameters(BuildConfiguration(audience: null));

        act.Should().Throw<InvalidOperationException>().WithMessage("*Audience*");
    }

    [Fact]
    public void Validation_RejectsTokenSignedWithDifferentKey()
    {
        var parameters = GetValidationParameters(BuildConfiguration());
        var token = CreateToken(secret: "another-secret-key-with-at-least-32-bytes!!");

        IsValid(token, parameters).Should().BeFalse();
    }

    [Fact]
    public void Validation_RejectsTokenWithWrongIssuer()
    {
        var parameters = GetValidationParameters(BuildConfiguration());

        IsValid(CreateToken(issuer: "OtherIssuer"), parameters).Should().BeFalse();
    }

    [Fact]
    public void Validation_RejectsTokenWithWrongAudience()
    {
        var parameters = GetValidationParameters(BuildConfiguration());

        IsValid(CreateToken(audience: "OtherAudience"), parameters).Should().BeFalse();
    }

    [Fact]
    public void Validation_RejectsExpiredToken()
    {
        var parameters = GetValidationParameters(BuildConfiguration());
        var token = CreateToken(expires: DateTime.UtcNow.AddMinutes(-1));

        IsValid(token, parameters).Should().BeFalse();
    }

    [Fact]
    public void Validation_RejectsTokenSignedWithUnexpectedAlgorithm()
    {
        // La misma clave (64+ bytes) firma el token, de modo que solo el algoritmo difiere.
        var longSecret = new string('k', 64);
        var parameters = GetValidationParameters(BuildConfiguration(secret: longSecret));
        var token = CreateToken(secret: longSecret, algorithm: SecurityAlgorithms.HmacSha512);

        IsValid(CreateToken(secret: longSecret), parameters).Should().BeTrue("la misma clave con HS256 sí es válida");

        IsValid(token, parameters).Should().BeFalse();
    }

    [Fact]
    public void Validation_RejectsUnsignedToken()
    {
        var parameters = GetValidationParameters(BuildConfiguration());
        var unsigned = new JwtSecurityToken(
            Issuer,
            Audience,
            new[] { new Claim(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()) },
            expires: DateTime.UtcNow.AddMinutes(5));

        IsValid(new JwtSecurityTokenHandler().WriteToken(unsigned), parameters).Should().BeFalse();
    }
}
