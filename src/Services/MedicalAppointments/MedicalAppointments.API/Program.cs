using CityServicesHub.BuildingBlocks.Logging;
using CityServicesHub.BuildingBlocks.Security;
using MedicalAppointments.Application;
using MedicalAppointments.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Host.UseSerilogLogging();

// Agregar servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Medical Appointments API", Version = "v1" });
});

// JWT Authentication (validación compartida entre servicios)
builder.Services.AddCityServicesJwtAuthentication(builder.Configuration);
builder.Services.AddCityServicesCurrentUser();

// Servicios de capas Application e Infrastructure
builder.Services.AddMedicalAppointmentsApplication();
builder.Services.AddMedicalAppointmentsInfrastructure(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("MedicalAppointmentsDb") ?? "");

// CORS
builder.Services.AddCityServicesCors(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors(CorsExtensions.PolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("Medical Appointments API iniciando en {Environment}", app.Environment.EnvironmentName);

app.Run();

public partial class Program { }
