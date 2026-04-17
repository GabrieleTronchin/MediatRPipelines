# Global Exception Handling with MediatR

[← Back to README](../README.md)

> Based on: [C# .NET 8 — Handle Exceptions with MediatR](https://medium.com/@gabrieletronchin/c-net-8-handle-exceptions-with-mediatr-48cbf80bae4e)

## Overview

`GlobalExceptionHandlingBehavior` is an `IPipelineBehavior` that wraps every MediatR request in a try/catch. It **logs the error and re-throws** — it never swallows exceptions or returns fallback responses. This makes it a pure logging/observability concern.

It complements the per-request exception handling covered in [Exception Handling](./exception-handling.md). The two mechanisms serve different purposes and work together.

Source: [`../src/MediatR.Playground.Domain/ExceptionsHandler/GlobalExceptionHandlingBehavior.cs`](../src/MediatR.Playground.Domain/ExceptionsHandler/GlobalExceptionHandlingBehavior.cs)

## Registration and Pipeline Position

Registered as an open generic with no marker interface constraint (`where TRequest : notnull`), so it runs for **every** request type:

```
CachingBehavior → LoggingBehavior → ValidationBehavior → GlobalExceptionHandlingBehavior → CommandAuthorizationBehavior → UnitOfWorkBehavior → Handler
```

It catches exceptions from behaviors registered after it (Auth, UoW, Handler). Exceptions from behaviors before it (Validation) do not pass through it.

Source: [`../src/MediatR.Playground.Domain/ServiceExtension.cs`](../src/MediatR.Playground.Domain/ServiceExtension.cs)

## Global vs. Request-Specific

| Scenario | Approach |
|----------|----------|
| Centralized logging / metrics | Global (`IPipelineBehavior`) — logs + rethrows |
| Fallback response for a specific request | Request-specific (`IRequestExceptionHandler`) — calls `SetHandled` |
| Both logging and fallback | Use both together |

In this project, `GlobalExceptionHandlingBehavior` logs first, then MediatR's `RequestExceptionProcessorBehavior` invokes any matching `IRequestExceptionHandler` to provide a fallback. You get centralized logging **and** request-specific recovery in a single flow.

## The NotFoundExceptionGlobalHandler Endpoint

`GET /Exceptions/NotFoundExceptionGlobalHandler` sends a `GetSampleEntityQuery` with `Guid.Empty`. The handler throws `ArgumentNullException`.

This endpoint intentionally has **no** `IRequestExceptionHandler` registered. The exception flows through `GlobalExceptionHandlingBehavior` (logged), but since nothing calls `SetHandled`, it propagates to ASP.NET Core's error middleware → **500 response**.

This is by design: it demonstrates that the global behavior only logs. Compare with `POST /Exceptions/SampleCommandWithIOException` where a per-request handler provides a fallback response.

## See Also

- [Exception Handling](./exception-handling.md) — per-request exception handlers with SetHandled
- [Pipelines](./pipelines.md) — pipeline behavior overview and registration order
