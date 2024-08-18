using MediatR;

namespace Sample.MediatRPipelines.Domain.Primitives;

public interface IQueryRequest<out IQueryResult> : IRequest<IQueryResult>
{
    public string CacheKey { get; }
}
