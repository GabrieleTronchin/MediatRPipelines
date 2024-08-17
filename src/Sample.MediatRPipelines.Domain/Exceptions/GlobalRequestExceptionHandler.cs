using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Sample.MediatRPipelines.Domain.Primitives;

namespace Sample.MediatRPipelines.Domain.Exceptions;


internal class GlobalRequestExceptionHandler<IRequest, TResponse, TException>
  : IRequestExceptionHandler<IRequest, TResponse, TException>
      where IRequest : IQueryRequest<IQueryResult>
      where TResponse: IQueryResult
      where TException : Exception
{
    private readonly ILogger<GlobalRequestExceptionHandler<IRequest, TResponse, TException>> _logger;
    public GlobalRequestExceptionHandler(
       ILogger<GlobalRequestExceptionHandler<IRequest, TResponse, TException>> logger)
    {
        _logger = logger;
    }


    public Task Handle(IRequest request, TException exception, RequestExceptionHandlerState<TResponse> state, CancellationToken cancellationToken)
    {
        _logger.LogError(
        exception,
                $"---- Exception Handler: '{nameof(GlobalRequestExceptionHandler<IRequest, TResponse, TException>)}'"
            );

        state.SetHandled(default);

        return Task.CompletedTask;
    }
}
