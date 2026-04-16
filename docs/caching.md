# Caching Pipeline with FusionCache

## Overview

The caching pipeline intercepts query requests and stores their results using [FusionCache](https://github.com/ZiggyCreatures/FusionCache). Instead of adding caching logic inside each query handler, a single `CachingBehavior` pipeline behavior handles it for all queries that implement `IQueryRequest<TResult>`. This keeps handlers focused on data retrieval while caching is managed transparently at the pipeline level.

## IQueryRequest and Cache Keys

The `IQueryRequest<TResult>` interface extends `IRequest<TResult>` and adds a `CacheKey` property. Any query that implements this interface automatically participates in the caching pipeline.

```csharp
public interface IQueryRequest<out IQueryResult> : IRequest<IQueryResult>
{
    public string CacheKey { get; }
}
```

Each query defines its own cache key, typically using the query type name combined with any distinguishing parameters. This ensures that different queries (or the same query with different parameters) produce unique cache entries.

For example, `GetAllSampleEntitiesQuery` uses a static key since it always returns the full collection, while `GetSampleEntityQuery` includes the entity ID to cache each entity separately:

```csharp
// Caches all entities under a single key
public class GetAllSampleEntitiesQuery : IQueryRequest<IEnumerable<GetAllSampleEntitiesQueryResult>>
{
    public string CacheKey => $"{nameof(GetAllSampleEntitiesQuery)}-ALL";
}

// Caches each entity by its ID
public class GetSampleEntityQuery : IQueryRequest<GetSampleEntityQueryResult>
{
    public Guid Id { get; set; }
    public string CacheKey => $"{nameof(GetAllSampleEntitiesQuery)}-{Id}";
}
```

Source: [`../src/MediatR.Playground.Model/Primitives/Request/IQueryRequest.cs`](../src/MediatR.Playground.Model/Primitives/Request/IQueryRequest.cs)
Source: [`../src/MediatR.Playground.Model/Queries/Entity/`](../src/MediatR.Playground.Model/Queries/Entity/)

## CachingBehavior

`CachingBehavior` is a pipeline behavior constrained to `IQueryRequest<TResponse>`. It uses FusionCache's `GetOrSetAsync` method to check the cache before executing the handler. If a cached result exists for the request's `CacheKey`, it is returned directly. Otherwise, the handler executes via `next()`, and the result is stored in the cache for subsequent requests.

```csharp
public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IQueryRequest<TResponse>
{
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
    private readonly IFusionCache _cache;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var inputRequest = request as IQueryRequest<TResponse>;

        return await _cache.GetOrSetAsync(
            inputRequest.CacheKey,
            await next(),
            options => options
                .SetDuration(TimeSpan.FromSeconds(5))
                .SetFailSafe(true)
                .SetFactoryTimeouts(TimeSpan.FromMilliseconds(200))
        );
    }
}
```

Because of the `where TRequest : IQueryRequest<TResponse>` constraint, this behavior only activates for queries. Commands and other request types pass through the pipeline without triggering the caching logic.

Source: [`../src/MediatR.Playground.Pipelines/Query/CachingBehavior.cs`](../src/MediatR.Playground.Pipelines/Query/CachingBehavior.cs)

## Cache Configuration

The `GetOrSetAsync` call configures three FusionCache options:

| Option | Value | Description |
|--------|-------|-------------|
| `SetDuration` | 5 seconds | How long a cached entry remains valid. After this period, the next request triggers a fresh handler execution. |
| `SetFailSafe` | `true` | If the handler fails (throws an exception) and a stale cached value exists, FusionCache returns the stale value instead of propagating the error. This provides resilience against transient failures. |
| `SetFactoryTimeouts` | 200 ms | If the handler takes longer than 200 ms and a stale cached value is available, FusionCache returns the stale value immediately while the handler continues executing in the background. This keeps response times predictable. |

These settings strike a balance between freshness and resilience. The short duration keeps data relatively current, while fail-safe and factory timeouts protect against slow or failing downstream dependencies.

## Pipeline-Level Caching vs Per-Handler Caching

Implementing caching as a pipeline behavior has several advantages over adding cache logic directly inside each query handler:

- **Single implementation**: The caching logic is written once in `CachingBehavior` and applies to every query that implements `IQueryRequest`. There is no need to duplicate cache-check-and-store patterns across handlers.
- **Separation of concerns**: Query handlers remain focused on data retrieval. They do not need to know about caching, cache keys, or cache configuration.
- **Consistent behavior**: All cached queries share the same duration, fail-safe, and timeout settings. Changing the caching strategy is a single-file change rather than a multi-handler refactor.
- **Opt-in via interface**: A query participates in caching simply by implementing `IQueryRequest<TResult>` and providing a `CacheKey`. Queries that should not be cached can use a different base interface (like `IRequest<TResult>` directly).

## Registration

FusionCache and `CachingBehavior` are registered in the DI container during service configuration. `CachingBehavior` is the first pipeline behavior registered, so it runs as the outermost behavior in the pipeline chain — cache hits return before any other behavior executes.

```csharp
services.AddFusionCache();

services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
// ... other pipeline behaviors follow
```

Source: [`../src/MediatR.Playground.Domain/ServiceExtension.cs`](../src/MediatR.Playground.Domain/ServiceExtension.cs)

## Further Reading

- [C# .NET — Caching Requests With MediatR Pipeline](https://blog.devgenius.io/c-net-caching-requests-with-mediatr-pipeline-44a7b92f9978) — Medium article covering the caching pipeline pattern in depth
