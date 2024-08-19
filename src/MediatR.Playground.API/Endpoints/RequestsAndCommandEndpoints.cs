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
            .WithOpenApi();

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
            .WithOpenApi();
    }
}
