using Sample.MediatRPipelines.Domain.Primitives;

namespace Sample.MediatRPipelines.Domain.TransactionCommand;

public class AddSampleEntityCommand : ITransactionCommand<AddSampleEntityCommandComplete>
{
    public Guid Id { get; set; }

    public DateTime EventTime { get; set; }

    public string Description { get; set; }
}

public record AddSampleEntityCommandComplete
{
    public bool IsSuccess { get; set; }
}
