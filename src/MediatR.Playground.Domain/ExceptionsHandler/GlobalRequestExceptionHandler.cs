using MediatR.Pipeline;
using MediatR.Playground.Model.Primitives.Request;
using Microsoft.Extensions.Logging;

namespace MediatR.Playground.Domain.ExceptionsHandler;

internal class GlobalRequestExceptionHandler<IRequest, TResponse, TException>
    : IRequestExceptionHandler<IRequest, TResponse, TException>
    where IRequest : IQueryRequest<IQueryResult>
    where TResponse : IQueryResult
    where TException : Exception
{
    private readonly ILogger<
        GlobalRequestExceptionHandler<IRequest, TResponse, TException>
    > _logger;

    public GlobalRequestExceptionHandler(
        ILogger<GlobalRequestExceptionHandler<IRequest, TResponse, TException>> logger
    )
    {
        _logger = logger;
    }

    public Task Handle(
        IRequest request,
        TException exception,
        RequestExceptionHandlerState<TResponse> state,
        CancellationToken cancellationToken
    )
    {
        _logger.LogError(
            exception,
            $"---- Exception Handler: '{nameof(GlobalRequestExceptionHandler<IRequest, TResponse, TException>)}'"
        );

        state.SetHandled(default);

        return Task.CompletedTask;
    }
}
