using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Sample.MediatRPipelines.Domain.Commands;

namespace Sample.MediatRPipelines.Domain.Exceptions.Commands;

internal class InvalidOperationExceptionHandler
    : IRequestExceptionHandler<SampleCommand, SampleCommandComplete, InvalidOperationException>
{
    private readonly ILogger<InvalidOperationExceptionHandler> _logger;

    public InvalidOperationExceptionHandler(ILogger<InvalidOperationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(
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
    }
}
