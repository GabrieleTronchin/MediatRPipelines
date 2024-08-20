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
                (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    return mediator.Publish(
                        new SampleNotification()
                        {
                            Id = Guid.NewGuid(),
                            NotificationTime = DateTime.Now,
                        },
                        cancellationToken
                    );
                }
            )
            .WithName("SequentialNotification")
            .WithOpenApi();

        group
            .MapPost(
                "/ParallelNotification",
                (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var publisher = mediator.Publish(
                        new SampleParallelNotification()
                        {
                            Id = Guid.NewGuid(),
                            NotificationTime = DateTime.Now,
                        },
                        cancellationToken
                    );
                }
            )
            .WithName("ParallelNotification")
            .WithOpenApi();

        group
            .MapPost(
                "/SamplePriorityNotification",
                (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    var publisher = mediator.Publish(
                        new SamplePriorityNotification()
                        {
                            Id = Guid.NewGuid(),
                            NotificationTime = DateTime.Now,
                        },
                        cancellationToken
                    );
                }
            )
            .WithName("SamplePriorityNotification")
            .WithOpenApi();
    }
}
