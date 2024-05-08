using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Sample.MediatRPipelines.Domain.Commands.SampleCommand;

internal class SampleCommandCommonExceptionHandler
    : IRequestExceptionHandler<SampleCommand, SampleCommandComplete, Exception>
{
    private readonly ILogger<SampleCommandCommonExceptionHandler> _logger;

    public SampleCommandCommonExceptionHandler(ILogger<SampleCommandCommonExceptionHandler> logger)
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
        _logger.LogError(
            exception,
            $"---- Exception Handler: '{nameof(SampleCommandCommonExceptionHandler)}'"
        );

        state.SetHandled(new SampleCommandComplete() { Id = Guid.Empty });
    }
}

internal class SampleCommandInvalidOperationExceptionHandler
    : IRequestExceptionHandler<SampleCommand, SampleCommandComplete, InvalidOperationException>
{
    private readonly ILogger<SampleCommandInvalidOperationExceptionHandler> _logger;

    public SampleCommandInvalidOperationExceptionHandler(
        ILogger<SampleCommandInvalidOperationExceptionHandler> logger
    )
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
            $"---- Exception Handler: '{nameof(SampleCommandInvalidOperationExceptionHandler)}'"
        );

        state.SetHandled(new SampleCommandComplete() { Id = Guid.Empty });
    }
}
