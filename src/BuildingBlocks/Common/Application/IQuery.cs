using MediatR;

namespace CityServicesHub.BuildingBlocks.Common.Application;

/// <summary>
/// Interface marcador para queries (lecturas).
/// Implementa patrón CQRS.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Interface para manejadores de queries.
/// </summary>
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
