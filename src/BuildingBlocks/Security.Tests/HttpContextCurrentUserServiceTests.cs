using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace CityServicesHub.BuildingBlocks.Security.Tests;

public class HttpContextCurrentUserServiceTests
{
    private static HttpContextCurrentUserService ServiceFor(ClaimsPrincipal? principal)
    {
        var accessor = new HttpContextAccessor
        {
            HttpContext = principal is null ? null : new DefaultHttpContext { User = principal }
        };

        return new HttpContextCurrentUserService(accessor);
    }

    private static ClaimsPrincipal Authenticated(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, authenticationType: "Test"));

    [Fact]
    public void WithAuthenticatedUser_ExposesIdAndRolesFromClaims()
    {
        var id = Guid.NewGuid().ToString();
        var service = ServiceFor(Authenticated(
            new Claim(ClaimTypes.NameIdentifier, id),
            new Claim(ClaimTypes.Role, "Administrator")));

        service.IsAuthenticated.Should().BeTrue();
        service.UserId.Should().Be(id);
        service.IsInRole("Administrator").Should().BeTrue();
        service.IsInRole("SuperAdmin").Should().BeFalse();
    }

    [Fact]
    public void WithSubClaimOnly_FallsBackToSub()
    {
        var id = Guid.NewGuid().ToString();
        var service = ServiceFor(Authenticated(new Claim("sub", id), new Claim("role", "Citizen")));

        service.UserId.Should().Be(id);
        service.IsInRole("Citizen").Should().BeTrue();
    }

    [Fact]
    public void WithAnonymousPrincipal_ExposesNoIdentity()
    {
        var service = ServiceFor(new ClaimsPrincipal(new ClaimsIdentity()));

        service.IsAuthenticated.Should().BeFalse();
        service.UserId.Should().BeNull();
        service.Roles.Should().BeEmpty();
    }

    [Fact]
    public void WithoutHttpContext_ExposesNoIdentity()
    {
        var service = ServiceFor(null);

        service.IsAuthenticated.Should().BeFalse();
        service.UserId.Should().BeNull();
    }
}
