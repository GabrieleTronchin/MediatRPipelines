using MediatR.Playground.Model.Command;
using Microsoft.Extensions.Logging;

namespace MediatR.Playground.Domain.CommandHandler;

public class SampleCommandHandler : IRequestHandler<SampleCommand, SampleCommandComplete>
{
    private readonly ILogger<SampleCommandHandler> _logger;

    public SampleCommandHandler(ILogger<SampleCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<SampleCommandComplete> Handle(
        SampleCommand request,
        CancellationToken cancellationToken
    )
    {
        if (request.RaiseException != null)
            throw request.RaiseException;

        _logger.LogInformation(
            "Command Executed Id:{Id};Description:{Description};EventTime:{EventTime}",
            request.Id,
            request.Description,
            request.EventTime
        );

        return new SampleCommandComplete() { Id = request.Id };
    }
}
