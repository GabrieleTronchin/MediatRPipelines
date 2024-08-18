using Sample.MediatRPipelines.Domain.Primitives;

namespace Sample.MediatRPipelines.Domain.Exceptions;

internal class CommonErrorReturn : IQueryResult
{
    public string Message { get; set; } = string.Empty;

    public string Detail { get; set; } = string.Empty;

}
