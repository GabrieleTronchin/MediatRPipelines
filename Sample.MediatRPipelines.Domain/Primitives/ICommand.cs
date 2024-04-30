using MediatR;

namespace Sample.MediatRPipelines.Domain.Primitives;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
