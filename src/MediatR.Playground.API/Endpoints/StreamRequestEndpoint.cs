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
            .WithSummary("Stream sample entities")
            .WithDescription("Creates an async stream of sample entities using MediatR's CreateStream, demonstrating stream request handling with logging behavior.")
            .Produces<IAsyncEnumerable<SampleStreamEntityQueryResult>>();

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
            .WithSummary("Stream sample entities with pipe filter")
            .WithDescription("Creates an async stream of sample entities that passes through a filtering pipeline behavior, demonstrating stream-level pipeline processing.")
            .Produces<IAsyncEnumerable<SampleStreamEntityWithPipeFilterQueryResult>>();
    }
}
