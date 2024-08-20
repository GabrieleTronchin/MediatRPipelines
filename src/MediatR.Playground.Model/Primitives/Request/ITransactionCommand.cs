namespace MediatR.Playground.Model.Primitives.Request;

public interface ITransactionCommand<out TResponse> : IRequest<TResponse> { }
