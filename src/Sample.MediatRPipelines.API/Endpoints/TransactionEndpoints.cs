using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sample.MediatRPipelines.API.Endpoints.Primitives;
using Sample.MediatRPipelines.API.Models;

namespace Sample.MediatRPipelines.API.Endpoints;

public class TransactionEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
                "/SampleEntity",
                (IMediator mediator) =>
                {
                    return mediator.Send(new SampleEntityQuery());
                }
            )
            .WithName("SampleEntity")
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
                            EventTime = DateTime.UtcNow
                        }
                    );
                }
            )
            .WithName("AddSampleRequest")
            .WithOpenApi();
    }
}
