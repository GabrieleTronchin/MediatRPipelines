using MediatR.NotificationPublishers;
using MediatR.Playground.Model.Primitives.Notifications;

namespace MediatR.Playground.Domain.NotificationHandler;

public class CustomNotificationPublisher : INotificationPublisher
{
    private const int DEFAULT_PRIORITY = 99;
    private readonly TaskWhenAllPublisher taskWhenAllPublisher;
    private readonly ForeachAwaitPublisher foreachAwaitPublisher;

    public CustomNotificationPublisher()
    {
        taskWhenAllPublisher = new TaskWhenAllPublisher();
        foreachAwaitPublisher = new ForeachAwaitPublisher();
    }

    /// <summary>
    /// This is just a sample of custom publisher
    /// </summary>
    /// <param name="handlerExecutors"></param>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Publish(
        IEnumerable<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken
    )
    {
        if (notification is IPriorityNotification)
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
        else if (notification is IParallelNotification)
        {
            await foreachAwaitPublisher
                .Publish(handlerExecutors, notification, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            await taskWhenAllPublisher.Publish(handlerExecutors, notification, cancellationToken);
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
