﻿using MediatR.NotificationPublishers;
using MediatR.Playground.Model.Primitives.Notifications;

namespace MediatR.Playground.Domain.NotificationHandler;

public class CustomNotificationPublisher : INotificationPublisher
{
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
        if (notification is IParallelNotification)
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
