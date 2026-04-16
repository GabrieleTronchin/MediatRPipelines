# MediatR Notifications and Notification Publishers

## Overview

MediatR supports a publish-subscribe pattern through `INotification` and `INotificationHandler<TNotification>`. Unlike requests (which have a single handler and return a response), notifications are dispatched to **all** registered handlers and do not return a value. This makes them well-suited for broadcasting events across multiple subscribers — logging, cache invalidation, side effects, and similar cross-cutting concerns.

The way notifications are delivered to their handlers is controlled by the **notification publisher**. MediatR ships with two built-in publishers, and this project implements custom publishers that select a delivery strategy based on the notification type.

## INotification and INotificationHandler

A notification is any class that implements `INotification`. A handler is any class that implements `INotificationHandler<TNotification>`. When you call `mediator.Publish(notification)`, MediatR resolves all registered handlers for that notification type and invokes them through the configured publisher.

A single notification can have multiple handlers. For example, `SampleNotification` has two handlers — `SampleNotificationFirstHandler` and `SampleNotificationSecondHandler` — both of which log the notification content when invoked.

Source: [`../src/MediatR.Playground.Model/Notifications/SampleNotification.cs`](../src/MediatR.Playground.Model/Notifications/SampleNotification.cs)
Source: [`../src/MediatR.Playground.Domain/NotificationHandler/Default/`](../src/MediatR.Playground.Domain/NotificationHandler/Default/)

## Built-in Publishing Strategies

MediatR provides two built-in notification publishers in the `MediatR.NotificationPublishers` namespace. Both implement `INotificationPublisher` and differ in how they invoke the list of handlers.

### ForeachAwaitPublisher (Sequential)

`ForeachAwaitPublisher` iterates through the handlers one at a time, awaiting each before moving to the next. Handlers execute in sequence — the second handler does not start until the first completes.

- **Execution order:** Sequential, one handler at a time
- **Error behavior:** If a handler throws, subsequent handlers are **not** invoked. The exception propagates immediately.
- **Use case:** When handler ordering matters or when handlers share state that requires sequential access

This is the default publisher used by MediatR when no custom publisher is configured.

### TaskWhenAllPublisher (Parallel)

`TaskWhenAllPublisher` starts all handlers concurrently using `Task.WhenAll`. Handlers run in parallel and the publisher awaits all of them to complete.

- **Execution order:** Parallel, all handlers start at the same time
- **Error behavior:** All handlers run to completion (or failure). If one or more handlers throw, the exceptions are collected into an `AggregateException`.
- **Use case:** When handlers are independent and can safely run concurrently, improving throughput

## Custom Notification Publishers

This project implements custom publishers that go beyond the two built-in options. They implement `INotificationPublisher` and compose the built-in publishers with additional logic.

### MultipleNotificationPublisher

`MultipleNotificationPublisher` is the publisher registered in the DI container for this project. It selects a publishing strategy based on the notification's type by checking which marker interface the notification implements:

1. **`IPriorityNotification`** → delegates to `PriorityNotificationPublisher` (sequential, ordered by handler priority)
2. **`IParallelNotification`** → delegates to `TaskWhenAllPublisher` (parallel)
3. **Default** (no marker interface) → delegates to `ForeachAwaitPublisher` (sequential)

This approach lets you control the delivery strategy per notification type without changing handler code. A notification that implements `IParallelNotification` will always be published in parallel, while a plain `INotification` will be published sequentially.

Source: [`../src/MediatR.Playground.Domain/NotificationHandler/MultipleNotificationPublisher.cs`](../src/MediatR.Playground.Domain/NotificationHandler/MultipleNotificationPublisher.cs)

### CustomNotificationPublisher

`CustomNotificationPublisher` is a simpler variant that only distinguishes between parallel and sequential delivery. It checks for `IParallelNotification` and delegates to `TaskWhenAllPublisher`; everything else goes through `ForeachAwaitPublisher`. It does not support priority-based publishing.

