using MediatR.Playground.API.Endpoints.Primitives;
using MediatR.Playground.API.Models;
using MediatR.Playground.Model.Queries.Entity;
using MediatR.Playground.Model.TransactionCommand;
using Microsoft.AspNetCore.Mvc;

namespace MediatR.Playground.API.Endpoints;

public class TransactionEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/TransactionRequests").WithTags("Transaction Requests Endpoints");

        group
            .MapGet(
                "/SampleEntity",
                (IMediator mediator) =>
                {
                    return mediator.Send(new GetAllSampleEntitiesQuery());
                }
            )
            .WithName("SampleEntity")
            .WithOpenApi();

        group
            .MapGet(
                "/SampleEntity/{id}",
                (Guid id, IMediator mediator) =>
                {
                    return mediator.Send(new GetSampleEntityQuery() { Id = id });
                }
            )
            .WithName("SampleEntityById")
            .WithOpenApi();

        group
            .MapPost(
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
