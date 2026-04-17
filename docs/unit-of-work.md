# Unit of Work Pattern with MediatR Pipeline

> **Note:** This documentation was AI-generated based on the original article:
> [C# .NET 8 — Unit of Work Pattern with MediatR Pipeline](https://medium.com/@gabrieletronchin/c-net-8-unit-of-work-pattern-with-mediatr-pipeline-d7a374df3dcb).
> It is intended as a companion reference for the code in this repository.

## Overview

The Unit of Work pattern centralizes transaction management by wrapping handler execution in a database transaction. In this project, it is implemented as a MediatR pipeline behavior (`UnitOfWorkBehavior`) that automatically begins a transaction before the handler runs, commits on success, and rolls back on error. This keeps transaction logic out of individual handlers and ensures consistent behavior across all transactional commands.

## ITransactionCommand Interface

Not every command needs transactional handling. The `ITransactionCommand<TResponse>` marker interface extends `IRequest<TResponse>` and is used to distinguish commands that require a transaction from regular commands:

```csharp
public interface ITransactionCommand<out TResponse> : IRequest<TResponse> { }
```

Any command that implements `ITransactionCommand` will pass through the `UnitOfWorkBehavior` pipeline. Commands implementing `ICommand` instead will skip it entirely, since the behavior has a generic constraint that only matches `ITransactionCommand`.

For example, `AddSampleEntityCommand` implements `ITransactionCommand<AddSampleEntityCommandComplete>`, so it is automatically wrapped in a transaction when sent through MediatR:

```csharp
public class AddSampleEntityCommand : ITransactionCommand<AddSampleEntityCommandComplete>
{
    public Guid Id { get; set; }
    public DateTime EventTime { get; set; }
    public string Description { get; set; }
}
```

Source: [`../src/MediatR.Playground.Model/Primitives/Request/ITransactionCommand.cs`](../src/MediatR.Playground.Model/Primitives/Request/ITransactionCommand.cs)

## UnitOfWorkBehavior

`UnitOfWorkBehavior<TRequest, TResponse>` is an `IPipelineBehavior` with the constraint `where TRequest : ITransactionCommand<TResponse>`. It depends on `IUnitOfWork` to manage the transaction lifecycle and `ILogger` for error logging.

### Transactional Flow

The behavior follows this sequence:

1. **Begin transaction** — calls `_uow.BeginTransaction()` to open a new database transaction
2. **Execute handler** — calls `await next()` to invoke the next behavior in the chain or the handler itself
3. **Commit on success** — if the handler completes without throwing, calls `_uow.Commit()` to save changes and commit the transaction
4. **Rollback on error** — if an exception is thrown, logs the error, calls `_uow.Rollback()` to undo all changes, and re-throws the exception so the caller receives a proper error response

```
Request → UnitOfWorkBehavior (begin transaction) → Handler → Commit / Rollback → Response
```

The behavior only depends on `IUnitOfWork` — it has no knowledge of the underlying transaction object (e.g. `IDbContextTransaction`). Transaction lifecycle details are fully encapsulated inside the `UnitOfWork` implementation.

The key implementation:

```csharp
public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
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
```

Source: [`../src/MediatR.Playground.Pipelines/TransactionCommand/UnitOfWorkBehavior.cs`](../src/MediatR.Playground.Pipelines/TransactionCommand/UnitOfWorkBehavior.cs)

## IUnitOfWork and UnitOfWork

`IUnitOfWork` defines the contract for transaction management. The interface deliberately hides the underlying transaction object (e.g. `IDbContextTransaction`) so that consumers only depend on the abstraction, not on EF Core internals:

```csharp
public interface IUnitOfWork
{
    Task BeginTransaction();
    Task Commit();
    Task Rollback();
    void Dispose();
}
```

| Method | Description |
|--------|-------------|
| `BeginTransaction()` | Opens a new database transaction |
| `Commit()` | Persists all tracked changes (`SaveChangesAsync`) and commits the transaction (`CommitAsync`) as a single atomic operation |
| `Rollback()` | Rolls back the current transaction, discarding all uncommitted changes |
| `Dispose()` | Disposes the transaction and the underlying `DbContext` |

The concrete `UnitOfWork` class wraps a `SampleDbContext` (Entity Framework Core with an in-memory database) and stores the `IDbContextTransaction` as a private field. All transaction lifecycle details are internal to this class — callers never interact with the transaction directly.

Source: [`../src/MediatR.Playground.Persistence/UoW/IUnitOfWork.cs`](../src/MediatR.Playground.Persistence/UoW/IUnitOfWork.cs) · [`../src/MediatR.Playground.Persistence/UoW/UnitOfWork.cs`](../src/MediatR.Playground.Persistence/UoW/UnitOfWork.cs)

## Repository Pattern

The persistence layer uses a generic repository abstraction to decouple data access from the domain logic.

### IRepository

`IRepository<T>` defines standard CRUD operations plus streaming support:

| Method | Description |
|--------|-------------|
| `GetById(Guid id)` | Retrieves a single entity by its identifier |
| `GetAll(CancellationToken)` | Returns all entities as an `IEnumerable<T>` |
| `GetStream(CancellationToken)` | Returns all entities as an `IAsyncEnumerable<T>` for streaming scenarios |
| `Add(T entity, CancellationToken)` | Adds a new entity to the data store |
| `Update(T entity)` | Marks an entity as modified |
| `Delete(T entity)` | Removes an entity from the data store |

Source: [`../src/MediatR.Playground.Persistence/Repository/IRepository.cs`](../src/MediatR.Playground.Persistence/Repository/IRepository.cs)

### EntityFrameworkRepository

`EntityFrameworkRepository<T>` implements `IRepository<T>` using Entity Framework Core. It operates on a `DbSet<T>` obtained from the `SampleDbContext`. Changes made through the repository (Add, Update, Delete) are not persisted until `SaveChangesAsync()` is called — which is exactly what the `UnitOfWork.Commit()` method does. This is what ties the repository and the Unit of Work together: the repository stages changes, and the Unit of Work commits them as a single atomic operation.

The `GetStream` method uses `AsAsyncEnumerable()` with `yield return` to stream entities one at a time, supporting cancellation via `CancellationToken`.

Source: [`../src/MediatR.Playground.Persistence/Repository/EntityFrameworkRepository.cs`](../src/MediatR.Playground.Persistence/Repository/EntityFrameworkRepository.cs)

## DI Registration

The persistence layer registers both the repository and the Unit of Work in the DI container:

```csharp
services.AddTransient<IRepository<SampleEntity>, EntityFrameworkRepository<SampleEntity>>();
services.AddScoped<IUnitOfWork, UnitOfWork>();
```

`IUnitOfWork` is registered as **scoped** so that all operations within a single request share the same transaction context. `IRepository` is registered as **transient**.

The `UnitOfWorkBehavior` itself is registered as an open generic pipeline behavior alongside the other behaviors (see [Pipelines — Registration Order](pipelines.md#registration-order)).

Source: [`../src/MediatR.Playground.Persistence/ServicesExtensions.cs`](../src/MediatR.Playground.Persistence/ServicesExtensions.cs)

## Further Reading

- [C# .NET — Unit Of Work Pattern with MediatR Pipeline](https://medium.com/@gabrieletronchin/c-net-8-unit-of-work-pattern-with-mediatr-pipeline-d7a374df3dcb) — Medium article covering the Unit of Work pattern in depth
