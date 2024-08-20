using MediatR.Playground.Model.Primitives.Notifications;

namespace MediatR.Playground.Domain.NotificationHandler;

public class CustomNotificationPublisher : INotificationPublisher
{
    private const int DEFAULT_PRIORITY = 99;

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

            try
            {

                var lookUp = handlerExecutors
                             .ToLookup(x => GetPriority(x.HandlerInstance), k => k)
                             .OrderBy(x => x.Key);

                foreach (var handler in lookUp)
                {

                    foreach (var notificationHandler in handler.ToList()) {

                        await notificationHandler
                       .HandlerCallback(notification, cancellationToken)
                       .ConfigureAwait(false);

                    }

  
                }
            }
            catch (Exception ex)
            {

                throw;
            }


          

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



    private int GetPriority(object handler)
    {

        var priority = handler.GetType().GetProperties().FirstOrDefault(t => t.Name == nameof(IPriorityNotificationHandler<IPriorityNotification>.Priority));

        if (priority == null)
            return DEFAULT_PRIORITY;


        return int.Parse(priority.GetValue(handler)?.ToString() ?? "0");


    }

}
