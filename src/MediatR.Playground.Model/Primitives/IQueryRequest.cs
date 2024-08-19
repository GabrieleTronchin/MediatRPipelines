namespace MediatR.Playground.Model.Primitives;

public interface IQueryRequest<out IQueryResult> : IRequest<IQueryResult>
{
    public string CacheKey { get; }
}
