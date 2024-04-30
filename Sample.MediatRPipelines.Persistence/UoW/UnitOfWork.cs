using Microsoft.EntityFrameworkCore.Storage;

namespace Sample.MediatRPipelines.Persistence.UoW;

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
