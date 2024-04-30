using FluentValidation;

namespace Sample.MediatRPipelines.Domain.SampleCommand;


public sealed class SampleCommandValidator : AbstractValidator<SampleCommand>
{
    public SampleCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty()
            .NotEqual(Guid.Empty);
    }
}
