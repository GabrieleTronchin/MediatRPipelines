using MediatR.Playground.API.Endpoints.Primitives;
using MediatR.Playground.API.Models;
using MediatR.Playground.Model.Command;
using MediatR.Playground.Model.Queries.Entity;
using Microsoft.AspNetCore.Mvc;

namespace MediatR.Playground.API.Endpoints;

public class SpecificExceptionsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("SpecificExceptions").WithTags("Specific Exceptions Endpoints");

        group
            .MapPost(
                "/InvalidOperationException",
                ([FromBody] SampleBody sampleBody, IMediator mediator) =>
                {
                    return mediator.Send(
                        new SampleCommand()
                        {
                            Id = Guid.NewGuid(),
                            Description = sampleBody.Description,
                            EventTime = DateTime.UtcNow,
                            RaiseException = new InvalidOperationException(
                                "Sample Invalid Operation"
                            ),
                        }
                    );
                }
            )
            .WithName("SampleCommandWithIOException")
            .WithOpenApi();

        group
            .MapPost(
                "/Exception",
                ([FromBody] SampleBody sampleBody, IMediator mediator) =>
                {
                    return mediator.Send(
                        new SampleCommand()
                        {
                            Id = Guid.NewGuid(),
                            Description = sampleBody.Description,
                            EventTime = DateTime.UtcNow,
                            RaiseException = new Exception("Sample Exception"),
                        }
                    );
                }
            )
            .WithName("SampleCommandWithException")
            .WithOpenApi();

    }
}
