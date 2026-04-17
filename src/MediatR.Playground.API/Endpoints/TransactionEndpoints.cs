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
            .WithSummary("Get all sample entities")
            .WithDescription("Retrieves all sample entities from the in-memory database. Returns an empty list if no entities have been created.")
            .Produces<IEnumerable<GetAllSampleEntitiesQueryResult>>(StatusCodes.Status200OK);

        group
            .MapGet(
                "/SampleEntity/{id}",
                (Guid id, IMediator mediator) =>
                {
                    return mediator.Send(new GetSampleEntityQuery() { Id = id });
                }
            )
            .WithName("SampleEntityById")
            .WithSummary("Get a sample entity by ID")
            .WithDescription("Retrieves a specific sample entity by its unique identifier. Returns a default result if the entity is not found.")
            .Produces<GetSampleEntityQueryResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

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
            .WithSummary("Create a new sample entity")
            .WithDescription("Creates a new sample entity within a Unit of Work transaction. Returns the created entity details including id, description, and event time.")
            .Produces<AddSampleEntityCommandComplete>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
