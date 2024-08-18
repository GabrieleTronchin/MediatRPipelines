namespace MediatR.Playground.Persistence.Repository;

public interface IRepository<T>
{
    Task<T?> GetById(Guid id);
    Task<IEnumerable<T>> GetAll(CancellationToken cancellationToken = default);
    IAsyncEnumerable<T> GetStream(CancellationToken cancellationToken = default);
    Task Add(T entity, CancellationToken cancellationToken = default);
    void Update(T entity);
    void Delete(T entity);
}
