using MediatR.Playground.Model.Primitives.Notifications;

namespace MediatR.Playground.Domain.NotificationHandler;

public class CustomNotificationPublisher : INotificationPublisher
{
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
            throw new NotImplementedException();
        }
        else if (notification is IParallelNotification)
        {
            var tasks = handlerExecutors
                .Select(handler => handler.HandlerCallback(notification, cancellationToken))
                .ToArray();

            await Task.WhenAll(tasks);
        }
        else
        {
            foreach (var handler in handlerExecutors)
            {
                await handler
                    .HandlerCallback(notification, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
