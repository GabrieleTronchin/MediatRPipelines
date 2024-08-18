namespace MediatR.Playground.Model.Request;

public class SampleRequest : IRequest<SampleRequestComplete>
{
    public Guid Id { get; set; }

    public DateTime EventTime { get; set; }

    public string Description { get; set; } = string.Empty;
}

public record SampleRequestComplete
{
    public Guid Id { get; set; }
}
