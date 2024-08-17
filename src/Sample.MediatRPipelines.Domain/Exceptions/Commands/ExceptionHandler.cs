using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Sample.MediatRPipelines.Domain.Commands;

namespace Sample.MediatRPipelines.Domain.Exceptions.Commands;

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
        _logger.LogError(
            exception,
            $"---- Exception Handler: '{nameof(ExceptionHandler)}'"
        );

        state.SetHandled(new SampleCommandComplete() { Id = Guid.Empty });
    }
}

