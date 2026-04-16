using MediatR.Playground.Model.TransactionCommand;
using MediatR.Playground.Persistence.UoW;
using MediatR.Playground.Pipelines.TransactionCommand;
using Microsoft.EntityFrameworkCore.Storage;
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
    private readonly IDbContextTransaction _transaction = Substitute.For<IDbContextTransaction>();
    private readonly ILogger<UnitOfWorkBehavior<AddSampleEntityCommand, AddSampleEntityCommandComplete>> _logger =
        Substitute.For<ILogger<UnitOfWorkBehavior<AddSampleEntityCommand, AddSampleEntityCommandComplete>>>();

    public UnitOfWorkBehaviorTests()
    {
        _uow.BeginTransaction().Returns(_transaction);
    }

    /// <summary>
    /// Validates: Requirements 5.1
    /// WHEN a transactional request is successfully processed by the next() delegate,
    /// UnitOfWorkBehavior invokes BeginTransaction(), then Commit(), and finally Dispose() on the transaction.
    /// </summary>
    [Fact]
    public async Task Handle_OnSuccess_CallsBeginTransactionThenCommitThenDispose()
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
        _transaction.Received().Dispose();
    }

    /// <summary>
    /// Validates: Requirements 5.2
    /// WHEN the next() delegate throws an exception during transactional request processing,
    /// UnitOfWorkBehavior invokes RollbackAsync() on the transaction and Dispose().
    /// </summary>
    [Fact]
    public async Task Handle_OnNextThrowing_CallsRollbackAsyncAndDispose()
    {
        // Arrange
        var next = Substitute.For<RequestHandlerDelegate<AddSampleEntityCommandComplete>>();
        next.Invoke().Throws(new InvalidOperationException("Something went wrong"));

        var behavior = new UnitOfWorkBehavior<AddSampleEntityCommand, AddSampleEntityCommandComplete>(_logger, _uow);

        // Act
        await behavior.Handle(_command, next, CancellationToken.None);

        // Assert
        await _uow.Received(1).BeginTransaction();
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        _transaction.Received().Dispose();
        await _uow.DidNotReceive().Commit();
    }

    /// <summary>
    /// Validates: Requirements 5.3
    /// WHEN the next() delegate throws an exception,
    /// UnitOfWorkBehavior returns the default value for the response type instead of propagating the exception.
    /// </summary>
    [Fact]
    public async Task Handle_OnNextThrowing_ReturnsDefaultInsteadOfPropagating()
    {
        // Arrange
        var next = Substitute.For<RequestHandlerDelegate<AddSampleEntityCommandComplete>>();
        next.Invoke().Throws(new InvalidOperationException("Something went wrong"));

        var behavior = new UnitOfWorkBehavior<AddSampleEntityCommand, AddSampleEntityCommandComplete>(_logger, _uow);

        // Act — should NOT throw
        var result = await behavior.Handle(_command, next, CancellationToken.None);

        // Assert — default for a reference type is null
        Assert.Null(result);
    }
}
