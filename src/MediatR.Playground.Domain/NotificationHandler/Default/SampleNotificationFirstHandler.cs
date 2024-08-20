using MediatR.Playground.Model.Notifications;
using Microsoft.Extensions.Logging;

namespace MediatR.Playground.Domain.NotificationHandler.Default;

internal class SampleNotificationFirstHandler(ILogger<SampleNotificationFirstHandler> logger)
    : INotificationHandler<SampleNotification>
{
    public async Task Handle(SampleNotification notification, CancellationToken cancellationToken)
    {
        await Task.Delay(1000);

        logger.LogInformation(
            "A notification hit following handler: {handler} Notification Content: Id={Id};NotificationTime={EventTime}",
            nameof(SampleNotificationFirstHandler),
            notification.Id,
            notification.NotificationTime
        );
    }
}
