# Project Structure

## Solution Layout

All source code lives under `src/`. Documentation lives under `docs/`.

```
src/
├── MediatR.Playground.sln
├── MediatR.Playground.API/          # ASP.NET Core Web API (entry point)
├── MediatR.Playground.Domain/       # MediatR handlers and service registration
├── MediatR.Playground.Model/        # Models, commands, queries, notifications, marker interfaces
├── MediatR.Playground.Pipelines/    # Pipeline behaviors (logging, validation, auth, caching, UoW)
├── MediatR.Playground.Persistence/  # EF Core DbContext, repository, Unit of Work
└── FakeAuth.Service/                # Fake auth/authz service
docs/                                # Markdown documentation per topic
```

## Dependency Flow

```
API → Domain → Pipelines → Model
                         → Persistence
                         → FakeAuth.Service
```

The API project only references Domain. Domain wires up MediatR, pipelines, and all dependencies.

## Key Patterns

### Endpoint Registration
Endpoints implement `IEndpoint` (in `API/Endpoints/Primitives/`) and are auto-discovered via assembly scanning in `ServiceExtension.AddEndpoints()`. Each endpoint class defines a `MapEndpoint` method that registers minimal API routes grouped by feature.

### Marker Interfaces for Pipeline Filtering
Pipeline behaviors are scoped to specific request types using marker interfaces defined in `Model/Primitives/Request/`:
- `ICommand<TResponse>` — targets command pipelines (logging, validation, authorization)
- `IQueryRequest<TResult>` — targets query pipelines (caching)
- `ITransactionCommand<TResponse>` — targets Unit of Work pipeline

### Pipeline Behavior Organization
Behaviors in `Pipelines/` are organized by the request type they target:
- `Command/` — LoggingBehavior, ValidationBehavior, CommandAuthorizationBehavior
- `Query/` — CachingBehavior
- `Stream/` — GenericStreamLoggingBehavior, SampleFilterStreamBehavior
- `TransactionCommand/` — UnitOfWorkBehavior

### Service Registration
Each project exposes a `ServiceExtension` (or `ServicesExtensions`) class with extension methods on `IServiceCollection` for DI registration. These are composed in `Program.cs`. The `AddMediatorSample` method accepts `IConfiguration` to wire the MediatR license key. Pipeline behaviors are registered via `cfg.AddOpenBehavior()` / `cfg.AddOpenStreamBehavior()` inside the `AddMediatR` lambda.

### Notification Publishers
Custom notification publishers live in `Domain/NotificationHandler/`. The `MultipleNotificationPublisher` selects delivery strategy (sequential, parallel, priority) based on notification marker interfaces (`IParallelNotification`, `IPriorityNotification`).

### Model Organization
Models in `Model/` are organized by MediatR concept:
- `Command/` — command request types
- `Request/` — generic request types
- `Queries/` — query request types (with sub-folders per query)
- `Notifications/` — notification types (including `DeduplicationNotification` for MediatR 14 de-duplication demo)
- `TransactionCommand/` — transactional command types
- `Primitives/` — marker interfaces for requests and notifications
