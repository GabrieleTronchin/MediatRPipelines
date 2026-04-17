namespace MediatR.Playground.Persistence.UoW;

public interface IUnitOfWork
{
    Task BeginTransaction();
    Task Commit();
    Task Rollback();
    void Dispose();
}
