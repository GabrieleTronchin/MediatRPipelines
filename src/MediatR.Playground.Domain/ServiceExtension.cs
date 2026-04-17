using FakeAuth.Service;
using FluentValidation;
using MediatR.Playground.Domain.ExceptionsHandler;
using MediatR.Playground.Domain.NotificationHandler;
using MediatR.Playground.Domain.NotificationHandler.Deduplication;
using MediatR.Playground.Model.Notifications;
using MediatR.Playground.Pipelines.Command;
using MediatR.Playground.Pipelines.Query;
using MediatR.Playground.Pipelines.Stream;
using MediatR.Playground.Pipelines.TransactionCommand;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MediatR.Playground.Domain;

public static class ServicesExtensions
{
    public static IServiceCollection AddMediatorSample(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFusionCache();

        services.AddMediatR(cfg =>
        {
            cfg.LicenseKey = configuration["MediatR:LicenseKey"];
            cfg.RegisterServicesFromAssembly(typeof(ServicesExtensions).Assembly);
            cfg.NotificationPublisher = new MultipleNotificationPublisher();
            cfg.NotificationPublisherType = typeof(MultipleNotificationPublisher);

            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(GlobalExceptionHandlingBehavior<,>));
            cfg.AddOpenBehavior(typeof(CommandAuthorizationBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));

            cfg.AddOpenStreamBehavior(typeof(GenericStreamLoggingBehavior<,>));
            cfg.AddOpenStreamBehavior(typeof(SampleFilterStreamBehavior<,>));
        });

        services.AddTransient<INotificationHandler<DeduplicationNotification>, DeduplicationNotificationHandler>();
        services.AddTransient<INotificationHandler<DeduplicationNotification>, DeduplicationNotificationHandler>();

        services.AddValidatorsFromAssembly(typeof(ServicesExtensions).Assembly);

        services.AddTransient<IAuthService, AuthService>();

        return services;
    }
}
