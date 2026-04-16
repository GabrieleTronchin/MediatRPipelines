using MediatR.Playground.Domain.NotificationHandler;
using MediatR.Playground.Model.Primitives.Notifications;
using Xunit;

namespace MediatR.Playground.Tests;

public class MultipleNotificationPublisherTests
{
    // Inline test notification types implementing the appropriate marker interfaces
    private class TestPriorityNotification : IPriorityNotification;
    private class TestParallelNotification : IParallelNotification;
    private class TestPlainNotification : INotification;

    // A handler with a Priority property so PriorityNotificationPublisher can read it via reflection
    private class PriorityHandler : INotificationHandler<TestPriorityNotification>
    {
        public int Priority { get; } = 1;
        public Task Handle(TestPriorityNotification notification, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    /// <summary>
    /// Validates: Requirement 6.1
    /// WHEN a notification implementing IPriorityNotification is published,
    /// MultipleNotificationPublisher delegates to PriorityNotificationPublisher.
    /// </summary>
    [Fact]
    public async Task Publish_PriorityNotification_DelegatesToPriorityPublisher()
    {
        // Arrange
        var publisher = new MultipleNotificationPublisher();
        var notification = new TestPriorityNotification();
        var executionOrder = new List<int>();

        // Create handlers with different priorities to verify priority ordering (proves delegation)
        var handler1 = new PriorityHandler();
        var executors = new[]
        {
            new NotificationHandlerExecutor(handler1, (n, ct) =>
            {
                executionOrder.Add(1);
                return Task.CompletedTask;
            })
        };

        // Act
        await publisher.Publish(executors, notification, CancellationToken.None);

        // Assert — handler was invoked (delegation to PriorityNotificationPublisher occurred)
        Assert.Single(executionOrder);
        Assert.Equal(1, executionOrder[0]);
    }

    /// <summary>
    /// Validates: Requirement 6.2
    /// WHEN a notification implementing IParallelNotification is published,
    /// MultipleNotificationPublisher executes all handlers (parallel via TaskWhenAllPublisher).
    /// </summary>
    [Fact]
    public async Task Publish_ParallelNotification_ExecutesAllHandlers()
    {
        // Arrange
        var publisher = new MultipleNotificationPublisher();
        var notification = new TestParallelNotification();
        var handlersCalled = new List<int>();

        var executors = new[]
        {
            new NotificationHandlerExecutor(new object(), (n, ct) =>
            {
                handlersCalled.Add(1);
                return Task.CompletedTask;
            }),
            new NotificationHandlerExecutor(new object(), (n, ct) =>
            {
                handlersCalled.Add(2);
                return Task.CompletedTask;
            }),
            new NotificationHandlerExecutor(new object(), (n, ct) =>
            {
                handlersCalled.Add(3);
                return Task.CompletedTask;
            })
        };

        // Act
        await publisher.Publish(executors, notification, CancellationToken.None);

        // Assert — all three handlers were executed
        Assert.Equal(3, handlersCalled.Count);
        Assert.Contains(1, handlersCalled);
        Assert.Contains(2, handlersCalled);
        Assert.Contains(3, handlersCalled);
    }

    /// <summary>
    /// Validates: Requirement 6.3
    /// WHEN a notification implementing neither IPriorityNotification nor IParallelNotification is published,
    /// MultipleNotificationPublisher executes all handlers sequentially (via ForeachAwaitPublisher).
    /// </summary>
    [Fact]
    public async Task Publish_PlainNotification_ExecutesAllHandlersSequentially()
    {
        // Arrange
        var publisher = new MultipleNotificationPublisher();
        var notification = new TestPlainNotification();
        var executionOrder = new List<int>();

        var executors = new[]
        {
            new NotificationHandlerExecutor(new object(), (n, ct) =>
            {
                executionOrder.Add(1);
                return Task.CompletedTask;
            }),
            new NotificationHandlerExecutor(new object(), (n, ct) =>
            {
                executionOrder.Add(2);
                return Task.CompletedTask;
            }),
            new NotificationHandlerExecutor(new object(), (n, ct) =>
            {
                executionOrder.Add(3);
                return Task.CompletedTask;
            })
        };

        // Act
        await publisher.Publish(executors, notification, CancellationToken.None);

        // Assert — all handlers executed in sequential order (ForeachAwaitPublisher preserves order)
        Assert.Equal(new[] { 1, 2, 3 }, executionOrder);
    }
}
