using Sample.MediatRPipelines.Domain.Primitives;

namespace Sample.MediatRPipelines.Domain.Queries.Entity;

public class GetAllSampleEntitiesQuery : IQueryRequest<IEnumerable<GetAllSampleEntitiesQueryResult>>
{
    public string CacheKey => $"{nameof(GetAllSampleEntitiesQuery)}-ALL";
}

public record GetAllSampleEntitiesQueryResult
{
    public Guid Id { get; set; }

    public DateTime EventTime { get; set; }

    public string Description { get; set; }
}
