# Exception Handling with MediatR

## Overview

MediatR provides a built-in mechanism for handling exceptions thrown during request processing through the `IRequestExceptionHandler<TRequest, TResponse, TException>` interface (from the `MediatR.Pipeline` namespace). Unlike pipeline behaviors that wrap the entire request flow, exception handlers are invoked **after** the handler throws an exception, giving you a chance to log the error and optionally provide a fallback response.

This approach is request-specific: each exception handler targets a concrete request type and a concrete exception type. MediatR matches the thrown exception against registered handlers and invokes the most specific match.

## How It Works

When a request handler throws an exception, MediatR looks for registered `IRequestExceptionHandler` implementations that match the request type and exception type. Each matching handler receives:

- The original **request** that caused the exception
- The **exception** that was thrown
- A `RequestExceptionHandlerState<TResponse>` object used to control the outcome
- A `CancellationToken`

The handler can log the error, perform cleanup, and decide whether to **handle** the exception by calling `state.SetHandled(response)`. If `SetHandled` is called, MediatR returns the provided response to the caller instead of propagating the exception. If no handler marks the exception as handled, the original exception continues to propagate up the call stack.

## Exception Handlers in This Project

This project implements two exception handlers for `SampleCommand`, demonstrating how to handle exceptions at different levels of specificity.

### ExceptionHandler (Catch-All)

`ExceptionHandler` implements `IRequestExceptionHandler<SampleCommand, SampleCommandComplete, Exception>`, catching **any** exception thrown by the `SampleCommand` handler.

- **Request type:** `SampleCommand`
- **Exception type:** `Exception` (base type — matches all exceptions)
- **Behavior:** Logs the error and calls `state.SetHandled(...)` with a fallback `SampleCommandComplete` response containing `Guid.Empty` as the `Id`

Source: [`../src/MediatR.Playground.Domain/ExceptionsHandler/Commands/ExceptionHandler.cs`](../src/MediatR.Playground.Domain/ExceptionsHandler/Commands/ExceptionHandler.cs)

### InvalidOperationExceptionHandler (Specific)

`InvalidOperationExceptionHandler` implements `IRequestExceptionHandler<SampleCommand, SampleCommandComplete, InvalidOperationException>`, catching **only** `InvalidOperationException` instances thrown by the `SampleCommand` handler.

- **Request type:** `SampleCommand`
- **Exception type:** `InvalidOperationException` (specific — only matches this exception type)
- **Behavior:** Logs the error and calls `state.SetHandled(...)` with a fallback `SampleCommandComplete` response containing `Guid.Empty` as the `Id`

Source: [`../src/MediatR.Playground.Domain/ExceptionsHandler/Commands/InvalidOperationExceptionHandler.cs`](../src/MediatR.Playground.Domain/ExceptionsHandler/Commands/InvalidOperationExceptionHandler.cs)

### Key Difference

The difference between the two handlers is the exception type parameter:

| Handler | Exception Type | Matches |
|---------|---------------|---------|
| `ExceptionHandler` | `Exception` | Any exception thrown by `SampleCommand` |
| `InvalidOperationExceptionHandler` | `InvalidOperationException` | Only `InvalidOperationException` from `SampleCommand` |

When `SampleCommand` throws an `InvalidOperationException`, MediatR invokes the more specific `InvalidOperationExceptionHandler` first. When it throws any other exception type (e.g., a generic `Exception`), only `ExceptionHandler` is invoked.

## RequestExceptionHandlerState and SetHandled

The `RequestExceptionHandlerState<TResponse>` object is the mechanism that controls whether an exception is swallowed or propagated.

- **`state.SetHandled(response)`** — Marks the exception as handled and provides an alternative response. MediatR returns this response to the caller as if the handler had completed successfully. The original exception is not propagated.
- **Not calling `SetHandled`** — The exception remains unhandled. If no other exception handler marks it as handled, the exception propagates normally.

In this project, both handlers call `SetHandled` with a `SampleCommandComplete { Id = Guid.Empty }`, meaning the caller receives a valid response object (with an empty GUID) instead of an exception. This pattern is useful for providing graceful degradation — the caller gets a recognizable fallback value rather than an error.

## How Exceptions Are Triggered

The `SampleCommand` includes an optional `RaiseException` property. When set, the `SampleCommandHandler` throws that exception during execution:

```csharp
if (request.RaiseException != null)
    throw request.RaiseException;
```

The API exposes two endpoints to test this behavior:

- **`POST /Exceptions/SampleCommandWithIOException`** — Sends a `SampleCommand` with `RaiseException` set to an `InvalidOperationException`, triggering the `InvalidOperationExceptionHandler`
- **`POST /Exceptions/SampleCommandWithException`** — Sends a `SampleCommand` with `RaiseException` set to a generic `Exception`, triggering the `ExceptionHandler`

Source: [`../src/MediatR.Playground.API/Endpoints/ExceptionsEndpoints.cs`](../src/MediatR.Playground.API/Endpoints/ExceptionsEndpoints.cs)

## Registration

Exception handlers are discovered automatically by MediatR's assembly scanning. The `RegisterServicesFromAssembly` call in the service configuration picks up all `IRequestExceptionHandler` implementations from the Domain assembly — no manual registration is needed.

Source: [`../src/MediatR.Playground.Domain/ServiceExtension.cs`](../src/MediatR.Playground.Domain/ServiceExtension.cs)

## Further Reading

- [C# .NET — Handle Exceptions with MediatR](https://medium.com/@gabrieletronchin/c-net-8-handle-exceptions-with-mediatr-48cbf80bae4e) — Medium article covering exception handling patterns with MediatR in depth
