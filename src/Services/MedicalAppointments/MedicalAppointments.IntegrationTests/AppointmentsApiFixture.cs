using CityServicesHub.IntegrationTests;
using MedicalAppointments.Domain.Entities;
using MedicalAppointments.Domain.Enums;
using MedicalAppointments.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace MedicalAppointments.IntegrationTests;

/// <summary>
/// Arranca la API real de turnos contra un PostgreSQL desechable. Las migraciones se aplican al
/// iniciar, igual que con Database:MigrateOnStartup en Docker Compose.
/// </summary>
public sealed class AppointmentsApiFixture : IAsyncLifetime
{
    private readonly PostgresFixture _postgres = new();
    private WebApplicationFactory<Program>? _factory;

    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgres.InitializeAsync();
        var connectionString = await _postgres.CreateServiceDatabaseAsync(
            "medical_appointments_db", "appointments_user", "it-appointments-pass");

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("JwtSettings:SecretKey", TestJwt.SecretKey);
            builder.UseSetting("ConnectionStrings:MedicalAppointmentsDb", connectionString);
            builder.UseSetting("Database:MigrateOnStartup", "true");
        });

        Client = _factory.CreateClient();
    }

    /// <summary>Crea un médico y un centro de salud nuevos, para que cada prueba tenga horarios propios.</summary>
    public async Task<(Guid DoctorId, Guid HealthCenterId)> SeedDoctorAndCenterAsync()
    {
        using var scope = _factory!.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MedicalAppointmentsDbContext>();

        var doctor = Doctor.Create("Ana", "García", $"MP-{Guid.NewGuid():N}", MedicalSpecialty.GeneralMedicine);
        var center = HealthCenter.Create("Centro 1", $"C-{Guid.NewGuid():N}"[..20], "Calle 1", "Córdoba", "Córdoba");
        db.Doctors.Add(doctor);
        db.HealthCenters.Add(center);
        await db.SaveChangesAsync();

        return (doctor.Id, center.Id);
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (_factory is not null)
            await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
