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
        var group = app.MapGroup("/Exceptions").WithTags("Exceptions Endpoints");

        group
            .MapPost(
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
            .WithSummary("Trigger an InvalidOperationException")
            .WithDescription("Sends a SampleCommand configured to raise an InvalidOperationException, demonstrating per-request exception handling via the pipeline.")
            .Produces<SampleCommandComplete>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group
            .MapPost(
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
            .WithSummary("Trigger a generic Exception")
            .WithDescription("Sends a SampleCommand configured to raise a generic Exception, demonstrating per-request exception handling via the pipeline.")
            .Produces<SampleCommandComplete>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group
            .MapGet(
                "/NotFoundExceptionGlobalHandler",
                (IMediator mediator) =>
                {
                    return mediator.Send(new GetSampleEntityQuery() { Id = Guid.Empty });
                }
            )
            .WithName("NotFoundExcptionGlobalHandler")
            .WithSummary("Trigger a not-found exception via global handler")
            .WithDescription("Sends a query with an empty GUID to trigger a not-found exception, demonstrating the global exception handling middleware.")
            .Produces<GetSampleEntityQueryResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
