# MediatR Pipelines

## Overview

MediatR pipelines allow you to intercept the request/response flow by implementing `IPipelineBehavior<TRequest, TResponse>`. Each pipeline behavior wraps around the handler execution, giving you a hook to run logic **before** and **after** the handler processes the request.

The `Handle` method in a pipeline behavior receives the request and a `RequestHandlerDelegate<TResponse> next` delegate. Calling `await next()` invokes the next behavior in the chain (or the handler itself if it's the last behavior). Any code before `next()` runs as pre-processing; any code after runs as post-processing.

```
Request → Behavior 1 (pre) → Behavior 2 (pre) → Handler → Behavior 2 (post) → Behavior 1 (post) → Response
```

## Pipeline Behaviors

### LoggingBehavior

Logs the start and end of every command execution, measuring elapsed time with a `Stopwatch`.

- **Constraint:** `where TRequest : ICommand<TResponse>` — only runs for commands
- **Pre-processing:** Logs the request type name and starts the stopwatch
- **Post-processing:** Stops the stopwatch and logs the response type name with elapsed milliseconds

Source: [`../src/MediatR.Playground.Pipelines/Command/LoggingBehavior.cs`](../src/MediatR.Playground.Pipelines/Command/LoggingBehavior.cs)

### ValidationBehavior

Validates incoming commands using FluentValidation before the handler executes. If any validation errors are found, a `ValidationException` is thrown and the handler is never called.

- **Constraint:** `where TRequest : ICommand<TResponse>` — only runs for commands
- **Pre-processing:** Runs all registered `IValidator<TRequest>` instances against the request
- **Post-processing:** None — validation is purely a pre-processing step

Source: [`../src/MediatR.Playground.Pipelines/Command/ValidationBehavior.cs`](../src/MediatR.Playground.Pipelines/Command/ValidationBehavior.cs)

### CommandAuthorizationBehavior

Checks authorization before executing a command by calling `IAuthService.OperationAlowed()`. If the authorization check fails, an exception is thrown and the handler is not invoked.

- **Constraint:** `where TRequest : ICommand<TResponse>` — only runs for commands
- **Pre-processing:** Calls the auth service and throws if the operation is not allowed
- **Post-processing:** None — authorization is purely a pre-processing step

Source: [`../src/MediatR.Playground.Pipelines/Command/CommandAuthorizationBehavior.cs`](../src/MediatR.Playground.Pipelines/Command/CommandAuthorizationBehavior.cs)

## Pipeline Filtering with Custom Interfaces

Not every pipeline behavior should run for every request. This project uses custom marker interfaces that extend `IRequest<TResponse>` to control which behaviors apply to which requests:

| Interface | Inherits From | Purpose |
|-----------|---------------|---------|
| `ICommand<TResponse>` | `IRequest<TResponse>` | Marks write operations (commands). Used by LoggingBehavior, ValidationBehavior, and CommandAuthorizationBehavior. |
| `IQueryRequest<TResult>` | `IRequest<TResult>` | Marks read operations (queries). Includes a `CacheKey` property used by CachingBehavior. |
| `ITransactionCommand<TResponse>` | `IRequest<TResponse>` | Marks commands that require transactional handling. Used by UnitOfWorkBehavior. |

By applying a `where TRequest : ICommand<TResponse>` constraint on a pipeline behavior, MediatR will only invoke that behavior for requests implementing `ICommand`. Requests that implement `IQueryRequest` or `ITransactionCommand` instead will skip those behaviors entirely. This keeps each pipeline focused on the request types it is designed to handle.

Source: [`../src/MediatR.Playground.Model/Primitives/Request/`](../src/MediatR.Playground.Model/Primitives/Request/)

## Registration Order

Pipeline behaviors are registered in the DI container as open generics. The order of registration determines the execution order — the first registered behavior is the outermost in the pipeline chain.

The current registration order in this project is:

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GlobalExceptionHandlingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CommandAuthorizationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));
```

For a command request, the execution flows through the behaviors in registration order. Only behaviors whose generic constraints match the request type will actually execute. For example, a request implementing `ICommand<TResponse>` will pass through CachingBehavior (skipped — constraint doesn't match), LoggingBehavior, ValidationBehavior, GlobalExceptionHandlingBehavior, CommandAuthorizationBehavior, and then the handler.

Source: [`../src/MediatR.Playground.Domain/ServiceExtension.cs`](../src/MediatR.Playground.Domain/ServiceExtension.cs)

## Further Reading

- [C# .NET — MediatR Pipelines](https://medium.com/@gabrieletronchin/c-net-8-mediatr-pipelines-edcc9ae8224b) — Medium article covering the pipeline concepts in depth
