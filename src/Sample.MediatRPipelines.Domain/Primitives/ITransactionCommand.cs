using MediatR;

namespace Sample.MediatRPipelines.Domain.Primitives;

public interface ITransactionCommand<out TResponse> : IRequest<TResponse> { }
