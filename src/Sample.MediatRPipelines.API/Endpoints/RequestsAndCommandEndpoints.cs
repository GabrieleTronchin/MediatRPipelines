using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sample.MediatRPipelines.API.Endpoints.Primitives;
using Sample.MediatRPipelines.API.Models;
using Sample.MediatRPipelines.Domain.Commands.SampleCommand;
using Sample.MediatRPipelines.Domain.Commands.SampleRequest;

namespace Sample.MediatRPipelines.API.Endpoints;

public class RequestsAndCommandEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/SampleCommand", ([FromBody] SampleBody sampleBody, IMediator mediator) =>
        {
            return mediator.Send(new SampleCommand() { Id = Guid.NewGuid(), Description = sampleBody.Description, EventTime = DateTime.UtcNow });
        })
        .WithName("SampleCommand")
        .WithOpenApi();

        app.MapPost("/SampleRequest", ([FromBody] SampleBody sampleBody, IMediator mediator) =>
        {
            return mediator.Send(new SampleRequest() { Id = Guid.NewGuid(), Description = sampleBody.Description, EventTime = DateTime.UtcNow });
        })
        .WithName("SampleRequest")
        .WithOpenApi();

    }
}