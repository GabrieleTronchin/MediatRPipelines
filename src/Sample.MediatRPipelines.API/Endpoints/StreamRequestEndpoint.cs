using MediatR;
using Sample.MediatRPipelines.API.Endpoints.Primitives;
using Sample.MediatRPipelines.Domain.Queries.StreamEntity;
using Sample.MediatRPipelines.Domain.Queries.StreamEntityWithFilter;

namespace Sample.MediatRPipelines.API.Endpoints;

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
