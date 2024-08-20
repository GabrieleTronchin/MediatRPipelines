using MediatR.Playground.Model.Notifications;
using Microsoft.Extensions.Logging;

namespace MediatR.Playground.Domain.NotificationHandler.Parallel;

internal class SampleParallelNotificationFirstHandler(
    ILogger<SampleParallelNotificationFirstHandler> logger
) : INotificationHandler<SampleParallelNotification>
{
    public async Task Handle(
        SampleParallelNotification notification,
        CancellationToken cancellationToken
    )
    {
        // Add  delay before log just to prove that task are on parallel
        await Task.Delay(1000);

        logger.LogInformation(
            "A notification hit following handler: {handler} Notification Content: Id={Id};NotificationTime={EventTime}",
            nameof(SampleParallelNotificationFirstHandler),
            notification.Id,
            notification.NotificationTime
        );
    }
}
