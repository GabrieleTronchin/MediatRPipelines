using System.Collections.Concurrent;
using MediatR.Playground.Model.Notifications;
using Microsoft.Extensions.Logging;

namespace MediatR.Playground.Domain.NotificationHandler.Deduplication;

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
