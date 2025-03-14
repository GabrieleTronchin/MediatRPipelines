﻿using FakeAuth.Service;
using FluentValidation;
using MediatR.Playground.Domain.ExceptionsHandler;
using MediatR.Playground.Domain.NotificationHandler;
using MediatR.Playground.Pipelines.Command;
using MediatR.Playground.Pipelines.Query;
using MediatR.Playground.Pipelines.Stream;
using MediatR.Playground.Pipelines.TransactionCommand;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Playground.Domain;

public static class ServicesExtensions
{
    public static IServiceCollection AddMediatorSample(this IServiceCollection services)
    {
        services.AddFusionCache();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ServicesExtensions).Assembly);
            cfg.NotificationPublisher = new MultipleNotificationPublisher();
            cfg.NotificationPublisherType = typeof(MultipleNotificationPublisher);
        });

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GlobalExceptionHandlingBehavior<,>));

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
