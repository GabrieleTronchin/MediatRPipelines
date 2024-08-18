using MediatR;
using MediatR.Playground.API.Endpoints.Primitives;
using MediatR.Playground.API.Models;
using MediatR.Playground.Model.NewFolder;
using Microsoft.AspNetCore.Mvc;
using Sample.MediatRPipelines.Domain.Queries.Entity;

namespace MediatR.Playground.API.Endpoints;

public class TransactionEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/SampleEntity",
                (IMediator mediator) =>
                {
                    return mediator.Send(new GetAllSampleEntitiesQuery());
                }
            )
            .WithName("SampleEntity")
            .WithOpenApi();

        app.MapGet(
                "/SampleEntity/{id}",
                (Guid id, IMediator mediator) =>
                {
                    return mediator.Send(new GetSampleEntityQuery() { Id = id });
                }
            )
            .WithName("SampleEntityById")
            .WithOpenApi();

        app.MapPost(
                "/AddSampleEntity",
                ([FromBody] SampleBody sampleBody, IMediator mediator) =>
                {
                    return mediator.Send(
                        new AddSampleEntityCommand()
                        {
                            Id = Guid.NewGuid(),
                            Description = sampleBody.Description,
                            EventTime = DateTime.UtcNow,
                        }
                    );
                }
            )
            .WithName("AddSampleRequest")
            .WithOpenApi();
    }
}
