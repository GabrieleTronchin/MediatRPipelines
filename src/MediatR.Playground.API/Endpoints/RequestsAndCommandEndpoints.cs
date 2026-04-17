using MediatR.Playground.API.Endpoints.Primitives;
using MediatR.Playground.API.Models;
using MediatR.Playground.Model.Command;
using MediatR.Playground.Model.Request;
using Microsoft.AspNetCore.Mvc;

namespace MediatR.Playground.API.Endpoints;

public class RequestsAndCommandEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/Requests").WithTags("Requests Endpoints");

        group
            .MapPost(
                "/SampleCommand",
                ([FromBody] SampleBody sampleBody, IMediator mediator) =>
                {
                    return mediator.Send(
                        new SampleCommand()
                        {
                            Id = Guid.NewGuid(),
                            Description = sampleBody.Description,
                            EventTime = DateTime.UtcNow,
                        }
                    );
                }
            )
            .WithName("SampleCommand")
            .WithSummary("Execute a sample command")
            .WithDescription("Sends a SampleCommand through the MediatR pipeline, including logging, validation, and authorization behaviors. Returns the command completion result.")
            .Produces<SampleCommandComplete>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group
            .MapPost(
                "/SampleRequest",
                ([FromBody] SampleBody sampleBody, IMediator mediator) =>
                {
                    return mediator.Send(
                        new SampleRequest()
                        {
                            Id = Guid.NewGuid(),
                            Description = sampleBody.Description,
                            EventTime = DateTime.UtcNow,
                        }
                    );
                }
            )
            .WithName("SampleRequest")
            .WithSummary("Execute a sample request")
            .WithDescription("Sends a SampleRequest through the MediatR pipeline with logging behavior. Unlike commands, requests do not go through authorization.")
            .Produces<SampleRequestComplete>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
