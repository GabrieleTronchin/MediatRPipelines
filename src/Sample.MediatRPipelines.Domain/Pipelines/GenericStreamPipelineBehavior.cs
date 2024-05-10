using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sample.MediatRPipelines.Domain.Pipelines;

public class GenericStreamPipelineBehavior<TRequest, TResponse> : IStreamPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<GenericStreamPipelineBehavior<TRequest, TResponse>> _logger;

    public GenericStreamPipelineBehavior(ILogger<GenericStreamPipelineBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async IAsyncEnumerable<TResponse> Handle(TRequest request, StreamHandlerDelegate<TResponse> next, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stream Request Start");
        await foreach (var response in next().WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            _logger.LogInformation("Processing message {json}", System.Text.Json.JsonSerializer.Serialize(response));

            yield return response;
        }
        _logger.LogInformation("Stream Request End");
    }
}