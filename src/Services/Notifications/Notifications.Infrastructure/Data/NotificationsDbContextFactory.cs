using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Notifications.Infrastructure.Data;

namespace Notifications.Infrastructure;

/// <summary>
/// Permite a las herramientas de EF Core (dotnet ef) crear el contexto sin arrancar la API ni
/// necesitar secretos. La cadena de conexión se toma de la variable de entorno
/// ConnectionStrings__NotificationsDb; si no existe se usa un valor sin credenciales, suficiente para
/// generar migraciones (que no abren conexión) pero no para aplicarlas.
/// </summary>
public class NotificationsDbContextFactory : IDesignTimeDbContextFactory<NotificationsDbContext>
{
    public NotificationsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__NotificationsDb")
            ?? "Host=localhost;Database=notifications_db";

        var options = new DbContextOptionsBuilder<NotificationsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new NotificationsDbContext(options);
    }
}
