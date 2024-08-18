using Microsoft.EntityFrameworkCore.Storage;

namespace MediatR.Playground.Persistence.UoW;

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransaction();
    Task Commit();
    void Dispose();
}
