using MediatR.Playground.Domain.CommandHandler;
using MediatR.Playground.Model.Command;
using Xunit;

namespace MediatR.Playground.Tests;

public class SampleCommandValidatorTests
{
    private readonly SampleCommandValidator _validator = new();

    /// <summary>
    /// Validates: Requirement 8.1
    /// WHEN a SampleCommand with Id equal to Guid.Empty is validated,
    /// SampleCommandValidator produces a validation error for the Id property.
    /// </summary>
    [Fact]
    public void Validate_WhenIdIsGuidEmpty_ProducesValidationError()
    {
        // Arrange
        var command = new SampleCommand
        {
            Id = Guid.Empty,
            EventTime = DateTime.UtcNow,
            Description = "Valid description"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    /// <summary>
    /// Validates: Requirement 8.2
    /// WHEN a SampleCommand with an empty or null Description is validated,
    /// SampleCommandValidator produces a validation error for the Description property.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenDescriptionIsEmptyOrNull_ProducesValidationError(string? description)
    {
        // Arrange
        var command = new SampleCommand
        {
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            Description = description!
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description");
    }

    /// <summary>
    /// Validates: Requirement 8.3
    /// WHEN a SampleCommand with a valid Id and non-empty Description is validated,
    /// SampleCommandValidator produces no validation errors.
    /// </summary>
    [Fact]
    public void Validate_WhenIdAndDescriptionAreValid_ProducesNoErrors()
    {
        // Arrange
        var command = new SampleCommand
        {
            Id = Guid.NewGuid(),
            EventTime = DateTime.UtcNow,
            Description = "Valid description"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
