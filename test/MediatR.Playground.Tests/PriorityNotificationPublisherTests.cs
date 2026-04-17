using MediatR.Playground.Domain.NotificationHandler;
using MediatR.Playground.Model.Primitives.Notifications;
using Xunit;

namespace MediatR.Playground.Tests;

public class PriorityNotificationPublisherTests
{
    private class TestPriorityNotification : IPriorityNotification;

    // Handler with an explicit Priority property (discoverable via reflection)
    private class PriorityHandlerWithValue
    {
        public int Priority { get; }

        public PriorityHandlerWithValue(int priority)
        {
            Priority = priority;
        }
    }

    // Handler without a Priority property — should receive default priority 99
    private class HandlerWithoutPriority;

    /// <summary>
    /// Validates: Requirement 7.1
    /// WHEN a notification is published via PriorityNotificationPublisher with handlers having different priorities,
    /// handlers are executed in ascending priority order (lower numeric value first).
    /// </summary>
    [Fact]
    public async Task Publish_HandlersWithDifferentPriorities_ExecutesInAscendingOrder()
    {
        // Arrange
        var publisher = new PriorityNotificationPublisher();
        var notification = new TestPriorityNotification();
        var executionOrder = new List<int>();

        // Create handlers with priorities: 10, 1, 5 — expected execution order: 1, 5, 10
        var handler1 = new PriorityHandlerWithValue(10);
        var handler2 = new PriorityHandlerWithValue(1);
        var handler3 = new PriorityHandlerWithValue(5);

        var executors = new[]
        {
            new NotificationHandlerExecutor(handler1, (n, ct) =>
            {
                executionOrder.Add(10);
                return Task.CompletedTask;
            }),
            new NotificationHandlerExecutor(handler2, (n, ct) =>
            {
                executionOrder.Add(1);
                return Task.CompletedTask;
            }),
            new NotificationHandlerExecutor(handler3, (n, ct) =>
            {
                executionOrder.Add(5);
                return Task.CompletedTask;
            })
        };

        // Act
        await publisher.Publish(executors, notification, CancellationToken.None);

        // Assert — handlers executed in ascending priority order
        Assert.Equal(new[] { 1, 5, 10 }, executionOrder);
    }

    /// <summary>
    /// Validates: Requirement 7.2
    /// WHEN a handler does not implement IPriorityNotificationHandler and has no Priority property,
    /// PriorityNotificationPublisher assigns the default priority (99) to that handler.
    /// </summary>
    [Fact]
    public async Task Publish_HandlerWithoutPriorityProperty_ReceivesDefaultPriority99()
    {
        // Arrange
        var publisher = new PriorityNotificationPublisher();
        var notification = new TestPriorityNotification();
        var executionOrder = new List<string>();

        // Handler with priority 50 should execute before the handler without priority (default 99)
        var handlerWithPriority = new PriorityHandlerWithValue(50);
        var handlerWithoutPriority = new HandlerWithoutPriority();

        var executors = new[]
        {
            // Place the handler without priority first in the list to prove ordering works
            new NotificationHandlerExecutor(handlerWithoutPriority, (n, ct) =>
            {
                executionOrder.Add("no-priority (default 99)");
                return Task.CompletedTask;
            }),
            new NotificationHandlerExecutor(handlerWithPriority, (n, ct) =>
            {
                executionOrder.Add("priority-50");
                return Task.CompletedTask;
            })
        };

        // Act
        await publisher.Publish(executors, notification, CancellationToken.None);

        // Assert — handler with priority 50 executes before handler with default priority 99
        Assert.Equal(2, executionOrder.Count);
        Assert.Equal("priority-50", executionOrder[0]);
        Assert.Equal("no-priority (default 99)", executionOrder[1]);
    }
}
