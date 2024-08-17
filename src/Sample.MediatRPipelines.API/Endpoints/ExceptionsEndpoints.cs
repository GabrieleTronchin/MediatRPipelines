using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sample.MediatRPipelines.API.Endpoints.Primitives;
using Sample.MediatRPipelines.API.Models;
using Sample.MediatRPipelines.Domain.Commands;
using Sample.MediatRPipelines.Domain.Queries.Entity;

namespace Sample.MediatRPipelines.API.Endpoints;

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
                    RaiseException = new InvalidOperationException("Sample Invalid Operation")
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
                        RaiseException = new Exception("Sample Exception")

                    }
                );
            }
            )
            .WithName("SampleCommandWithException")
            .WithOpenApi();

        app.MapGet(
           "/NotFoundExceptionGlobalHandler",
               (Guid id, IMediator mediator) =>
               {
                   return mediator.Send(new GetSampleEntityQuery() { Id = Guid.Empty });
               }
           )
           .WithName("NotFoundExcptionGlobalHandler")
           .WithOpenApi();


    }
}
