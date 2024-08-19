namespace MediatR.Playground.Model.Primitives;

public interface ITransactionCommand<out TResponse> : IRequest<TResponse> { }
