using MediatR;
using MediatR.Playground.API.Endpoints.Primitives;
using MediatR.Playground.API.Models;
using MediatR.Playground.Model.Command;
using MediatR.Playground.Model.Queries.Entity;
using Microsoft.AspNetCore.Mvc;

namespace MediatR.Playground.API.Endpoints;

public class ExceptionsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/SampleCommandWithIOException",
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

        app.MapPost(
                "/SampleCommandWithException",
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

        app.MapGet(
                "/NotFoundExceptionGlobalHandler",
                (IMediator mediator) =>
                {
                    return mediator.Send(new GetSampleEntityQuery() { Id = Guid.Empty });
                }
            )
            .WithName("NotFoundExcptionGlobalHandler")
            .WithOpenApi();
    }
}
