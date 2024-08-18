using FluentValidation;
using MediatR.Playground.Model.Command;

namespace MediatR.Playground.Domain.CommandHandler;

public sealed class SampleCommandValidator : AbstractValidator<SampleCommand>
{
    public SampleCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().NotEqual(Guid.Empty);

        RuleFor(command => command.Description).NotEmpty();
    }
}
