using Sample.MediatRPipelines.Domain.Primitives;

namespace Sample.MediatRPipelines.Domain.Commands;

public class SampleCommand : ICommand<SampleCommandComplete>
{
    public Guid Id { get; set; }

    public DateTime EventTime { get; set; }

    public required string Description { get; set; }

    public Exception? RaiseException { get; set; }

}

public record SampleCommandComplete
{
    public Guid Id { get; set; }
}
