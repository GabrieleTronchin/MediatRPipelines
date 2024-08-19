using MediatR.Playground.Model.Notifications;
using Microsoft.Extensions.Logging;

namespace MediatR.Playground.Domain.NotificationHandler;

internal class SampleNotificationSecondHandler(ILogger<SampleNotificationSecondHandler> logger)
    : INotificationHandler<SampleNotification>
{
    public async Task Handle(SampleNotification notification, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "A notification hit following handler: {handler} Notification Content: Id={Id};NotificationTime={EventTime}",
            nameof(SampleNotificationSecondHandler),
            notification.Id,
            notification.NotificationTime
        );
    }
}