Source: [`../src/MediatR.Playground.Domain/NotificationHandler/CustomNotificationPublisher.cs`](../src/MediatR.Playground.Domain/NotificationHandler/CustomNotificationPublisher.cs)

### PriorityNotificationPublisher

`PriorityNotificationPublisher` executes handlers sequentially but orders them by a `Priority` property. Handlers that implement `IPriorityNotificationHandler<TNotification>` expose a `Priority` value; the publisher reads it via reflection and groups handlers by priority (lower values run first). Handlers without a priority property default to priority `99`.

For full details on the priority publisher, see [Priority Notification Publisher](./priority-notification-publisher.md).

Source: [`../src/MediatR.Playground.Domain/NotificationHandler/PriorityNotificationPublisher.cs`](../src/MediatR.Playground.Domain/NotificationHandler/PriorityNotificationPublisher.cs)

## Marker Interfaces

The project defines marker interfaces that extend `INotification` to tag notifications with their intended delivery strategy. `MultipleNotificationPublisher` inspects these interfaces at publish time to select the appropriate publisher.

| Interface | Inherits From | Purpose |
|-----------|---------------|---------|
| `IParallelNotification` | `INotification` | Marks notifications for parallel delivery via `TaskWhenAllPublisher` |
| `IPriorityNotification` | `INotification` | Marks notifications for priority-ordered delivery via `PriorityNotificationPublisher` |
| `IDataUpdateNotification` | `INotification` | Marks data update notifications; includes `Id` and `CacheKey` properties for cache invalidation scenarios |

`IParallelNotification` and `IPriorityNotification` are pure marker interfaces with no additional members. `IDataUpdateNotification` adds `Id` (Guid) and `CacheKey` (string) properties, making it useful for notifications that need to carry cache invalidation metadata.

Source: [`../src/MediatR.Playground.Model/Primitives/Notifications/`](../src/MediatR.Playground.Model/Primitives/Notifications/)

## Notification Models in This Project

| Notification | Implements | Delivery Strategy |
|-------------|------------|-------------------|
| `SampleNotification` | `INotification` | Sequential (default) |
| `SampleParallelNotification` | `IParallelNotification` | Parallel |
| `SamplePriorityNotification` | `IPriorityNotification` | Priority-ordered sequential |
| `DataUpdateNotification` | `IDataUpdateNotification` | Sequential (default — `IDataUpdateNotification` is not checked by `MultipleNotificationPublisher`) |

Source: [`../src/MediatR.Playground.Model/Notifications/`](../src/MediatR.Playground.Model/Notifications/)

## Registration

The notification publisher is configured in the MediatR registration:

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ServicesExtensions).Assembly);
    cfg.NotificationPublisher = new MultipleNotificationPublisher();
    cfg.NotificationPublisherType = typeof(MultipleNotificationPublisher);
});
```

Setting both `NotificationPublisher` and `NotificationPublisherType` ensures MediatR uses `MultipleNotificationPublisher` for all notification dispatching. Notification handlers are auto-discovered from the assembly via `RegisterServicesFromAssembly`.

Source: [`../src/MediatR.Playground.Domain/ServiceExtension.cs`](../src/MediatR.Playground.Domain/ServiceExtension.cs)

## API Endpoints

The notification endpoints are exposed under the `/Notifications` group:

| Endpoint | Notification Type | Strategy |
|----------|------------------|----------|
| `POST /Notifications/SequentialNotification` | `SampleNotification` | Sequential |
| `POST /Notifications/ParallelNotification` | `SampleParallelNotification` | Parallel |
| `POST /Notifications/SamplePriorityNotification` | `SamplePriorityNotification` | Priority-ordered |

Source: [`../src/MediatR.Playground.API/Endpoints/NotificationEndpoint.cs`](../src/MediatR.Playground.API/Endpoints/NotificationEndpoint.cs)

## Further Reading

- [C# .NET — MediatR Notifications and Notification Publisher](https://medium.com/@gabrieletronchin/c-net-8-mediatr-notifications-and-notification-publisher-b72a36f0e9ee) — Medium article covering notification concepts and custom publishers in depth
