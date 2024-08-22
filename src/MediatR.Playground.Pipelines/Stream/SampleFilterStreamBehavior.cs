using FakeAuth.Service;
using MediatR.Playground.Model.Queries.StreamEntityWithFilter;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace MediatR.Playground.Pipelines.Stream;

public class SampleFilterStreamBehavior<TRequest, TResponse>
    : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : SampleStreamEntityWithPipeFilterQuery
    where TResponse : SampleStreamEntityWithPipeFilterQueryResult
{
    private readonly ILogger<SampleFilterStreamBehavior<TRequest, TResponse>> _logger;
    private readonly IAuthService _authService;

    public SampleFilterStreamBehavior(
        IAuthService authService,
        ILogger<SampleFilterStreamBehavior<TRequest, TResponse>> logger
    )
    {
        _logger = logger;
        _authService = authService;
    }

    public async IAsyncEnumerable<TResponse> Handle(
        TRequest request,
        StreamHandlerDelegate<TResponse> next,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        await foreach (
            var response in next().WithCancellation(cancellationToken).ConfigureAwait(false)
        )
        {
            var isAllowed = _authService.OperationAlowed();

            if (isAllowed.IsSuccess)
            {
                yield return response;
            }
            else
            {
                _logger.LogWarning(
                    "User is not allowed to get this data, entity {json} has not be returned.",
                    System.Text.Json.JsonSerializer.Serialize(response)
                );
            }
        }
    }
}
