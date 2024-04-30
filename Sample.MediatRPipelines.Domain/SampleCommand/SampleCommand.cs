using Sample.MediatRPipelines.Domain.Primitives;

namespace Sample.MediatRPipelines.Domain.SampleCommand;

public class SampleCommand : ICommand<SampleCommandComplete>
{
    public Guid Id { get; set; }

    public DateTime EventTime { get; set; }

    public string Description { get; set; }

}

public record SampleCommandComplete
{
    public Guid Id { get; set; }
}
