using MediatR.Playground.Model.Primitives.Request;
using MediatR.Playground.Persistence.UoW;
using Microsoft.Extensions.Logging;

namespace MediatR.Playground.Pipelines.TransactionCommand;

public class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ITransactionCommand<TResponse>
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<UnitOfWorkBehavior<TRequest, TResponse>> _logger;

    public UnitOfWorkBehavior(
        ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger,
        IUnitOfWork uow
    )
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        await _uow.BeginTransaction();
        try
        {
            var response = await next();
            await _uow.Commit();
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred on transaction.");
            await _uow.Rollback();
            throw;
        }
    }
}
