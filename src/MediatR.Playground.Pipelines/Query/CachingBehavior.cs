using MediatR.Playground.Model.Primitives.Request;
using Microsoft.Extensions.Logging;
using ZiggyCreatures.Caching.Fusion;

namespace MediatR.Playground.Pipelines.Query;

public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IQueryRequest<TResponse>
{
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly IFusionCache _cache;

    public CachingBehavior(ILogger<CachingBehavior<TRequest, TResponse>> logger, IFusionCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        var inputRequest = request as IQueryRequest<TResponse>;

        return await _cache.GetOrSetAsync(
            inputRequest.CacheKey,
            await next(),
            options =>
                options
                    .SetDuration(TimeSpan.FromSeconds(5))
                    .SetFailSafe(true)
                    .SetFactoryTimeouts(TimeSpan.FromMilliseconds(200))
        );
    }
}
