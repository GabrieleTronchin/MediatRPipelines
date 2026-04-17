using Microsoft.EntityFrameworkCore.Storage;

namespace MediatR.Playground.Persistence.UoW;

public class UnitOfWork(SampleDbContext context) : IDisposable, IUnitOfWork
{
    private IDbContextTransaction? _transaction;

    public async Task BeginTransaction()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task Commit()
    {
        await context.SaveChangesAsync();

        if (_transaction is not null)
        {
            await _transaction.CommitAsync();
        }
    }

    public async Task Rollback()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }
}
