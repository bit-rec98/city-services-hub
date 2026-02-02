using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Identity.Application.Behaviors;
using System.Reflection;

namespace Identity.Application;

/// <summary>
/// Extensión para registrar servicios de la capa Application.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
