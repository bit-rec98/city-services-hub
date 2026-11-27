using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MedicalAppointments.Infrastructure.Data;

namespace MedicalAppointments.Infrastructure;

/// <summary>
/// Permite a las herramientas de EF Core (dotnet ef) crear el contexto sin arrancar la API ni
/// necesitar secretos. La cadena de conexión se toma de la variable de entorno
/// ConnectionStrings__MedicalAppointmentsDb; si no existe se usa un valor sin credenciales, suficiente para
/// generar migraciones (que no abren conexión) pero no para aplicarlas.
/// </summary>
public class MedicalAppointmentsDbContextFactory : IDesignTimeDbContextFactory<MedicalAppointmentsDbContext>
{
    public MedicalAppointmentsDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__MedicalAppointmentsDb")
            ?? "Host=localhost;Database=medical_appointments_db";

        var options = new DbContextOptionsBuilder<MedicalAppointmentsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new MedicalAppointmentsDbContext(options);
    }
}
