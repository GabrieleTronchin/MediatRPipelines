namespace MediatR.Playground.Model.Primitives.Request;

public interface IQueryRequest<out IQueryResult> : IRequest<IQueryResult>
{
    public string CacheKey { get; }
}
