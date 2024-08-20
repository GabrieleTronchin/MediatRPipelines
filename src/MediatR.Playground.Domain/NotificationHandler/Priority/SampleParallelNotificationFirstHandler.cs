using MediatR.Playground.Model.Notifications;
using Microsoft.Extensions.Logging;
using MediatR.Playground.Model.Primitives.Notifications;


namespace MediatR.Playground.Domain.NotificationHandler;

public class SamplePriorityNotificationFirstHandler(
    ILogger<SamplePriorityNotificationFirstHandler> logger
) : IPriorityNotificationHandler<SamplePriorityNotification>
{
    public int Priority { get; } = 3;

    public async Task Handle(
        SamplePriorityNotification notification,
        CancellationToken cancellationToken
    )
    {
        // Add  delay before log just to prove that task are on parallel
        await Task.Delay(1000);

        logger.LogInformation(
            "A notification hit following handler: {handler} Notification Content: Id={Id};NotificationTime={EventTime}",
            nameof(SamplePriorityNotificationFirstHandler),
            notification.Id,
            notification.NotificationTime
        );
    }
}
