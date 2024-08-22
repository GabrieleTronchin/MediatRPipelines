using MediatR.Playground.Model.Primitives.Notifications;

namespace MediatR.Playground.Domain.NotificationHandler;

public class PriorityNotificationPublisher : INotificationPublisher
{
    private const int DEFAULT_PRIORITY = 99;

    public async Task Publish(
        IEnumerable<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken
    )
    {
        var lookUp = handlerExecutors
                .ToLookup(key => GetPriority(key.HandlerInstance), value => value)
                .OrderBy(k => k.Key);

        foreach (var handler in lookUp)
        {
            foreach (var notificationHandler in handler.ToList())
            {
                await notificationHandler
                    .HandlerCallback(notification, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }

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
}
