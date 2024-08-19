namespace MediatR.Playground.Model.Primitives.Request;

public interface ICommand<out TResponse> : IRequest<TResponse> { }
