using MediatR.Playground.Model.Notifications;
using Microsoft.Extensions.Logging;

namespace MediatR.Playground.Domain.NotificationHandler;

internal class SamplePriorityNotificationFourthHandler(
    ILogger<SamplePriorityNotificationFourthHandler> logger
) : INotificationHandler<SamplePriorityNotification>
{
    public async Task Handle(
        SamplePriorityNotification notification,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "A notification hit following handler: {handler} Notification Content: Id={Id};NotificationTime={EventTime}",
            nameof(SamplePriorityNotificationFourthHandler),
            notification.Id,
            notification.NotificationTime
        );
    }
}
