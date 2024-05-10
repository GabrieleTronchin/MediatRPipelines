using MediatR;

namespace Sample.MediatRPipelines.Domain.Request;

public class SampleRequest : IRequest<SampleRequestComplete>
{
    public Guid Id { get; set; }

    public DateTime EventTime { get; set; }

    public string Description { get; set; }
}

public record SampleRequestComplete
{
    public Guid Id { get; set; }
}
