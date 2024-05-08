namespace Sample.MediatRPipelines.Persistence.Repository;

public interface IRepository<T>
{
    Task<T> GetById(int id);
    Task<IEnumerable<T>> GetAll();
    Task Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}
