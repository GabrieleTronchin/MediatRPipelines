# Priority Notification Publisher

[← Back to README](../README.md)

> Based on: [C# .NET 8 — MediatR Notifications and Notification Publisher](https://medium.com/@gabrieletronchin/c-net-8-mediatr-notifications-and-notification-publisher-b72a36f0e9ee)

## Overview

`PriorityNotificationPublisher` is a custom `INotificationPublisher` that executes handlers **sequentially, ordered by priority** (lower values run first). It is used automatically when a notification implements `IPriorityNotification` — the `MultipleNotificationPublisher` delegates to it based on the notification type.

For the broader notification system, see [Notifications](./notifications.md).

## How It Works

1. Reads the `Priority` property from each handler via reflection
2. Groups and orders handlers by priority (ascending)
3. Executes handlers sequentially within each group

Handlers implementing `IPriorityNotificationHandler<T>` expose a `Priority` property. Handlers without it default to priority **99** (last).

Source: [`../src/MediatR.Playground.Domain/NotificationHandler/PriorityNotificationPublisher.cs`](../src/MediatR.Playground.Domain/NotificationHandler/PriorityNotificationPublisher.cs)

## Interfaces

**`IPriorityNotification`** — marker interface extending `INotification`. Routes the notification to the priority publisher.

**`IPriorityNotificationHandler<T>`** — extends `INotificationHandler<T>`, adds `int Priority { get; }`. Lower values run first.

Source: [`../src/MediatR.Playground.Model/Primitives/Notifications/`](../src/MediatR.Playground.Model/Primitives/Notifications/)

## Execution Order Example

This project registers four handlers for `SamplePriorityNotification`:

| Handler | Implements | Priority | Order |
|---------|-----------|----------|-------|
| `SamplePriorityNotificationThirdHandler` | `IPriorityNotificationHandler` | 1 | 1st |
| `SamplePriorityNotificationSecondHandler` | `IPriorityNotificationHandler` | 2 | 2nd |
| `SamplePriorityNotificationFirstHandler` | `IPriorityNotificationHandler` | 3 | 3rd |
| `SamplePriorityNotificationFourthHandler` | `INotificationHandler` | 99 (default) | 4th |

Execution order is determined by `Priority` value, not by class name or registration order.

Source: [`../src/MediatR.Playground.Domain/NotificationHandler/Priority/`](../src/MediatR.Playground.Domain/NotificationHandler/Priority/)

## API Endpoint

| Endpoint | Notification Type | Strategy |
|----------|------------------|----------|
| `POST /Notifications/SamplePriorityNotification` | `SamplePriorityNotification` | Priority-ordered sequential |

Check the application logs to see handlers execute in priority order.

## See Also

- [Notifications](./notifications.md) — notification system overview and publishing strategies
- [Pipelines](./pipelines.md) — pipeline behavior overview and registration order
