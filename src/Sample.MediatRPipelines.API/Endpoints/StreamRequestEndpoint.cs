using MediatR;
using Sample.MediatRPipelines.API.Endpoints.Primitives;
using Sample.MediatRPipelines.Domain.Queries;

namespace Sample.MediatRPipelines.API.Endpoints;

public class StreamRequestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/SampleStreamEntity",
                async (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    return mediator.CreateStream(new SampleStreamEntityQuery(), cancellationToken);
                }
            )
            .WithName("SampleStreamEntity")
            .Produces<IAsyncEnumerable<SampleStreamEntityQueryComplete>>()
            .WithOpenApi();
    }
}
