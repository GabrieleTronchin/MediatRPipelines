using MediatR;

namespace MediatR.Playground.Model.Primitives;

public interface ICommand<out TResponse> : IRequest<TResponse> { }
