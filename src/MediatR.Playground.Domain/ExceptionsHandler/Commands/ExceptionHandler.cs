using MediatR.Pipeline;
using MediatR.Playground.Model.Command;
using Microsoft.Extensions.Logging;

namespace MediatR.Playground.Domain.ExceptionsHandler.Commands;

internal class ExceptionHandler
    : IRequestExceptionHandler<SampleCommand, SampleCommandComplete, Exception>
{
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILogger<ExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(
        SampleCommand request,
        Exception exception,
        RequestExceptionHandlerState<SampleCommandComplete> state,
        CancellationToken cancellationToken
    )
    {
        _logger.LogError(exception, $"---- Exception Handler: '{nameof(ExceptionHandler)}'");

        state.SetHandled(new SampleCommandComplete() { Id = Guid.Empty });
    }
}
