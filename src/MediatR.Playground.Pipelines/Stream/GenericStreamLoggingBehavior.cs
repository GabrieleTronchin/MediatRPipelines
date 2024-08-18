using MediatR;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace MediatRPlayground.Pipelines.Stream;

public class GenericStreamLoggingBehavior<TRequest, TResponse>
    : IStreamPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<GenericStreamLoggingBehavior<TRequest, TResponse>> _logger;

    public GenericStreamLoggingBehavior(
        ILogger<GenericStreamLoggingBehavior<TRequest, TResponse>> logger
    )
    {
        _logger = logger;
    }

    public async IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        StreamHandlerDelegate<TResponse> next,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Stream Request Start");
        await foreach (
            var response in next().WithCancellation(cancellationToken).ConfigureAwait(false)
        )
        {
            _logger.LogInformation(
                "Processing message {json}",
                System.Text.Json.JsonSerializer.Serialize(response)
            );

            yield return response;
        }
        _logger.LogInformation("Stream Request End");
    }
}
