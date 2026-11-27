using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CityServicesHub.IntegrationTests;
using Microsoft.EntityFrameworkCore;

namespace Identity.IntegrationTests;

/// <summary>
/// Pruebas HTTP de extremo a extremo de Identity: registro, login, tokens, roles y 2FA contra
/// PostgreSQL real. Cubren fallos que las pruebas unitarias con mocks no detectaron.
/// </summary>
[Trait("Category", "Integration")]
public class IdentityApiTests : IClassFixture<IdentityApiFixture>
{
    private const string Password = "Str0ng!Pass";

    private readonly IdentityApiFixture _api;

    public IdentityApiTests(IdentityApiFixture api) => _api = api;

    // ---- helpers ----

    private static string NewEmail() => $"user-{Guid.NewGuid():N}@example.com";
    private static string NewDocumentNumber() => Random.Shared.Next(10_000_000, 99_999_999).ToString();

    private static object Registration(
        string? email = null, string? document = null, int documentType = 0,
        string password = Password, string? confirmPassword = null) => new
    {
        firstName = "Ana",
        lastName = "López",
        email = email ?? NewEmail(),
        documentNumber = document ?? NewDocumentNumber(),
        documentType,
        password,
        confirmPassword = confirmPassword ?? password
    };

    private Task<HttpResponseMessage> PostAsync(string path, object body) =>
        _api.Client.PostAsJsonAsync("/api/v1/auth" + path, body);

    private Task<HttpResponseMessage> GetAsync(string path, string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth" + path);
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _api.Client.SendAsync(request);
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    private async Task<(string Email, Guid UserId)> RegisterAsync()
    {
        var email = NewEmail();
        var response = await PostAsync("/register", Registration(email));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (email, (await ReadJsonAsync(response)).GetProperty("id").GetGuid());
    }

    private Task<HttpResponseMessage> LoginAsync(string email, string password = Password, string? twoFactorCode = null) =>
        PostAsync("/login", new { email, password, twoFactorCode });

    // ---- registration ----

    [Fact]
    public async Task Register_WithValidData_CreatesUserWithoutExposingThePassword()
    {
        var email = NewEmail();

        var response = await PostAsync("/register", Registration(email));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain(email);
        body.ToLowerInvariant().Should().NotContain("password").And.NotContain(Password);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Register_AcceptsEveryDefinedDocumentType(int documentType)
    {
        var response = await PostAsync("/register", Registration(documentType: documentType));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_WithUndefinedDocumentType_ReturnsBadRequestNamingTheField()
    {
        var response = await PostAsync("/register", Registration(documentType: 99));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.Content.ReadAsStringAsync()).Should().Contain("DocumentType");
    }

    [Fact]
    public async Task Register_WithWeakPassword_ReturnsBadRequest()
    {
        var response = await PostAsync("/register", Registration(password: "weak"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ReturnsBadRequest()
    {
        var response = await PostAsync("/register", Registration(confirmPassword: "Different1!"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var email = NewEmail();
        (await PostAsync("/register", Registration(email))).StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await PostAsync("/register", Registration(email));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithDuplicateDocument_ReturnsConflict()
    {
        var document = NewDocumentNumber();
        (await PostAsync("/register", Registration(document: document))).StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await PostAsync("/register", Registration(document: document));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // ---- login and tokens ----

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokensAndPersistsTheRefreshToken()
    {
        var (email, userId) = await RegisterAsync();

        var response = await LoginAsync(email);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ReadJsonAsync(response);
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("user").GetProperty("roles").EnumerateArray().Select(r => r.GetString())
            .Should().BeEquivalentTo(new[] { "Citizen" });

        var stored = await _api.WithDbAsync(db => db.RefreshTokens.CountAsync(t => t.UserId == userId));
        stored.Should().Be(1);
    }

    [Fact]
    public async Task Login_WithWrongPasswordOrUnknownEmail_GivesTheSameUnauthorizedAnswer()
    {
        var (email, _) = await RegisterAsync();

        var wrongPassword = await LoginAsync(email, "Wrong-pass1!");
        var unknownEmail = await LoginAsync(NewEmail());

        wrongPassword.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        unknownEmail.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await wrongPassword.Content.ReadAsStringAsync()).Should().Be(await unknownEmail.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_AfterFiveFailedAttempts_LocksTheAccountEvenWithTheRightPassword()
    {
        var (email, _) = await RegisterAsync();
        for (var attempt = 0; attempt < 5; attempt++)
            (await LoginAsync(email, "Wrong-pass1!")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var response = await LoginAsync(email);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await ReadJsonAsync(response)).GetProperty("message").GetString().Should().Contain("bloqueada");
    }

    [Fact]
    public async Task Me_WithIssuedToken_ReturnsTheUser_AndWithoutTokenIsUnauthorized()
    {
        var (email, userId) = await RegisterAsync();
        var token = (await ReadJsonAsync(await LoginAsync(email))).GetProperty("accessToken").GetString()!;

        var withToken = await GetAsync("/me", token);
        var withoutToken = await GetAsync("/me");

        withToken.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await ReadJsonAsync(withToken);
        user.GetProperty("id").GetGuid().Should().Be(userId);
        user.GetProperty("email").GetString().Should().Be(email);
        withoutToken.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserById_IsRestrictedToAdministrators()
    {
        var (email, userId) = await RegisterAsync();
        var citizenToken = (await ReadJsonAsync(await LoginAsync(email))).GetProperty("accessToken").GetString()!;

        (await GetAsync($"/{userId}", citizenToken)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await GetAsync($"/{userId}", TestJwt.Create(Guid.NewGuid(), "Administrator"))).StatusCode
            .Should().Be(HttpStatusCode.OK);
    }

    // ---- two-factor authentication ----

    [Fact]
    public async Task Login_WithTwoFactorEnabled_RequestsACodeAndNeverAcceptsOne()
    {
        var (email, userId) = await RegisterAsync();
        await _api.WithDbAsync(async db =>
        {
            var user = await db.Users.FirstAsync(u => u.Id == userId);
            user.EnableTwoFactor("test-secret");
            return await db.SaveChangesAsync();
        });

        var withoutCode = await LoginAsync(email);
        var withCode = await LoginAsync(email, twoFactorCode: "123456");

        withoutCode.StatusCode.Should().Be(HttpStatusCode.OK);
        (await ReadJsonAsync(withoutCode)).GetProperty("requiresTwoFactor").GetBoolean().Should().BeTrue();
        withCode.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await withCode.Content.ReadAsStringAsync()).Should().NotContain("accessToken");

        var tokens = await _api.WithDbAsync(db => db.RefreshTokens.CountAsync(t => t.UserId == userId));
        tokens.Should().Be(0);
    }
}
