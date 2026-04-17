# MediatR Notifications and Notification Publishers

[← Back to README](../README.md)

> Based on: [C# .NET 8 — MediatR Notifications and Notification Publisher](https://medium.com/@gabrieletronchin/c-net-8-mediatr-notifications-and-notification-publisher-b72a36f0e9ee)

## Overview

Notifications use `INotification` and `INotificationHandler<T>`. Unlike requests, notifications are dispatched to **all** registered handlers and return no value. The **notification publisher** controls how handlers are invoked.

## Publishing Strategies

MediatR ships with two built-in publishers:

| Publisher | Execution | Error behavior |
|-----------|-----------|----------------|
| `ForeachAwaitPublisher` | Sequential, one at a time | Stops on first exception |
| `TaskWhenAllPublisher` | Parallel via `Task.WhenAll` | Collects all exceptions into `AggregateException` |

## Custom Publishers in This Project

### MultipleNotificationPublisher (registered in DI)

Selects strategy based on the notification's marker interface:

| Notification implements | Delegates to | Strategy |
|------------------------|--------------|----------|
| `IPriorityNotification` | `PriorityNotificationPublisher` | Sequential, ordered by handler priority |
| `IParallelNotification` | `TaskWhenAllPublisher` | Parallel |
| _(default)_ | `ForeachAwaitPublisher` | Sequential |

Source: [`../src/MediatR.Playground.Domain/NotificationHandler/MultipleNotificationPublisher.cs`](../src/MediatR.Playground.Domain/NotificationHandler/MultipleNotificationPublisher.cs)

### PriorityNotificationPublisher

Executes handlers sequentially, ordered by `Priority` property (lower = first). See [Priority Notification Publisher](./priority-notification-publisher.md) for details.

## Marker Interfaces

| Interface | Purpose |
|-----------|---------|
| `IParallelNotification` | Parallel delivery |
| `IPriorityNotification` | Priority-ordered delivery |
| `IDataUpdateNotification` | Adds `Id` and `CacheKey` for cache invalidation scenarios |

Source: [`../src/MediatR.Playground.Model/Primitives/Notifications/`](../src/MediatR.Playground.Model/Primitives/Notifications/)

## Notification Models

| Notification | Implements | Strategy |
|-------------|------------|----------|
| `SampleNotification` | `INotification` | Sequential |
| `SampleParallelNotification` | `IParallelNotification` | Parallel |
| `SamplePriorityNotification` | `IPriorityNotification` | Priority-ordered |
| `DeduplicationNotification` | `INotification` | Sequential |

Source: [`../src/MediatR.Playground.Model/Notifications/`](../src/MediatR.Playground.Model/Notifications/)

## Registration

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ServicesExtensions).Assembly);
    cfg.NotificationPublisher = new MultipleNotificationPublisher();
    cfg.NotificationPublisherType = typeof(MultipleNotificationPublisher);
});
```

Handlers are auto-discovered via assembly scanning.

Source: [`../src/MediatR.Playground.Domain/ServiceExtension.cs`](../src/MediatR.Playground.Domain/ServiceExtension.cs)

## API Endpoints

| Endpoint | Notification Type | Strategy |
|----------|------------------|----------|
| `POST /Notifications/SequentialNotification` | `SampleNotification` | Sequential |
| `POST /Notifications/ParallelNotification` | `SampleParallelNotification` | Parallel |
| `POST /Notifications/SamplePriorityNotification` | `SamplePriorityNotification` | Priority-ordered |
| `POST /Notifications/DeduplicationNotification` | `DeduplicationNotification` | Sequential |

Source: [`../src/MediatR.Playground.API/Endpoints/NotificationEndpoint.cs`](../src/MediatR.Playground.API/Endpoints/NotificationEndpoint.cs)

## Notification Handler De-duplication

MediatR 14 includes built-in notification handler de-duplication. When the same handler type is registered multiple times in the DI container (a common mistake with assembly scanning or explicit registration), MediatR ensures each handler type executes only once per `Publish` call.

### How It Works

The `DeduplicationNotificationHandler` is intentionally registered twice in `ServiceExtension` to simulate a duplicate registration scenario:

```csharp
services.AddTransient<INotificationHandler<DeduplicationNotification>, DeduplicationNotificationHandler>();
services.AddTransient<INotificationHandler<DeduplicationNotification>, DeduplicationNotificationHandler>();
```

Despite the double registration, MediatR 14's de-duplication logic detects that both registrations resolve to the same handler type and invokes it only once.

### Observing De-duplication

The `DeduplicationNotificationHandler` tracks invocations using a static `ConcurrentDictionary<Guid, int>` counter keyed by notification ID. When you call `POST /Notifications/DeduplicationNotification`, the handler logs an `InvocationCount` of 1 for each unique notification ID — confirming that the handler ran exactly once, not twice.

Source: [`../src/MediatR.Playground.Domain/NotificationHandler/Deduplication/DeduplicationNotificationHandler.cs`](../src/MediatR.Playground.Domain/NotificationHandler/Deduplication/DeduplicationNotificationHandler.cs)

## See Also

- [Priority Notification Publisher](./priority-notification-publisher.md) — priority-ordered handler execution
- [Pipelines](./pipelines.md) — pipeline behavior overview and registration order
