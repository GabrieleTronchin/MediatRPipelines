using FluentValidation;

namespace Sample.MediatRPipelines.Domain.Commands.SampleCommand;

public sealed class SampleCommandValidator : AbstractValidator<SampleCommand>
{
    public SampleCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().NotEqual(Guid.Empty);

        RuleFor(command => command.Description).NotEmpty();
    }
}
