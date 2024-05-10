using Sample.MediatRPipelines.Domain.Primitives;

namespace Sample.MediatRPipelines.Domain.Queries.Entity;

public class SampleEntityQuery : IQueryRequest<IEnumerable<SampleEntityQueryComplete>> { }

public record SampleEntityQueryComplete
{
    public Guid Id { get; set; }

    public DateTime EventTime { get; set; }

    public string Description { get; set; }
}
