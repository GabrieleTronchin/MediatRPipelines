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
                "/Notify",
                (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    return mediator.Publish(
                        new SampleNotification()
                        {
                            Id = new Guid(),
                            NotificationTime = DateTime.Now,
                        }
                    );
                }
            )
            .WithName("Notify")
            .WithOpenApi();
    }
}
