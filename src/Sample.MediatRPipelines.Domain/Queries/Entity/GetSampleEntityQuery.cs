using Sample.MediatRPipelines.Domain.Primitives;

namespace Sample.MediatRPipelines.Domain.Queries.Entity;

public class GetSampleEntityQuery : IQueryRequest<GetSampleEntityQueryResult>
{
    public Guid Id { get; set; }
    public string CacheKey => $"{nameof(GetAllSampleEntitiesQuery)}-{Id}";
}

public record GetSampleEntityQueryResult
{
    public Guid Id { get; set; }

    public DateTime EventTime { get; set; }

    public string Description { get; set; }
}
