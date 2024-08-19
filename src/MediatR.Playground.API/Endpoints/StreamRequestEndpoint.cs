using MediatR.Playground.API.Endpoints.Primitives;
using MediatR.Playground.Model.Queries.StreamEntity;
using MediatR.Playground.Model.Queries.StreamEntityWithFilter;

namespace MediatR.Playground.API.Endpoints;

public class StreamRequestEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/StreamRequests").WithTags("Stream Requests Endpoints");

        group
            .MapGet(
                "/SampleStreamEntity",
                (IMediator mediator, CancellationToken cancellationToken) =>
                {
                    return mediator.CreateStream(new SampleStreamEntityQuery(), cancellationToken);
                }
            )
            .WithName("SampleStreamEntity")
            .Produces<IAsyncEnumerable<SampleStreamEntityQueryResult>>()
            .WithOpenApi();

        group
            .MapGet(
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
