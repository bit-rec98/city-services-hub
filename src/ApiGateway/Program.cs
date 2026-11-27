using CityServicesHub.BuildingBlocks.Logging;
using CityServicesHub.BuildingBlocks.Security;
using AspNetCoreRateLimit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
builder.Host.UseSerilogLogging();

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// YARP Reverse Proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// JWT Authentication (validación compartida entre servicios)
builder.Services.AddCityServicesJwtAuthentication(builder.Configuration);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? 
                new[] { "http://localhost:3000", "http://localhost:5173" })
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Middleware pipeline
app.UseIpRateLimiting();
app.UseSerilogRequestLogging();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

// YARP Reverse Proxy
app.MapReverseProxy();

// Health check endpoint
app.MapHealthChecks("/health");

// Info endpoint
app.MapGet("/", () => new
{
    Name = "City Services Hub - API Gateway",
    Version = "1.0.0",
    Timestamp = DateTime.UtcNow
});

Log.Information("API Gateway iniciando en {Environment}", app.Environment.EnvironmentName);

app.Run();
