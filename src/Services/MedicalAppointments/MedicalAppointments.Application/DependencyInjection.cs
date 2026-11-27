using FluentValidation;
using MedicalAppointments.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MedicalAppointments.Application;

/// <summary>
/// Extensión para registrar servicios de la capa Application.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddMedicalAppointmentsApplication(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
