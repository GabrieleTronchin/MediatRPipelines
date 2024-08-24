using MediatR.Pipeline;
using MediatR.Playground.Model.Command;
using Microsoft.Extensions.Logging;

namespace MediatR.Playground.Domain.ExceptionsHandler.Commands;

internal class InvalidOperationExceptionHandler
    : IRequestExceptionHandler<SampleCommand, SampleCommandComplete, InvalidOperationException>
{
    private readonly ILogger<InvalidOperationExceptionHandler> _logger;

    public InvalidOperationExceptionHandler(ILogger<InvalidOperationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(
        SampleCommand request,
        InvalidOperationException exception,
        RequestExceptionHandlerState<SampleCommandComplete> state,
        CancellationToken cancellationToken
    )
    {
        _logger.LogError(
            exception,
            $"---- Exception Handler: '{nameof(InvalidOperationExceptionHandler)}'"
        );

        state.SetHandled(new SampleCommandComplete() { Id = Guid.Empty });

        return Task.CompletedTask;
    }
}
