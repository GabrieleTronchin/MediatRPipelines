using MediatR.Playground.Model.Primitives.Request;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MediatR.Playground.Pipelines.Command;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        Stopwatch stopwatch = new();
        _logger.LogInformation($"Handling {typeof(TRequest).Name}");
        stopwatch.Start();

        var response = await next();

        stopwatch.Stop();

        _logger.LogInformation(
            $"Handled {typeof(TResponse).Name} in {stopwatch.ElapsedMilliseconds} ms"
        );
        stopwatch.Reset();

        return response;
    }
}
