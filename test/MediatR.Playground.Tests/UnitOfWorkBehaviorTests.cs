using MediatR.Playground.Model.TransactionCommand;
using MediatR.Playground.Persistence.UoW;
using MediatR.Playground.Pipelines.TransactionCommand;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace MediatR.Playground.Tests;

public class UnitOfWorkBehaviorTests
{
    private readonly AddSampleEntityCommand _command = new()
    {
        Id = Guid.NewGuid(),
        EventTime = DateTime.UtcNow,
        Description = "Test transaction command"
    };

    private readonly AddSampleEntityCommandComplete _expectedResponse = new() { IsSuccess = true };

    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ILogger<UnitOfWorkBehavior<AddSampleEntityCommand, AddSampleEntityCommandComplete>> _logger =
        Substitute.For<ILogger<UnitOfWorkBehavior<AddSampleEntityCommand, AddSampleEntityCommandComplete>>>();

    /// <summary>
    /// Validates: Requirements 5.1
    /// WHEN a transactional request is successfully processed by the next() delegate,
    /// UnitOfWorkBehavior invokes BeginTransaction(), then Commit().
    /// </summary>
    [Fact]
    public async Task Handle_OnSuccess_CallsBeginTransactionThenCommit()
    {
        // Arrange
        var next = Substitute.For<RequestHandlerDelegate<AddSampleEntityCommandComplete>>();
        next.Invoke().Returns(_expectedResponse);

        var behavior = new UnitOfWorkBehavior<AddSampleEntityCommand, AddSampleEntityCommandComplete>(_logger, _uow);

        // Act
        var result = await behavior.Handle(_command, next, CancellationToken.None);

        // Assert
        Assert.Equal(_expectedResponse, result);
        await _uow.Received(1).BeginTransaction();
        await next.Received(1).Invoke();
        await _uow.Received(1).Commit();
        await _uow.DidNotReceive().Rollback();
    }

    /// <summary>
    /// Validates: Requirements 5.2
    /// WHEN the next() delegate throws an exception during transactional request processing,
    /// UnitOfWorkBehavior invokes Rollback() and does not call Commit().
    /// </summary>
    [Fact]
    public async Task Handle_OnNextThrowing_CallsRollbackAndDoesNotCommit()
    {
        // Arrange
        var next = Substitute.For<RequestHandlerDelegate<AddSampleEntityCommandComplete>>();
        next.Invoke().Throws(new InvalidOperationException("Something went wrong"));

        var behavior = new UnitOfWorkBehavior<AddSampleEntityCommand, AddSampleEntityCommandComplete>(_logger, _uow);

        // Act & Assert — exception is now propagated
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(_command, next, CancellationToken.None));

        await _uow.Received(1).BeginTransaction();
        await _uow.Received(1).Rollback();
        await _uow.DidNotReceive().Commit();
    }

    /// <summary>
    /// Validates: Requirements 5.3
    /// WHEN the next() delegate throws an exception,
    /// UnitOfWorkBehavior propagates the exception to the caller.
    /// </summary>
    [Fact]
    public async Task Handle_OnNextThrowing_PropagatesException()
    {
        // Arrange
        var next = Substitute.For<RequestHandlerDelegate<AddSampleEntityCommandComplete>>();
        next.Invoke().Throws(new InvalidOperationException("Something went wrong"));

        var behavior = new UnitOfWorkBehavior<AddSampleEntityCommand, AddSampleEntityCommandComplete>(_logger, _uow);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(_command, next, CancellationToken.None));

        Assert.Equal("Something went wrong", ex.Message);
    }
}
