using CityServicesHub.IntegrationTests;
using Identity.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Identity.IntegrationTests;

/// <summary>
/// Arranca la API real de Identity contra un PostgreSQL desechable (migraciones al iniciar). Redis
/// se sustituye por una caché distribuida en memoria: las pruebas no necesitan ese contenedor.
/// </summary>
public sealed class IdentityApiFixture : IAsyncLifetime
{
    private readonly PostgresFixture _postgres = new();
    private WebApplicationFactory<Program>? _factory;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.InitializeAsync();
        var connectionString = await _postgres.CreateServiceDatabaseAsync(
            "identity_db", "identity_user", "it-identity-pass");

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("JwtSettings:SecretKey", TestJwt.SecretKey);
            builder.UseSetting("ConnectionStrings:IdentityDb", connectionString);
            builder.UseSetting("Database:MigrateOnStartup", "true");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IDistributedCache>();
                services.AddDistributedMemoryCache();
            });
        });

        Client = _factory.CreateClient();
    }

    /// <summary>Ejecuta una acción con el DbContext de la API (para preparar o inspeccionar datos).</summary>
    public async Task<T> WithDbAsync<T>(Func<IdentityDbContext, Task<T>> action)
    {
        using var scope = _factory!.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<IdentityDbContext>());
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (_factory is not null)
            await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
