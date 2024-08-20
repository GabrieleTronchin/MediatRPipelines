using MediatR.Playground.Model.Notifications;
using MediatR.Playground.Model.Primitives.Notifications;
using Microsoft.Extensions.Logging;

namespace MediatR.Playground.Domain.NotificationHandler;

internal class SamplePriorityNotificationThirdHandler(
    ILogger<SamplePriorityNotificationThirdHandler> logger
) : IPriorityNotificationHandler<SamplePriorityNotification>
{
    public int Priority => 1;

    public async Task Handle(
        SamplePriorityNotification notification,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "A notification hit following handler: {handler} Notification Content: Id={Id};NotificationTime={EventTime}",
            nameof(SamplePriorityNotificationThirdHandler),
            notification.Id,
            notification.NotificationTime
        );
    }
}
