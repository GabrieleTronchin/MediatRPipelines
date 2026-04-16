using FakeAuth.Service;
using MediatR.Playground.Model.Command;
using MediatR.Playground.Pipelines.Command;
using NSubstitute;
using Xunit;

namespace MediatR.Playground.Tests;

public class CommandAuthorizationBehaviorTests
{
    private readonly SampleCommand _sampleCommand = new()
    {
        Id = Guid.NewGuid(),
        EventTime = DateTime.UtcNow,
        Description = "Test command"
    };

    private readonly SampleCommandComplete _expectedResponse = new() { Id = Guid.NewGuid() };

    /// <summary>
    /// Validates: Requirements 2.1
    /// WHEN the authorization service returns IsSuccess = false with an exception,
    /// CommandAuthorizationBehavior throws the exception returned by the authorization service.
    /// </summary>
    [Fact]
    public async Task Handle_WhenAuthFailsWithException_ThrowsSpecificException()
    {
        // Arrange
        var authService = Substitute.For<IAuthService>();
        var next = Substitute.For<RequestHandlerDelegate<SampleCommandComplete>>();

        var specificException = new InvalidOperationException("Not authorized");
        authService.OperationAlowed().Returns(new AuthResponse
        {
            IsSuccess = false,
            Exception = specificException
        });

        var behavior = new CommandAuthorizationBehavior<SampleCommand, SampleCommandComplete>(authService);

        // Act & Assert
        var thrown = await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(_sampleCommand, next, CancellationToken.None));

        Assert.Same(specificException, thrown);
        await next.DidNotReceive().Invoke();
    }

    /// <summary>
    /// Validates: Requirements 2.2
    /// WHEN the authorization service returns IsSuccess = false without a specific exception,
    /// CommandAuthorizationBehavior throws a generic Exception.
    /// </summary>
    [Fact]
    public async Task Handle_WhenAuthFailsWithoutException_ThrowsGenericException()
    {
        // Arrange
        var authService = Substitute.For<IAuthService>();
        var next = Substitute.For<RequestHandlerDelegate<SampleCommandComplete>>();

        authService.OperationAlowed().Returns(new AuthResponse
        {
            IsSuccess = false,
            Exception = null
        });

        var behavior = new CommandAuthorizationBehavior<SampleCommand, SampleCommandComplete>(authService);

        // Act & Assert
        var thrown = await Assert.ThrowsAsync<Exception>(
            () => behavior.Handle(_sampleCommand, next, CancellationToken.None));

        Assert.Equal(typeof(Exception), thrown.GetType());
        await next.DidNotReceive().Invoke();
    }

    /// <summary>
    /// Validates: Requirements 2.3
    /// WHEN the authorization service returns IsSuccess = true,
    /// CommandAuthorizationBehavior invokes next() and returns the response.
    /// </summary>
    [Fact]
    public async Task Handle_WhenAuthSucceeds_CallsNextAndReturnsResponse()
    {
        // Arrange
        var authService = Substitute.For<IAuthService>();
        var next = Substitute.For<RequestHandlerDelegate<SampleCommandComplete>>();

        authService.OperationAlowed().Returns(new AuthResponse { IsSuccess = true });
        next.Invoke().Returns(_expectedResponse);

        var behavior = new CommandAuthorizationBehavior<SampleCommand, SampleCommandComplete>(authService);

        // Act
        var result = await behavior.Handle(_sampleCommand, next, CancellationToken.None);

        // Assert
        Assert.Equal(_expectedResponse, result);
        await next.Received(1).Invoke();
    }
}
