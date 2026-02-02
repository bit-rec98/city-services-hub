using CityServicesHub.BuildingBlocks.Logging;
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

// JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var jwtSettings = builder.Configuration.GetSection("JwtSettings");
        options.Authority = jwtSettings["Issuer"];
        options.Audience = jwtSettings["Audience"];
        options.RequireHttpsMetadata = false; // Solo para desarrollo
    });

builder.Services.AddAuthorization();

// Servicios de capas Application e Infrastructure
builder.Services.AddMedicalAppointmentsApplication();
builder.Services.AddMedicalAppointmentsInfrastructure(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("MedicalAppointmentsDb") ?? "");

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

Log.Information("Medical Appointments API iniciando en {Environment}", app.Environment.EnvironmentName);

app.Run();

public partial class Program { }
