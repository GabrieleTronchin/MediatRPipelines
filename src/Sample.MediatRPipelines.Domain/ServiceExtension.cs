﻿using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Sample.MediatRPipelines.Domain.FakeAuth;
using Sample.MediatRPipelines.Domain.Pipelines.Command;
using Sample.MediatRPipelines.Domain.Pipelines.Stream;
using Sample.MediatRPipelines.Domain.Pipelines.TransactionCommand;

namespace Sample.MediatRPipelines.Domain;

public static class ServicesExtensions
{
    public static IServiceCollection AddMediatorSample(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ServicesExtensions).Assembly)
        );

        //Just register the behaviors in the order you would like them to be called.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(CommandAuthorizationBehavior<,>)
        );
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

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
