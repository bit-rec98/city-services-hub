using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CityServicesHub.BuildingBlocks.Security.Tests;

public class CorsExtensionsTests
{
    private static CorsPolicy BuildPolicy(params string[] origins)
    {
        var values = origins
            .Select((origin, index) => KeyValuePair.Create<string, string?>($"Cors:AllowedOrigins:{index}", origin));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();

        var services = new ServiceCollection();
        services.AddCityServicesCors(configuration);
        using var provider = services.BuildServiceProvider();

        return provider.GetRequiredService<IOptions<CorsOptions>>().Value.GetPolicy(CorsExtensions.PolicyName)!;
    }

    [Fact]
    public void AddCityServicesCors_WithConfiguredOrigins_AllowsOnlyThoseOrigins()
    {
        var policy = BuildPolicy("http://localhost:5173");

        policy.Origins.Should().BeEquivalentTo("http://localhost:5173");
        policy.AllowAnyOrigin.Should().BeFalse();
    }

    [Fact]
    public void AddCityServicesCors_WithoutConfiguredOrigins_AllowsNoOrigin()
    {
        var policy = BuildPolicy();

        policy.Origins.Should().BeEmpty();
        policy.AllowAnyOrigin.Should().BeFalse();
    }

    [Fact]
    public void AddCityServicesCors_WithWildcardOrigin_ThrowsAtStartup()
    {
        var act = () => BuildPolicy("*");

        act.Should().Throw<InvalidOperationException>();
    }
}
