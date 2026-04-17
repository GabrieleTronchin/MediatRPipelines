# Breaking Changes: MediatR 12.5.0 → 14.1.0

[← Back to README](../README.md)

## License Key (New Requirement)

MediatR 14.1.0 requires a license key. Configure it in `appsettings.json`:

```json
{
  "MediatR": {
    "LicenseKey": ""
  }
}
```

Set the actual key in `appsettings.Development.json` (or environment variables / secrets for production). The `AddMediatorSample` method now accepts `IConfiguration` to read it:

```csharp
public static IServiceCollection AddMediatorSample(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddMediatR(cfg =>
    {
        cfg.LicenseKey = configuration["MediatR:LicenseKey"];
        // ...
    });
}
```

If the key is missing, MediatR logs a warning but still works.

## Package Changes

| Project | Before | After |
|---------|--------|-------|
| Domain | `MediatR 12.5.0` | `MediatR 14.1.0` |
| Pipelines | `MediatR 12.5.0` | `MediatR 14.1.0` |
| Model | `MediatR 12.5.0` | `MediatR.Contracts 2.0.1` |

## MediatR.Contracts

The Model project now references `MediatR.Contracts` instead of the full `MediatR` package. It contains only the contract interfaces (`IRequest<T>`, `INotification`, etc.) — no runtime, no DI. This keeps the Model layer lightweight and decoupled from the mediator implementation.

## Behavior Registration

`AddTransient(typeof(IPipelineBehavior<,>), ...)` is replaced by `AddOpenBehavior` / `AddOpenStreamBehavior` inside the `AddMediatR` lambda.

**Before:**

```csharp
services.AddMediatR(cfg => { /* ... */ });

services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(GenericStreamLoggingBehavior<,>));
```

**After:**

```csharp
services.AddMediatR(cfg =>
{
    // ...
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenStreamBehavior(typeof(GenericStreamLoggingBehavior<,>));
});
```

Registration order still determines execution order.

## Notification Handler De-duplication

MediatR 14 de-duplicates notification handlers by type. If the same handler is registered multiple times, it executes only once per `Publish` call.

The `DeduplicationNotificationHandler` is intentionally registered twice to demonstrate this:

```csharp
services.AddTransient<INotificationHandler<DeduplicationNotification>, DeduplicationNotificationHandler>();
services.AddTransient<INotificationHandler<DeduplicationNotification>, DeduplicationNotificationHandler>();
```

Call `POST /Notifications/DeduplicationNotification` and check the logs — `InvocationCount` will be 1, not 2.

## See Also

- [Notifications](./notifications.md)
- [Pipelines](./pipelines.md)
