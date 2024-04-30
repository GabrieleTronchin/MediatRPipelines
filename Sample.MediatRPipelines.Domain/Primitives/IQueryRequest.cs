using MediatR;

namespace Sample.MediatRPipelines.Domain.Primitives;

public interface IQueryRequest<out TResponse> : IRequest<TResponse>
{
}
