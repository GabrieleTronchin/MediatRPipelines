using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Sample.MediatRPipelines.Domain.PipelineBehavior;

namespace Sample.MediatRPipelines.Domain;

public static class ServicesExtensions
{
    public static IServiceCollection AddMediatRDomainSample(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServicesExtensions).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CommandAuthorizationBehavior<,>));

        services.AddValidatorsFromAssembly(typeof(ServicesExtensions).Assembly);

        services.AddTransient<IAuthService, AuthService>();

        return services;

    }
}
