# Global Exception Handling with MediatR

## Overview

`GlobalExceptionHandlingBehavior` is a pipeline behavior (`IPipelineBehavior<TRequest, TResponse>`) that wraps every MediatR request in a try/catch block. When any exception is thrown during request processing, the behavior logs the error and re-throws it. This provides a single, centralized place for exception logging across all requests — without swallowing the exception or altering the response.

This approach complements the request-specific exception handling covered in [Exception Handling with MediatR](./exception-handling.md). The two mechanisms serve different purposes and can be used together.

## How It Works

`GlobalExceptionHandlingBehavior<TRequest, TResponse>` implements `IPipelineBehavior<TRequest, TResponse>` with a `where TRequest : notnull` constraint. Because there is no further filtering (no `ICommand`, `IQueryRequest`, or other marker interface constraint), this behavior runs for **every** MediatR request in the pipeline.

The `Handle` method wraps the call to `next()` in a try/catch:

1. Calls `await next()` to invoke the next behavior in the pipeline (or the handler itself)
2. If the handler completes successfully, the response is returned as-is
3. If an exception is thrown, the behavior logs the error using `ILogger` and **re-throws** the original exception

The key detail: the behavior does not call `SetHandled` or return a fallback response. It logs and re-throws, so the exception continues to propagate up the call stack. This makes it a pure logging/observability concern — it does not change the outcome of the request.

Source: [`../src/MediatR.Playground.Domain/ExceptionsHandler/GlobalExceptionHandlingBehavior.cs`](../src/MediatR.Playground.Domain/ExceptionsHandler/GlobalExceptionHandlingBehavior.cs)

## Registration

`GlobalExceptionHandlingBehavior` is registered as an open generic `IPipelineBehavior<,>` in the DI container:

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GlobalExceptionHandlingBehavior<,>));
```

Its position in the registration order determines where in the pipeline chain it catches exceptions. In this project, it is registered after `ValidationBehavior` and before `CommandAuthorizationBehavior`:

```
CachingBehavior → LoggingBehavior → ValidationBehavior → GlobalExceptionHandlingBehavior → CommandAuthorizationBehavior → UnitOfWorkBehavior → Handler
```

This means `GlobalExceptionHandlingBehavior` will catch exceptions thrown by `CommandAuthorizationBehavior`, `UnitOfWorkBehavior`, and the handler itself. Exceptions thrown by behaviors registered before it (like `ValidationBehavior`) will not pass through it.

Source: [`../src/MediatR.Playground.Domain/ServiceExtension.cs`](../src/MediatR.Playground.Domain/ServiceExtension.cs)

## Global vs. Request-Specific Exception Handling

MediatR offers two distinct mechanisms for dealing with exceptions. They operate at different levels and serve different purposes.

### Global Exception Handling (IPipelineBehavior)

`GlobalExceptionHandlingBehavior` uses `IPipelineBehavior<TRequest, TResponse>` to intercept exceptions across all requests. It acts as a cross-cutting concern in the pipeline.

- **Scope:** All requests (constrained only by `where TRequest : notnull`)
- **Mechanism:** Try/catch around `next()` in the pipeline
- **Typical use:** Centralized logging, metrics, telemetry
- **Exception outcome:** Re-throws the exception — does not alter the response

### Request-Specific Exception Handling (IRequestExceptionHandler)

`IRequestExceptionHandler<TRequest, TResponse, TException>` targets a specific request type and exception type. MediatR invokes matching handlers after the request handler throws.

- **Scope:** A specific request type and exception type (e.g., `SampleCommand` + `InvalidOperationException`)
- **Mechanism:** MediatR's built-in exception handler pipeline, invoked after the handler throws
- **Typical use:** Providing fallback responses, graceful degradation for specific error scenarios
- **Exception outcome:** Can call `state.SetHandled(response)` to swallow the exception and return an alternative response

For details on request-specific exception handling, see [Exception Handling with MediatR](./exception-handling.md).

### When to Use Each Approach

| Scenario | Approach |
|----------|----------|
| Centralized logging for all requests | Global (`IPipelineBehavior`) |
| Metrics and telemetry collection | Global (`IPipelineBehavior`) |
| Returning a fallback response for a specific request | Request-specific (`IRequestExceptionHandler`) |
| Handling a particular exception type differently per request | Request-specific (`IRequestExceptionHandler`) |
| Both logging globally and providing fallback responses | Use both together |

The two approaches are not mutually exclusive. In this project, `GlobalExceptionHandlingBehavior` logs the exception and re-throws it. If the request also has a registered `IRequestExceptionHandler`, MediatR will invoke that handler afterward, giving it a chance to provide a fallback response. This means you get centralized logging **and** request-specific recovery in a single request flow.

## Further Reading

- [C# .NET — Handle Exceptions with MediatR](https://medium.com/@gabrieletronchin/c-net-8-handle-exceptions-with-mediatr-48cbf80bae4e) — Medium article covering exception handling patterns with MediatR, including global exception handling
