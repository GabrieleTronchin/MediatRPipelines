namespace Sample.MediatRPipelines.API.Endpoints.Primitives;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
