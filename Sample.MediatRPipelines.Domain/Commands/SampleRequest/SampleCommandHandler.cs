using MediatR;
using Microsoft.Extensions.Logging;

namespace Sample.MediatRPipelines.Domain.Commands.SampleRequest;

public class SampleRequestHandler : IRequestHandler<SampleRequest, SampleRequestComplete>
{
    private readonly ILogger<SampleRequestHandler> _logger;


    public SampleRequestHandler(ILogger<SampleRequestHandler> logger)

    {
        _logger = logger;
    }

    public async Task<SampleRequestComplete> Handle(SampleRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request Executed Id:{Id};Description:{Description};EventTime:{EventTime}", request.Id, request.Description, request.EventTime);
        return new SampleRequestComplete() { Id = request.Id };
    }
}