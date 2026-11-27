using CityServicesHub.BuildingBlocks.Logging;
using CityServicesHub.BuildingBlocks.Security;
using Microsoft.EntityFrameworkCore;
using Identity.Infrastructure.Data;
using Identity.Application;
using Identity.Infrastructure;
using Identity.API.Middleware;
using Identity.API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Host.UseSerilogLogging();

// Agregar servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerConfiguration();

// JWT Authentication
builder.Services.AddCityServicesJwtAuthentication(builder.Configuration);

// Servicios de capas Application e Infrastructure
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("IdentityDb") ?? "")
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "");

// CORS
builder.Services.AddCityServicesCors(builder.Configuration);

var app = builder.Build();

// Aplica las migraciones pendientes solo si se habilita de forma explícita (Database:MigrateOnStartup),
// p. ej. en desarrollo local. Por defecto el esquema se aplica con `dotnet ef database update`.
if (app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
{
    using var migrationScope = app.Services.CreateScope();
    migrationScope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.Migrate();
}

// Pipeline de middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseCors(CorsExtensions.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Log de inicio
Log.Information("Identity API iniciando en {Environment}", app.Environment.EnvironmentName);

app.Run();

// Para tests de integración
public partial class Program { }
