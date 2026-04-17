# Exception Handling with MediatR

[← Back to README](../README.md)

> Based on: [C# .NET 8 — Handle Exceptions with MediatR](https://medium.com/@gabrieletronchin/c-net-8-handle-exceptions-with-mediatr-48cbf80bae4e)

## Overview

MediatR provides `IRequestExceptionHandler<TRequest, TResponse, TException>` for handling exceptions thrown during request processing. Each handler targets a **specific request type + exception type**. When the handler throws, MediatR invokes the most specific matching exception handler.

The handler receives a `RequestExceptionHandlerState<TResponse>` object:
- **`state.SetHandled(response)`** — swallows the exception and returns the fallback response
- **Not calling `SetHandled`** — the exception propagates normally

## Exception Handlers in This Project

Both handlers target `SampleCommand` and call `SetHandled` with a fallback `SampleCommandComplete { Id = Guid.Empty }`:

| Handler | Exception Type | Matches |
|---------|---------------|---------|
| `ExceptionHandler` | `Exception` | Any exception from `SampleCommand` |
| `InvalidOperationExceptionHandler` | `InvalidOperationException` | Only `InvalidOperationException` from `SampleCommand` |

When `SampleCommand` throws an `InvalidOperationException`, MediatR invokes the more specific handler first. For any other exception type, only the catch-all `ExceptionHandler` runs.

Source: [`../src/MediatR.Playground.Domain/ExceptionsHandler/Commands/`](../src/MediatR.Playground.Domain/ExceptionsHandler/Commands/)

## How Exceptions Are Triggered

`SampleCommand` has an optional `RaiseException` property. When set, the handler throws it:

```csharp
if (request.RaiseException != null)
    throw request.RaiseException;
```

Two endpoints test this:
- `POST /Exceptions/SampleCommandWithIOException` → `InvalidOperationException` → handled by specific handler
- `POST /Exceptions/SampleCommandWithException` → generic `Exception` → handled by catch-all handler

Source: [`../src/MediatR.Playground.API/Endpoints/ExceptionsEndpoints.cs`](../src/MediatR.Playground.API/Endpoints/ExceptionsEndpoints.cs)

## Registration

Exception handlers are auto-discovered by MediatR's assembly scanning — no manual registration needed.

Source: [`../src/MediatR.Playground.Domain/ServiceExtension.cs`](../src/MediatR.Playground.Domain/ServiceExtension.cs)

## See Also

- [Global Exception Handling](./global-exception-handling.md) — cross-cutting exception logging behavior
- [Pipelines](./pipelines.md) — pipeline behavior overview and registration order
