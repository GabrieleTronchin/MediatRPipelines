using MediatR.Playground.Model.Command;
using MediatR.Playground.Pipelines.Command;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace MediatR.Playground.Tests;

public class LoggingBehaviorTests
{
    private readonly SampleCommand _sampleCommand = new()
    {
        Id = Guid.NewGuid(),
        EventTime = DateTime.UtcNow,
        Description = "Test command"
    };

    private readonly SampleCommandComplete _expectedResponse = new() { Id = Guid.NewGuid() };

    /// <summary>
    /// Validates: Requirements 3.1
    /// WHEN a request is processed by LoggingBehavior,
    /// LoggingBehavior invokes the next() delegate and returns the response produced by the delegate.
    /// </summary>
    [Fact]
    public async Task Handle_CallsNextAndReturnsResponse()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<SampleCommand, SampleCommandComplete>>>();
        var next = Substitute.For<RequestHandlerDelegate<SampleCommandComplete>>();
        next.Invoke().Returns(_expectedResponse);

        var behavior = new LoggingBehavior<SampleCommand, SampleCommandComplete>(logger);

        // Act
        var result = await behavior.Handle(_sampleCommand, next, CancellationToken.None);

        // Assert
        Assert.Equal(_expectedResponse, result);
        await next.Received(1).Invoke();
    }

    /// <summary>
    /// Validates: Requirements 3.2
    /// WHEN a request is processed by LoggingBehavior,
    /// LoggingBehavior logs a message before and after invoking the next() delegate.
    /// </summary>
    [Fact]
    public async Task Handle_LogsBeforeAndAfterCallingNext()
    {
        // Arrange
        var logger = Substitute.For<ILogger<LoggingBehavior<SampleCommand, SampleCommandComplete>>>();
        var next = Substitute.For<RequestHandlerDelegate<SampleCommandComplete>>();
        next.Invoke().Returns(_expectedResponse);

        var behavior = new LoggingBehavior<SampleCommand, SampleCommandComplete>(logger);

        // Act
        await behavior.Handle(_sampleCommand, next, CancellationToken.None);

        // Assert — LogInformation is an extension method; verify the underlying ILogger.Log calls
        logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Handling SampleCommand")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());

        logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Handled SampleCommandComplete")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
