using MediatR.Playground.API.Endpoints.Primitives;
using MediatR.Playground.Model.Queries.Entity;

namespace MediatR.Playground.API.Endpoints;

public class GlobalExceptionsEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("GlobalExceptions").WithTags("Global Exceptions Endpoints");

 
        group
            .MapGet(
                "/NotFoundException",
                (IMediator mediator) =>
                {
                    return mediator.Send(new GetSampleEntityQuery() { Id = Guid.Empty });
                }
            )
            .WithName("NotFoundExcptionGlobalHandler")
            .WithOpenApi();


        group
            .MapGet(
                "/InvalidOperationException",
                (IMediator mediator) =>
                {
                    return mediator.Send(new GetAllSampleEntitiesQuery() { RaiseException = true});
                }
            )
            .WithName("InvalidOperationException")
            .WithOpenApi();

    }
}
