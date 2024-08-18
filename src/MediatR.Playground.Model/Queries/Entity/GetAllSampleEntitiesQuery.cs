using MediatR.Playground.Model.Primitives;

namespace MediatR.Playground.Model.Queries.Entity;

public class GetAllSampleEntitiesQuery : IQueryRequest<IEnumerable<GetAllSampleEntitiesQueryResult>>
{
    public string CacheKey => $"{nameof(GetAllSampleEntitiesQuery)}-ALL";
}

public record GetAllSampleEntitiesQueryResult : IQueryResult
{
    public Guid Id { get; set; }

    public DateTime EventTime { get; set; }

    public string Description { get; set; } = string.Empty;
}
