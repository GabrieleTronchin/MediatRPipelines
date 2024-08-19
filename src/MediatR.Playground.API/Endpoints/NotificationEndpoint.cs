using MediatR.Playground.API.Endpoints.Primitives;
using MediatR.Playground.Model.Notifications;

namespace MediatR.Playground.API.Endpoints;

public class NotificationEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/Notifications").WithTags("Notifications Endpoints");

        group
            .MapGet(
                "/DefaultNotification",
                (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    return mediator.Publish(
                        new SampleNotification()
                        {
                            Id = Guid.NewGuid(),
                            NotificationTime = DateTime.Now,
                        }
                    );
                }
            )
            .WithName("DefaultNotification")
            .WithOpenApi();

        group
            .MapGet(
                "/ParallelNotification",
                (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var publisher = mediator.Publish(
                        new SampleParallelNotification()
                        {
                            Id = Guid.NewGuid(),
                            NotificationTime = DateTime.Now,
                        }
                    );
                }
            )
            .WithName("ParallelNotification")
            .WithOpenApi();

        group
            .MapGet(
                "/SamplePriorityNotification",
                (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var publisher = mediator.Publish(
                        new SamplePriorityNotification()
                        {
                            Id = Guid.NewGuid(),
                            NotificationTime = DateTime.Now,
                        }
                    );
                }
            )
            .WithName("SamplePriorityNotification")
            .WithOpenApi();
    }
}
