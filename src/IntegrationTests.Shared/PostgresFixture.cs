using Npgsql;
using Testcontainers.PostgreSql;

namespace CityServicesHub.IntegrationTests;

/// <summary>
/// Levanta un PostgreSQL desechable en Docker y crea, por cada servicio bajo prueba, un rol y una
/// base propios (igual que scripts/init-databases.sh), de modo que la aplicación se ejecuta con
/// un rol de mínimo privilegio y no con el superusuario. La imagen se puede cambiar con la variable
/// de entorno TEST_POSTGRES_IMAGE (por defecto postgres:15-alpine, la misma que docker-compose.yml).
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private const string DefaultImage = "postgres:15-alpine";
    private readonly PostgreSqlContainer _container;

    public PostgresFixture()
    {
        var image = Environment.GetEnvironmentVariable("TEST_POSTGRES_IMAGE") ?? DefaultImage;
        _container = new PostgreSqlBuilder(image).Build();
    }

    public async Task InitializeAsync()
    {
        try
        {
            await _container.StartAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "No se pudo iniciar PostgreSQL con Testcontainers. Las pruebas de integración necesitan Docker " +
                "en ejecución y acceso a la imagen (variable TEST_POSTGRES_IMAGE para cambiarla). " +
                "Para ejecutar solo las pruebas unitarias: dotnet test --filter \"Category!=Integration\".", ex);
        }
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    /// <summary>Crea un rol y una base de su propiedad y devuelve la cadena de conexión de ese rol.</summary>
    public async Task<string> CreateServiceDatabaseAsync(string database, string role, string password)
    {
        await using (var admin = new NpgsqlConnection(_container.GetConnectionString()))
        {
            await admin.OpenAsync();
            foreach (var sql in new[]
                     {
                         $"CREATE ROLE {role} LOGIN PASSWORD '{password}'",
                         $"CREATE DATABASE {database} OWNER {role}",
                         $"REVOKE ALL ON DATABASE {database} FROM PUBLIC"
                     })
            {
                await using var command = new NpgsqlCommand(sql, admin);
                await command.ExecuteNonQueryAsync();
            }
        }

        return new NpgsqlConnectionStringBuilder(_container.GetConnectionString())
        {
            Database = database,
            Username = role,
            Password = password
        }.ConnectionString;
    }
}
