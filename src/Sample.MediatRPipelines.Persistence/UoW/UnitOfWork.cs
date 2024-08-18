using Microsoft.EntityFrameworkCore.Storage;

namespace MediatR.Playground.Persistence.UoW;

public class UnitOfWork(SampleDbContext context) : IDisposable, IUnitOfWork
{
    public async Task<IDbContextTransaction> BeginTransaction()
    {
        return await context.Database.BeginTransactionAsync();
    }

    public async Task Commit()
    {
        await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}
