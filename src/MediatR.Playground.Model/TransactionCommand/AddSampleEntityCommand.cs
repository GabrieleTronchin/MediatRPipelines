using MediatR.Playground.Model.Primitives.Request;

namespace MediatR.Playground.Model.TransactionCommand;

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
