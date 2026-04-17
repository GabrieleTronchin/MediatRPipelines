using MediatR.Playground.API.Endpoints.Primitives;
using MediatR.Playground.Model.Notifications;

namespace MediatR.Playground.API.Endpoints;

public class NotificationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/Notifications").WithTags("Notifications Endpoints");

        group
            .MapPost(
                "/SequentialNotification",
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var notification = new SampleNotification()
                    {
                        Id = Guid.NewGuid(),
                        NotificationTime = DateTime.Now,
                    };
                    await mediator.Publish(notification, cancellationToken);
                    return new { notification.Id, notification.NotificationTime, Type = "Sequential" };
                }
            )
            .WithName("SequentialNotification")
            .WithSummary("Publish a sequential notification")
            .WithDescription("Publishes a notification that is handled sequentially by all registered notification handlers, one at a time in order.")
            .Produces(StatusCodes.Status200OK, typeof(object));

        group
            .MapPost(
                "/ParallelNotification",
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var notification = new SampleParallelNotification()
                    {
                        Id = Guid.NewGuid(),
                        NotificationTime = DateTime.Now,
                    };
                    await mediator.Publish(notification, cancellationToken);
                    return new { notification.Id, notification.NotificationTime, Type = "Parallel" };
                }
            )
            .WithName("ParallelNotification")
            .WithSummary("Publish a parallel notification")
            .WithDescription("Publishes a notification that is handled in parallel by all registered notification handlers using Task.WhenAll.")
            .Produces(StatusCodes.Status200OK, typeof(object));

        group
            .MapPost(
                "/SamplePriorityNotification",
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var notification = new SamplePriorityNotification()
                    {
                        Id = Guid.NewGuid(),
                        NotificationTime = DateTime.Now,
                    };
                    await mediator.Publish(notification, cancellationToken);
                    return new { notification.Id, notification.NotificationTime, Type = "Priority" };
                }
            )
            .WithName("SamplePriorityNotification")
            .WithSummary("Publish a priority notification")
            .WithDescription("Publishes a notification that is handled by notification handlers in priority order, using the custom PriorityNotificationPublisher.")
            .Produces(StatusCodes.Status200OK, typeof(object));

        group
            .MapPost(
                "/DeduplicationNotification",
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var notification = new DeduplicationNotification()
                    {
                        Id = Guid.NewGuid(),
                        NotificationTime = DateTime.Now,
                    };
                    await mediator.Publish(notification, cancellationToken);
                    return new { notification.Id, notification.NotificationTime, Type = "Deduplication" };
                }
            )
            .WithName("DeduplicationNotification")
            .WithSummary("Publish a deduplication notification")
            .WithDescription("Publishes a notification that demonstrates MediatR 14's handler de-duplication feature. The handler is registered twice but executes only once per publish call.")
            .Produces(StatusCodes.Status200OK, typeof(object));
    }
}
