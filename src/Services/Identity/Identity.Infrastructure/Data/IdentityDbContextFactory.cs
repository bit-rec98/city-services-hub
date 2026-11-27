using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Identity.Infrastructure.Data;

namespace Identity.Infrastructure;

/// <summary>
/// Permite a las herramientas de EF Core (dotnet ef) crear el contexto sin arrancar la API ni
/// necesitar secretos. La cadena de conexión se toma de la variable de entorno
/// ConnectionStrings__IdentityDb; si no existe se usa un valor sin credenciales, suficiente para
/// generar migraciones (que no abren conexión) pero no para aplicarlas.
/// </summary>
public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
    public IdentityDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__IdentityDb")
            ?? "Host=localhost;Database=identity_db";

        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new IdentityDbContext(options);
    }
}
