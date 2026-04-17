# Stream Requests and Stream Pipelines

[← Back to README](../README.md)

> Based on: [C# .NET 8 — Stream Request and Pipeline with MediatR](https://medium.com/@gabrieletronchin/c-net-8-stream-request-and-pipeline-with-mediatr-a26ddb911b39)

## Overview

MediatR supports streaming via `IStreamRequest<TResponse>` and `IAsyncEnumerable<TResponse>`. Instead of returning a complete collection, a stream request yields results one element at a time using `yield return`.

Stream requests have their own pipeline system — `IStreamPipelineBehavior<TRequest, TResponse>` — which operates on the `IAsyncEnumerable` stream rather than a single response. This means behaviors can inspect, transform, or filter **each individual element** as it flows through.

```
Standard:  Request → Behavior → Handler → single Response
Stream:    Request → Behavior → Handler → element₁ → element₂ → ... → elementₙ
```

## Stream Request Model

A stream request implements `IStreamRequest<T>`, its handler implements `IStreamRequestHandler<TRequest, T>` and returns `IAsyncEnumerable<T>`. On the caller side, `IMediator.CreateStream` initiates the stream.

Source: [`../src/MediatR.Playground.Model/Queries/StreamEntity/`](../src/MediatR.Playground.Model/Queries/StreamEntity/) · [`../src/MediatR.Playground.Domain/QueryHandler/StreamEntity/`](../src/MediatR.Playground.Domain/QueryHandler/StreamEntity/)

## Stream Pipeline Behaviors

### GenericStreamLoggingBehavior (all stream requests)

No `where` constraints — runs for every `IStreamRequest`. Logs stream start/end and each element as it passes through. Always yields every element.

Source: [`../src/MediatR.Playground.Pipelines/Stream/GenericStreamLoggingBehavior.cs`](../src/MediatR.Playground.Pipelines/Stream/GenericStreamLoggingBehavior.cs)

### SampleFilterStreamBehavior (specific request only)

Constrained to `SampleStreamEntityWithPipeFilterQuery`. Acts as a **filter** — checks `IAuthService.OperationAlowed()` per element and only yields elements that pass. Failed elements are logged and silently dropped.

Source: [`../src/MediatR.Playground.Pipelines/Stream/SampleFilterStreamBehavior.cs`](../src/MediatR.Playground.Pipelines/Stream/SampleFilterStreamBehavior.cs)

## Registration

Stream behaviors are registered as open generics. Registration order determines execution order:

```csharp
services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(GenericStreamLoggingBehavior<,>));
services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(SampleFilterStreamBehavior<,>));
```

Source: [`../src/MediatR.Playground.Domain/ServiceExtension.cs`](../src/MediatR.Playground.Domain/ServiceExtension.cs)

## API Endpoints

| Endpoint | Pipeline Behaviors |
|----------|-------------------|
| `GET /StreamRequests/SampleStreamEntity` | `GenericStreamLoggingBehavior` only |
| `GET /StreamRequests/SampleStreamEntityWithPipeFilter` | `GenericStreamLoggingBehavior` + `SampleFilterStreamBehavior` |

Source: [`../src/MediatR.Playground.API/Endpoints/StreamRequestEndpoint.cs`](../src/MediatR.Playground.API/Endpoints/StreamRequestEndpoint.cs)

## See Also

- [Pipelines](./pipelines.md) — standard pipeline behaviors (IPipelineBehavior)
- [Caching Pipeline](./caching.md) — query-level caching via FusionCache
