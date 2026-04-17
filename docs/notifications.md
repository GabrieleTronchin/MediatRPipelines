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

Source: [`../src/MediatR.Playground.API/Endpoints/NotificationEndpoint.cs`](../src/MediatR.Playground.API/Endpoints/NotificationEndpoint.cs)

## See Also

- [Priority Notification Publisher](./priority-notification-publisher.md) — priority-ordered handler execution
- [Pipelines](./pipelines.md) — pipeline behavior overview and registration order
