# Caching Pipeline with FusionCache

[← Back to README](../README.md)

> Based on: [C# .NET — Caching Requests with MediatR Pipeline](https://blog.devgenius.io/c-net-caching-requests-with-mediatr-pipeline-44a7b92f9978)

## Overview

A single `CachingBehavior` pipeline behavior handles caching for all queries that implement `IQueryRequest<TResult>`, using [FusionCache](https://github.com/ZiggyCreatures/FusionCache). Handlers stay focused on data retrieval — caching is transparent at the pipeline level.

## IQueryRequest and Cache Keys

`IQueryRequest<TResult>` extends `IRequest<TResult>` and adds a `CacheKey` property. Each query defines its own key:

```csharp
public interface IQueryRequest<out IQueryResult> : IRequest<IQueryResult>
{
    public string CacheKey { get; }
}
```

- `GetAllSampleEntitiesQuery` → `"GetAllSampleEntitiesQuery-ALL"` (static key)
- `GetSampleEntityQuery` → `"GetAllSampleEntitiesQuery-{Id}"` (per-entity key)

Source: [`../src/MediatR.Playground.Model/Primitives/Request/IQueryRequest.cs`](../src/MediatR.Playground.Model/Primitives/Request/IQueryRequest.cs) · [`../src/MediatR.Playground.Model/Queries/Entity/`](../src/MediatR.Playground.Model/Queries/Entity/)

## CachingBehavior

Constrained to `IQueryRequest<TResponse>`. Uses `GetOrSetAsync` — on cache hit returns immediately, on miss executes the handler and stores the result.

Source: [`../src/MediatR.Playground.Pipelines/Query/CachingBehavior.cs`](../src/MediatR.Playground.Pipelines/Query/CachingBehavior.cs)

## Cache Configuration

| Option | Value | Description |
|--------|-------|-------------|
| `SetDuration` | 5 seconds | TTL for cached entries |
| `SetFailSafe` | `true` | Returns stale value if handler throws |
| `SetFactoryTimeouts` | 200 ms | Returns stale value if handler is slow, continues in background |

## Registration

`CachingBehavior` is the first behavior registered, so cache hits return before any other behavior executes:

```csharp
services.AddFusionCache();
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
```

Source: [`../src/MediatR.Playground.Domain/ServiceExtension.cs`](../src/MediatR.Playground.Domain/ServiceExtension.cs)

## See Also

- [Pipelines](./pipelines.md) — pipeline behavior overview and registration order
- [Unit of Work](./unit-of-work.md) — transaction management pipeline
