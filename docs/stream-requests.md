# Stream Requests and Stream Pipelines

[← Back to README](../README.md)

> **Note:** This documentation was AI-generated based on the original article:
> [C# .NET 8 — Stream Request and Pipeline with MediatR](https://medium.com/@gabrieletronchin/c-net-8-stream-request-and-pipeline-with-mediatr-a26ddb911b39).
> It is intended as a companion reference for the code in this repository.

## Overview

MediatR supports streaming data retrieval through `IStreamRequest<TResponse>` and `IAsyncEnumerable<TResponse>`. Instead of returning a complete collection in a single response, a stream request yields results one element at a time. This is useful when working with large datasets or real-time data where you want to start processing items as they become available rather than waiting for the entire result set.

Stream requests have their own pipeline system — `IStreamPipelineBehavior<TRequest, TResponse>` — which works similarly to `IPipelineBehavior` but operates on the `IAsyncEnumerable` stream rather than a single response object.

## IStreamRequest and IStreamRequestHandler

A stream request implements `IStreamRequest<TResponse>`, and its handler implements `IStreamRequestHandler<TRequest, TResponse>`. The handler returns `IAsyncEnumerable<TResponse>` and uses `yield return` to emit results one at a time.

### Stream Query Model

```csharp
public class SampleStreamEntityQuery : IStreamRequest<SampleStreamEntityQueryResult> { }

public record SampleStreamEntityQueryResult
{
    public Guid Id { get; set; }
    public DateTime EventTime { get; set; }
    public string Description { get; set; } = string.Empty;
}
```

Source: [`../src/MediatR.Playground.Model/Queries/StreamEntity/SampleStreamEntityQuery.cs`](../src/MediatR.Playground.Model/Queries/StreamEntity/SampleStreamEntityQuery.cs)

### Stream Query Handler

The handler iterates over the repository's `GetStream` method (which itself returns `IAsyncEnumerable<T>`) and yields each mapped result:

```csharp
public class SampleStreamQueryHandler
    : IStreamRequestHandler<SampleStreamEntityQuery, SampleStreamEntityQueryResult>
{
    private readonly IRepository<SampleEntity> _repository;

    public SampleStreamQueryHandler(IRepository<SampleEntity> repository)
    {
        _repository = repository;
    }

    public async IAsyncEnumerable<SampleStreamEntityQueryResult> Handle(
        SampleStreamEntityQuery request,
        CancellationToken cancellationToken
    )
    {
        await foreach (var entity in _repository.GetStream(cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            yield return new SampleStreamEntityQueryResult()
            {
                Id = entity.Id,
                Description = entity.Description,
                EventTime = entity.RegistrationTime,
            };
        }
    }
}
```

Source: [`../src/MediatR.Playground.Domain/QueryHandler/StreamEntity/SampleStreamQueryHandler.cs`](../src/MediatR.Playground.Domain/QueryHandler/StreamEntity/SampleStreamQueryHandler.cs)

### Sending a Stream Request

On the caller side, `IMediator.CreateStream` initiates the stream:

```csharp
return mediator.CreateStream(new SampleStreamEntityQuery(), cancellationToken);
```

Source: [`../src/MediatR.Playground.API/Endpoints/StreamRequestEndpoint.cs`](../src/MediatR.Playground.API/Endpoints/StreamRequestEndpoint.cs)

## IStreamPipelineBehavior vs IPipelineBehavior

`IPipelineBehavior<TRequest, TResponse>` intercepts standard request/response flows. It receives a `RequestHandlerDelegate<TResponse>` that returns a single `TResponse`. You call `await next()` once and get the complete response.

`IStreamPipelineBehavior<TRequest, TResponse>` intercepts stream request flows. It receives a `StreamHandlerDelegate<TResponse>` that returns `IAsyncEnumerable<TResponse>`. Instead of getting a single response, you iterate over the stream with `await foreach` and use `yield return` to pass each element downstream.

This is the key difference: a stream pipeline behavior processes elements **one at a time** as they flow through the stream. It can inspect, transform, filter, or log each individual element — something that is not possible with a standard pipeline behavior.

```
Standard:  Request → Behavior (pre) → Handler → single Response → Behavior (post) → Response
Stream:    Request → Behavior → Handler → element₁ → element₂ → ... → elementₙ → Behavior end
```

The `Handle` method signature for a stream pipeline behavior:

```csharp
public async IAsyncEnumerable<TResponse> Handle(
    TRequest request,
    StreamHandlerDelegate<TResponse> next,
    [EnumeratorCancellation] CancellationToken cancellationToken
)
```

The `[EnumeratorCancellation]` attribute ensures the cancellation token is properly propagated when the caller uses `.WithCancellation()` on the resulting `IAsyncEnumerable`.

## Stream Pipeline Behaviors

This project implements two stream pipeline behaviors that demonstrate different filtering techniques.

### GenericStreamLoggingBehavior (Generic Pipeline)

A generic stream pipeline behavior that runs for **all** stream requests. It has no `where` constraints on `TRequest` or `TResponse`, so MediatR invokes it for every `IStreamRequest` in the application.

It logs the start and end of the stream, and logs each element as it passes through:

```csharp
public class GenericStreamLoggingBehavior<TRequest, TResponse>
    : IStreamPipelineBehavior<TRequest, TResponse>
{
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
```

The pattern here is straightforward: call `next()` to get the stream from the handler (or the next behavior in the chain), iterate over it, do something with each element, and `yield return` it to the caller.

Source: [`../src/MediatR.Playground.Pipelines/Stream/GenericStreamLoggingBehavior.cs`](../src/MediatR.Playground.Pipelines/Stream/GenericStreamLoggingBehavior.cs)

### SampleFilterStreamBehavior (Specific Pipeline)

A specific stream pipeline behavior that runs **only** for `SampleStreamEntityWithPipeFilterQuery` requests. It uses `where` constraints on both `TRequest` and `TResponse` to target a concrete request/response pair:

```csharp
public class SampleFilterStreamBehavior<TRequest, TResponse>
    : IStreamPipelineBehavior<TRequest, TResponse>
    where TRequest : SampleStreamEntityWithPipeFilterQuery
    where TResponse : SampleStreamEntityWithPipeFilterQueryResult
```

This behavior acts as a **filter** — it checks authorization for each element in the stream and only yields elements that pass the check. Elements that fail the authorization check are logged and silently dropped:

```csharp
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
```

Notice that unlike `GenericStreamLoggingBehavior`, this behavior does **not** always `yield return` every element. It conditionally yields based on the authorization result, effectively filtering the stream.

Source: [`../src/MediatR.Playground.Pipelines/Stream/SampleFilterStreamBehavior.cs`](../src/MediatR.Playground.Pipelines/Stream/SampleFilterStreamBehavior.cs)

## Generic Pipeline vs Specific Pipeline

The two stream behaviors demonstrate the same filtering technique used by standard `IPipelineBehavior` implementations (see [Pipelines](./pipelines.md)), applied to the stream context:

| Aspect | Generic Pipeline | Specific Pipeline |
|--------|-----------------|-------------------|
| **Example** | `GenericStreamLoggingBehavior` | `SampleFilterStreamBehavior` |
| **Constraints** | None — `IStreamPipelineBehavior<TRequest, TResponse>` | `where TRequest : SampleStreamEntityWithPipeFilterQuery` and `where TResponse : SampleStreamEntityWithPipeFilterQueryResult` |
| **Scope** | Runs for every stream request in the application | Runs only for `SampleStreamEntityWithPipeFilterQuery` |
| **Use case** | Cross-cutting concerns (logging, metrics) | Request-specific logic (authorization filtering, data transformation) |

When MediatR resolves stream pipeline behaviors from the DI container, it checks the generic constraints. A behavior with `where TRequest : SomeConcreteType` will only be instantiated and invoked when the request type matches. This is the same mechanism used by standard pipeline behaviors — the DI container handles the filtering based on the generic type constraints.

## Registration

Stream pipeline behaviors are registered as open generics in the DI container, just like standard pipeline behaviors. The registration order determines the execution order:

```csharp
services.AddTransient(
    typeof(IStreamPipelineBehavior<,>),
    typeof(GenericStreamLoggingBehavior<,>)
);

services.AddTransient(
    typeof(IStreamPipelineBehavior<,>),
    typeof(SampleFilterStreamBehavior<,>)
);
```

For a `SampleStreamEntityWithPipeFilterQuery`, both behaviors execute: `GenericStreamLoggingBehavior` wraps the outer stream, and `SampleFilterStreamBehavior` wraps the inner stream (closer to the handler). For a `SampleStreamEntityQuery`, only `GenericStreamLoggingBehavior` executes because `SampleFilterStreamBehavior`'s constraints do not match.

Source: [`../src/MediatR.Playground.Domain/ServiceExtension.cs`](../src/MediatR.Playground.Domain/ServiceExtension.cs)

## API Endpoints

The stream requests are exposed through two endpoints:

| Endpoint | Request | Pipeline Behaviors |
|----------|---------|-------------------|
| `GET /StreamRequests/SampleStreamEntity` | `SampleStreamEntityQuery` | `GenericStreamLoggingBehavior` only |
| `GET /StreamRequests/SampleStreamEntityWithPipeFilter` | `SampleStreamEntityWithPipeFilterQuery` | `GenericStreamLoggingBehavior` + `SampleFilterStreamBehavior` |

The second endpoint demonstrates the filtering behavior — some elements may be dropped from the stream depending on the authorization result.

Source: [`../src/MediatR.Playground.API/Endpoints/StreamRequestEndpoint.cs`](../src/MediatR.Playground.API/Endpoints/StreamRequestEndpoint.cs)

## Further Reading

- [C# .NET — Stream Request and Pipeline With MediatR](https://medium.com/@gabrieletronchin/c-net-8-stream-request-and-pipeline-with-mediatr-a26ddb911b39) — Medium article covering stream request and pipeline concepts in depth

## See Also

- [Pipelines](./pipelines.md) — standard pipeline behaviors (IPipelineBehavior)
- [Caching Pipeline](./caching.md) — query-level caching via FusionCache
