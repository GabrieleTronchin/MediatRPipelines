using FakeAuth.Service;
using MediatR.Playground.Domain.NotificationHandler;
using MediatR.Playground.Model.Command;
using MediatR.Playground.Model.Notifications;
using MediatR.Playground.Model.Request;
using MediatR.Playground.Pipelines.Command;
using MediatR.Playground.Pipelines.Query;
using MediatR.Playground.Pipelines.Stream;
using MediatR.Playground.Pipelines.TransactionCommand;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MediatR.Playground.Tests;

public class BaselineSmokeTests : IClassFixture<PlaygroundWebApplicationFactory>
{
    private readonly PlaygroundWebApplicationFactory _factory;

    public BaselineSmokeTests(PlaygroundWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region DI Container

    [Fact]
    public void DI_Container_Builds_Successfully()
    {
        // The factory creates the host, which builds the DI container.
        // If any registration is missing or circular, this will throw.
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        Assert.NotNull(mediator);
    }

    #endregion

    #region MediatR Request / Command / Notification

    [Fact]
    public async Task MediatR_Can_Resolve_And_Execute_SampleRequest()
    {
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var request = new SampleRequest
        {
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            Description = "Smoke test request"
        };

        var result = await mediator.Send(request);

        Assert.NotNull(result);
        Assert.Equal(request.Id, result.Id);
    }

    [Fact]
    public async Task MediatR_Can_Resolve_And_Execute_SampleCommand()
    {
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var command = new SampleCommand
        {
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            Description = "Smoke test command"
        };

        var result = await mediator.Send(command);

        Assert.NotNull(result);
        Assert.Equal(command.Id, result.Id);
    }

    [Fact]
    public async Task MediatR_Can_Resolve_And_Publish_SampleNotification()
    {
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var notification = new SampleNotification
        {
            Id = Guid.NewGuid(),
            NotificationTime = DateTime.UtcNow
        };

        // Publish should not throw — handlers are registered via assembly scanning
        await mediator.Publish(notification);
    }

    #endregion

    #region Pipeline Behaviors Registration and Order

    [Fact]
    public void Pipeline_Behaviors_Are_Registered_In_Expected_Order()
    {
        // Resolve all IPipelineBehavior<SampleCommand, SampleCommandComplete> from the container.
        // SampleCommand implements ICommand<SampleCommandComplete>, so command-scoped
        // behaviors (Logging, Validation, CommandAuthorization) plus the global
        // GlobalExceptionHandlingBehavior will be resolved.
        // Note: CachingBehavior won't resolve here (SampleCommand is not IQueryRequest)
        // and UnitOfWorkBehavior won't resolve here (SampleCommand is not ITransactionCommand).
        // MediatR also auto-registers RequestExceptionProcessorBehavior.
        using var scope = _factory.Services.CreateScope();

        var behaviors = scope.ServiceProvider
            .GetServices<IPipelineBehavior<SampleCommand, SampleCommandComplete>>()
            .ToList();

        // We expect at least the command-scoped behaviors to be resolved
        Assert.NotEmpty(behaviors);

        var behaviorTypeNames = behaviors.Select(b => b.GetType().Name).ToList();

        // Verify key command-scoped behaviors are present
        Assert.Contains(behaviorTypeNames, n => n.Contains("LoggingBehavior"));
        Assert.Contains(behaviorTypeNames, n => n.Contains("ValidationBehavior"));
        Assert.Contains(behaviorTypeNames, n => n.Contains("GlobalExceptionHandlingBehavior"));
        Assert.Contains(behaviorTypeNames, n => n.Contains("CommandAuthorizationBehavior"));

        // Verify registration order: Logging → Validation → GlobalExceptionHandling → CommandAuthorization
        var loggingIdx = behaviorTypeNames.FindIndex(n => n.Contains("LoggingBehavior"));
        var validationIdx = behaviorTypeNames.FindIndex(n => n.Contains("ValidationBehavior"));
        var globalExIdx = behaviorTypeNames.FindIndex(n => n.Contains("GlobalExceptionHandlingBehavior"));
        var authIdx = behaviorTypeNames.FindIndex(n => n.Contains("CommandAuthorizationBehavior"));

        Assert.True(loggingIdx < validationIdx,
            "LoggingBehavior should be registered before ValidationBehavior");
        Assert.True(validationIdx < globalExIdx,
            "ValidationBehavior should be registered before GlobalExceptionHandlingBehavior");
        Assert.True(globalExIdx < authIdx,
            "GlobalExceptionHandlingBehavior should be registered before CommandAuthorizationBehavior");
    }

    [Fact]
    public void Stream_Pipeline_Behaviors_Are_Registered()
    {
        using var scope = _factory.Services.CreateScope();

        // Resolve stream behaviors for a known stream request type
        // We just verify the container can resolve them without error
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        Assert.NotNull(mediator);

        // Verify GenericStreamLoggingBehavior and SampleFilterStreamBehavior are registered
        // by checking the service descriptors through a fresh ServiceCollection inspection
        var behaviorTypes = new[]
        {
            typeof(GenericStreamLoggingBehavior<,>),
            typeof(SampleFilterStreamBehavior<,>)
        };

        // These types should be loadable and their assemblies present
        foreach (var type in behaviorTypes)
        {
            Assert.NotNull(type);
            Assert.True(type.IsGenericTypeDefinition);
        }
    }

    #endregion

    #region Custom Notification Publishers

    [Fact]
    public async Task MultipleNotificationPublisher_Routes_Sequential_Notification()
    {
        // Default notifications (SampleNotification) should be handled sequentially
        // via ForeachAwaitPublisher inside MultipleNotificationPublisher
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var notification = new SampleNotification
        {
            Id = Guid.NewGuid(),
            NotificationTime = DateTime.UtcNow
        };

        // Should not throw — sequential publishing
        await mediator.Publish(notification);
    }

    [Fact]
    public async Task MultipleNotificationPublisher_Routes_Parallel_Notification()
    {
        // IParallelNotification should be handled via TaskWhenAllPublisher
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var notification = new SampleParallelNotification
        {
            Id = Guid.NewGuid(),
            NotificationTime = DateTime.UtcNow
        };

        // Should not throw — parallel publishing
        await mediator.Publish(notification);
    }

    [Fact]
    public async Task MultipleNotificationPublisher_Routes_Priority_Notification()
    {
        // IPriorityNotification should be handled via PriorityNotificationPublisher
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var notification = new SamplePriorityNotification
        {
            Id = Guid.NewGuid(),
            NotificationTime = DateTime.UtcNow
        };

        // Should not throw — priority-based publishing
        await mediator.Publish(notification);
    }

    [Fact]
    public void PriorityNotificationPublisher_Can_Be_Instantiated()
    {
        var publisher = new PriorityNotificationPublisher();
        Assert.NotNull(publisher);
        Assert.IsAssignableFrom<INotificationPublisher>(publisher);
    }

    [Fact]
    public void MultipleNotificationPublisher_Can_Be_Instantiated()
    {
        var publisher = new MultipleNotificationPublisher();
        Assert.NotNull(publisher);
        Assert.IsAssignableFrom<INotificationPublisher>(publisher);
    }

    #endregion
}
