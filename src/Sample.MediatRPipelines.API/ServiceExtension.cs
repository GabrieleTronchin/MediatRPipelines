using Microsoft.Extensions.DependencyInjection.Extensions;
using Sample.MediatRPipelines.API.Endpoints.Primitives;
using System.Reflection;

namespace Sample.MediatRPipelines.API;

public static class ServiceExtension
{
    public static IServiceCollection AddEndpoints(
    this IServiceCollection services,
    Assembly assembly)
    {
        ServiceDescriptor[] serviceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        return services;
    }


    public static IApplicationBuilder MapEndpoints(
    this WebApplication app,
    RouteGroupBuilder? routeGroupBuilder = null)
    {
        IEnumerable<IEndpoint> endpoints = app.Services
            .GetRequiredService<IEnumerable<IEndpoint>>();

        IEndpointRouteBuilder builder =
            routeGroupBuilder is null ? app : routeGroupBuilder;

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(builder);
        }

        return app;
    }
}
