using MediatR;
using Sample.MediatRPipelines.API.Endpoints.Primitives;
using Sample.MediatRPipelines.Domain.Queries;

namespace Sample.MediatRPipelines.API.Endpoints;

public class StreamRequestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/SampleEntity",
                (IMediator mediator) =>
                {
                    return mediator.Send(new SampleStreamEntityQuery());
                }
            )
            .WithName("SampleEntity")
            .WithOpenApi();
    }
}
