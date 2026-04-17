# Priority Notification Publisher

[← Back to README](../README.md)

> **Note:** This documentation was AI-generated based on the original article:
> [C# .NET 8 — MediatR Notifications and Notification Publisher](https://medium.com/@gabrieletronchin/c-net-8-mediatr-notifications-and-notification-publisher-b72a36f0e9ee).
> It is intended as a companion reference for the code in this repository.

## Overview

The `PriorityNotificationPublisher` is a custom `INotificationPublisher` that executes notification handlers **sequentially**, ordered by a priority value. Handlers with lower priority numbers run first. This gives you explicit control over handler execution order — something the built-in MediatR publishers do not support.

This publisher is used automatically when a notification implements the `IPriorityNotification` marker interface. The `MultipleNotificationPublisher` (the publisher registered in DI) checks the notification type and delegates to `PriorityNotificationPublisher` for any `IPriorityNotification`.

For the broader notification system and how the publisher is selected, see [Notifications and Notification Publishers](./notifications.md).

## How It Works

When `PriorityNotificationPublisher.Publish` is called, it:

1. **Reads the priority** from each handler instance using reflection
2. **Groups handlers** by their priority value using `ToLookup`
3. **Orders the groups** by priority in ascending order (lowest number first)
4. **Executes handlers sequentially** within each group, awaiting each one before moving to the next

Handlers that implement `IPriorityNotificationHandler<TNotification>` expose a `Priority` property. The publisher reads this property via reflection by looking for a property named `Priority` on the handler instance's type. If a handler does not have a `Priority` property (i.e., it implements `INotificationHandler<T>` directly instead of `IPriorityNotificationHandler<T>`), it is assigned a default priority of **99**, placing it at the end of the execution order.

Source: [`../src/MediatR.Playground.Domain/NotificationHandler/PriorityNotificationPublisher.cs`](../src/MediatR.Playground.Domain/NotificationHandler/PriorityNotificationPublisher.cs)

## IPriorityNotification

`IPriorityNotification` is a marker interface that extends `INotification`. It carries no additional members — its sole purpose is to signal that the notification should be dispatched through `PriorityNotificationPublisher`.

```csharp
public interface IPriorityNotification : INotification { }
```

Any notification class that implements `IPriorityNotification` will be routed to the priority publisher by `MultipleNotificationPublisher`.

Source: [`../src/MediatR.Playground.Model/Primitives/Notifications/IPriorityNotification.cs`](../src/MediatR.Playground.Model/Primitives/Notifications/IPriorityNotification.cs)

## IPriorityNotificationHandler

`IPriorityNotificationHandler<TNotification>` extends `INotificationHandler<TNotification>` and adds a `Priority` property. This is the interface handlers implement to declare their execution priority.

```csharp
public interface IPriorityNotificationHandler<in TNotification>
    : INotificationHandler<TNotification>
    where TNotification : IPriorityNotification
{
    public int Priority { get; }
}
```

- **`Priority`** — An integer that determines execution order. Lower values run first.
- The generic constraint `where TNotification : IPriorityNotification` ensures this interface is only used with priority-enabled notifications.

Source: [`../src/MediatR.Playground.Model/Primitives/Notifications/IPriorityNotificationHandler.cs`](../src/MediatR.Playground.Model/Primitives/Notifications/IPriorityNotificationHandler.cs)

## Reflection Mechanism

The publisher uses reflection to read the `Priority` value from each handler at publish time. The `GetPriority` method:

1. Calls `handler.GetType().GetProperties()` to get all public properties on the handler instance
2. Looks for a property named `Priority` (matched against `nameof(IPriorityNotificationHandler<IPriorityNotification>.Priority)`)
3. If found, reads the value with `GetValue(handler)` and parses it to an integer
4. If not found, returns the default priority of **99**

```csharp
private int GetPriority(object handler)
{
    var priority = handler
        .GetType()
        .GetProperties()
        .FirstOrDefault(t =>
            t.Name == nameof(IPriorityNotificationHandler<IPriorityNotification>.Priority)
        );

    if (priority == null)
        return DEFAULT_PRIORITY;

    return int.Parse(priority.GetValue(handler)?.ToString() ?? DEFAULT_PRIORITY.ToString());
}
```

This reflection-based approach means the publisher does not require handlers to implement `IPriorityNotificationHandler` — it works with any handler that has a public `Priority` property. However, using the `IPriorityNotificationHandler<T>` interface is the recommended approach since it provides compile-time type safety and makes the intent explicit.

## Default Priority

Handlers that do not have a `Priority` property are assigned a default priority of **99**. This means:

- Handlers implementing `IPriorityNotificationHandler<T>` with explicit priorities will generally run before handlers that only implement `INotificationHandler<T>`
- You can use any integer value for priority — there is no fixed range
- Multiple handlers can share the same priority value; they will be grouped together and executed sequentially within that group

## Creating a Priority Notification Handler

### Step 1: Define the Notification

Create a notification class that implements `IPriorityNotification`:

```csharp
using MediatR.Playground.Model.Primitives.Notifications;

public class SamplePriorityNotification : IPriorityNotification
{
    public Guid Id { get; set; }
    public DateTime NotificationTime { get; set; }
}
```

Source: [`../src/MediatR.Playground.Model/Notifications/SamplePriorityNotification.cs`](../src/MediatR.Playground.Model/Notifications/SamplePriorityNotification.cs)

### Step 2: Create Handlers with Priority

Implement `IPriorityNotificationHandler<TNotification>` and set the `Priority` property. Lower values execute first:

```csharp
using MediatR.Playground.Model.Notifications;
using MediatR.Playground.Model.Primitives.Notifications;
using Microsoft.Extensions.Logging;

internal class SamplePriorityNotificationThirdHandler(
    ILogger<SamplePriorityNotificationThirdHandler> logger
) : IPriorityNotificationHandler<SamplePriorityNotification>
{
    public int Priority => 1;

    public async Task Handle(
        SamplePriorityNotification notification,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "Handler: {handler}, Id={Id}, Time={EventTime}",
            nameof(SamplePriorityNotificationThirdHandler),
            notification.Id,
            notification.NotificationTime
        );
    }
}
```

Source: [`../src/MediatR.Playground.Domain/NotificationHandler/Priority/SamplePriorityNotificationThirdHandler.cs`](../src/MediatR.Playground.Domain/NotificationHandler/Priority/SamplePriorityNotificationThirdHandler.cs)

### Step 3: Mix Priority and Non-Priority Handlers (Optional)

You can also register a standard `INotificationHandler<T>` for the same notification. It will still be invoked, but without a `Priority` property it defaults to priority **99** and runs last:

```csharp
internal class SamplePriorityNotificationFourthHandler(
    ILogger<SamplePriorityNotificationFourthHandler> logger
) : INotificationHandler<SamplePriorityNotification>
{
    public async Task Handle(
        SamplePriorityNotification notification,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "Handler: {handler}, Id={Id}, Time={EventTime}",
            nameof(SamplePriorityNotificationFourthHandler),
            notification.Id,
            notification.NotificationTime
        );
    }
}
```

Source: [`../src/MediatR.Playground.Domain/NotificationHandler/Priority/SamplePriorityNotificationFourthHandler.cs`](../src/MediatR.Playground.Domain/NotificationHandler/Priority/SamplePriorityNotificationFourthHandler.cs)

## Execution Order Example

This project registers four handlers for `SamplePriorityNotification`:

| Handler | Implements | Priority | Execution Order |
|---------|-----------|----------|-----------------|
| `SamplePriorityNotificationThirdHandler` | `IPriorityNotificationHandler` | 1 | 1st |
| `SamplePriorityNotificationSecondHandler` | `IPriorityNotificationHandler` | 2 | 2nd |
| `SamplePriorityNotificationFirstHandler` | `IPriorityNotificationHandler` | 3 | 3rd |
| `SamplePriorityNotificationFourthHandler` | `INotificationHandler` | 99 (default) | 4th |

Despite the class names, the execution order is determined entirely by the `Priority` value — not by the handler name or registration order. `ThirdHandler` runs first because it has the lowest priority value (1), and `FourthHandler` runs last because it lacks a `Priority` property and defaults to 99.

Source: [`../src/MediatR.Playground.Domain/NotificationHandler/Priority/`](../src/MediatR.Playground.Domain/NotificationHandler/Priority/)

## Integration with MultipleNotificationPublisher

`PriorityNotificationPublisher` is not registered directly in the DI container. Instead, `MultipleNotificationPublisher` creates an instance internally and delegates to it when the notification implements `IPriorityNotification`:

```csharp
if (notification is IPriorityNotification)
{
    await priorityNotificationPublisher.Publish(handlerExecutors, notification, cancellationToken);
}
```

This means you do not need to configure anything beyond implementing `IPriorityNotification` on your notification class. The routing happens automatically.

Source: [`../src/MediatR.Playground.Domain/NotificationHandler/MultipleNotificationPublisher.cs`](../src/MediatR.Playground.Domain/NotificationHandler/MultipleNotificationPublisher.cs)

## API Endpoint

The priority notification can be tested through the Swagger UI:

| Endpoint | Notification Type | Strategy |
|----------|------------------|----------|
| `POST /Notifications/SamplePriorityNotification` | `SamplePriorityNotification` | Priority-ordered sequential |

Check the application logs after calling this endpoint to see the handlers execute in priority order.

Source: [`../src/MediatR.Playground.API/Endpoints/NotificationEndpoint.cs`](../src/MediatR.Playground.API/Endpoints/NotificationEndpoint.cs)

## See Also

- [Notifications](./notifications.md) — notification system overview and publishing strategies
- [Pipelines](./pipelines.md) — pipeline behavior overview and registration order
