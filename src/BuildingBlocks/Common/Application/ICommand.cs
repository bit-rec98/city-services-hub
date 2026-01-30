using MediatR;

namespace CityServicesHub.BuildingBlocks.Common.Application;

/// <summary>
/// Interface marcador para commands (escrituras).
/// Implementa patrón CQRS.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Interface para commands que retornan un valor específico.
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Interface para manejadores de commands.
/// </summary>
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Interface para manejadores de commands con respuesta.
/// </summary>
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
