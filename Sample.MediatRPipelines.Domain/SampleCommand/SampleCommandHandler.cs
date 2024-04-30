using MediatR;
using Microsoft.Extensions.Logging;

namespace Sample.MediatRPipelines.Domain.SampleCommand;

public class SampleCommandHandler : IRequestHandler<SampleCommand, SampleCommandComplete>
{
    private readonly ILogger<SampleCommandHandler> _logger;


    public SampleCommandHandler(ILogger<SampleCommandHandler> logger)

    {
        _logger = logger;
    }

    public async Task<SampleCommandComplete> Handle(SampleCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Command Executed Id:{Id};Description:{Description};EventTime:{EventTime}", request.Id, request.Description, request.EventTime);
        return new SampleCommandComplete() { Id = request.Id };
    }
}