using MediatR;
using MediatR.Playground.API.Endpoints.Primitives;
using MediatR.Playground.Model.Queries.StreamEntity;
using MediatR.Playground.Model.Queries.StreamEntityWithFilter;

namespace MediatR.Playground.API.Endpoints;

public class StreamRequestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/SampleStreamEntity",
                (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    return mediator.CreateStream(new SampleStreamEntityQuery(), cancellationToken);
                }
            )
            .WithName("SampleStreamEntity")
            .Produces<IAsyncEnumerable<SampleStreamEntityQueryResult>>()
            .WithOpenApi();

        app.MapGet(
                "/SampleStreamEntityWithPipeFilter",
                (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    return mediator.CreateStream(
                        new SampleStreamEntityWithPipeFilterQuery(),
                        cancellationToken
                    );
                }
            )
            .WithName("SampleStreamEntityWithPipeFilter")
            .Produces<IAsyncEnumerable<SampleStreamEntityWithPipeFilterQueryResult>>()
            .WithOpenApi();
    }
}
