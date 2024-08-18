using MediatR.Playground.Model.Primitives;

namespace MediatR.Playground.Model.Queries.Entity;

public class GetSampleEntityQuery : IQueryRequest<GetSampleEntityQueryResult>
{
    public Guid Id { get; set; }
    public string CacheKey => $"{nameof(GetAllSampleEntitiesQuery)}-{Id}";
}

public record GetSampleEntityQueryResult : IQueryResult
{
    public Guid Id { get; set; }

    public DateTime EventTime { get; set; }

    public string Description { get; set; } = string.Empty;
}
