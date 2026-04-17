# Upgrading from MediatR 12.5.0 to 14.1.0

[← Back to README](../README.md)

## Overview

This guide covers the migration of the MediatR Playground from MediatR 12.5.0 (the last free Apache-2.0 release) to MediatR 14.1.0 (commercial, RPL-1.5 license). The upgrade touches package references, DI registration patterns, license key configuration, and introduces the notification handler de-duplication feature.

## Package Version Changes

Three projects reference MediatR packages. Each one changed differently:

| Project | Before | After | Notes |
|---------|--------|-------|-------|
| `MediatR.Playground.Domain` | `MediatR 12.5.0` | `MediatR 14.1.0` | Full package — hosts handlers and DI registration |
| `MediatR.Playground.Pipelines` | `MediatR 12.5.0` | `MediatR 14.1.0` | Full package — pipeline behaviors need the runtime |
| `MediatR.Playground.Model` | `MediatR 12.5.0` | `MediatR.Contracts 2.0.1` | Lightweight contracts-only package (see [MediatR.Contracts Separation](#mediatrcontracts-separation)) |

### Domain Project

```xml
<!-- Before -->
<PackageReference Include="MediatR" Version="12.5.0" />

<!-- After -->
<PackageReference Include="MediatR" Version="14.1.0" />
```

### Pipelines Project

```xml
<!-- Before -->
<PackageReference Include="MediatR" Version="12.5.0" />

<!-- After -->
<PackageReference Include="MediatR" Version="14.1.0" />
```

### Model Project

```xml
<!-- Before -->
<PackageReference Include="MediatR" Version="12.5.0" />

<!-- After -->
<PackageReference Include="MediatR.Contracts" Version="2.0.1" />
```

## License Key Configuration

MediatR 14.1.0 requires a license key. The key is configured through the standard ASP.NET Core configuration system so it never needs to be hardcoded.

### Configuration Files

**`appsettings.json`** — empty placeholder, checked into source control:

```json
{
  "MediatR": {
    "LicenseKey": ""
  }
}
```

**`appsettings.Development.json`** — actual key for local development:

```json
{
  "MediatR": {
    "LicenseKey": "<your-license-key>"
  }
}
```

The standard ASP.NET Core configuration layering means `appsettings.Development.json` overrides `appsettings.json` when running in the Development environment. For production, supply the key via environment variables, Azure Key Vault, or your preferred secrets provider.

### Wiring in ServiceExtension

The `AddMediatorSample` extension method now accepts `IConfiguration` as a second parameter and passes the key to MediatR during registration:

```csharp
public static IServiceCollection AddMediatorSample(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddMediatR(cfg =>
    {
        cfg.LicenseKey = configuration["MediatR:LicenseKey"];
        // ... rest of configuration
    });

    return services;
}
```

**`Program.cs`** passes the configuration in:

```csharp
builder.Services.AddMediatorSample(builder.Configuration);
```

### What Happens Without a Key

If the license key is missing or empty, MediatR 14.1.0 logs a warning via the `LuckyPennySoftware.MediatR.License` logging category. The application still starts and functions — the key is not enforced at startup.

## Behavior Registration Migration

MediatR 14.1.0 introduces `AddOpenBehavior` and `AddOpenStreamBehavior` methods on the configuration object. These replace the previous pattern of registering behaviors as open-generic transients on `IServiceCollection`.

### Before (MediatR 12.5.0)

Pipeline behaviors were registered outside the `AddMediatR` block using `AddTransient`:

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ServicesExtensions).Assembly);
    cfg.NotificationPublisher = new MultipleNotificationPublisher();
    cfg.NotificationPublisherType = typeof(MultipleNotificationPublisher);
});

// Pipeline behaviors
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GlobalExceptionHandlingBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CommandAuthorizationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

