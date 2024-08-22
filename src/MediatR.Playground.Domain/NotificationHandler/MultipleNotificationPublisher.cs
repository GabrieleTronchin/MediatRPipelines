using MediatR.NotificationPublishers;
using MediatR.Playground.Model.Primitives.Notifications;

namespace MediatR.Playground.Domain.NotificationHandler;

public class MultipleNotificationPublisher : INotificationPublisher
{
    private readonly TaskWhenAllPublisher taskWhenAllPublisher;
    private readonly ForeachAwaitPublisher foreachAwaitPublisher;
    private readonly PriorityNotificationPublisher priorityNotificationPublisher;

    public MultipleNotificationPublisher()
    {
        taskWhenAllPublisher = new TaskWhenAllPublisher();
        foreachAwaitPublisher = new ForeachAwaitPublisher();
        priorityNotificationPublisher = new PriorityNotificationPublisher();
    }

    public async Task Publish(
        IEnumerable<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken
    )
    {
        if (notification is IPriorityNotification)
        {
            await priorityNotificationPublisher.Publish(handlerExecutors, notification, cancellationToken);
        }
        else if (notification is IParallelNotification)
        {
            await taskWhenAllPublisher
                .Publish(handlerExecutors, notification, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            await foreachAwaitPublisher.Publish(handlerExecutors, notification, cancellationToken);
        }
    }
}
