using FluentValidation;
using FluentValidation.Results;
using MediatR.Playground.Model.Command;
using MediatR.Playground.Pipelines.Command;
using NSubstitute;
using Xunit;

namespace MediatR.Playground.Tests;

public class ValidationBehaviorTests
{
    private readonly SampleCommand _sampleCommand = new()
    {
        Id = Guid.NewGuid(),
        EventTime = DateTime.UtcNow,
        Description = "Test command"
    };

    private readonly SampleCommandComplete _expectedResponse = new() { Id = Guid.NewGuid() };

    /// <summary>
    /// Validates: Requirements 1.1
    /// WHEN a request with invalid data is processed and at least one validator returns errors,
    /// ValidationBehavior throws FluentValidation.ValidationException.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValidatorsReturnErrors_ThrowsValidationException()
    {
        // Arrange
        var validator = Substitute.For<IValidator<SampleCommand>>();
        var next = Substitute.For<RequestHandlerDelegate<SampleCommandComplete>>();

        var failure = new ValidationFailure("Description", "Description is required");

        // ValidateAsync is called first but its result is not used for the error check
        validator.ValidateAsync(Arg.Any<ValidationContext<SampleCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(new[] { failure }));

        // Validate (synchronous) is what actually drives the error check
        validator.Validate(Arg.Any<ValidationContext<SampleCommand>>())
            .Returns(new ValidationResult(new[] { failure }));

        var behavior = new ValidationBehavior<SampleCommand, SampleCommandComplete>(new[] { validator });

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(_sampleCommand, next, CancellationToken.None));

        // next() should NOT have been called
        await next.DidNotReceive().Invoke();
    }

    /// <summary>
    /// Validates: Requirements 1.2
    /// WHEN a request with valid data is processed and no validator returns errors,
    /// ValidationBehavior invokes next() and returns the response.
    /// </summary>
    [Fact]
    public async Task Handle_WhenValidationPasses_CallsNextAndReturnsResponse()
    {
        // Arrange
        var validator = Substitute.For<IValidator<SampleCommand>>();
        var next = Substitute.For<RequestHandlerDelegate<SampleCommandComplete>>();

        next.Invoke().Returns(_expectedResponse);

        // ValidateAsync returns no errors
        validator.ValidateAsync(Arg.Any<ValidationContext<SampleCommand>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        // Validate (synchronous) returns no errors
        validator.Validate(Arg.Any<ValidationContext<SampleCommand>>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<SampleCommand, SampleCommandComplete>(new[] { validator });

        // Act
        var result = await behavior.Handle(_sampleCommand, next, CancellationToken.None);

        // Assert
        Assert.Equal(_expectedResponse, result);
        await next.Received(1).Invoke();
    }

    /// <summary>
    /// Validates: Requirements 1.3
    /// WHEN no validators are registered for the request type,
    /// ValidationBehavior invokes next() without throwing.
    /// </summary>
    [Fact]
    public async Task Handle_WhenNoValidatorsRegistered_CallsNextWithoutThrowing()
    {
        // Arrange
        var next = Substitute.For<RequestHandlerDelegate<SampleCommandComplete>>();
        next.Invoke().Returns(_expectedResponse);

        var behavior = new ValidationBehavior<SampleCommand, SampleCommandComplete>(
            Enumerable.Empty<IValidator<SampleCommand>>());

        // Act
        var result = await behavior.Handle(_sampleCommand, next, CancellationToken.None);

        // Assert
        Assert.Equal(_expectedResponse, result);
        await next.Received(1).Invoke();
    }
}
