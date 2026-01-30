using CityServices.Shared.Common;
using CityServices.Shared.Events;
using MedicalAppointments.Application.Commands.CreateAppointment;
using MedicalAppointments.Domain.Entities;
using MedicalAppointments.Infrastructure.Data;
using MedicalAppointments.Infrastructure.EventBus;
using MedicalAppointments.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppointmentsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

// MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateAppointmentCommand).Assembly));

// Repositories
builder.Services.AddScoped<IRepository<Appointment>>(sp =>
    new Repository<Appointment>(sp.GetRequiredService<AppointmentsDbContext>()));
builder.Services.AddScoped<IRepository<Doctor>>(sp =>
    new Repository<Doctor>(sp.GetRequiredService<AppointmentsDbContext>()));

// Event Bus
builder.Services.AddSingleton<IEventBus, RedisEventBus>();

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

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "medical-appointments" }));

app.Run();
