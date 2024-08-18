namespace MediatR.Playground.API.Endpoints.Primitives;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