// Stream pipeline behaviors
services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(GenericStreamLoggingBehavior<,>));
services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(SampleFilterStreamBehavior<,>));
```

### After (MediatR 14.1.0)

All behavior registrations move inside the `AddMediatR` configuration lambda:

```csharp
services.AddMediatR(cfg =>
{
    cfg.LicenseKey = configuration["MediatR:LicenseKey"];
    cfg.RegisterServicesFromAssembly(typeof(ServicesExtensions).Assembly);
    cfg.NotificationPublisher = new MultipleNotificationPublisher();
    cfg.NotificationPublisherType = typeof(MultipleNotificationPublisher);

    // Pipeline behaviors (order determines execution order)
    cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
    cfg.AddOpenBehavior(typeof(GlobalExceptionHandlingBehavior<,>));
    cfg.AddOpenBehavior(typeof(CommandAuthorizationBehavior<,>));
    cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));

    // Stream pipeline behaviors
    cfg.AddOpenStreamBehavior(typeof(GenericStreamLoggingBehavior<,>));
    cfg.AddOpenStreamBehavior(typeof(SampleFilterStreamBehavior<,>));
});
```

### Key Differences

| Aspect | Old Pattern | New Pattern |
|--------|-------------|-------------|
| Registration location | Outside `AddMediatR` on `IServiceCollection` | Inside `AddMediatR` lambda on `MediatRServiceConfiguration` |
| Request behaviors | `services.AddTransient(typeof(IPipelineBehavior<,>), typeof(...))` | `cfg.AddOpenBehavior(typeof(...))` |
| Stream behaviors | `services.AddTransient(typeof(IStreamPipelineBehavior<,>), typeof(...))` | `cfg.AddOpenStreamBehavior(typeof(...))` |
| Execution order | Determined by registration order | Same — registration order still controls execution order |

The registration order was preserved exactly during migration to maintain the same pipeline execution behavior.

## MediatR.Contracts Separation

### What Is MediatR.Contracts?

`MediatR.Contracts` is a lightweight NuGet package that contains only the MediatR contract interfaces (`IRequest<T>`, `INotification`, `IStreamRequest<T>`, etc.) without the full mediator implementation, DI wiring, or pipeline infrastructure.

### Why Use It?

The Model project defines request types, notification types, and marker interfaces. It only needs the contract interfaces to declare that a class implements `IRequest<T>` or `INotification` — it never calls `IMediator.Send()` or registers services.

By referencing `MediatR.Contracts` instead of the full `MediatR` package, the Model project:

- **Pulls in fewer dependencies** — no DI framework references, no pipeline infrastructure
- **Compiles faster** — smaller dependency graph
- **Enforces architectural boundaries** — the Model layer physically cannot depend on mediator runtime behavior
- **Reduces version coupling** — `MediatR.Contracts 2.0.1` is stable and changes less frequently than the full package

### Which Projects Use Which Package?

```
MediatR.Playground.Model       → MediatR.Contracts 2.0.1  (interfaces only)
MediatR.Playground.Pipelines   → MediatR 14.1.0           (needs IPipelineBehavior runtime)
MediatR.Playground.Domain      → MediatR 14.1.0           (needs IMediator, handler registration)
```

Projects that reference Model transitively get the contract interfaces. Only projects that need the mediator runtime (sending requests, registering handlers, defining behaviors) reference the full `MediatR` package.

## Notification Handler De-duplication

### The Problem

In MediatR 12.x, if a notification handler was registered multiple times in the DI container (a common mistake with assembly scanning or manual registration), it would execute once for each registration. This could cause duplicate side effects — double emails, double log entries, double database writes.

### MediatR 14's Solution

MediatR 14.1.0 includes built-in notification handler de-duplication. When `IMediator.Publish()` is called, MediatR de-duplicates the resolved handler instances by type before invoking them. Each handler type executes exactly once per publish call, regardless of how many times it was registered.

### Demo in This Project

The `DeduplicationNotificationHandler` is intentionally registered twice in `ServiceExtension.cs` to demonstrate this feature:

```csharp
// Intentional duplicate registration to demo de-duplication
services.AddTransient<INotificationHandler<DeduplicationNotification>, DeduplicationNotificationHandler>();
services.AddTransient<INotificationHandler<DeduplicationNotification>, DeduplicationNotificationHandler>();
```

The handler tracks invocations using a thread-safe static counter:

```csharp
internal class DeduplicationNotificationHandler(
    ILogger<DeduplicationNotificationHandler> logger)
    : INotificationHandler<DeduplicationNotification>
{
    private static readonly ConcurrentDictionary<Guid, int> InvocationCounter = new();

    public Task Handle(DeduplicationNotification notification, CancellationToken cancellationToken)
    {
        var count = InvocationCounter.AddOrUpdate(notification.Id, 1, (_, c) => c + 1);

        logger.LogInformation(
            "Handler: {Handler} | Id={Id} | NotificationTime={Time} | InvocationCount={Count}",
            nameof(DeduplicationNotificationHandler),
            notification.Id,
            notification.NotificationTime,
            count);

        return Task.CompletedTask;
    }
}
```

### How to Verify

1. Start the API and open Swagger
2. Call `POST /Notifications/DeduplicationNotification`
3. Check the application logs — you should see `InvocationCount=1` for the notification ID
4. Despite the handler being registered twice, MediatR 14 ensures it only runs once

If you were running MediatR 12.5.0 with the same double registration, the logs would show the handler executing twice (count reaching 2 for a single publish call).

### Endpoint

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/Notifications/DeduplicationNotification` | Publishes a `DeduplicationNotification` and returns `{ Id, NotificationTime, Type: "Deduplication" }` |

## See Also

- [Notifications](./notifications.md) — notification publishing strategies and the de-duplication section
- [Pipelines](./pipelines.md) — pipeline behavior registration and execution order
