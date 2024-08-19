using MediatR.Playground.Model.Notifications;
using Microsoft.Extensions.Logging;

namespace MediatR.Playground.Domain.NotificationHandler;

internal class SampleParallelNotificationSecondHandler(
    ILogger<SampleParallelNotificationSecondHandler> logger
) : INotificationHandler<SampleParallelNotification>
{
    public async Task Handle(
        SampleParallelNotification notification,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "A notification hit following handler: {handler} Notification Content: Id={Id};NotificationTime={EventTime}",
            nameof(SampleParallelNotificationSecondHandler),
            notification.Id,
            notification.NotificationTime
        );
    }
}
