using FakeAuth.Service;
using FluentValidation;
using MediatRPlayground.Pipelines.Command;
using MediatRPlayground.Pipelines.Query;
using MediatRPlayground.Pipelines.Stream;
using MediatRPlayground.Pipelines.TransactionCommand;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Playground.Domain;

public static class ServicesExtensions
{
    public static IServiceCollection AddMediatorSample(this IServiceCollection services)
    {
        services.AddFusionCache();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServicesExtensions).Assembly)
        );

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(CommandAuthorizationBehavior<,>)
        );

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(UnitOfWorkBehavior<,>));

        services.AddTransient(
            typeof(IStreamPipelineBehavior<,>),
            typeof(GenericStreamLoggingBehavior<,>)
        );

        services.AddTransient(
            typeof(IStreamPipelineBehavior<,>),
            typeof(SampleFilterStreamBehavior<,>)
        );

        services.AddValidatorsFromAssembly(typeof(ServicesExtensions).Assembly);

        services.AddTransient<IAuthService, AuthService>();

        return services;
    }
}
