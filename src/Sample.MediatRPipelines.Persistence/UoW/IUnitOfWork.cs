using Microsoft.EntityFrameworkCore.Storage;

namespace Sample.MediatRPipelines.Persistence.UoW;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransaction();
    Task Commit();
    void Dispose();
}